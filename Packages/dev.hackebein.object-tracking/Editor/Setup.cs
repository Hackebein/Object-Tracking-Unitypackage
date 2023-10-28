#if VRC_SDK_VRCSDK3
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Hackebein.ObjectTracking
{
    public class ObjectTrackingSetup : MonoBehaviour
    {
        [Flags]
        public enum Modes
        {
            Simple = 0,
            Advanced = 1,
            Expert = 2,
        }

        public Modes mode = Modes.Advanced;
        public string[] modesText = { "Simple", "Advanced", "Expert" };
        public string trackerSerialNumber = "XXX-XXXXXXXX";
        public string assetFolder = "Hackebein/ObjectTracking/Generated";
        public GameObject rootObjectOfAvatar;
        public AnimatorController fxController;
        public VRCExpressionParameters expressionParameters;
        public float eyeHeight = 1.50f;
        public int bitsPX = 10;
        public int bitsPY = 9;
        public int bitsPZ = 10;
        public int bitsRX = 6;
        public int bitsRY = 6;
        public int bitsRZ = 6;
        public int minPX = -5;
        public int minPY = 0;
        public int minPZ = -5;
        public int minRX = 0;
        public int minRY = 0;
        public int minRZ = 0;
        public int maxPX = 5;
        public int maxPY = 3;
        public int maxPZ = 5;
        public int maxRX = 360;
        public int maxRY = 360;
        public int maxRZ = 360;

        private AnimatorControllerParameter CreateBoolAnimatorParameter(string name, bool value = false)
        {
            AnimatorControllerParameter parameter = new AnimatorControllerParameter();
            parameter.type = AnimatorControllerParameterType.Bool;
            parameter.name = name;
            parameter.defaultBool = value;

            return parameter;
        }

        private AnimatorController CreateBoolParameterAndAddToAnimator(AnimatorController controller, string name, bool value = false)
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

        private AnimatorControllerParameter CreateFloatAnimatorParameter(string name, float value = 0.0f)
        {
            AnimatorControllerParameter parameter = new AnimatorControllerParameter();
            parameter.type = AnimatorControllerParameterType.Float;
            parameter.name = name;
            parameter.defaultFloat = value;

            return parameter;
        }

        private AnimatorController CreateFloatParameterAndAddToAnimator(AnimatorController controller, string name, float value = 0.0f)
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

        private AnimatorControllerParameter CreateIntAnimatorParameter(string name, int value = 0)
        {
            AnimatorControllerParameter parameter = new AnimatorControllerParameter();
            parameter.type = AnimatorControllerParameterType.Int;
            parameter.name = name;
            parameter.defaultInt = value;

            return parameter;
        }

        private AnimatorController CreateIntParameterAndAddToAnimator(AnimatorController controller, string name, int value = 0)
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

        private VRCExpressionParameters.Parameter CreateIntExpressionParameter(string name, int value = 0, bool saved = true, bool synced = true)
        {
            VRCExpressionParameters.Parameter parameter = new VRCExpressionParameters.Parameter();
            parameter.name = name;
            parameter.valueType = VRCExpressionParameters.ValueType.Int;
            parameter.defaultValue = value;
            parameter.saved = saved;
            parameter.networkSynced = synced;

            return parameter;
        }

        private VRCExpressionParameters CreateIntParameterAndAddToExpressionParameters(VRCExpressionParameters expression, string name, int value = 0, bool saved = true, bool synced = true)
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

        private VRCExpressionParameters.Parameter CreateFloatExpressionParameter(string name, float value = 0.0f, bool saved = true, bool synced = true)
        {
            VRCExpressionParameters.Parameter parameter = new VRCExpressionParameters.Parameter();
            parameter.name = name;
            parameter.valueType = VRCExpressionParameters.ValueType.Float;
            parameter.defaultValue = value;
            parameter.saved = saved;
            parameter.networkSynced = synced;

            return parameter;
        }

        private VRCExpressionParameters CreateFloatParameterAndAddToExpressionParameters(VRCExpressionParameters expression, string name, float value = 0.0f, bool saved = true, bool synced = true)
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

        private VRCExpressionParameters.Parameter CreateBoolExpressionParameter(string name, bool value = false, bool saved = true, bool synced = true)
        {
            VRCExpressionParameters.Parameter parameter = new VRCExpressionParameters.Parameter();
            parameter.name = name;
            parameter.valueType = VRCExpressionParameters.ValueType.Bool;
            parameter.defaultValue = value ? 1 : 0;
            parameter.saved = saved;
            parameter.networkSynced = synced;

            return parameter;
        }

        private VRCExpressionParameters CreateBoolParameterAndAddToExpressionParameters(VRCExpressionParameters expression, string name, bool value = false, bool saved = true, bool synced = true)
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

        private AnimatorControllerLayer CreateLayer(string name)
        {
            AnimatorControllerLayer layer = new AnimatorControllerLayer();
            layer.name = name;
            layer.defaultWeight = 1;
            AnimatorStateMachine stateMachine = new AnimatorStateMachine();
            layer.stateMachine = stateMachine;

            return layer;
        }

        private VRCAvatarParameterDriver.Parameter ParameterDriverParameterIntToFloat(string from, string to)
        {
            VRCAvatarParameterDriver.Parameter parameterDriverParameter = new VRCAvatarParameterDriver.Parameter();
            parameterDriverParameter.type = VRCAvatarParameterDriver.ChangeType.Copy;
            parameterDriverParameter.source = from;
            parameterDriverParameter.name = to; //destination
            parameterDriverParameter.convertRange = true;
            parameterDriverParameter.sourceMin = 0;
            parameterDriverParameter.sourceMax = 255;
            parameterDriverParameter.destMin = 0;
            parameterDriverParameter.destMax = 1;

            return parameterDriverParameter;
        }

        private VRCAvatarParameterDriver.Parameter ParameterDriverParameterBoolToFloat(string from, string to)
        {
            VRCAvatarParameterDriver.Parameter parameterDriverParameter = new VRCAvatarParameterDriver.Parameter();
            parameterDriverParameter.type = VRCAvatarParameterDriver.ChangeType.Copy;
            parameterDriverParameter.source = from;
            parameterDriverParameter.name = to; //destination
            parameterDriverParameter.convertRange = true;
            parameterDriverParameter.sourceMin = 0;
            parameterDriverParameter.sourceMax = 1;
            parameterDriverParameter.destMin = 0;
            parameterDriverParameter.destMax = 1;

            return parameterDriverParameter;
        }

        private AnimatorStateTransition CreateBoolTransition(string name, string variable, bool value, AnimatorState target)
        {
            AnimatorCondition condition0 = new AnimatorCondition();
            condition0.parameter = variable;
            condition0.mode = value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot;

            AnimatorCondition[] conditions = new AnimatorCondition[1]; // 1 = one condition
            conditions[0] = condition0; // first condition

            AnimatorStateTransition transition = new AnimatorStateTransition();
            transition.name = name;
            transition.conditions = conditions;
            transition.destinationState = target;
            transition.duration = 0;
            transition.hasExitTime = false;
            transition.exitTime = 0;

            return transition;
        }

        private AnimatorTransition CreateEntryBoolTransition(string name, string variable, bool value, AnimatorState target)
        {
            AnimatorCondition condition0 = new AnimatorCondition();
            condition0.parameter = variable;
            condition0.mode = value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot;

            AnimatorCondition[] conditions = new AnimatorCondition[1]; // 1 = one condition
            conditions[0] = condition0; // first condition

            AnimatorTransition transition = new AnimatorTransition();
            transition.name = name;
            transition.conditions = conditions;
            transition.destinationState = target;

            return transition;
        }

        private AnimationClip CreateClip(string name, string path, string property, float start, float end)
        {
            AnimationClip clip = new AnimationClip();
            clip.name = name;
            EditorCurveBinding binding = EditorCurveBinding.FloatCurve(path, typeof(GameObject), property);
            AnimationCurve curve = AnimationCurve.Linear(0, start, 1 / 60f, end);
            AnimationUtility.SetEditorCurve(clip, binding, curve);
            AssetDatabase.CreateAsset(clip, "Assets/" + assetFolder + "/" + name + ".anim");

            return clip;
        }

        private BlendTree CreateDirectBlendTree(string name, Dictionary<string, AnimationClip> motions)
        {
            BlendTree blendTree = new BlendTree();
            blendTree.name = name;
            blendTree.blendType = BlendTreeType.Direct;

            foreach (KeyValuePair<string, AnimationClip> pair in motions)
            {
                ChildMotion motion = new ChildMotion();
                motion.motion = pair.Value;
                motion.directBlendParameter = pair.Key;
                motion.timeScale = 1;
                blendTree.children = blendTree.children.Append(motion).ToArray();
            }

            return blendTree;
        }

        private GameObject CreateEmptyGameObject(string name, GameObject parent)
        {
            GameObject gameObject = new GameObject();
            gameObject.name = name;
            gameObject.transform.parent = parent.transform;

            return gameObject;
        }

        public void ValidateAndRemoveAll()
        {
            Remove("", rootObjectOfAvatar, fxController, expressionParameters);
        }

        public void ValidateAndRemove()
        {
            Remove(trackerSerialNumber, rootObjectOfAvatar, fxController, expressionParameters);
        }

        private void Remove(string name, GameObject root, AnimatorController controller, VRCExpressionParameters expressionParameters)
        {
            RemoveLayer("ObjectTracking/" + name, controller);
            RemoveAnimatorParameters("ObjectTracking/" + name, controller);
            RemoveExpressionParameters("ObjectTracking/" + name, expressionParameters);
            RemoveGameObjects(name, root);
            RemoveAssets("Assets/" + assetFolder + "/" + name);
        }

        private void RemoveLayer(string name, AnimatorController controller)
        {
            controller.layers = controller.layers.Where(val => !val.name.StartsWith(name)).ToArray();
        }

        private void RemoveAnimatorParameters(string name, AnimatorController controller)
        {
            controller.parameters = controller.parameters.Where(val => !val.name.StartsWith(name)).ToArray();
        }

        private void RemoveExpressionParameters(string name, VRCExpressionParameters expressionParameters)
        {
            expressionParameters.parameters =
                expressionParameters.parameters.Where(val => !val.name.StartsWith(name)).ToArray();
        }

        private void RemoveGameObjects(string name, GameObject root)
        {
            foreach (Transform child in root.transform)
            {
                if (child.name.StartsWith(name))
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        private void RemoveAssets(string path)
        {
            if (System.IO.Directory.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.Refresh();
            }
        }

        public void ValidateAndCreate()
        {
            Dictionary<string, int[]> axes = new Dictionary<string, int[]>();
            axes.Add("PX", new int[] { bitsPX, minPX, maxPX });
            axes.Add("PY", new int[] { bitsPY, minPY, maxPY });
            axes.Add("PZ", new int[] { bitsPZ, minPZ, maxPZ });
            axes.Add("RX", new int[] { bitsRX, minRX, maxRX });
            axes.Add("RY", new int[] { bitsRY, minRY, maxRY });
            axes.Add("RZ", new int[] { bitsRZ, minRZ, maxRZ });

            VRCAvatarDescriptor avatarDescriptor = (VRCAvatarDescriptor)rootObjectOfAvatar.GetComponent(typeof(VRCAvatarDescriptor));
            if (avatarDescriptor == null)
            {
                Debug.LogError("Avatar Descriptor not found");
                return;
            }

            float scale = avatarDescriptor.ViewPosition.y / eyeHeight;

            Remove(trackerSerialNumber, rootObjectOfAvatar, fxController, expressionParameters);
            Create(trackerSerialNumber, rootObjectOfAvatar, fxController, expressionParameters, axes, scale);
        }

        private void Create(string name, GameObject root, AnimatorController controller, VRCExpressionParameters expressionParameters, Dictionary<string, int[]> axes, float scale)
        {
            // Layer Design:
            //   x:   30, y: 190
            //   x: +240, y: +80

            if (!System.IO.Directory.Exists("Assets/" + assetFolder + "/" + name))
            {
                System.IO.Directory.CreateDirectory("Assets/" + assetFolder + "/" + name);
                AssetDatabase.ImportAsset("Assets/" + assetFolder + "/" + name);
            }

            // Parameters
            controller = CreateBoolParameterAndAddToAnimator(controller, "IsLocal");
            foreach (KeyValuePair<string, int[]> pair in axes)
            {
                int accuracy = pair.Value[0];
                int accuracyBytes = accuracy / 8;
                int accuracyBits = accuracy - (accuracyBytes * 8);
                controller =
                    CreateFloatParameterAndAddToAnimator(controller, "ObjectTracking/" + name + "/" + pair.Key);
                controller =
                    CreateFloatParameterAndAddToAnimator(controller,
                        "ObjectTracking/" + name + "/" + pair.Key + "-Raw");
                expressionParameters = CreateFloatParameterAndAddToExpressionParameters(expressionParameters,
                    "ObjectTracking/" + name + "/" + pair.Key + "-Raw", 0.0f, false, false);
                for (int i = 0; i < accuracyBits; i++)
                {
                    controller = CreateBoolParameterAndAddToAnimator(controller,
                        "ObjectTracking/" + name + "/" + pair.Key + "-Bit" + i);
                    expressionParameters = CreateBoolParameterAndAddToExpressionParameters(expressionParameters,
                        "ObjectTracking/" + name + "/" + pair.Key + "-Bit" + i, false, false, true);
                    controller = CreateFloatParameterAndAddToAnimator(controller,
                        "ObjectTracking/" + name + "/" + pair.Key + "-Bit" + i + "-Float");
                }

                for (int i = 0; i < accuracyBytes; i++)
                {
                    controller = CreateIntParameterAndAddToAnimator(controller,
                        "ObjectTracking/" + name + "/" + pair.Key + "-Byte" + i);
                    expressionParameters = CreateIntParameterAndAddToExpressionParameters(expressionParameters,
                        "ObjectTracking/" + name + "/" + pair.Key + "-Byte" + i, 0, false, true);
                    controller = CreateFloatParameterAndAddToAnimator(controller,
                        "ObjectTracking/" + name + "/" + pair.Key + "-Byte" + i + "-Float");
                }
            }

            // Objects
            GameObject x = CreateEmptyGameObject(name, root);
            GameObject y = CreateEmptyGameObject(name, x);
            GameObject z = CreateEmptyGameObject(name, y);
            GameObject w = CreateEmptyGameObject(name, z);
            x.transform.localScale = new Vector3(scale, scale, scale);

            // Constraints
            ParentConstraint constraint = w.AddComponent<ParentConstraint>();
            constraint.constraintActive = true;
            constraint.enabled = true;
            constraint.locked = true;

            ConstraintSource source1 = new ConstraintSource();
            source1.weight = 1;
            source1.sourceTransform = z.transform;
            constraint.AddSource(source1);

            ConstraintSource source2 = new ConstraintSource();
            source2.weight = 3;
            source2.sourceTransform = w.transform;
            constraint.AddSource(source2);

            // Animation Controller
            CreateProcessingLayer(name, controller, axes);
            foreach (KeyValuePair<string, int[]> pair in axes)
            {
                CreateTransitionLayer(name, controller, pair);
            }
        }

        private void CreateTransitionLayer(string name, AnimatorController controller, KeyValuePair<string, int[]> axe)
        {
            //        [ Entry ]
            //         ||    \\
            //         v      v
            // [ Local ] <=> [ Remote ]
            //
            // Transitions:
            //   Entry => Local: (default)
            //   Entry => Remote: IsLocal = False
            //   Local => Remote: IsLocal = False
            //   Remote => Local: IsLocal = True
            //
            // Animations:
            //   2 Frame (min/max) Animation
            //   Motion Time:
            //     Local: using raw value
            //     Remote: AAP result from Processing Layer
            //

            // Animation State Local
            AnimatorState stateLocal = new AnimatorState();
            stateLocal.name = "Local";
            stateLocal.writeDefaultValues = false;
            stateLocal.timeParameterActive = true;
            stateLocal.timeParameter = "ObjectTracking/" + name + "/" + axe.Key + "-Raw";

            ChildAnimatorState stateLocalChild = new ChildAnimatorState();
            stateLocalChild.state = stateLocal;
            stateLocalChild.position = new Vector3(30, 190, 0);

            // Animation State Remote
            AnimatorState stateRemote = new AnimatorState();
            stateRemote.name = "Remote";
            stateRemote.writeDefaultValues = false;
            stateRemote.timeParameterActive = true;
            stateRemote.timeParameter = "ObjectTracking/" + name + "/" + axe.Key;

            ChildAnimatorState stateRemoteChild = new ChildAnimatorState();
            stateRemoteChild.state = stateRemote;
            stateRemoteChild.position = new Vector3(270, 190, 0);

            // Layer
            AnimatorControllerLayer layer = CreateLayer("ObjectTracking/" + name + "/" + axe.Key);
            layer.stateMachine.states = new[] { stateLocalChild, stateRemoteChild };

            AnimatorControllerLayer[] layers = controller.layers;
            layers = layers.Append(layer).ToArray();
            controller.layers = layers;

            // Transition Conditions
            layer.stateMachine.entryTransitions = new[]
                { CreateEntryBoolTransition("isRemote", "IsLocal", false, stateRemote) };
            stateLocal.transitions = new[] { CreateBoolTransition("isRemote", "IsLocal", false, stateRemote) };
            stateRemote.transitions = new[] { CreateBoolTransition("isLocal", "IsLocal", true, stateLocal) };

            // Clip
            string path = "";
            string property = "";
            switch (axe.Key)
            {
                case "PX":
                    path = name + "/" + name;
                    property = "m_LocalPosition.x";
                    break;
                case "PY":
                    path = name + "/" + name + "/" + name;
                    property = "m_LocalPosition.y";
                    break;
                case "PZ":
                    path = name + "/" + name + "/" + name + "/" + name;
                    property = "m_LocalPosition.z";
                    break;
                case "RX":
                    path = name + "/" + name + "/" + name + "/" + name + "/" + name;
                    property = "m_TranslationOffsets.Array.data[0].x";
                    break;
                case "RY":
                    path = name + "/" + name + "/" + name + "/" + name + "/" + name;
                    property = "m_TranslationOffsets.Array.data[0].y";
                    break;
                case "RZ":
                    path = name + "/" + name + "/" + name + "/" + name + "/" + name;
                    property = "m_TranslationOffsets.Array.data[0].z";
                    break;
            }

            stateLocal.motion = stateRemote.motion =
                CreateClip(name + "/" + axe.Key, path, property, axe.Value[1], axe.Value[2]);

            AddSubAssetsToDatabase(layer, controller);
        }

        private void CreateProcessingLayer(string name, AnimatorController controller, Dictionary<string, int[]> axes)
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

            // Animation State Local
            AnimatorState stateLocal = new AnimatorState();
            stateLocal.name = "Local";
            stateLocal.writeDefaultValues = true;

            ChildAnimatorState stateLocalChild = new ChildAnimatorState();
            stateLocalChild.state = stateLocal;
            stateLocalChild.position = new Vector3(30, 190, 0);

            // Animation State Remote
            List<VRCAvatarParameterDriver.Parameter> parameterDriverParameters =
                new List<VRCAvatarParameterDriver.Parameter>();
            foreach (KeyValuePair<string, int[]> pair in axes)
            {
                int accuracy = pair.Value[0];
                int accuracyBytes = accuracy / 8;
                int accuracyBits = accuracy - (accuracyBytes * 8);
                for (int i = 0; i < accuracyBits; i++)
                {
                    parameterDriverParameters.Add(ParameterDriverParameterBoolToFloat(
                        "ObjectTracking/" + name + "/" + pair.Key + "-Bit" + i,
                        "ObjectTracking/" + name + "/" + pair.Key + "-Bit" + i + "-Float"));
                }

                for (int i = 0; i < accuracyBytes; i++)
                {
                    parameterDriverParameters.Add(ParameterDriverParameterIntToFloat(
                        "ObjectTracking/" + name + "/" + pair.Key + "-Byte" + i,
                        "ObjectTracking/" + name + "/" + pair.Key + "-Byte" + i + "-Float"));
                }
            }

            VRCAvatarParameterDriver parameterDriver = ScriptableObject.CreateInstance<VRCAvatarParameterDriver>();
            parameterDriver.parameters = parameterDriverParameters;
            parameterDriver.localOnly = false;

            AnimatorState stateRemote = new AnimatorState();
            stateRemote.name = "Remote";
            stateRemote.writeDefaultValues = true;
            stateRemote.behaviours = new[] { parameterDriver };

            ChildAnimatorState stateRemoteChild = new ChildAnimatorState();
            stateRemoteChild.state = stateRemote;
            stateRemoteChild.position = new Vector3(270, 190, 0);

            // Layer
            AnimatorControllerLayer layer = CreateLayer("ObjectTracking/" + name + "/Processing");
            layer.stateMachine.states = new[] { stateLocalChild, stateRemoteChild };

            AnimatorControllerLayer[] layers = controller.layers;
            layers = layers.Append(layer).ToArray();
            controller.layers = layers;

            // Transition Conditions
            layer.stateMachine.entryTransitions = new[] { CreateEntryBoolTransition("isRemote", "IsLocal", false, stateRemote) };
            stateLocal.transitions = new[] { CreateBoolTransition("isRemote", "IsLocal", false, stateRemote) };
            stateRemote.transitions = new[]
            {
                CreateBoolTransition("isRemote", "IsLocal", false, stateRemote),
                CreateBoolTransition("isLocal", "IsLocal", true, stateLocal)
            };

            // Clips
            stateLocal.motion = CreateClip("ignore", "_ignore", "IsActive", 0, 0);
            Dictionary<string, AnimationClip> motions = new Dictionary<string, AnimationClip>();
            foreach (KeyValuePair<string, int[]> axe in axes)
            {
                int accuracy = axe.Value[0];
                int accuracyBytes = accuracy / 8;
                int accuracyBits = accuracy - (accuracyBytes * 8);
                int offset = 0;
                for (int i = 0; i < accuracyBytes; i++)
                {
                    float multiplicator = GetAAPMultiplicator(accuracy, offset, 8);
                    motions.Add("ObjectTracking/" + name + "/" + axe.Key + "-Byte" + i + "-Float",
                        CreateClip(name + "/" + axe.Key + "-Byte" + i, "", "ObjectTracking/" + name + "/" + axe.Key, multiplicator, multiplicator));
                    offset += 8;
                }

                for (int i = 0; i < accuracyBits; i++)
                {
                    float multiplicator = GetAAPMultiplicator(accuracy, offset);
                    motions.Add("ObjectTracking/" + name + "/" + axe.Key + "-Bit" + i + "-Float",
                        CreateClip(name + "/" + axe.Key + "-Bit" + i, "", "ObjectTracking/" + name + "/" + axe.Key,multiplicator, multiplicator));
                    offset += 1;
                }
            }

            stateRemote.motion = CreateDirectBlendTree(name + "/processing", motions);

            AddSubAssetsToDatabase(layer, controller);
        }

        private float GetAAPMultiplicator(float accuracy, int offset, int bits = 1)
        {
            double multiplicator = 0;
            for (int i = 0; i < bits; i++)
            {
                multiplicator += Math.Pow(2, offset + i) / (Math.Pow(2, accuracy) - 1);
            }

            return (float)multiplicator;
        }

        private void AddSubAssetsToDatabase(AnimatorControllerLayer animatorControllerLayer, AnimatorController controller)
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