#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Object = UnityEngine.Object;

namespace Hackebein.ObjectTracking
{
    [Serializable]
    public class SetupTracker
    {
        public string name = "XXX-XXXXXXXX";
        public Utility.TrackerType trackerType = Utility.TrackerType.None;
        public int bitsRPX = 10;
        public int bitsRPY = 9;
        public int bitsRPZ = 10;
        public int bitsRRX = 6;
        public int bitsRRY = 6;
        public int bitsRRZ = 6;
        public int minLPX = -42;
        public int minRPX = -5;
        public int minLPY = -42;
        public int minRPY = 0;
        public int minLPZ = -42;
        public int minRPZ = -5;
        public int minLRX = -180;
        public int minRRX = -180;
        public int minLRY = -180;
        public int minRRY = -180;
        public int minLRZ = -180;
        public int minRRZ = -180;
        public int maxLPX = 42;
        public int maxRPX = 5;
        public int maxLPY = 42;
        public int maxRPY = 3;
        public int maxLPZ = 42;
        public int maxRPZ = 5;
        public int maxLRX = 180;
        public int maxRRX = 180;
        public int maxLRY = 180;
        public int maxRRY = 180;
        public int maxLRZ = 180;
        public int maxRRZ = 180;

        public GameObject AppendObjects(GameObject parent)
        {
            // Object for position (x, y, z)
            GameObject x = Utility.FindOrCreateEmptyGameObject(name, parent);
            GameObject y = Utility.FindOrCreateEmptyGameObject(name, x);
            GameObject z = Utility.FindOrCreateEmptyGameObject(name, y);

            // Object with constraint for rotation (x, y, z)
            GameObject r = Utility.FindOrCreateEmptyGameObject(name, z);

            // Constraint
            ParentConstraint constraint = r.AddComponent<ParentConstraint>();
            constraint.constraintActive = true;
            constraint.enabled = true;
            constraint.locked = true;

            ConstraintSource source1 = new ConstraintSource
            {
                weight = 1,
                sourceTransform = z.transform
            };
            constraint.AddSource(source1);

            ConstraintSource source2 = new ConstraintSource
            {
                // TODO: add linear interpolation
                weight = 3,
                sourceTransform = r.transform
            };
            constraint.AddSource(source2);

            // Tracker
            String path = Utility.TrackerTypeGameObjects[trackerType.GetHashCode()];
            if (path != null)
            {
                Utility.RemoveGameObjects(Utility.TrackerTypeText[trackerType.GetHashCode()], r);
                GameObject gameObject = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path), new Vector3(0, 0, 0), Quaternion.identity, r.transform);
                gameObject.name = Utility.TrackerTypeText[trackerType.GetHashCode()];
                gameObject.tag = "EditorOnly";
            }
            else if (trackerType != Utility.TrackerType.None)
            {
                Debug.LogWarning(Utility.TrackerTypeText[trackerType.GetHashCode()] + " has no Model (yet)!");
            }


            return r;
        }

        private Dictionary<string[], int[]> Axes()
        {
            Dictionary<string[], int[]> axes = new Dictionary<string[], int[]>
            {
                { AxePathAndProperty("PX"), new[] { bitsRPX, minLPX, maxLPX, minRPX, maxRPX } },
                { AxePathAndProperty("PY"), new[] { bitsRPY, minLPY, maxLPY, minRPY, maxRPY } },
                { AxePathAndProperty("PZ"), new[] { bitsRPZ, minLPZ, maxLPZ, minRPZ, maxRPZ } },
                { AxePathAndProperty("RX"), new[] { bitsRRX, minLRX, maxLRX, minRRX, maxRRX } },
                { AxePathAndProperty("RY"), new[] { bitsRRY, minLRY, maxLRY, minRRY, maxRRY } },
                { AxePathAndProperty("RZ"), new[] { bitsRRZ, minLRZ, maxLRZ, minRRZ, maxRRZ } }
            };

            return axes;
        }

        private string[] AxePathAndProperty(string name)
        {
            string path = "";
            string property = "";
            switch (name)
            {
                case "PX":
                    path = "ObjectTracking/" + this.name;
                    property = "m_LocalPosition.x";
                    break;
                case "PY":
                    path = "ObjectTracking/" + this.name + "/" + this.name;
                    property = "m_LocalPosition.y";
                    break;
                case "PZ":
                    path = "ObjectTracking/" + this.name + "/" + this.name + "/" + this.name;
                    property = "m_LocalPosition.z";
                    break;
                case "RX":
                    path = "ObjectTracking/" + this.name + "/" + this.name + "/" + this.name + "/" + this.name;
                    property = "m_TranslationOffsets.Array.data[0].x";
                    break;
                case "RY":
                    path = "ObjectTracking/" + this.name + "/" + this.name + "/" + this.name + "/" + this.name;
                    property = "m_TranslationOffsets.Array.data[0].y";
                    break;
                case "RZ":
                    path = "ObjectTracking/" + this.name + "/" + this.name + "/" + this.name + "/" + this.name;
                    property = "m_TranslationOffsets.Array.data[0].z";
                    break;
            }

            return new[] { name, path, property };
        }

        public void AppendTransitionLayers(AnimatorController controller, String assetFolder)
        {
            foreach (KeyValuePair<string[], int[]> pair in Axes())
            {
                AppendTransitionLayer(controller, assetFolder, pair);
            }
        }

        private void AppendTransitionLayer(AnimatorController controller, String assetFolder, KeyValuePair<string[], int[]> axe)
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
            // Layer Design:
            //   x:   30, y: 190
            //   x: +240, y: +80

            // Animation State Local
            AnimatorState stateLocal = new AnimatorState
            {
                name = "Local",
                writeDefaultValues = false,
                timeParameterActive = true,
                timeParameter = "ObjectTracking/" + name + "/" + axe.Key[0] + "-Raw"
            };

            ChildAnimatorState stateLocalChild = new ChildAnimatorState
            {
                state = stateLocal,
                position = new Vector3(30, 190, 0)
            };

            // Animation State Remote
            AnimatorState stateRemote = new AnimatorState
            {
                name = "Remote",
                writeDefaultValues = false,
                timeParameterActive = true,
                timeParameter = "ObjectTracking/" + name + "/" + axe.Key[0]
            };

            ChildAnimatorState stateRemoteChild = new ChildAnimatorState
            {
                state = stateRemote,
                position = new Vector3(270, 190, 0)
            };

            // Layer
            AnimatorControllerLayer layer = Utility.CreateLayer("ObjectTracking/" + name + "/" + axe.Key[0]);
            layer.stateMachine.states = new[] { stateLocalChild, stateRemoteChild };
            controller.layers = controller.layers.Append(layer).ToArray();

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
                { "ObjectTracking/IsRemotePreview", true }
            };

            layer.stateMachine.entryTransitions = new[]
            {
                Utility.CreateEntryBoolTransition("isRemote", conditionsToRemote, stateRemote),
                Utility.CreateEntryBoolTransition("isRemotePreview", conditionsToRemotePreview, stateRemote),
            };
            stateLocal.transitions = new[]
            {
                Utility.CreateBoolTransition("isRemote", conditionsToRemote, stateRemote),
                Utility.CreateBoolTransition("isRemotePreview", conditionsToRemotePreview, stateRemote),
            };
            stateRemote.transitions = new[]
            {
                Utility.CreateBoolTransition("isLocal", conditionsToLocal, stateLocal),
            };

            // Clip
            stateLocal.motion =
                Utility.CreateClip(name + "/" + axe.Key[0] + "_local", axe.Key[1], axe.Key[2], axe.Value[1], axe.Value[2], assetFolder);
            stateRemote.motion =
                Utility.CreateClip(name + "/" + axe.Key[0] + "_remote", axe.Key[1], axe.Key[2], axe.Value[3], axe.Value[4], assetFolder);

            Utility.AddSubAssetsToDatabase(layer, controller);
        }

        public void AppendAvatarParameterDriverParameters(List<VRCAvatarParameterDriver.Parameter> parameterDriverParameters)
        {
            foreach (KeyValuePair<string[], int[]> pair in Axes())
            {
                int accuracy = pair.Value[0];
                int accuracyBytes = accuracy / 8;
                int accuracyBits = accuracy - (accuracyBytes * 8);
                for (int i = 0; i < accuracyBits; i++)
                {
                    parameterDriverParameters.Add(Utility.ParameterDriverParameterBoolToFloat(
                        "ObjectTracking/" + name + "/" + pair.Key[0] + "-Bit" + i,
                        "ObjectTracking/" + name + "/" + pair.Key[0] + "-Bit" + i + "-Float"));
                }

                for (int i = 0; i < accuracyBytes; i++)
                {
                    parameterDriverParameters.Add(Utility.ParameterDriverParameterIntToFloat(
                        "ObjectTracking/" + name + "/" + pair.Key[0] + "-Byte" + i,
                        "ObjectTracking/" + name + "/" + pair.Key[0] + "-Byte" + i + "-Float"));
                }
            }
        }

        public void AppendAAPMotionList(String assetFolder, Dictionary<string, AnimationClip> motions)
        {
            foreach (KeyValuePair<string[], int[]> axe in Axes())
            {
                int accuracy = axe.Value[0];
                int accuracyBytes = accuracy / 8;
                int accuracyBits = accuracy - (accuracyBytes * 8);
                int offset = 0;
                for (int i = 0; i < accuracyBytes; i++)
                {
                    float multiplicator = Utility.GetAAPMultiplicator(accuracy, offset, 8);
                    motions.Add("ObjectTracking/" + name + "/" + axe.Key[0] + "-Byte" + i + "-Float",
                        Utility.CreateClip(name + "/" + axe.Key[0] + "-Byte" + i, "", "ObjectTracking/" + name + "/" + axe.Key[0], multiplicator, multiplicator, assetFolder));
                    offset += 8;
                }

                for (int i = 0; i < accuracyBits; i++)
                {
                    float multiplicator = Utility.GetAAPMultiplicator(accuracy, offset);
                    motions.Add("ObjectTracking/" + name + "/" + axe.Key[0] + "-Bit" + i + "-Float",
                        Utility.CreateClip(name + "/" + axe.Key[0] + "-Bit" + i, "", "ObjectTracking/" + name + "/" + axe.Key[0], multiplicator, multiplicator, assetFolder));
                    offset += 1;
                }
            }
        }

        public void AppendResetList(Dictionary<string[], float[]> reset)
        {
            foreach (KeyValuePair<string[], int[]> axe in Axes())
            {
                reset.Add(new[] { axe.Key[1], "IsActive" }, new[] { 1f, 1f });
                reset.Add(new[] { axe.Key[1], "m_LocalPosition.x" }, new[] { 0f, 0f });
                reset.Add(new[] { axe.Key[1], "m_LocalPosition.y" }, new[] { 0f, 0f });
                reset.Add(new[] { axe.Key[1], "m_LocalPosition.z" }, new[] { 0f, 0f });
                reset.Add(new[] { axe.Key[1], "m_LocalRotation.x" }, new[] { 0f, 0f });
                reset.Add(new[] { axe.Key[1], "m_LocalRotation.y" }, new[] { 0f, 0f });
                reset.Add(new[] { axe.Key[1], "m_LocalRotation.z" }, new[] { 0f, 0f });
                reset.Add(new[] { axe.Key[1], "m_LocalRotation.w" }, new[] { 1f, 1f });
                reset.Add(new[] { axe.Key[1], "m_LocalScale.x" }, new[] { 1f, 1f });
                reset.Add(new[] { axe.Key[1], "m_LocalScale.y" }, new[] { 1f, 1f });
                reset.Add(new[] { axe.Key[1], "m_LocalScale.z" }, new[] { 1f, 1f });
            }
        }

        public void AppendAnimatorControllerParameters(AnimatorController controller)
        {
            foreach (KeyValuePair<string[], int[]> pair in Axes())
            {
                int accuracy = pair.Value[0];
                int accuracyBytes = accuracy / 8;
                int accuracyBits = accuracy - (accuracyBytes * 8);
                controller = Utility.CreateFloatParameterAndAddToAnimator(controller, "ObjectTracking/" + name + "/" + pair.Key[0]);
                controller = Utility.CreateFloatParameterAndAddToAnimator(controller, "ObjectTracking/" + name + "/" + pair.Key[0] + "-Raw");
                for (int i = 0; i < accuracyBits; i++)
                {
                    controller = Utility.CreateBoolParameterAndAddToAnimator(controller, "ObjectTracking/" + name + "/" + pair.Key[0] + "-Bit" + i);
                    controller = Utility.CreateFloatParameterAndAddToAnimator(controller, "ObjectTracking/" + name + "/" + pair.Key[0] + "-Bit" + i + "-Float");
                }

                for (int i = 0; i < accuracyBytes; i++)
                {
                    controller = Utility.CreateIntParameterAndAddToAnimator(controller, "ObjectTracking/" + name + "/" + pair.Key[0] + "-Byte" + i);
                    controller = Utility.CreateFloatParameterAndAddToAnimator(controller, "ObjectTracking/" + name + "/" + pair.Key[0] + "-Byte" + i + "-Float");
                }
            }
        }

        public void AppendExpressionParameters(VRCExpressionParameters expressionParameters)
        {
            foreach (KeyValuePair<string[], int[]> pair in Axes())
            {
                int accuracy = pair.Value[0];
                int accuracyBytes = accuracy / 8;
                int accuracyBits = accuracy - (accuracyBytes * 8);
                expressionParameters = Utility.CreateFloatParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/" + name + "/" + pair.Key[0] + "-Raw", 0.0f, false, false);
                for (int i = 0; i < accuracyBits; i++)
                {
                    expressionParameters = Utility.CreateBoolParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/" + name + "/" + pair.Key[0] + "-Bit" + i, false, false, true);
                }

                for (int i = 0; i < accuracyBytes; i++)
                {
                    expressionParameters = Utility.CreateIntParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/" + name + "/" + pair.Key[0] + "-Byte" + i, 0, false, true);
                }
            }
        }

        public int GetExpressionParameters()
        {
            int costs = 0;
            foreach (KeyValuePair<string[], int[]> pair in Axes())
            {
                costs += pair.Value[0] / 8;
                costs += pair.Value[0] % 8;
            }

            return costs;
        }

        public int GetExpressionParameterCosts()
        {
            int costs = 0;
            foreach (KeyValuePair<string[], int[]> pair in Axes())
            {
                costs += pair.Value[0];
            }

            return costs;
        }
    }
}
#endif