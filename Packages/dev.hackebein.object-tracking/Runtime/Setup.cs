#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;

namespace Hackebein.ObjectTracking
{
    public class Setup : MonoBehaviour, IEditorOnly
    {
        public Utility.Modes mode = Utility.Modes.Advanced;
        public string assetFolder = "Assets/Hackebein/ObjectTracking/Generated";
        public string uuid = Guid.NewGuid().ToString();
        public GameObject rootGameObject;
        public AnimatorController controller;
        public VRCExpressionParameters expressionParameters;
        public float scale = 1.0f;
        public List<SetupTracker> trackers = new List<SetupTracker>();
        private AnimationClip ignoreClip;

        public void AddTracker()
        {
            trackers.Add(new SetupTracker());
        }

        public void RemoveTracker(int index)
        {
            trackers.RemoveAt(index);
        }

        public int CountUsedLayers()
        {
            return controller ? Utility.CountLayerStartingWith("ObjectTracking/", controller) : 0;
        }

        public int CountUsedAnimatorParameters()
        {
            return controller ? Utility.CountAnimatorParametersStartingWith("ObjectTracking/", controller) : 0;
        }

        public static int CountMaxExpressionParameters()
        {
            return 256;
        }

        public int CountExpectedExpressionParameters()
        {
            int costs = 0;
            foreach (SetupTracker tracker in trackers)
            {
                costs += tracker.GetExpressionParameters();
            }

            return costs;
        }

        public int CountUsedExpressionParameters()
        {
            return expressionParameters ? Utility.CountExpressionParametersStartingWith("ObjectTracking/", expressionParameters) : 0;
        }

        public int CountUsedTotalExpressionParameters()
        {
            return expressionParameters ? Utility.CountExpressionParametersStartingWith("", expressionParameters) : 0;
        }

        public static int GetMaxExpressionParameterCosts()
        {
            return VRCExpressionParameters.MAX_PARAMETER_COST;
        }

        public int GetExpectedExpressionParameterCosts()
        {
            int costs = 0;
            foreach (SetupTracker tracker in trackers)
            {
                costs += tracker.GetExpressionParameterCosts();
            }

            return costs;
        }

        public int GetUsedExpressionParameterCosts()
        {
            return expressionParameters ? Utility.GetExpressionParameterCostStartingWith("ObjectTracking/", expressionParameters) : 0;
        }

        public int GetUsedTotalExpressionParameterCosts()
        {
            return expressionParameters ? Utility.GetExpressionParameterCostStartingWith("", expressionParameters) : 0;
        }

        public bool IsInstalled()
        {
            return CountUsedLayers() + CountUsedAnimatorParameters() + CountUsedExpressionParameters() > 0;
        }

        public void Remove()
        {
            if (Directory.Exists(assetFolder))
            {
                Utility.RemoveAssets(assetFolder + "/" + uuid);
                if (Directory.GetDirectories(assetFolder, "*", SearchOption.AllDirectories).Length == 0)
                {
                    AssetDatabase.DeleteAsset(assetFolder);
                }
            }

            if (controller != null)
            {
                Utility.RemoveLayerStartingWith("ObjectTracking/", controller);
                Utility.RemoveAnimatorParametersStartingWith("ObjectTracking/", controller);
            }

            if (expressionParameters != null)
            {
                Utility.RemoveExpressionParametersStartingWith("ObjectTracking/", expressionParameters);
            }
        }

        public void Create()
        {
            // Cleanup
            Remove();
            
            // ignore animation
            ignoreClip = Utility.CreateClip("ignore", "_ignore", "IsActive", 0, 0, assetFolder);

            // Parameters
            controller = Utility.CreateBoolParameterAndAddToAnimator(controller, "IsLocal");
            controller = Utility.CreateBoolParameterAndAddToAnimator(controller, "ObjectTracking/IsRemotePreview");
            controller = Utility.CreateBoolParameterAndAddToAnimator(controller, "ObjectTracking/config/global");

            expressionParameters = Utility.CreateBoolParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/IsRemotePreview", false, false, false);
            expressionParameters = Utility.CreateBoolParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/config/global", false, false, false);
            foreach (SetupTracker tracker in trackers)
            {
                controller = Utility.CreateBoolParameterAndAddToAnimator(controller, "ObjectTracking/config/" + tracker.name);
                expressionParameters = Utility.CreateBoolParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/config/" + tracker.name, false, false, false);
            }
            for (int i = 0; i < 30; i++)
            {
                controller = Utility.CreateIntParameterAndAddToAnimator(controller, "ObjectTracking/config/value" + i);
                expressionParameters = Utility.CreateIntParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/config/value" + i, 0, false, false);
            }
            foreach (SetupTracker tracker in trackers)
            {
                tracker.AppendAnimatorControllerParameters(controller);
            }

            // Object for scaling
            GameObject scaleGameObject = Utility.FindOrCreateEmptyGameObject("ObjectTracking", rootGameObject);
            Utility.ResetGameObject(scaleGameObject);

            // Objects for tracking
            foreach (SetupTracker tracker in trackers)
            {
                tracker.AppendObjects(scaleGameObject);
            }

            // Set scale
            scaleGameObject.transform.localScale = new Vector3(scale, scale, scale);

            // Animation Controller
            CreateProcessingLayer();
            foreach (SetupTracker tracker in trackers)
            {
                tracker.AppendTransitionLayers(controller, assetFolder + "/" + uuid);
            }
        }

        private void CreateProcessingLayer()
        {
            //        [ Entry ]
            //         ||    \\
            //         v      v
            // [ Local ] <=> [ Remote ] <
            //
            // Transitions:
            //   Entry => Local: (default)
            //   Entry => Remote: IsLocal = False
            //   Local => Remote: IsLocal = False
            //   Remote => Local: IsLocal = True
            //   Remote => Remote: IsLocal = False
            //
            // Animations:
            //   Local: Empty (2 Frames)
            //   Remote: BlendTree
            //     DirectBlendTree: AAP (binary addition)
            //
            // Components:
            //   Remote: Parameter Driver
            //     Copy every bool/int to float (0-1/0-255 => 0-1)
            //
            // Layer Design:
            //   x:   30, y: 190
            //   x: +240, y: +80

            // Layer
            AnimatorControllerLayer layer = Utility.CreateLayer("ObjectTracking/Processing");
            controller.layers = controller.layers.Append(layer).ToArray();
            
            // Animation State Init
            AnimatorState stateInit = new AnimatorState
            {
                name = "Init",
                writeDefaultValues = false
            };

            Dictionary<string[], float[]> propsInit = new Dictionary<string[], float[]>
            {
                { new[] { "ObjectTracking", "IsActive" }, new[] { 1f, 1f } },
                { new[] { "ObjectTracking", "m_LocalPosition.x" }, new[] { 0f, 0f } },
                { new[] { "ObjectTracking", "m_LocalPosition.y" }, new[] { 0f, 0f } },
                { new[] { "ObjectTracking", "m_LocalPosition.z" }, new[] { 0f, 0f } },
                { new[] { "ObjectTracking", "m_LocalRotation.x" }, new[] { 0f, 0f } },
                { new[] { "ObjectTracking", "m_LocalRotation.y" }, new[] { 0f, 0f } },
                { new[] { "ObjectTracking", "m_LocalRotation.z" }, new[] { 0f, 0f } },
                { new[] { "ObjectTracking", "m_LocalRotation.w" }, new[] { 1f, 1f } },
                { new[] { "ObjectTracking", "m_LocalScale.x" }, new[] { scale, scale } },
                { new[] { "ObjectTracking", "m_LocalScale.y" }, new[] { scale, scale } },
                { new[] { "ObjectTracking", "m_LocalScale.z" }, new[] { scale, scale } },
            };
            foreach (SetupTracker tracker in trackers)
            {
                tracker.AppendResetList(propsInit);
            }

            stateInit.motion = Utility.CreateClip("init", propsInit, assetFolder);

            ChildAnimatorState stateInitChild = new ChildAnimatorState
            {
                state = stateInit,
                position = new Vector3(270, 110, 0)
            };

            layer.stateMachine.states = layer.stateMachine.states.Append(stateInitChild).ToArray();

            // Animation State Local
            AnimatorState stateLocal = new AnimatorState
            {
                name = "Local",
                writeDefaultValues = false,
                motion = ignoreClip
            };

            List<VRCAvatarParameterDriver.Parameter> parameterDriverParametersLocal = new List<VRCAvatarParameterDriver.Parameter>
            {
                Utility.ParameterDriverParameterSet("ObjectTracking/config/global", false),
            };
            foreach (SetupTracker tracker in trackers)
            {
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/" + tracker.name, false));
            }
            VRCAvatarParameterDriver parameterDriverLocal = ScriptableObject.CreateInstance<VRCAvatarParameterDriver>();
            parameterDriverLocal.parameters = parameterDriverParametersLocal;
            parameterDriverLocal.localOnly = false;
            stateLocal.behaviours = new StateMachineBehaviour[] { parameterDriverLocal };

            ChildAnimatorState stateLocalChild = new ChildAnimatorState
            {
                state = stateLocal,
                position = new Vector3(30, 190, 0)
            };

            layer.stateMachine.states = layer.stateMachine.states.Append(stateLocalChild).ToArray();
            
            // Animation State Remote
            AnimatorState stateRemote = new AnimatorState
            {
                name = "Remote",
                writeDefaultValues = true
            };

            Dictionary<string, AnimationClip> motionsRemote = new Dictionary<string, AnimationClip>();
            foreach (SetupTracker tracker in trackers)
            {
                tracker.AppendAAPMotionList(assetFolder + "/" + uuid, motionsRemote);
            }

            stateRemote.motion = Utility.CreateDirectBlendTree("processing", motionsRemote);

            List<VRCAvatarParameterDriver.Parameter> parameterDriverParametersRemote =
                new List<VRCAvatarParameterDriver.Parameter>();
            foreach (SetupTracker tracker in trackers)
            {
                tracker.AppendAvatarParameterDriverParameters(parameterDriverParametersRemote);
                tracker.AppendExpressionParameters(expressionParameters);
            }

            VRCAvatarParameterDriver parameterDriverRemote = ScriptableObject.CreateInstance<VRCAvatarParameterDriver>();
            parameterDriverRemote.parameters = parameterDriverParametersRemote;
            parameterDriverRemote.localOnly = false;
            stateRemote.behaviours = new StateMachineBehaviour[] { parameterDriverRemote };

            ChildAnimatorState stateRemoteChild = new ChildAnimatorState
            {
                state = stateRemote,
                position = new Vector3(270, 190, 0)
            };

            layer.stateMachine.states = layer.stateMachine.states.Append(stateRemoteChild).ToArray();

            // Transition Conditions
            Dictionary<string, bool> conditionsToLocal = new Dictionary<string, bool>
            {
                { "IsLocal", true },
                { "ObjectTracking/IsRemotePreview", false },
            };
            Dictionary<string, bool> conditionsToRemote = new Dictionary<string, bool>
            {
                { "IsLocal", false },
            };
            Dictionary<string, bool> conditionsToRemotePreview = new Dictionary<string, bool>
            {
                { "ObjectTracking/IsRemotePreview", true },
            };
            
            stateInit.transitions = new[]
            {
                Utility.CreateBoolTransition("isRemote", conditionsToRemote, stateRemote),
                Utility.CreateBoolTransition("isRemotePreview", conditionsToRemotePreview, stateRemote),
                Utility.CreateBoolTransition("isLocal", conditionsToLocal, stateLocal)
            };
            stateLocal.transitions = new[]
            {
                Utility.CreateBoolTransition("isRemote", conditionsToRemote, stateRemote),
                Utility.CreateBoolTransition("isRemotePreview", conditionsToRemotePreview, stateRemote),
            };
            stateRemote.transitions = new[]
            {
                Utility.CreateBoolTransition("isRemote", conditionsToRemote, stateRemote),
                Utility.CreateBoolTransition("isRemotePreview", conditionsToRemotePreview, stateRemote),
                Utility.CreateBoolTransition("isLocal", conditionsToLocal, stateLocal),
            };
            
            AnimatorState stateGlobal = new AnimatorState
            {
                name = "Global Config",
                writeDefaultValues = false,
                motion = ignoreClip
            };
 
            VRCAvatarParameterDriver parameterDriverGlobal = ScriptableObject.CreateInstance<VRCAvatarParameterDriver>();
            parameterDriverGlobal.parameters = new List<VRCAvatarParameterDriver.Parameter>
            {
                Utility.ParameterDriverParameterSet("ObjectTracking/config/value0", 1), // version number
            };
            parameterDriverGlobal.localOnly = false;
            stateGlobal.behaviours = new StateMachineBehaviour[] { parameterDriverGlobal };

            ChildAnimatorState stateGlobalChild = new ChildAnimatorState
            {
                state = stateGlobal,
                position = new Vector3(30, 270, 0)
            };

            stateLocal.AddTransition(Utility.CreateBoolTransition("isGlobal", "ObjectTracking/config/global", true, stateGlobal));
            stateGlobal.AddTransition(Utility.CreateBoolTransition("reset", "ObjectTracking/config/global", false, stateLocal));

            layer.stateMachine.states = layer.stateMachine.states.Append(stateGlobalChild).ToArray();
            
            // Animation State Config Groups
            for (int i = 0; i < trackers.Count; i++)
            {
                SetupTracker tracker = trackers[i];
                AnimatorState stateDevice = new AnimatorState
                {
                    name = "Tracker Config " + tracker.name,
                    writeDefaultValues = false,
                    motion = ignoreClip
                };

                VRCAvatarParameterDriver parameterDriverDevice = ScriptableObject.CreateInstance<VRCAvatarParameterDriver>();
                parameterDriverDevice.parameters = new List<VRCAvatarParameterDriver.Parameter>
                {
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value0", tracker.bitsRPX),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value1", tracker.bitsRPY),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value2", tracker.bitsRPZ),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value3", tracker.bitsRRX),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value4", tracker.bitsRRY),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value5", tracker.bitsRRZ),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value6", tracker.minLPX),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value7", tracker.minLPY),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value8", tracker.minLPZ),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value9", tracker.minLRX),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value10", tracker.minLRY),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value11", tracker.minLRZ),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value12", tracker.minRPX),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value13", tracker.minRPY),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value14", tracker.minRPZ),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value15", tracker.minRRX),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value16", tracker.minRRY),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value17", tracker.minRRZ),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value18", tracker.maxLPX),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value19", tracker.maxLPY),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value20", tracker.maxLPZ),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value21", tracker.maxLRX),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value22", tracker.maxLRY),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value23", tracker.maxLRZ),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value24", tracker.maxRPX),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value25", tracker.maxRPY),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value26", tracker.maxRPZ),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value27", tracker.maxRRX),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value28", tracker.maxRRY),
                    Utility.ParameterDriverParameterSet("ObjectTracking/config/value29", tracker.maxRRZ),
                    //Utility.ParameterDriverParameterSet("ObjectTracking/config/" + tracker.name, false),
                };
                parameterDriverDevice.localOnly = false;
                stateDevice.behaviours = new StateMachineBehaviour[] { parameterDriverDevice };

                ChildAnimatorState stateDeviceChild = new ChildAnimatorState
                {
                    state = stateDevice,
                    position = new Vector3(30, 350 + (i * 80), 0)
                };
                stateLocal.AddTransition(Utility.CreateBoolTransition("isDevice " + tracker.name, "ObjectTracking/config/" + tracker.name, true, stateDevice));
                stateDevice.AddTransition(Utility.CreateBoolTransition("reset", "ObjectTracking/config/global", false, stateLocal));

                layer.stateMachine.states = layer.stateMachine.states.Append(stateDeviceChild).ToArray();

            }

            Utility.AddSubAssetsToDatabase(layer, controller);
        }
    }
}
#endif