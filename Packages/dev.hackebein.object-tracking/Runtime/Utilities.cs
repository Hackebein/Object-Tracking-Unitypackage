#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Hackebein.ObjectTracking
{
    public static class Utility
    {
        [Flags]
        public enum Modes
        {
            Simple = 0,
            Advanced = 1,
            Expert = 2,
        }

        public static readonly string[] ModesText =
        {
            "Simple",
            "Advanced",
            "Expert"
        };

        [Flags]
        public enum TrackerType
        {
            None,
            EZtrackSWAN,
            HtcVive10,
            HtcVive20,
            HtcVive30,
            LogitechVRInkStylus,
            ManusSteamVRProTracker,
            TundraLabsTundraTrackerScrew,
            TundraLabsTundraTrackerScrewRotated,
            TundraLabsTundraTrackerStrap,
        }

        public static readonly string[] TrackerTypeText =
        {
            "None",
            "EZtrack - SWAN (coming soon)",
            "HTC - VIVE 1.0",
            "HTC - VIVE 2.0",
            "HTC - VIVE 3.0",
            "Logitech - VR Ink Stylus (coming soon)",
            "Manus - SteamVR Pro Tracker (coming soon)",
            "Tundra Labs - Tundra Tracker (1\u22154\" Screw Mount)",
            "Tundra Labs - Tundra Tracker (1\u22154\" Screw Mount, rotated)",
            "Tundra Labs - Tundra Tracker (Strap Mount)",
        };

        public static readonly String[] TrackerTypeGameObjects =
        {
            null, // None
            null, // EZtrack SWAN
            "Packages/dev.hackebein.object-tracking/Prefab/HTC Vive 2.0.fbx",
            "Packages/dev.hackebein.object-tracking/Prefab/HTC Vive 2.0.fbx",
            "Packages/dev.hackebein.object-tracking/Prefab/HTC Vive 3.0.fbx",
            null, // Logitech VR Ink Stylus
            null, // Manus SteamVR Pro Tracker
            "Packages/dev.hackebein.object-tracking/Prefab/Tundra Labs Tundra Tracker screw.fbx",
            "Packages/dev.hackebein.object-tracking/Prefab/Tundra Labs Tundra Tracker screw rotated.fbx",
            "Packages/dev.hackebein.object-tracking/Prefab/Tundra Labs Tundra Tracker strap.fbx",
        };
/*
        public static AnimatorControllerParameter CreateAnimatorParameter<T>(string name, T defaultValue)
        {
            AnimatorControllerParameter parameter = new AnimatorControllerParameter
            {
                name = name
            };

            switch (defaultValue)
            {
                case bool b:
                    parameter.type = AnimatorControllerParameterType.Bool;
                    parameter.defaultBool = b;
                    break;
                case float f:
                    parameter.type = AnimatorControllerParameterType.Float;
                    parameter.defaultFloat = f;
                    break;
                case int i:
                    parameter.type = AnimatorControllerParameterType.Int;
                    parameter.defaultInt = i;
                    break;
                default:
                    throw new ArgumentException("Unsupported parameter type");
            }

            return parameter;
        }
*/
        public static AnimatorControllerParameter CreateBoolAnimatorParameter(string name, bool value = false)
        {
            AnimatorControllerParameter parameter = new AnimatorControllerParameter
            {
                type = AnimatorControllerParameterType.Bool,
                name = name,
                defaultBool = value
            };

            return parameter;
        }

        public static AnimatorController CreateBoolParameterAndAddToAnimator(AnimatorController controller, string name, bool value = false)
        {
            AnimatorControllerParameter parameter = CreateBoolAnimatorParameter(name, value);
            AnimatorControllerParameter[] parameters = controller.parameters;
            if (parameters.Where(val => val.name == name).ToArray().Length > 0)
            {
                return controller;
            }

            controller.parameters = parameters.Append(parameter).ToArray();

            return controller;
        }

        public static AnimatorControllerParameter CreateFloatAnimatorParameter(string name, float value = 0.0f)
        {
            AnimatorControllerParameter parameter = new AnimatorControllerParameter
            {
                type = AnimatorControllerParameterType.Float,
                name = name,
                defaultFloat = value
            };

            return parameter;
        }

        public static AnimatorController CreateFloatParameterAndAddToAnimator(AnimatorController controller, string name, float value = 0.0f)
        {
            AnimatorControllerParameter parameter = CreateFloatAnimatorParameter(name, value);
            AnimatorControllerParameter[] parameters = controller.parameters;
            if (parameters.Where(val => val.name == name).ToArray().Length > 0)
            {
                return controller;
            }

            controller.parameters = parameters.Append(parameter).ToArray();

            return controller;
        }

        public static AnimatorControllerParameter CreateIntAnimatorParameter(string name, int value = 0)
        {
            AnimatorControllerParameter parameter = new AnimatorControllerParameter
            {
                type = AnimatorControllerParameterType.Int,
                name = name,
                defaultInt = value
            };

            return parameter;
        }

        public static AnimatorController CreateIntParameterAndAddToAnimator(AnimatorController controller, string name, int value = 0)
        {
            AnimatorControllerParameter parameter = CreateIntAnimatorParameter(name, value);
            AnimatorControllerParameter[] parameters = controller.parameters;
            if (parameters.Where(val => val.name == name).ToArray().Length > 0)
            {
                return controller;
            }

            controller.parameters = parameters.Append(parameter).ToArray();

            return controller;
        }

        public static VRCExpressionParameters.Parameter CreateIntExpressionParameter(string name, int value = 0, bool saved = true, bool synced = true)
        {
            VRCExpressionParameters.Parameter parameter = new VRCExpressionParameters.Parameter
            {
                name = name,
                valueType = VRCExpressionParameters.ValueType.Int,
                defaultValue = value,
                saved = saved,
                networkSynced = synced
            };

            return parameter;
        }

        public static VRCExpressionParameters CreateIntParameterAndAddToExpressionParameters(VRCExpressionParameters expression, string name, int value = 0, bool saved = true, bool synced = true)
        {
            VRCExpressionParameters.Parameter parameter = CreateIntExpressionParameter(name, value, saved, synced);
            List<VRCExpressionParameters.Parameter> expressionParameters =
                new List<VRCExpressionParameters.Parameter>(expression.parameters);
            if (expressionParameters.Where(val => val.name == name).ToArray().Length > 0)
            {
                return expression;
            }

            expression.parameters = expressionParameters.Append(parameter).ToArray();

            return expression;
        }

        public static VRCExpressionParameters.Parameter CreateFloatExpressionParameter(string name, float value = 0.0f, bool saved = true, bool synced = true)
        {
            VRCExpressionParameters.Parameter parameter = new VRCExpressionParameters.Parameter
            {
                name = name,
                valueType = VRCExpressionParameters.ValueType.Float,
                defaultValue = value,
                saved = saved,
                networkSynced = synced
            };

            return parameter;
        }

        public static VRCExpressionParameters CreateFloatParameterAndAddToExpressionParameters(VRCExpressionParameters expression, string name, float value = 0.0f, bool saved = true, bool synced = true)
        {
            VRCExpressionParameters.Parameter parameter = CreateFloatExpressionParameter(name, value, saved, synced);
            VRCExpressionParameters.Parameter[] parameters = expression.parameters;
            if (parameters.Where(val => val.name == name).ToArray().Length > 0)
            {
                return expression;
            }

            expression.parameters = parameters.Append(parameter).ToArray();

            return expression;
        }

        public static VRCExpressionParameters.Parameter CreateBoolExpressionParameter(string name, bool value = false, bool saved = true, bool synced = true)
        {
            VRCExpressionParameters.Parameter parameter = new VRCExpressionParameters.Parameter
            {
                name = name,
                valueType = VRCExpressionParameters.ValueType.Bool,
                defaultValue = value ? 1 : 0,
                saved = saved,
                networkSynced = synced
            };

            return parameter;
        }

        public static VRCExpressionParameters CreateBoolParameterAndAddToExpressionParameters(VRCExpressionParameters expression, string name, bool value = false, bool saved = true, bool synced = true)
        {
            VRCExpressionParameters.Parameter parameter = CreateBoolExpressionParameter(name, value, saved, synced);
            List<VRCExpressionParameters.Parameter> expressionParameters =
                new List<VRCExpressionParameters.Parameter>(expression.parameters);
            if (expressionParameters.Where(val => val.name == name).ToArray().Length > 0)
            {
                return expression;
            }

            expression.parameters = expressionParameters.Append(parameter).ToArray();

            return expression;
        }

        public static AnimatorControllerLayer CreateLayer(string name)
        {
            AnimatorControllerLayer layer = new AnimatorControllerLayer
            {
                name = name,
                defaultWeight = 1
            };
            AnimatorStateMachine stateMachine = new AnimatorStateMachine();
            layer.stateMachine = stateMachine;

            return layer;
        }

        public static VRCAvatarParameterDriver.Parameter ParameterDriverParameterSet(string name, bool value)
        {
            return ParameterDriverParameterSet(name, value ? 1 : 0);
        }

        public static VRCAvatarParameterDriver.Parameter ParameterDriverParameterSet(string name, int value)
        {
            VRCAvatarParameterDriver.Parameter parameterDriverParameter = new VRCAvatarParameterDriver.Parameter
            {
                type = VRCAvatarParameterDriver.ChangeType.Set,
                name = name,
                value = value
            };

            return parameterDriverParameter;
        }

        public static VRCAvatarParameterDriver.Parameter ParameterDriverParameterIntToFloat(string from, string to)
        {
            VRCAvatarParameterDriver.Parameter parameterDriverParameter = new VRCAvatarParameterDriver.Parameter
            {
                type = VRCAvatarParameterDriver.ChangeType.Copy,
                source = from,
                name = to, //destination
                convertRange = true,
                sourceMin = 0,
                sourceMax = 255,
                destMin = 0,
                destMax = 1
            };

            return parameterDriverParameter;
        }

        public static VRCAvatarParameterDriver.Parameter ParameterDriverParameterBoolToFloat(string from, string to)
        {
            VRCAvatarParameterDriver.Parameter parameterDriverParameter = new VRCAvatarParameterDriver.Parameter
            {
                type = VRCAvatarParameterDriver.ChangeType.Copy,
                source = from,
                name = to, //destination
                convertRange = true,
                sourceMin = 0,
                sourceMax = 1,
                destMin = 0,
                destMax = 1
            };

            return parameterDriverParameter;
        }
        
        private static AnimatorCondition[] CreateAnimatorCondition(Tuple<string, bool>[] boolConditions, Tuple<string, AnimatorConditionMode, int>[] intConditions, Tuple<string, AnimatorConditionMode, float>[] floatConditions)
        {
            AnimatorCondition[] animatorConditions = Array.Empty<AnimatorCondition>();
            foreach (Tuple<string, bool> pair in boolConditions)
            {
                AnimatorCondition condition = new AnimatorCondition
                {
                    parameter = pair.Item1,
                    mode = pair.Item2 ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot
                };

                animatorConditions = animatorConditions.Append(condition).ToArray();
            }
            foreach (Tuple<string, AnimatorConditionMode, int> pair in intConditions)
            {
                AnimatorCondition condition = new AnimatorCondition
                {
                    parameter = pair.Item1,
                    mode = pair.Item2,
                    threshold = pair.Item3,
                };

                animatorConditions = animatorConditions.Append(condition).ToArray();
            }
            foreach (Tuple<string, AnimatorConditionMode, float> pair in floatConditions)
            {
                AnimatorCondition condition = new AnimatorCondition
                {
                    parameter = pair.Item1,
                    mode = pair.Item2,
                    threshold = pair.Item3,
                };

                animatorConditions = animatorConditions.Append(condition).ToArray();
            }

            return animatorConditions;
        }

        public static AnimatorStateTransition CreateTransition(string name, Tuple<string, bool>[] boolConditions, Tuple<string, AnimatorConditionMode, int>[] intConditions, Tuple<string, AnimatorConditionMode, float>[] floatConditions, AnimatorState target)
        {
            return new AnimatorStateTransition
            {
                name = name,
                conditions = CreateAnimatorCondition(boolConditions, intConditions, floatConditions),
                destinationState = target,
                duration = 0,
                hasExitTime = false,
                exitTime = 0
            };
        }

        public static AnimatorTransition CreateEntryTransition(string name, Tuple<string, bool>[] boolConditions, Tuple<string, AnimatorConditionMode, int>[] intConditions, Tuple<string, AnimatorConditionMode, float>[] floatConditions, AnimatorState target)
        {
            return new AnimatorTransition 
            {
                name = name,
                conditions = CreateAnimatorCondition(boolConditions, intConditions, floatConditions),
                destinationState = target
            };
        }

        public static AnimatorStateTransition CreateTransition(string name, Tuple<string, bool>[] boolConditions, AnimatorState target)
        {
            return CreateTransition(name, boolConditions, Array.Empty<Tuple<string, AnimatorConditionMode, int>>(), Array.Empty<Tuple<string, AnimatorConditionMode, float>>(), target);
        }

        public static AnimatorStateTransition CreateTransition(string name, Tuple<string, AnimatorConditionMode, int>[] intConditions, AnimatorState target)
        {
            return CreateTransition(name, Array.Empty<Tuple<string, bool>>(), intConditions, Array.Empty<Tuple<string, AnimatorConditionMode, float>>(), target);
        }

        public static AnimatorStateTransition CreateTransition(string name, Tuple<string, AnimatorConditionMode, float>[] floatConditions, AnimatorState target)
        {
            return CreateTransition(name, Array.Empty<Tuple<string, bool>>(), Array.Empty<Tuple<string, AnimatorConditionMode, int>>(), floatConditions, target);
        }

        public static AnimatorStateTransition CreateTransition(string name, Tuple<string, bool>[] boolConditions, Tuple<string, AnimatorConditionMode, int>[] intConditions, AnimatorState target)
        {
            return CreateTransition(name, boolConditions, intConditions, Array.Empty<Tuple<string, AnimatorConditionMode, float>>(), target);
        }

        public static AnimatorStateTransition CreateTransition(string name, Tuple<string, bool>[] boolConditions, Tuple<string, AnimatorConditionMode, float>[] floatConditions, AnimatorState target)
        {
            return CreateTransition(name, boolConditions, Array.Empty<Tuple<string, AnimatorConditionMode, int>>(), floatConditions, target);
        }

        public static AnimatorStateTransition CreateTransition(string name, Tuple<string, AnimatorConditionMode, int>[] intConditions, Tuple<string, AnimatorConditionMode, float>[] floatConditions, AnimatorState target)
        {
            return CreateTransition(name, Array.Empty<Tuple<string, bool>>(), intConditions, floatConditions, target);
        }
        
        public static AnimatorTransition CreateEntryTransition(string name, Tuple<string, bool>[] boolConditions, AnimatorState target)
        {
            return CreateEntryTransition(name, boolConditions, Array.Empty<Tuple<string, AnimatorConditionMode, int>>(), Array.Empty<Tuple<string, AnimatorConditionMode, float>>(), target);
        }

        public static AnimatorTransition CreateEntryTransition(string name, Tuple<string, AnimatorConditionMode, int>[] intConditions, AnimatorState target)
        {
            return CreateEntryTransition(name, Array.Empty<Tuple<string, bool>>(), intConditions, Array.Empty<Tuple<string, AnimatorConditionMode, float>>(), target);
        }

        public static AnimatorTransition CreateEntryTransition(string name, Tuple<string, AnimatorConditionMode, float>[] floatConditions, AnimatorState target)
        {
            return CreateEntryTransition(name, Array.Empty<Tuple<string, bool>>(), Array.Empty<Tuple<string, AnimatorConditionMode, int>>(), floatConditions, target);
        }

        public static AnimatorTransition CreateEntryTransition(string name, Tuple<string, bool>[] boolConditions, Tuple<string, AnimatorConditionMode, int>[] intConditions, AnimatorState target)
        {
            return CreateEntryTransition(name, boolConditions, intConditions, Array.Empty<Tuple<string, AnimatorConditionMode, float>>(), target);
        }

        public static AnimatorTransition CreateEntryTransition(string name, Tuple<string, bool>[] boolConditions, Tuple<string, AnimatorConditionMode, float>[] floatConditions, AnimatorState target)
        {
            return CreateEntryTransition(name, boolConditions, Array.Empty<Tuple<string, AnimatorConditionMode, int>>(), floatConditions, target);
        }

        public static AnimatorTransition CreateEntryTransition(string name, Tuple<string, AnimatorConditionMode, int>[] intConditions, Tuple<string, AnimatorConditionMode, float>[] floatConditions, AnimatorState target)
        {
            return CreateEntryTransition(name, Array.Empty<Tuple<string, bool>>(), intConditions, floatConditions, target);
        }
        
        public static AnimationClip CreateClip(string name, string path, string property, float start, float end, string assetFolder)
        {
            Dictionary<string[], float[]> props = new Dictionary<string[], float[]> { { new[] { path, property }, new[] { start, end } } };
            return CreateClip(name, props, assetFolder);
        }

        public static AnimationClip CreateClip(string name, Dictionary<string[], float[]> props, string assetFolder)
        {
            AnimationClip clip = new AnimationClip
            {
                name = name
            };
            foreach (KeyValuePair<string[], float[]> prop in props)
            {
                Type type = typeof(GameObject);
                switch (prop.Key[1].Split('.')[0])
                {
                    case "m_TranslationOffsets":
                    case "m_RotationOffsets":
                        type = typeof(ParentConstraint);
                        break;
                    case "m_LocalPosition":
                    case "m_LocalRotation":
                    case "m_LocalScale":
                        type = typeof(Transform);
                        break;
                    case "m_IsActive":
                        type = typeof(GameObject);
                        break;
                    default:
                        if (prop.Key[0] == "")
                        {
                            // we expect only AAP on root level
                            type = typeof(Animator);
                        } else {
                            Debug.LogWarning("Can't map property to a type, fall back to GameObject: " + prop.Key[1]);
                        }
                        break;
                }
                EditorCurveBinding binding = EditorCurveBinding.FloatCurve(prop.Key[0], type, prop.Key[1]);
                AnimationCurve curve = AnimationCurve.Linear(0, prop.Value[0], 1 / 60f, prop.Value[1]);
                AnimationUtility.SetEditorCurve(clip, binding, curve);
            }

            CreatePathRecursive(System.IO.Path.GetDirectoryName(assetFolder + "/" + name + ".anim"));
            AssetDatabase.CreateAsset(clip, assetFolder + "/" + name + ".anim");

            return clip;
        }

        public static BlendTree CreateDirectBlendTree(string name, Dictionary<string, AnimationClip> motions)
        {
            BlendTree blendTree = new BlendTree
            {
                name = name,
                blendType = BlendTreeType.Direct
            };

            foreach (KeyValuePair<string, AnimationClip> pair in motions)
            {
                ChildMotion motion = new ChildMotion
                {
                    motion = pair.Value,
                    directBlendParameter = pair.Key,
                    timeScale = 1
                };
                blendTree.children = blendTree.children.Append(motion).ToArray();
            }

            return blendTree;
        }

        public static void ResetGameObject(GameObject gameObject)
        {
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;
            // Remove all components
            foreach (Component component in gameObject.GetComponents<Component>())
            {
                if (component is Transform)
                {
                    continue;
                }

                GameObject.DestroyImmediate(component);
            }
        }

        public static GameObject FindOrCreateEmptyGameObject(string name, [CanBeNull] GameObject parent = null)
        {
            GameObject gameObject = FindOrCreateGameObject(name, parent);
            ResetGameObject(gameObject);
            return gameObject;
        }

        public static GameObject FindOrCreateGameObject(string name, [CanBeNull] GameObject parent = null)
        {
            if (parent == null)
            {
                IEnumerable<GameObject> gameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().Where(val => val.name == name);
                if (gameObjects.Any())
                {
                    return gameObjects.First();
                }
            }
            else
            {
                Transform transform = parent.transform.Find(name);
                if (transform != null)
                {
                    return transform.gameObject;
                }
            }

            return CreateEmptyGameObject(name, parent);
        }

        public static GameObject CreateEmptyGameObject(string name, GameObject parent)
        {
            GameObject gameObject = new GameObject
            {
                name = name
            };
            if (parent != null)
            {
                gameObject.transform.parent = parent.transform;
            }

            return gameObject;
        }

        public static void RemoveLayerStartingWith(string name, AnimatorController controller)
        {
            controller.layers = controller.layers.Where(val => !val.name.StartsWith(name)).ToArray();
        }

        public static int CountLayerStartingWith(string name, AnimatorController controller)
        {
            return controller.layers.Where(val => val.name.StartsWith(name)).ToArray().Length;
        }

        public static void RemoveAnimatorParametersStartingWith(string name, AnimatorController controller)
        {
            controller.parameters = controller.parameters.Where(val => !val.name.StartsWith(name)).ToArray();
        }

        public static int CountAnimatorParametersStartingWith(string name, AnimatorController controller)
        {
            return controller.parameters.Where(val => val.name.StartsWith(name)).ToArray().Length;
        }

        public static void RemoveExpressionParametersStartingWith(string name, VRCExpressionParameters expressionParameters)
        {
            expressionParameters.parameters =
                expressionParameters.parameters.Where(val => !val.name.StartsWith(name)).ToArray();
        }

        public static int CountExpressionParametersStartingWith(string name, VRCExpressionParameters expressionParameters)
        {
            return expressionParameters.parameters.Where(val => val.name.StartsWith(name)).ToArray().Length;
        }

        public static int GetExpressionParameterCostStartingWith(string name, VRCExpressionParameters expressionParameters)
        {
            return expressionParameters.parameters.Where(val => val.name.StartsWith(name) && val.networkSynced).Sum(val => val.valueType == VRCExpressionParameters.ValueType.Bool ? 1 : 8);
        }

        public static bool IsCostViable(int expected, int max, int usedTotal, int used) => expected <= max - usedTotal + used;

        public static void RemoveGameObjects(string name, GameObject root)
        {
            foreach (Transform child in root.transform)
            {
                if (child.name.StartsWith(name))
                {
                    GameObject.DestroyImmediate(child.gameObject);
                }
            }
        }

        public static void CreatePathRecursive(string path)
        {
            if (System.IO.Directory.Exists(path))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(parent))
            {
                CreatePathRecursive(parent);
            }

            System.IO.Directory.CreateDirectory(path);
            AssetDatabase.ImportAsset(path);
        }

        public static void RemoveAssets(string path)
        {
            if (System.IO.Directory.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.Refresh();
            }
        }

        public static float GetAAPMultiplicator(float accuracy, int offset, int bits = 1)
        {
            double multiplicator = 0;
            for (int i = 0; i < bits; i++)
            {
                multiplicator += Math.Pow(2, offset + i) / (Math.Pow(2, accuracy) - 1);
            }

            return (float)multiplicator;
        }

        public static void AddSubAssetsToDatabase(AnimatorControllerLayer animatorControllerLayer, AnimatorController controller)
        {
            AssetDatabase.RemoveObjectFromAsset(animatorControllerLayer.stateMachine);
            AssetDatabase.AddObjectToAsset(animatorControllerLayer.stateMachine, controller);
            animatorControllerLayer.stateMachine.hideFlags = HideFlags.HideInHierarchy;

            foreach (var childAnimatorState in animatorControllerLayer.stateMachine.states)
            {
                AssetDatabase.RemoveObjectFromAsset(childAnimatorState.state);
                AssetDatabase.AddObjectToAsset(childAnimatorState.state, controller);
                childAnimatorState.state.hideFlags = HideFlags.HideInHierarchy;
                foreach (var animatorStateTransition in childAnimatorState.state.transitions)
                {
                    AssetDatabase.RemoveObjectFromAsset(animatorStateTransition);
                    AssetDatabase.AddObjectToAsset(animatorStateTransition, controller);
                    animatorStateTransition.hideFlags = HideFlags.HideInHierarchy;
                }

                foreach (var stateMachineBehaviour in childAnimatorState.state.behaviours)
                {
                    AssetDatabase.RemoveObjectFromAsset(stateMachineBehaviour);
                    AssetDatabase.AddObjectToAsset(stateMachineBehaviour, controller);
                    stateMachineBehaviour.hideFlags = HideFlags.HideInHierarchy;
                }

                if (childAnimatorState.state.motion is BlendTree tree)
                {
                    Queue<BlendTree> trees = new Queue<BlendTree>();
                    trees.Enqueue(tree);
                    while (trees.Count > 0)
                    {
                        tree = trees.Dequeue();
                        AssetDatabase.RemoveObjectFromAsset(tree);
                        AssetDatabase.AddObjectToAsset(tree, controller);
                        tree.hideFlags = HideFlags.HideInHierarchy;

                        foreach (var childMotion in tree.children)
                        {
                            if (childMotion.motion is BlendTree childTree)
                            {
                                trees.Enqueue(childTree);
                            }
                        }
                    }
                }
            }
        }
    }
}
#endif