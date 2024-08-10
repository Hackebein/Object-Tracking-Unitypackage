#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
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
        public Utility.Modes mode = Utility.Modes.Simple;
        public string generatedAssetFolder = "Assets/Hackebein/ObjectTracking/Generated";
        public string uuid = Guid.NewGuid().ToString();
        public GameObject rootGameObject;
        public AnimatorController controller;
        public VRCExpressionParameters expressionParameters;
        public float scale = 1.0f;
        public float zOffset = 0.0f;
        public List<SetupTracker> trackers = new List<SetupTracker>();
        public bool debug = false;
        private AnimationClip ignoreClip;
        public float _lastRealHeight = 1.7f;
        public Dictionary<string[], float[]> _lastTrackerList = new Dictionary<string[], float[]>{
            {new string[]{"Playspace", "Playspace"}, new float[]{0, 0, 0, 0, 0, 0}}
        };

        /*public void AddTracker(string name)
        {
            trackers.Add(new SetupTracker(name));
        }*/

        /*public void RemoveTracker(int index)
        {
            trackers.RemoveAt(index);
        }*/

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
            return 8192;
        }

        public int CountExpectedExpressionParameters()
        {
            int costs = 0;
            costs++; // config/global
            costs++; // config/index
            costs++; // config/value
            costs++; // isRemotePreview
            costs++; // goStabilized
            costs++; // isStabilized
            foreach (SetupTracker tracker in trackers)
            {
                costs++; // config/<tracker.name>
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
            if (Directory.Exists(generatedAssetFolder))
            {
                Utility.RemoveAssets(generatedAssetFolder + "/" + uuid);
                if (Directory.GetDirectories(generatedAssetFolder, "*", SearchOption.AllDirectories).Length == 0)
                {
                    AssetDatabase.DeleteAsset(generatedAssetFolder);
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
            ignoreClip = Utility.CreateClip("ignore", "_ignore", "m_IsActive", 0, 0, generatedAssetFolder);

            // Parameters
            controller = Utility.CreateBoolParameterAndAddToAnimator(controller, "IsLocal");
            controller = Utility.CreateBoolParameterAndAddToAnimator(controller, "InStation");
            controller = Utility.CreateFloatParameterAndAddToAnimator(controller, "VelocityX");
            controller = Utility.CreateFloatParameterAndAddToAnimator(controller, "VelocityY");
            controller = Utility.CreateFloatParameterAndAddToAnimator(controller, "VelocityZ");
            controller = Utility.CreateBoolParameterAndAddToAnimator(controller, "ObjectTracking/isRemotePreview");
            controller = Utility.CreateBoolParameterAndAddToAnimator(controller, "ObjectTracking/goStabilized");
            controller = Utility.CreateBoolParameterAndAddToAnimator(controller, "ObjectTracking/isStabilized");
            controller = Utility.CreateBoolParameterAndAddToAnimator(controller, "ObjectTracking/config/global");
            controller = Utility.CreateIntParameterAndAddToAnimator(controller, "ObjectTracking/config/index");
            controller = Utility.CreateIntParameterAndAddToAnimator(controller, "ObjectTracking/config/value");

            expressionParameters = Utility.CreateBoolParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/isRemotePreview", false, false, false);
            expressionParameters = Utility.CreateBoolParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/goStabilized", false, false, false);
            expressionParameters = Utility.CreateBoolParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/isStabilized", false, false, false);
            expressionParameters = Utility.CreateBoolParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/config/global", false, false, false);
            expressionParameters = Utility.CreateIntParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/config/index", 0, false, false);
            expressionParameters = Utility.CreateIntParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/config/value", 0, false, false);
            foreach (SetupTracker tracker in trackers)
            {
                controller = Utility.CreateBoolParameterAndAddToAnimator(controller, "ObjectTracking/config/" + tracker.name);
                expressionParameters = Utility.CreateBoolParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/config/" + tracker.name, false, false, false);
            }
            foreach (SetupTracker tracker in trackers)
            {
                tracker.AppendAnimatorControllerParameters(controller);
            }

            // Object for scaling and z-offset
            GameObject scaleGameObject = Utility.FindOrCreateEmptyGameObject("ObjectTracking", rootGameObject);
            Utility.ResetGameObject(scaleGameObject);

            // Objects for tracking
            foreach (SetupTracker tracker in trackers)
            {
                tracker.AppendObjects(scaleGameObject);
            }

            // Set scale
            scaleGameObject.transform.localScale = new Vector3(scale, scale, scale);
            
            // TODO: check if this needs to be applied before or after scaling.
            scaleGameObject.transform.localPosition = new Vector3(0, 0, zOffset);

            // Animation Controller
            CreateProcessingLayer();
            CreateStabilizationLayer();
            foreach (SetupTracker tracker in trackers)
            {
                tracker.AppendTransitionLayers(controller, generatedAssetFolder + "/" + uuid);
            }

            Utility.MarkDirty(expressionParameters);
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
                { new[] { "ObjectTracking", "m_IsActive" }, new[] { 1f, 1f } },
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

            stateInit.motion = Utility.CreateClip("init", propsInit, generatedAssetFolder);

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
                Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 0),
                Utility.ParameterDriverParameterSet("ObjectTracking/config/value", 0),
                Utility.ParameterDriverParameterSet("ObjectTracking/config/global", true),
                Utility.ParameterDriverParameterSet("ObjectTracking/config/value", 1), // version number
                Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 1),
                Utility.ParameterDriverParameterSet("ObjectTracking/config/global", false),
                Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 0),
                Utility.ParameterDriverParameterSet("ObjectTracking/config/value", 0),
            };
            for (int i = 0; i < trackers.Count; i++)
            {
                SetupTracker tracker = trackers[i];
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/" + tracker.name, true));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.bitsRPX));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 1));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.bitsRPY));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 2));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.bitsRPZ));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 3));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.bitsRRX));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 4));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.bitsRRY));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 5));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.bitsRRZ));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 6));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.minLPX));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 7));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.minLPY));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 8));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.minLPZ));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 9));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.minLRX));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 10));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.minLRY));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 11));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.minLRZ));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 12));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.minRPX));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 13));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.minRPY));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 14));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.minRPZ));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 15));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.minRRX));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 16));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.minRRY));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 17));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.minRRZ));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 18));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.maxLPX));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 19));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.maxLPY));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 20));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.maxLPZ));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 21));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.maxLRX));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 22));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.maxLRY));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 23));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.maxLRZ));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 24));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.maxRPX));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 25));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.maxRPY));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 26));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.maxRPZ));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 27));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.maxRRX));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 28));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.maxRRY));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 29));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", tracker.maxRRZ));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 30));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/" + tracker.name, false));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 0));
                parameterDriverParametersLocal.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", 0));
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
                tracker.AppendAAPMotionList(generatedAssetFolder + "/" + uuid, motionsRemote);
            }

            stateRemote.motion = Utility.CreateDirectBlendTree("processing", motionsRemote);

            List<VRCAvatarParameterDriver.Parameter> parameterDriverParametersRemote = new List<VRCAvatarParameterDriver.Parameter>();
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
            Tuple<string, bool>[] conditionsToLocal = new Tuple<string, bool>[]
            {
                Tuple.Create("IsLocal", true),
                Tuple.Create("ObjectTracking/isRemotePreview", false),
            };
            Tuple<string, bool>[] conditionsToRemote = new Tuple<string, bool>[]
            {
                Tuple.Create("IsLocal", false),
            };
            Tuple<string, bool>[] conditionsToRemotePreview = new Tuple<string, bool>[]
            {
                Tuple.Create("ObjectTracking/isRemotePreview", true),
            };
            Tuple<string, bool>[] conditionsReload = new Tuple<string, bool>[]
            {
                Tuple.Create("ObjectTracking/config/global", true),
            };
            
            stateInit.transitions = new[]
            {
                Utility.CreateTransition("isRemote", conditionsToRemote, stateRemote),
                Utility.CreateTransition("isRemotePreview", conditionsToRemotePreview, stateRemote),
                Utility.CreateTransition("isLocal", conditionsToLocal, stateLocal)
            };
            stateLocal.transitions = new[]
            {
                Utility.CreateTransition("isRemote", conditionsToRemote, stateRemote),
                Utility.CreateTransition("isRemotePreview", conditionsToRemotePreview, stateRemote),
                Utility.CreateTransition("reload", conditionsReload, stateLocal)
            };
            stateRemote.transitions = new[]
            {
                Utility.CreateTransition("isRemote", conditionsToRemote, stateRemote),
                Utility.CreateTransition("isRemotePreview", conditionsToRemotePreview, stateRemote),
                Utility.CreateTransition("isLocal", conditionsToLocal, stateLocal),
            };

            Utility.AddSubAssetsToDatabase(layer, controller);
        }

        public void CreateStabilizationLayer()
        {
            // Layer
            AnimatorControllerLayer layer = Utility.CreateLayer("ObjectTracking/Stabilization");
            controller.layers = controller.layers.Append(layer).ToArray();
            
            // Animation State Off
            AnimatorState stateOff = new AnimatorState
            {
                name = "Off",
                writeDefaultValues = false,
                motion = ignoreClip
            };

            VRCAnimatorLocomotionControl locomotionControlOff = ScriptableObject.CreateInstance<VRCAnimatorLocomotionControl>();
            locomotionControlOff.disableLocomotion = false;
            
            VRCAvatarParameterDriver parameterDriverOff = ScriptableObject.CreateInstance<VRCAvatarParameterDriver>();
            parameterDriverOff.parameters = new List<VRCAvatarParameterDriver.Parameter>
            {
                Utility.ParameterDriverParameterSet("ObjectTracking/isStabilized", false)
            };
            parameterDriverOff.localOnly = false;
            stateOff.behaviours = new StateMachineBehaviour[] { locomotionControlOff, parameterDriverOff };

            ChildAnimatorState stateOffChild = new ChildAnimatorState
            {
                state = stateOff,
                position = new Vector3(30, 190, 0)
            };

            layer.stateMachine.states = layer.stateMachine.states.Append(stateOffChild).ToArray();
            
            // Animation State On
            AnimatorState stateOn = new AnimatorState
            {
                name = "On",
                writeDefaultValues = false,
                motion = ignoreClip
            };

            VRCAnimatorLocomotionControl locomotionControlOn = ScriptableObject.CreateInstance<VRCAnimatorLocomotionControl>();
            locomotionControlOn.disableLocomotion = true;
            
            VRCAvatarParameterDriver parameterDriverOn = ScriptableObject.CreateInstance<VRCAvatarParameterDriver>();
            parameterDriverOn.parameters = new List<VRCAvatarParameterDriver.Parameter>
            {
                Utility.ParameterDriverParameterSet("ObjectTracking/isStabilized", true)
            };
            parameterDriverOn.localOnly = false;
            stateOn.behaviours = new StateMachineBehaviour[] { locomotionControlOn, parameterDriverOn };
            
            ChildAnimatorState stateOnChild = new ChildAnimatorState
            {
                state = stateOn,
                position = new Vector3(270, 190, 0)
            };
            
            layer.stateMachine.states = layer.stateMachine.states.Append(stateOnChild).ToArray();
            
            // Transition Conditions
            Tuple<string, bool>[] conditionsToOnBools = new Tuple<string, bool>[]
            {
                Tuple.Create("IsLocal", true),
                Tuple.Create("ObjectTracking/goStabilized", true),
                Tuple.Create("InStation", false),
            };
            Tuple<string, AnimatorConditionMode, float>[] conditionsToOnFloats = new Tuple<string, AnimatorConditionMode, float>[]
            {
                Tuple.Create("VelocityX", AnimatorConditionMode.Greater, (float)-1 / 127),
                Tuple.Create("VelocityX", AnimatorConditionMode.Less, (float)1 / 127),
                Tuple.Create("VelocityY", AnimatorConditionMode.Greater, (float)-1 / 127),
                Tuple.Create("VelocityY", AnimatorConditionMode.Less, (float)1 / 127),
                Tuple.Create("VelocityZ", AnimatorConditionMode.Greater, (float)0),
            };
            Tuple<string, bool>[] conditionsToOff = new Tuple<string, bool>[]
            {
                Tuple.Create("ObjectTracking/goStabilized", false)
            };
            
            stateOff.transitions = new[]
            {
                Utility.CreateTransition("isStabilization", conditionsToOnBools, conditionsToOnFloats, stateOn)
            };
            
            stateOn.transitions = new[]
            {
                Utility.CreateTransition("isUnstabilization", conditionsToOff, stateOff)
            };

            Utility.AddSubAssetsToDatabase(layer, controller);
        }
    }
}
#endif