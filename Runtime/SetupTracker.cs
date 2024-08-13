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
        public string name;
        public Utility.TrackerType trackerType;
        public int bitsRPX = 10;
        public int bitsRPY = 9;
        public int bitsRPZ = 10;
        public int bitsRRX = 6;
        public int bitsRRY = 6;
        public int bitsRRZ = 6;
        public int minLPX = -12;
        public int minRPX = -5;
        public int minLPY = -12;
        public int minRPY = 0;
        public int minLPZ = -12;
        public int minRPZ = -5;
        public int minLRX = -180;
        public int minRRX = -180;
        public int minLRY = -180;
        public int minRRY = -180;
        public int minLRZ = -180;
        public int minRRZ = -180;
        public int maxLPX = 12;
        public int maxRPX = 5;
        public int maxLPY = 12;
        public int maxRPY = 3;
        public int maxLPZ = 12;
        public int maxRPZ = 5;
        public int maxLRX = 180;
        public int maxRRX = 180;
        public int maxLRY = 180;
        public int maxRRY = 180;
        public int maxLRZ = 180;
        public int maxRRZ = 180;
        public float defaultPX = 0;
        public float defaultPY = 0;
        public float defaultPZ = 0;
        public float defaultRX = 0;
        public float defaultRY = 0;
        public float defaultRZ = 0;
        public bool applyLastPosition = true;
        public bool hideBeyondLimits = true;
        public bool debug = false;
        
        public SetupTracker(string name, Utility.TrackerType trackerType = Utility.TrackerType.None, float defaultPX = 0, float defaultPY = 0, float defaultPZ = 0, float defaultRX = 0, float defaultRY = 0, float defaultRZ = 0)
        {
            this.name = name;
            this.trackerType = trackerType;
            this.defaultPX = defaultPX;
            this.defaultPY = defaultPY;
            this.defaultPZ = defaultPZ;
            this.defaultRX = defaultRX;
            this.defaultRY = defaultRY;
            this.defaultRZ = defaultRZ;
        }

        public GameObject AppendObjects(GameObject parent)
        {
            // Objects for position (x, y, z)
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
            constraint.translationAxis = Axis.None;

            ConstraintSource source1 = new ConstraintSource
            {
                weight = 1,
                sourceTransform = z.transform
            };
            constraint.AddSource(source1);

            ConstraintSource source2 = new ConstraintSource
            {
                // TODO: add linear interpolation
                weight = 0,
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
                gameObject.transform.localScale = new Vector3(1f, 1f, -1f); 
            }
            else if (trackerType != Utility.TrackerType.None)
            {
                Debug.LogWarning(Utility.TrackerTypeText[trackerType.GetHashCode()] + " has no Model (yet)!");
            }
            
            // Debug Gizmos
            if (debug)
            {
                // https://sketchfab.com/3d-models/transform-gizmo-8d1edffdedda4898b3fb1c3c4c08113c
                string gizmo = "Packages/dev.hackebein.object-tracking/Prefab/GizmoUnity.fbx";
                Material transparentMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Transparent.mat");
                Utility.RemoveGameObjects("Gizmo", x);
                GameObject x_debug = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(gizmo), new Vector3(0, 0, 0), Quaternion.identity, x.transform);
                x_debug.name = "Gizmo";
                x_debug.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                foreach (MeshRenderer meshRenderer in x_debug.GetComponents<MeshRenderer>())
                {
                    Material[] materials = meshRenderer.sharedMaterials;
                    materials[1] = transparentMaterial;
                    materials[2] = transparentMaterial;
                    meshRenderer.sharedMaterials = materials;
                }
                Utility.RemoveGameObjects("Gizmo", y);
                GameObject y_debug = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(gizmo), new Vector3(0, 0, 0), Quaternion.identity, y.transform);
                y_debug.name = "Gizmo";
                y_debug.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                foreach (MeshRenderer meshRenderer in y_debug.GetComponents<MeshRenderer>())
                {
                    Material[] materials = meshRenderer.sharedMaterials;
                    materials[0] = transparentMaterial;
                    materials[1] = transparentMaterial;
                    meshRenderer.sharedMaterials = materials;
                }
                Utility.RemoveGameObjects("Gizmo", z);
                GameObject z_debug = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(gizmo), new Vector3(0, 0, 0), Quaternion.identity, z.transform);
                z_debug.name = "Gizmo";
                z_debug.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                foreach (MeshRenderer meshRenderer in z_debug.GetComponents<MeshRenderer>())
                {
                    Material[] materials = meshRenderer.sharedMaterials;
                    materials[0] = transparentMaterial;
                    materials[2] = transparentMaterial;
                    meshRenderer.sharedMaterials = materials;
                }
                Utility.RemoveGameObjects("Gizmo", r);
                GameObject r_debug = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(gizmo), new Vector3(0, 0, 0), Quaternion.identity, r.transform);
                r_debug.name = "Gizmo";
                r_debug.transform.localScale = new Vector3(0.04f, 0.04f, -0.04f);
            }

            // apply default transform
            if (applyLastPosition)
            {
                x.transform.localPosition = new Vector3(defaultPX, 0, 0);
                y.transform.localPosition = new Vector3(0, defaultPY, 0);
                z.transform.localPosition = new Vector3(0, 0, defaultPZ);
                constraint.SetRotationOffset(0, Quaternion.Euler(defaultRX, defaultRY, defaultRZ).eulerAngles);
            }
            
            return r;
        }

        private Dictionary<string[], int[]> Axes()
        {
            Dictionary<string[], int[]> axes = new Dictionary<string[], int[]>
            {
                { AxePathAndProperty("PX"), new[] { bitsRPX, minLPX, maxLPX, minRPX, bitsRPX > 0 ? maxRPX : minRPX } },
                { AxePathAndProperty("PY"), new[] { bitsRPY, minLPY, maxLPY, minRPY, bitsRPY > 0 ? maxRPY : minRPY } },
                { AxePathAndProperty("PZ"), new[] { bitsRPZ, minLPZ, maxLPZ, minRPZ, bitsRPZ > 0 ? maxRPZ : minRPZ } },
                { AxePathAndProperty("RX"), new[] { bitsRRX, minLRX, maxLRX, minRRX, bitsRRX > 0 ? maxRRX : minRRX } },
                { AxePathAndProperty("RY"), new[] { bitsRRY, minLRY, maxLRY, minRRY, bitsRRY > 0 ? maxRRY : minRRY } },
                { AxePathAndProperty("RZ"), new[] { bitsRRZ, minLRZ, maxLRZ, minRRZ, bitsRRZ > 0 ? maxRRZ : minRRZ } }
            };

            return axes;
        }

        private string[] AxePathAndProperty(string name)
        {
            string path = "";
            string property = "";
            // TODO: add support for multiple properties
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
                    property = "m_RotationOffsets.Array.data[0].x";
                    // VRC Constraints: Sources.source0.ParentRotationOffset.x
                    break;
                case "RY":
                    path = "ObjectTracking/" + this.name + "/" + this.name + "/" + this.name + "/" + this.name;
                    property = "m_RotationOffsets.Array.data[0].y";
                    // VRC Constraints: Sources.source0.ParentRotationOffset.y
                    break;
                case "RZ":
                    path = "ObjectTracking/" + this.name + "/" + this.name + "/" + this.name + "/" + this.name;
                    property = "m_RotationOffsets.Array.data[0].z";
                    // VRC Constraints: Sources.source0.ParentRotationOffset.z
                    break;
            }

            return new[] { name, path, property };
        }
        
        public void AppendHideBeyondLimitsLayer(AnimatorController controller, String assetFolder)
        {
            if (!hideBeyondLimits)
            {
                return;
            }
            // Animation State Local
            AnimatorState stateShow = new AnimatorState
            {
                name = "Show",
                writeDefaultValues = false
            };

            ChildAnimatorState stateShowChild = new ChildAnimatorState
            {
                state = stateShow,
                position = new Vector3(30, 190, 0)
            };

            // Animation State Remote
            AnimatorState stateHide = new AnimatorState
            {
                name = "Hide",
                writeDefaultValues = false
            };

            ChildAnimatorState stateHideChild = new ChildAnimatorState
            {
                state = stateHide,
                position = new Vector3(270, 190, 0)
            };

            // Layer
            AnimatorControllerLayer layer = Utility.CreateLayer("ObjectTracking/HideBeyondLimits");
            layer.stateMachine.states = new[] { stateShowChild, stateHideChild };
            controller.layers = controller.layers.Append(layer).ToArray();

            // Transition Conditions
            /// Show
            Tuple<string, bool>[] conditionsToShowLocalBool = new Tuple<string, bool>[]
            {
                Tuple.Create("IsLocal", true),
                Tuple.Create("ObjectTracking/isRemotePreview", false),
            };
            List<Tuple<string, AnimatorConditionMode, float>> conditionsToShowLocalFloat = new List<Tuple<string, AnimatorConditionMode, float>> {};
            foreach (KeyValuePair<string[], int[]> pair in Axes())
            {
                conditionsToShowLocalFloat.Add(Tuple.Create("ObjectTracking/" + name + "/L" + pair.Key[0], AnimatorConditionMode.Greater, (float)(1 / Math.Pow(2, 32))));
                conditionsToShowLocalFloat.Add(Tuple.Create("ObjectTracking/" + name + "/L" + pair.Key[0], AnimatorConditionMode.Less, (float)(1 - 1 / Math.Pow(2, 32))));
            }
            
            Tuple<string, bool>[] conditionsToShowRemotePreviewBool = new Tuple<string, bool>[]
            {
                Tuple.Create("IsLocal", true),
                Tuple.Create("ObjectTracking/isRemotePreview", true),
            };
            List<Tuple<string, AnimatorConditionMode, float>> conditionsToShowRemotePreviewFloat = new List<Tuple<string, AnimatorConditionMode, float>> {};
            foreach (KeyValuePair<string[], int[]> pair in Axes())
            {
                conditionsToShowRemotePreviewFloat.Add(Tuple.Create("ObjectTracking/" + name + "/R" + pair.Key[0], AnimatorConditionMode.Greater, (float)(1 / Math.Pow(2, pair.Value[0]))));
                conditionsToShowRemotePreviewFloat.Add(Tuple.Create("ObjectTracking/" + name + "/R" + pair.Key[0], AnimatorConditionMode.Less, (float)(1 - 1 / Math.Pow(2, pair.Value[0]))));
            }
            
            Tuple<string, bool>[] conditionsToShowRemoteBool = new Tuple<string, bool>[]
            {
                Tuple.Create("IsLocal", false),
            };
            List<Tuple<string, AnimatorConditionMode, float>> conditionsToShowRemoteFloat = new List<Tuple<string, AnimatorConditionMode, float>> {};
            foreach (KeyValuePair<string[], int[]> pair in Axes())
            {
                conditionsToShowRemoteFloat.Add(Tuple.Create("ObjectTracking/" + name + "/R" + pair.Key[0], AnimatorConditionMode.Greater, (float)(1 / Math.Pow(2, pair.Value[0]))));
                conditionsToShowRemoteFloat.Add(Tuple.Create("ObjectTracking/" + name + "/R" + pair.Key[0], AnimatorConditionMode.Less, (float)(1 - 1 / Math.Pow(2, pair.Value[0]))));
            }
            
            /*
            // TODO: add entry transitions 
            layer.stateMachine.entryTransitions = new[]
            {
                Utility.CreateEntryTransition("isLocal", conditionsToShowLocalBool, conditionsToShowLocalFloat.ToArray(), stateShow),
                Utility.CreateEntryTransition("isRemotePreview", conditionsToShowRemotePreviewBool, conditionsToShowRemotePreviewFloat.ToArray(), stateShow),
                Utility.CreateEntryTransition("isRemote", conditionsToShowRemoteBool, conditionsToShowRemoteFloat.ToArray(), stateShow),
            };
            */
            stateHide.transitions = new[]
            {
                Utility.CreateTransition("isLocal", conditionsToShowLocalBool, conditionsToShowLocalFloat.ToArray(), stateShow),
                Utility.CreateTransition("isRemotePreview", conditionsToShowRemotePreviewBool, conditionsToShowRemotePreviewFloat.ToArray(), stateShow),
                Utility.CreateTransition("isRemote", conditionsToShowRemoteBool, conditionsToShowRemoteFloat.ToArray(), stateShow),
            };

            /// Hide
            //stateShow.transitions = new[] {};
            foreach (KeyValuePair<string[], int[]> pair in Axes())
            {
                Tuple<string, bool>[] conditionsToHideBool = new Tuple<string, bool>[]
                {
                    Tuple.Create("IsLocal", true),
                    Tuple.Create("ObjectTracking/isRemotePreview", false),
                };
                Tuple<string, AnimatorConditionMode, float>[] conditionsToHideFloat1 = new Tuple<string, AnimatorConditionMode, float>[]
                {
                    Tuple.Create("ObjectTracking/" + name + "/L" + pair.Key[0], AnimatorConditionMode.Less, (float)(1 / Math.Pow(2, 32))),
                };
                stateShow.transitions = stateShow.transitions.Append(Utility.CreateTransition("isLocal (<" + pair.Key[0] + ")", conditionsToHideBool, conditionsToHideFloat1, stateHide)).ToArray();
                Tuple<string, AnimatorConditionMode, float>[] conditionsToHideFloat2 = new Tuple<string, AnimatorConditionMode, float>[]
                {
                    Tuple.Create("ObjectTracking/" + name + "/L" + pair.Key[0], AnimatorConditionMode.Greater, (float)(1 - 1 / Math.Pow(2, 32))),
                };
                stateShow.transitions = stateShow.transitions.Append(Utility.CreateTransition("isLocal (>" + pair.Key[0] + ")", conditionsToHideBool, conditionsToHideFloat2, stateHide)).ToArray();
            }
            foreach (KeyValuePair<string[], int[]> pair in Axes())
            {
                Tuple<string, bool>[] conditionsToHideBool = new Tuple<string, bool>[]
                {
                    Tuple.Create("IsLocal", true),
                    Tuple.Create("ObjectTracking/isRemotePreview", true),
                };
                Tuple<string, AnimatorConditionMode, float>[] conditionsToHideFloat1 = new Tuple<string, AnimatorConditionMode, float>[]
                {
                    Tuple.Create("ObjectTracking/" + name + "/R" + pair.Key[0], AnimatorConditionMode.Less, (float)(1 / Math.Pow(2, pair.Value[0]))),
                };
                stateShow.transitions = stateShow.transitions.Append(Utility.CreateTransition("isRemotePreview (<" + pair.Key[0] + ")", conditionsToHideBool, conditionsToHideFloat1, stateHide)).ToArray();
                Tuple<string, AnimatorConditionMode, float>[] conditionsToHideFloat2 = new Tuple<string, AnimatorConditionMode, float>[]
                {
                    Tuple.Create("ObjectTracking/" + name + "/R" + pair.Key[0], AnimatorConditionMode.Greater, (float)(1 - 1 / Math.Pow(2, pair.Value[0]))),
                };
                stateShow.transitions = stateShow.transitions.Append(Utility.CreateTransition("isRemotePreview (>" + pair.Key[0] + ")", conditionsToHideBool, conditionsToHideFloat2, stateHide)).ToArray();
            }
            foreach (KeyValuePair<string[], int[]> pair in Axes())
            {
                Tuple<string, bool>[] conditionsToHideBool = new Tuple<string, bool>[]
                {
                    Tuple.Create("IsLocal", false),
                };
                Tuple<string, AnimatorConditionMode, float>[] conditionsToHideFloat1 = new Tuple<string, AnimatorConditionMode, float>[]
                {
                    Tuple.Create("ObjectTracking/" + name + "/R" + pair.Key[0], AnimatorConditionMode.Less, (float)(1 / Math.Pow(2, pair.Value[0]))),
                };
                stateShow.transitions = stateShow.transitions.Append(Utility.CreateTransition("isRemote (<" + pair.Key[0] + ")", conditionsToHideBool, conditionsToHideFloat1, stateHide)).ToArray();
                Tuple<string, AnimatorConditionMode, float>[] conditionsToHideFloat2 = new Tuple<string, AnimatorConditionMode, float>[]
                {
                    Tuple.Create("ObjectTracking/" + name + "/R" + pair.Key[0], AnimatorConditionMode.Greater, (float)(1 - 1 / Math.Pow(2, pair.Value[0]))),
                };
                stateShow.transitions = stateShow.transitions.Append(Utility.CreateTransition("isRemote (>" + pair.Key[0] + ")", conditionsToHideBool, conditionsToHideFloat2, stateHide)).ToArray();
            }

            // Clip
            stateShow.motion =
                Utility.CreateClip(name + "/ShowInsideLimits", "ObjectTracking/" + name, "m_IsActive", 1, 1, assetFolder);
            stateHide.motion =
                Utility.CreateClip(name + "/HideBeyondLimits", "ObjectTracking/" + name, "m_IsActive", 0, 0, assetFolder);

            Utility.AddSubAssetsToDatabase(layer, controller);
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
                timeParameter = "ObjectTracking/" + name + "/L" + axe.Key[0]
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
                timeParameter = "ObjectTracking/" + name + "/R" + axe.Key[0]
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

            layer.stateMachine.entryTransitions = new[]
            {
                Utility.CreateEntryTransition("isRemote", conditionsToRemote, stateRemote),
                Utility.CreateEntryTransition("isRemotePreview", conditionsToRemotePreview, stateRemote),
            };
            stateLocal.transitions = new[]
            {
                Utility.CreateTransition("isRemote", conditionsToRemote, stateRemote),
                Utility.CreateTransition("isRemotePreview", conditionsToRemotePreview, stateRemote),
            };
            stateRemote.transitions = new[]
            {
                Utility.CreateTransition("isLocal", conditionsToLocal, stateLocal),
            };

            // Clip
            stateLocal.motion =
                Utility.CreateClip(name + "/L" + axe.Key[0], axe.Key[1], axe.Key[2], axe.Value[1], axe.Value[2], assetFolder);
            stateRemote.motion =
                Utility.CreateClip(name + "/R" + axe.Key[0], axe.Key[1], axe.Key[2], axe.Value[3], axe.Value[4], assetFolder);

            Utility.AddSubAssetsToDatabase(layer, controller);
        }

        public void AppendAvatarParameterDriverParameters(List<VRCAvatarParameterDriver.Parameter> parameterDriverParameters)
        {
            foreach (KeyValuePair<string[], int[]> pair in Axes())
            {
                int accuracy = pair.Value[0];
                // TODO: simplification if accuracy is 8: skip
                int accuracyBytes = accuracy / 8;
                int accuracyBits = accuracy - (accuracyBytes * 8);
                for (int i = 0; i < accuracyBits; i++)
                {
                    parameterDriverParameters.Add(Utility.ParameterDriverParameterBoolToFloat(
                        "ObjectTracking/" + name + "/R" + pair.Key[0] + "-Bit" + i,
                        "ObjectTracking/" + name + "/R" + pair.Key[0] + "-Bit" + i + "-Float"));
                }

                for (int i = 0; i < accuracyBytes; i++)
                {
                    parameterDriverParameters.Add(Utility.ParameterDriverParameterIntToFloat(
                        "ObjectTracking/" + name + "/R" + pair.Key[0] + "-Byte" + i,
                        "ObjectTracking/" + name + "/R" + pair.Key[0] + "-Byte" + i + "-Float"));
                }
            }
        }

        public void AppendAAPMotionList(String assetFolder, Dictionary<string, AnimationClip> motions)
        {
            foreach (KeyValuePair<string[], int[]> axe in Axes())
            {
                int accuracy = axe.Value[0]; // bits
                // TODO: simplification if accuracy is 8: skip
                int accuracyBytes = accuracy / 8;
                int accuracyBits = accuracy - (accuracyBytes * 8);
                for (int i = 0; i < accuracyBytes; i++)
                {
                    float multiplicator = Utility.GetAAPMultiplicator(accuracy, 8 * i, 8);
                    motions.Add("ObjectTracking/" + name + "/R" + axe.Key[0] + "-Byte" + i + "-Float",
                        Utility.CreateClip(name + "/R" + axe.Key[0] + "-Byte" + i, "", "ObjectTracking/" + name + "/R" + axe.Key[0], multiplicator, multiplicator, assetFolder));
                }

                for (int i = 0; i < accuracyBits; i++)
                {
                    float multiplicator = Utility.GetAAPMultiplicator(accuracy, 8 * accuracyBytes + i, 1);
                    motions.Add("ObjectTracking/" + name + "/R" + axe.Key[0] + "-Bit" + i + "-Float",
                        Utility.CreateClip(name + "/R" + axe.Key[0] + "-Bit" + i, "", "ObjectTracking/" + name + "/R" + axe.Key[0], multiplicator, multiplicator, assetFolder));
                }
            }
        }

        public void AppendResetList(Dictionary<string[], float[]> reset)
        {
            foreach (KeyValuePair<string[], int[]> axe in Axes())
            {
                reset.Add(new[] { axe.Key[1], "m_IsActive" }, new[] { 1f, 1f });
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
                if (axe.Key[0] == "PX") {
                    reset.Add(new[] { axe.Key[1], "m_RotationOffsets.Array.data[1].weight" }, new[] { 3f, 3f });
                }
            }
        }

        public void AppendAnimatorControllerParameters(AnimatorController controller)
        {
            foreach (KeyValuePair<string[], int[]> pair in Axes())
            {
                int accuracy = pair.Value[0];
                // TODO: simplification if accuracy is 8: skip both for-loops
                int accuracyBytes = accuracy / 8;
                int accuracyBits = accuracy - (accuracyBytes * 8);
                controller = Utility.CreateFloatParameterAndAddToAnimator(controller, "ObjectTracking/" + name + "/L" + pair.Key[0]);
                controller = Utility.CreateFloatParameterAndAddToAnimator(controller, "ObjectTracking/" + name + "/R" + pair.Key[0]);
                for (int i = 0; i < accuracyBits; i++)
                {
                    controller = Utility.CreateBoolParameterAndAddToAnimator(controller, "ObjectTracking/" + name + "/R" + pair.Key[0] + "-Bit" + i);
                    controller = Utility.CreateFloatParameterAndAddToAnimator(controller, "ObjectTracking/" + name + "/R" + pair.Key[0] + "-Bit" + i + "-Float");
                }

                for (int i = 0; i < accuracyBytes; i++)
                {
                    controller = Utility.CreateIntParameterAndAddToAnimator(controller, "ObjectTracking/" + name + "/R" + pair.Key[0] + "-Byte" + i);
                    controller = Utility.CreateFloatParameterAndAddToAnimator(controller, "ObjectTracking/" + name + "/R" + pair.Key[0] + "-Byte" + i + "-Float");
                }
            }
        }

        public void AppendExpressionParameters(VRCExpressionParameters expressionParameters)
        {
            foreach (KeyValuePair<string[], int[]> pair in Axes())
            {
                int accuracy = pair.Value[0];
                // TODO: simplification if accuracy is 8: skip both for-loops and do a synced parameter like local instead
                int accuracyBytes = accuracy / 8;
                int accuracyBits = accuracy - (accuracyBytes * 8);
                expressionParameters = Utility.CreateFloatParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/" + name + "/L" + pair.Key[0], 0f, false, false);
                for (int i = 0; i < accuracyBits; i++)
                {
                    expressionParameters = Utility.CreateBoolParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/" + name + "/R" + pair.Key[0] + "-Bit" + i, false, false, true);
                }

                for (int i = 0; i < accuracyBytes; i++)
                {
                    expressionParameters = Utility.CreateIntParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/" + name + "/R" + pair.Key[0] + "-Byte" + i, 0, false, true);
                }
            }
        }

        public int GetExpressionParameters()
        {
            int costs = 0;
            foreach (KeyValuePair<string[], int[]> pair in Axes())
            {
                costs += 1; // L<PX|PY|PZ|RX|RY|RZ>
                costs += pair.Value[0] / 8; // R<PX|PY|PZ|RX|RY|RZ>-Byte<0-4>
                costs += pair.Value[0] % 8; // R<PX|PY|PZ|RX|RY|RZ>-Bit<0-7>
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