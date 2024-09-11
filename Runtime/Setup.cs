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
        public bool stabilizationSupport = true;
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
            if (stabilizationSupport)
            {
                costs++; // goStabilized
                costs++; // isStabilized
            }
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
            ignoreClip = Utility.CreateClip("ignore", "_ignore", "GameObject.m_IsActive", 0, 0, generatedAssetFolder);

            // Parameters
            controller = Utility.CreateBoolParameterAndAddToAnimator(controller, "IsLocal");
            controller = Utility.CreateBoolParameterAndAddToAnimator(controller, "InStation");
            controller = Utility.CreateFloatParameterAndAddToAnimator(controller, "VelocityX");
            controller = Utility.CreateFloatParameterAndAddToAnimator(controller, "VelocityY");
            controller = Utility.CreateFloatParameterAndAddToAnimator(controller, "VelocityZ");
            controller = Utility.CreateBoolParameterAndAddToAnimator(controller, "ObjectTracking/isRemotePreview");
            if (stabilizationSupport)
            {
                controller = Utility.CreateBoolParameterAndAddToAnimator(controller, "ObjectTracking/goStabilized");
                controller = Utility.CreateBoolParameterAndAddToAnimator(controller, "ObjectTracking/isStabilized");
            }
            controller = Utility.CreateBoolParameterAndAddToAnimator(controller, "ObjectTracking/config/global");
            controller = Utility.CreateIntParameterAndAddToAnimator(controller, "ObjectTracking/config/index");
            controller = Utility.CreateIntParameterAndAddToAnimator(controller, "ObjectTracking/config/value");

            expressionParameters = Utility.CreateBoolParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/isRemotePreview", false, false, false);
            if (stabilizationSupport)
            {
                expressionParameters = Utility.CreateBoolParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/goStabilized", false, false, false);
                expressionParameters = Utility.CreateBoolParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/isStabilized", false, false, false);
            }
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
            if (stabilizationSupport)
            {
                CreateStabilizationLayer();
            }
            foreach (SetupTracker tracker in trackers)
            {
                tracker.AppendHideBeyondLimitsLayer(controller, generatedAssetFolder + "/" + uuid);
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
                // Trying to prevent the user to break Object Tracking
                { new[] { "ObjectTracking", "GameObject.m_IsActive" }, new[] { 1f, 1f } },
                { new[] { "ObjectTracking", "Transform.m_LocalPosition.x" }, new[] { 0f, 0f } },
                { new[] { "ObjectTracking", "Transform.m_LocalPosition.y" }, new[] { 0f, 0f } },
                { new[] { "ObjectTracking", "Transform.m_LocalPosition.z" }, new[] { zOffset, zOffset } },
                { new[] { "ObjectTracking", "Transform.m_LocalRotation.x" }, new[] { 0f, 0f } },
                { new[] { "ObjectTracking", "Transform.m_LocalRotation.y" }, new[] { 0f, 0f } },
                { new[] { "ObjectTracking", "Transform.m_LocalRotation.z" }, new[] { 0f, 0f } },
                { new[] { "ObjectTracking", "Transform.m_LocalRotation.w" }, new[] { 1f, 1f } },
                { new[] { "ObjectTracking", "Transform.m_LocalScale.x" }, new[] { scale, scale } },
                { new[] { "ObjectTracking", "Transform.m_LocalScale.y" }, new[] { scale, scale } },
                { new[] { "ObjectTracking", "Transform.m_LocalScale.z" }, new[] { scale, scale } },
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

            // Local Init
            Dictionary<string[], float[]> propsLocalInit = new Dictionary<string[], float[]> {};
            foreach (SetupTracker tracker in trackers)
            {
                tracker.AppendLocalResetList(propsLocalInit);
            }
            
            // Animation State Local
            AnimatorState stateLocal = new AnimatorState
            {
                name = "Local",
                writeDefaultValues = false,
                motion = Utility.CreateClip("init (local)", propsLocalInit, generatedAssetFolder)
            };

            List<VRCAvatarParameterDriver.Parameter> parameterDriverParametersLocal = new List<VRCAvatarParameterDriver.Parameter>
            {
                Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 0),
                Utility.ParameterDriverParameterSet("ObjectTracking/config/value", 0),
            };
            Utility.AddTrackerStart(parameterDriverParametersLocal, "global");
            Utility.AddConfigValue(parameterDriverParametersLocal, 1, 1); // version number
            Utility.AddTrackerEnd(parameterDriverParametersLocal, "global");
            for (int i = 0; i < trackers.Count; i++)
            {
                SetupTracker tracker = trackers[i];
                Utility.AddTrackerStart(parameterDriverParametersLocal, tracker.name);
                Utility.AddConfigValue(parameterDriverParametersLocal, 1, tracker.bitsRPX);
                Utility.AddConfigValue(parameterDriverParametersLocal, 2, tracker.bitsRPY);
                Utility.AddConfigValue(parameterDriverParametersLocal, 3, tracker.bitsRPZ);
                Utility.AddConfigValue(parameterDriverParametersLocal, 4, tracker.bitsRRX);
                Utility.AddConfigValue(parameterDriverParametersLocal, 5, tracker.bitsRRY);
                Utility.AddConfigValue(parameterDriverParametersLocal, 6, tracker.bitsRRZ);
                Utility.AddConfigValue(parameterDriverParametersLocal, 7, tracker.minLPX);
                Utility.AddConfigValue(parameterDriverParametersLocal, 8, tracker.minLPY);
                Utility.AddConfigValue(parameterDriverParametersLocal, 9, tracker.minLPZ);
                Utility.AddConfigValue(parameterDriverParametersLocal, 10, tracker.minLRX);
                Utility.AddConfigValue(parameterDriverParametersLocal, 11, tracker.minLRY);
                Utility.AddConfigValue(parameterDriverParametersLocal, 12, tracker.minLRZ);
                Utility.AddConfigValue(parameterDriverParametersLocal, 13, tracker.minRPX);
                Utility.AddConfigValue(parameterDriverParametersLocal, 14, tracker.minRPY);
                Utility.AddConfigValue(parameterDriverParametersLocal, 15, tracker.minRPZ);
                Utility.AddConfigValue(parameterDriverParametersLocal, 16, tracker.minRRX);
                Utility.AddConfigValue(parameterDriverParametersLocal, 17, tracker.minRRY);
                Utility.AddConfigValue(parameterDriverParametersLocal, 18, tracker.minRRZ);
                Utility.AddConfigValue(parameterDriverParametersLocal, 19, tracker.maxLPX);
                Utility.AddConfigValue(parameterDriverParametersLocal, 20, tracker.maxLPY);
                Utility.AddConfigValue(parameterDriverParametersLocal, 21, tracker.maxLPZ);
                Utility.AddConfigValue(parameterDriverParametersLocal, 22, tracker.maxLRX);
                Utility.AddConfigValue(parameterDriverParametersLocal, 23, tracker.maxLRY);
                Utility.AddConfigValue(parameterDriverParametersLocal, 24, tracker.maxLRZ);
                Utility.AddConfigValue(parameterDriverParametersLocal, 25, tracker.maxRPX);
                Utility.AddConfigValue(parameterDriverParametersLocal, 26, tracker.maxRPY);
                Utility.AddConfigValue(parameterDriverParametersLocal, 27, tracker.maxRPZ);
                Utility.AddConfigValue(parameterDriverParametersLocal, 28, tracker.maxRRX);
                Utility.AddConfigValue(parameterDriverParametersLocal, 29, tracker.maxRRY);
                Utility.AddConfigValue(parameterDriverParametersLocal, 30, tracker.maxRRZ);
                Utility.AddTrackerEnd(parameterDriverParametersLocal, tracker.name);
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
                Utility.CreateTransition("isRemote", conditionsToRemote, stateInit),
                Utility.CreateTransition("isRemotePreview", conditionsToRemotePreview, stateInit),
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