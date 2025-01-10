#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Dynamics.Constraint.Components;
using JetBrains.Annotations;

namespace hackebein.objecttracking
{
    public static class Utility
    {
        public static GameObject DefaultChildGameObject = null;
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

        public static VRCAvatarParameterDriver.Parameter ParameterDriverParameterSet(string name, float value)
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
                String path = prop.Key[0];
                String[] parts = prop.Key[1].Split('.', 2);
                String typeString = parts[0];
                String property = parts[1];
                float start = prop.Value[0];
                float stop = prop.Value[1];
                Dictionary<string, Type> typeMapping = new Dictionary<string, Type>
                {
                    { "Animator", typeof(Animator) },
                    { "GameObject", typeof(GameObject) },
                    { "Transform", typeof(Transform) },
                    { "VRCParentConstraint", typeof(VRCParentConstraint) },
                };
                if (!typeMapping.TryGetValue(typeString, out Type type))
                {
                    Debug.LogWarning("[Hackebein's Object Tracking] Can't map property to a type, fall back to GameObject: " + typeString);
                    type = typeof(GameObject);
                }
                EditorCurveBinding binding = EditorCurveBinding.FloatCurve(path, type, property);
                AnimationCurve curve = AnimationCurve.Linear(0, start, 1 / 60f, stop);
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
                    threshold = 0f,
                    directBlendParameter = pair.Key,
                    timeScale = 1f
                };
                blendTree.children = blendTree.children.Append(motion).ToArray();
            }

            return blendTree;
        }

        public static void ResetGameObject(GameObject gameObject, List<Type> ignoredComponentTypes = null)
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
                
                if (ignoredComponentTypes != null && ignoredComponentTypes.Contains(component.GetType()))
                {
                    continue;
                }

                GameObject.DestroyImmediate(component);
            }
            if (DefaultChildGameObject != null)
            {
                GameObject.Instantiate(DefaultChildGameObject).transform.parent = gameObject.transform;
            }
        }

        public static GameObject FindOrCreateEmptyGameObject(string name, [CanBeNull] GameObject parent = null, List<Type> ignoredComponentTypes = null)
        {
            GameObject gameObject = FindOrCreateGameObject(name, parent);
            ResetGameObject(gameObject, ignoredComponentTypes);
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
                name = name,
                tag = "EditorOnly"
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
        
        public static void MarkDirty(Object obj) {
            EditorUtility.SetDirty(obj);
        
            // obsolete for Untiy 2020+
            if (obj is GameObject go) {
                MarkSceneDirty(go.scene);
            } else if (obj is UnityEngine.Component c) {
                MarkSceneDirty(((dynamic)c).gameObject.owner().scene);
            }
        }
        
        private static void MarkSceneDirty(Scene scene) {
            if (Application.isPlaying) return;
            if (scene == null) return;
            if (!scene.isLoaded) return;
            if (!scene.IsValid()) return;
            EditorSceneManager.MarkSceneDirty(scene);
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
        
        public static (float centerX, float centerY) FindCenter(List<(float x, float y)> points)
        {
            int n = points.Count;
            float sumX = 0, sumY = 0, sumX2 = 0, sumY2 = 0, sumXY = 0, sumR = 0, sumRX = 0, sumRY = 0;

            foreach (var point in points)
            {
                float x = point.x;
                float y = point.y;
                float r = x * x + y * y;
                sumX += x;
                sumY += y;
                sumX2 += x * x;
                sumY2 += y * y;
                sumXY += x * y;
                sumR += r;
                sumRX += r * x;
                sumRY += r * y;
            }

            float C = n * sumX2 + n * sumY2 - sumX * sumX - sumY * sumY;
            float Xc = (sumR * sumX - sumRX * n) / C;
            float Yc = (sumR * sumY - sumRY * n) / C;

            return (Xc, Yc);
        }
        
        public static (float X, float Y) FindCircleCenter((float X, float Y)[] points)
        {
            int n = points.Length;
            float sumX = points.Sum(p => p.X);
            float sumY = points.Sum(p => p.Y);
            float sumX2 = points.Sum(p => p.X * p.X);
            float sumY2 = points.Sum(p => p.Y * p.Y);
            float sumXY = points.Sum(p => p.X * p.Y);
            float sumR2 = points.Sum(p => p.X * p.X + p.Y * p.Y);

            float A = 2 * (sumX * sumX - n * sumX2);
            float B = 2 * (sumY * sumY - n * sumY2);
            float C = 2 * (sumX * sumY - n * sumXY);

            float D = sumX * sumR2 - n * sumX * sumX2 - sumX * sumY2;
            float E = sumY * sumR2 - n * sumY * sumY2 - sumY * sumX2;

            float centerX = (D * B - E * C) / (A * B - C * C);
            float centerY = (A * E - C * D) / (A * B - C * C);

            return (centerX, centerY);
        }
        
        public static (float X, float Y) FindLineCenter((float X, float Y)[] points)
        {
            int n = points.Length;
            float sumX = points.Sum(p => p.X);
            float sumY = points.Sum(p => p.Y);
            float sumXY = points.Sum(p => p.X * p.Y);
            float sumX2 = points.Sum(p => p.X * p.X);

            float slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            float intercept = (sumY - slope * sumX) / n;

            float centerX = sumX / n;
            float centerY = slope * centerX + intercept;

            return (centerX, centerY);
        }

        public static void AddTrackerStart(List<VRCAvatarParameterDriver.Parameter> parameterDriverParameters, string name)
        {
            parameterDriverParameters.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/" + name, true));
        }

        public static void AddTrackerEnd(List<VRCAvatarParameterDriver.Parameter> parameterDriverParameters, string name)
        {
            parameterDriverParameters.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/" + name, false));
            parameterDriverParameters.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 0));
            parameterDriverParameters.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", 0));
        }
        
        public static void AddConfigValue(List<VRCAvatarParameterDriver.Parameter> parameterDriverParameters, int index, int value)
        {
            parameterDriverParameters.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", value));
            parameterDriverParameters.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", index));
        }
        
        public static void AddConfigValue(List<VRCAvatarParameterDriver.Parameter> parameterDriverParameters, int index, float value)
        {
            parameterDriverParameters.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/value", value));
            parameterDriverParameters.Add(Utility.ParameterDriverParameterSet("ObjectTracking/config/index", index));
        }

    }
}
#endif