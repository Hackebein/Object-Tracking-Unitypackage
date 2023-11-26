#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Hackebein.ObjectTracking
{
    [CustomEditor(typeof(Setup))]
    public class ObjectTrackingEditor : Editor
    {
        private float _realEyeHeight = 1.7f;

        private int RangeNumberInputField(int value, int min, int max, GUILayoutOption[] guiLayoutOption)
        {
            return (int)RangeNumberInputField((float)value, min, max, guiLayoutOption);
        }

        private float RangeNumberInputField(float value, int min, int max, GUILayoutOption[] guiLayoutOption)
        {
            value = EditorGUILayout.FloatField(value, guiLayoutOption);
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }

            return value;
        }

        private int SliderNumberInputField(int value, int min, int max, GUILayoutOption[] guiLayoutOption)
        {
            return (int)GUILayout.HorizontalSlider(value, min, max, guiLayoutOption.Append(GUILayout.Height(20)).ToArray());
        }

        private void LabelAccuracy(int range, int bits, string suffix, GUILayoutOption[] guiLayoutOption)
        {
            using (new GUILayout.VerticalScope())
            {
                float accuracy = (float)range / (1L << bits);
                if (bits == 32 && range == 42)
                {
                    // ;)
                    GUILayout.Label("^,....^ remember i love you", guiLayoutOption);
                }
                else if (suffix == "m" && accuracy < 0.01)
                {
                    GUILayout.Label("<0.01" + suffix, guiLayoutOption);
                    GUILayout.Label("<0.4in", guiLayoutOption);
                }
                else if (suffix == "°" && accuracy < 0.1)
                {
                    GUILayout.Label("<0.1" + suffix, guiLayoutOption);
                }
                else if (bits > 0 && suffix == "m")
                {
                    GUILayout.Label(accuracy.ToString("F3") + suffix, guiLayoutOption);
                    GUILayout.Label((accuracy / 0.0254).ToString("F3") + "in", guiLayoutOption);
                }
                else if (bits > 0 && suffix == "°")
                {
                    GUILayout.Label(accuracy.ToString("F2") + suffix, guiLayoutOption);
                }
                else
                {
                    GUILayout.Label("n/A", guiLayoutOption);
                }
            }
        }

        private GUILayoutOption[] RelativeWidth(float width)
        {
            return new[] { GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.95f * width) };
        }

        private string GenerateCostsString(string unit, int expected, int used, int usedTotal, int max)
        {
            return expected + " / " + (max - usedTotal + used) + " (max: " + max + ") " + unit;
        }

        public override void OnInspectorGUI()
        {
            Setup setup = (Setup)target;

            setup.mode = (Utility.Modes)GUILayout.Toolbar(setup.mode.GetHashCode(), Utility.ModesText);

            if (setup.mode == Utility.Modes.Expert)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Game Object:");
                    setup.rootGameObject =
                        (GameObject)EditorGUILayout.ObjectField(setup.rootGameObject,
                            typeof(GameObject), true, RelativeWidth(3 / 5f));
                    // TODO: Load everything from the avatar and fill the fields?
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Animator Controller:");
                    setup.controller =
                        (AnimatorController)EditorGUILayout.ObjectField(setup.controller,
                            typeof(AnimatorController), true, RelativeWidth(3 / 5f));
                    // TODO: load tracker data from animator
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Expression Parameters:");
                    setup.expressionParameters =
                        (VRCExpressionParameters)EditorGUILayout.ObjectField(setup.expressionParameters,
                            typeof(VRCExpressionParameters), true, RelativeWidth(3 / 5f));
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Scale:");
                    setup.scale = EditorGUILayout.FloatField(setup.scale, RelativeWidth(3 / 5f));
                    if (setup.scale <= 0)
                    {
                        setup.scale = 1;
                    }
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Asset folder:");
                    using (new GUILayout.HorizontalScope(RelativeWidth(3 / 5f)))
                    {
                        String path = setup.assetFolder;
                        path = EditorGUILayout.TextField(path);
                        if (GUILayout.Button("Select"))
                        {
                            path = EditorUtility.OpenFolderPanel("Select asset folder", path, "").Replace(Application.dataPath, "Assets");
                        }

                        if (!path.StartsWith("Assets/"))
                        {
                            path = "Assets/Hackebein/ObjectTracking/Generated";
                        }

                        setup.assetFolder = path;
                    }
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("UUID:");
                    using (new GUILayout.HorizontalScope(RelativeWidth(3 / 5f)))
                    {
                        setup.uuid = EditorGUILayout.TextField(setup.uuid);
                        if (GUILayout.Button("Generate") || setup.uuid.Length == 0)
                        {
                            setup.uuid = Guid.NewGuid().ToString();
                        }
                    }
                }
            }
            else
            {
                float avatarEyeHeight = 0;
                VRCAvatarDescriptor avatarDescriptor = null;

                if (setup.rootGameObject != null)
                {
                    avatarDescriptor = (VRCAvatarDescriptor)setup.rootGameObject.GetComponent(typeof(VRCAvatarDescriptor));
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Avatar:");
                    avatarDescriptor = (VRCAvatarDescriptor)EditorGUILayout.ObjectField(avatarDescriptor,
                        typeof(VRCAvatarDescriptor), true, RelativeWidth(3 / 5f));
                }

                setup.expressionParameters = null;
                setup.controller = null;
                if (avatarDescriptor == null)
                {
                    setup.rootGameObject = null;
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("");
                        GUILayout.Label("No Avatar found!", RelativeWidth(3 / 5f));
                    }
                }
                else
                {
                    setup.rootGameObject = avatarDescriptor.gameObject;
                    setup.expressionParameters = avatarDescriptor.expressionParameters;
                    VRCAvatarDescriptor.CustomAnimLayer[] customAnimLayers = avatarDescriptor.baseAnimationLayers;
                    for (int i = 0; i < customAnimLayers.Length; i++)
                    {
                        if (customAnimLayers[i].type == VRCAvatarDescriptor.AnimLayerType.FX &&
                            customAnimLayers[i].animatorController != null)
                        {
                            setup.controller = (AnimatorController)customAnimLayers[i].animatorController;
                            break;
                        }
                    }

                    if (setup.controller == null)
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("");
                            GUILayout.Label("No FX Controller found!", RelativeWidth(3 / 5f));
                        }
                    }

                    if (setup.expressionParameters == null)
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("");
                            GUILayout.Label("No Expression Parameter List found!", RelativeWidth(3 / 5f));
                        }
                    }

                    avatarEyeHeight = avatarDescriptor.ViewPosition.y;
                }

                if (avatarEyeHeight > 0)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Avatar eye height");
                        GUILayout.Label(avatarEyeHeight + "m", RelativeWidth(3 / 5f));
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Real eye height (in m)");
                        // TODO: measure with HMD over OpenVR?
                        _realEyeHeight = EditorGUILayout.FloatField(_realEyeHeight, RelativeWidth(3 / 5f));
                    }

                    if (_realEyeHeight > 0)
                    {
                        setup.scale = avatarEyeHeight / _realEyeHeight;
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Eye height ratio");
                            GUILayout.Label(setup.scale.ToString("F4") + "x", RelativeWidth(3 / 5f));
                        }
                    }
                }
            }

            foreach (SetupTracker tracker in setup.trackers)
            {
                using (new GUILayout.VerticalScope("box"))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Serial Number:");
                        tracker.name = EditorGUILayout.TextField(tracker.name, RelativeWidth(3 / 5f));
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Tracker Type:");
                        tracker.trackerType = (Utility.TrackerType)EditorGUILayout.Popup(tracker.trackerType.GetHashCode(), Utility.TrackerTypeText, RelativeWidth(3 / 5f));
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Prefab:");
                        // TODO: add prefabs for easy installation
                        EditorGUILayout.Popup(0, new string[]
                        {
                            "None",
                            "Hackebein - X-Pole Pole Silkii mount (coming soon)",
                            "Hackebein - X-Pole Aerial Hoop mount (coming soon)",
                            "Jangxx - Bottle Mount (coming soon)", // https://www.thingiverse.com/thing:4732305
                        }, RelativeWidth(3 / 5f));
                        
                        // TODO: show extra information about the prefab (License, Author, URL, etc.)
                    }
                    if (setup.mode == Utility.Modes.Expert)
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Data Transmit Type:");
                            // TODO: Implement Data Transmit Types
                            EditorGUILayout.Popup(0, new string[]
                            {
                                "Mixed (int8, bool)",
                                "Native (float8) (coming soon)", // Force accuracy of 8 bits per axe
                                "Native (bool) (coming soon)", // Force accuracy of 1 bit per axe
                            }, RelativeWidth(3 / 5f));
                        }
                    }

                    if (setup.mode >= Utility.Modes.Advanced)
                    {
                        using (new GUILayout.HorizontalScope("box"))
                        {
                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("Position", RelativeWidth((float)1 / 5));
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("Bits:", RelativeWidth((float)1 / 5));
                                if (setup.mode == Utility.Modes.Expert)
                                {
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        GUILayout.Label("Remote", RelativeWidth((float)1 / 5));
                                    }
                                }
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("Min (in m):", RelativeWidth((float)1 / 5));
                                if (setup.mode == Utility.Modes.Expert)
                                {
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        GUILayout.Label("Local", RelativeWidth((float)1 / 5 / 2));
                                        GUILayout.Label("Remote", RelativeWidth((float)1 / 5 / 2));
                                    }
                                }
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("Max (in m):", RelativeWidth((float)1 / 5));
                                if (setup.mode == Utility.Modes.Expert)
                                {
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        GUILayout.Label("Local", RelativeWidth((float)1 / 5 / 2));
                                        GUILayout.Label("Remote", RelativeWidth((float)1 / 5 / 2));
                                    }
                                }
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("Accuracy:", RelativeWidth((float)1 / 5));
                                if (setup.mode == Utility.Modes.Expert)
                                {
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        GUILayout.Label("Local", RelativeWidth((float)1 / 5 / 2));
                                        GUILayout.Label("Remote", RelativeWidth((float)1 / 5 / 2));
                                    }
                                }
                            }
                        }

                        using (new GUILayout.HorizontalScope("box"))
                        {
                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("X:", RelativeWidth((float)1 / 5));
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                tracker.bitsRPX = RangeNumberInputField(tracker.bitsRPX, 0, 32, RelativeWidth((float)1 / 5));
                                tracker.bitsRPX = SliderNumberInputField(tracker.bitsRPX, 0, 32, RelativeWidth((float)1 / 5));
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        tracker.minLPX = EditorGUILayout.IntField(tracker.minLPX, RelativeWidth((float)1 / 5 / 2));
                                    }

                                    if (tracker.bitsRPX == 0)
                                    {
                                        GUI.enabled = false;
                                    }

                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        tracker.minRPX = EditorGUILayout.IntField(tracker.minRPX, RelativeWidth((float)1 / 5 / 2));
                                    }
                                    else
                                    {
                                        tracker.minRPX = EditorGUILayout.IntField(tracker.minRPX, RelativeWidth((float)1 / 5));
                                    }

                                    GUI.enabled = true;
                                }
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        tracker.maxLPX = EditorGUILayout.IntField(tracker.maxLPX, RelativeWidth((float)1 / 5 / 2));
                                    }

                                    if (tracker.bitsRPX == 0)
                                    {
                                        GUI.enabled = false;
                                    }

                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        tracker.maxRPX = EditorGUILayout.IntField(tracker.maxRPX, RelativeWidth((float)1 / 5 / 2));
                                    }
                                    else
                                    {
                                        tracker.maxRPX = EditorGUILayout.IntField(tracker.maxRPX, RelativeWidth((float)1 / 5));
                                    }

                                    GUI.enabled = true;
                                }
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        LabelAccuracy(tracker.maxLPX - tracker.minLPX, 32, "m", RelativeWidth((float)1 / 5 / 2));
                                        LabelAccuracy(tracker.maxRPX - tracker.minRPX, tracker.bitsRPX, "m", RelativeWidth((float)1 / 5 / 2));
                                    }
                                    else
                                    {
                                        LabelAccuracy(tracker.maxRPX - tracker.minRPX, tracker.bitsRPX, "m", RelativeWidth((float)1 / 5));
                                    }
                                }
                            }
                        }

                        using (new GUILayout.HorizontalScope("box"))
                        {
                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("Y:", RelativeWidth((float)1 / 5));
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                tracker.bitsRPY = RangeNumberInputField(tracker.bitsRPY, 0, 32, RelativeWidth((float)1 / 5));
                                tracker.bitsRPY = SliderNumberInputField(tracker.bitsRPY, 0, 32, RelativeWidth((float)1 / 5));
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        tracker.minLPY = EditorGUILayout.IntField(tracker.minLPY, RelativeWidth((float)1 / 5 / 2));
                                    }

                                    if (tracker.bitsRPY == 0)
                                    {
                                        GUI.enabled = false;
                                    }

                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        tracker.minRPY = EditorGUILayout.IntField(tracker.minRPY, RelativeWidth((float)1 / 5 / 2));
                                    }
                                    else
                                    {
                                        tracker.minRPY = EditorGUILayout.IntField(tracker.minRPY, RelativeWidth((float)1 / 5));
                                    }

                                    GUI.enabled = true;
                                }
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        tracker.maxLPY = EditorGUILayout.IntField(tracker.maxLPY, RelativeWidth((float)1 / 5 / 2));
                                    }

                                    if (tracker.bitsRPY == 0)
                                    {
                                        GUI.enabled = false;
                                    }

                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        tracker.maxRPY = EditorGUILayout.IntField(tracker.maxRPY, RelativeWidth((float)1 / 5 / 2));
                                    }
                                    else
                                    {
                                        tracker.maxRPY = EditorGUILayout.IntField(tracker.maxRPY, RelativeWidth((float)1 / 5));
                                    }

                                    GUI.enabled = true;
                                }
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        LabelAccuracy(tracker.maxLPY - tracker.minLPY, 32, "m", RelativeWidth((float)1 / 5 / 2));
                                        LabelAccuracy(tracker.maxRPY - tracker.minRPY, tracker.bitsRPY, "m", RelativeWidth((float)1 / 5 / 2));
                                    }
                                    else
                                    {
                                        LabelAccuracy(tracker.maxRPY - tracker.minRPY, tracker.bitsRPY, "m", RelativeWidth((float)1 / 5));
                                    }
                                }
                            }
                        }

                        using (new GUILayout.HorizontalScope("box"))
                        {
                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("Z:", RelativeWidth((float)1 / 5));
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                tracker.bitsRPZ = RangeNumberInputField(tracker.bitsRPZ, 0, 32, RelativeWidth((float)1 / 5));
                                tracker.bitsRPZ = SliderNumberInputField(tracker.bitsRPZ, 0, 32, RelativeWidth((float)1 / 5));
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        tracker.minLPZ = EditorGUILayout.IntField(tracker.minLPZ, RelativeWidth((float)1 / 5 / 2));
                                    }

                                    if (tracker.bitsRPZ == 0)
                                    {
                                        GUI.enabled = false;
                                    }

                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        tracker.minRPZ = EditorGUILayout.IntField(tracker.minRPZ, RelativeWidth((float)1 / 5 / 2));
                                    }
                                    else
                                    {
                                        tracker.minRPZ = EditorGUILayout.IntField(tracker.minRPZ, RelativeWidth((float)1 / 5));
                                    }

                                    GUI.enabled = true;
                                }
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        tracker.maxLPZ = EditorGUILayout.IntField(tracker.maxLPZ, RelativeWidth((float)1 / 5 / 2));
                                    }

                                    if (tracker.bitsRPZ == 0)
                                    {
                                        GUI.enabled = false;
                                    }

                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        tracker.maxRPZ = EditorGUILayout.IntField(tracker.maxRPZ, RelativeWidth((float)1 / 5 / 2));
                                    }
                                    else
                                    {
                                        tracker.maxRPZ = EditorGUILayout.IntField(tracker.maxRPZ, RelativeWidth((float)1 / 5));
                                    }

                                    GUI.enabled = true;
                                }
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        LabelAccuracy(tracker.maxLPZ - tracker.minLPZ, 32, "m", RelativeWidth((float)1 / 5 / 2));
                                        LabelAccuracy(tracker.maxRPZ - tracker.minRPZ, tracker.bitsRPZ, "m", RelativeWidth((float)1 / 5 / 2));
                                    }
                                    else
                                    {
                                        LabelAccuracy(tracker.maxRPZ - tracker.minRPZ, tracker.bitsRPZ, "m", RelativeWidth((float)1 / 5));
                                    }
                                }
                            }
                        }

                        EditorGUILayout.Space();

                        using (new GUILayout.HorizontalScope("box"))
                        {
                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("Rotation", RelativeWidth((float)1 / 5));
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("Bits:", RelativeWidth((float)1 / 5));
                                if (setup.mode == Utility.Modes.Expert)
                                {
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        GUILayout.Label("Remote", RelativeWidth((float)1 / 5));
                                    }
                                }
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                if (setup.mode == Utility.Modes.Expert)
                                {
                                    GUILayout.Label("Min (in °):", RelativeWidth((float)1 / 5));
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        GUILayout.Label("Local", RelativeWidth((float)1 / 5 / 2));
                                        GUILayout.Label("Remote", RelativeWidth((float)1 / 5 / 2));
                                    }
                                }
                                else
                                {
                                    GUILayout.Label("", RelativeWidth((float)1 / 5));
                                }
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                if (setup.mode == Utility.Modes.Expert)
                                {
                                    GUILayout.Label("Max (in °):", RelativeWidth((float)1 / 5));
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        GUILayout.Label("Local", RelativeWidth((float)1 / 5 / 2));
                                        GUILayout.Label("Remote", RelativeWidth((float)1 / 5 / 2));
                                    }
                                }
                                else
                                {
                                    GUILayout.Label("", RelativeWidth((float)1 / 5));
                                }
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("Accuracy:", RelativeWidth((float)1 / 5));
                                if (setup.mode == Utility.Modes.Expert)
                                {
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        GUILayout.Label("Local", RelativeWidth((float)1 / 5 / 2));
                                        GUILayout.Label("Remote", RelativeWidth((float)1 / 5 / 2));
                                    }
                                }
                            }
                        }

                        using (new GUILayout.HorizontalScope("box"))
                        {
                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("X:", RelativeWidth((float)1 / 5));
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                tracker.bitsRRX = RangeNumberInputField(tracker.bitsRRX, 0, 32, RelativeWidth((float)1 / 5));
                                tracker.bitsRRX = SliderNumberInputField(tracker.bitsRRX, 0, 32, RelativeWidth((float)1 / 5));
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        tracker.minLRX = EditorGUILayout.IntField(tracker.minLRX, RelativeWidth((float)1 / 5 / 2));
                                        if (tracker.bitsRRX == 0)
                                        {
                                            GUI.enabled = false;
                                        }

                                        tracker.minRRX = EditorGUILayout.IntField(tracker.minRRX, RelativeWidth((float)1 / 5 / 2));
                                        GUI.enabled = true;
                                    }
                                    else
                                    {
                                        GUILayout.Label("", RelativeWidth((float)1 / 5));
                                    }
                                }
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        tracker.maxLRX = EditorGUILayout.IntField(tracker.maxLRX, RelativeWidth((float)1 / 5 / 2));
                                        if (tracker.bitsRRX == 0)
                                        {
                                            GUI.enabled = false;
                                        }

                                        tracker.maxRRX = EditorGUILayout.IntField(tracker.maxRRX, RelativeWidth((float)1 / 5 / 2));
                                        GUI.enabled = true;
                                    }
                                    else
                                    {
                                        GUILayout.Label("", RelativeWidth((float)1 / 5));
                                    }
                                }
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        LabelAccuracy(tracker.maxLRX - tracker.minLRX, 32, "°", RelativeWidth((float)1 / 5 / 2));
                                        LabelAccuracy(tracker.maxRRX - tracker.minRRX, tracker.bitsRRX, "°", RelativeWidth((float)1 / 5 / 2));
                                    }
                                    else
                                    {
                                        LabelAccuracy(tracker.maxRRX - tracker.minRRX, tracker.bitsRRX, "°", RelativeWidth((float)1 / 5));
                                    }
                                }
                            }
                        }

                        using (new GUILayout.HorizontalScope("box"))
                        {
                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("Y:", RelativeWidth((float)1 / 5));
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                tracker.bitsRRY = RangeNumberInputField(tracker.bitsRRY, 0, 32, RelativeWidth((float)1 / 5));
                                tracker.bitsRRY = SliderNumberInputField(tracker.bitsRRY, 0, 32, RelativeWidth((float)1 / 5));
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        tracker.minLRY = EditorGUILayout.IntField(tracker.minLRY, RelativeWidth((float)1 / 5 / 2));
                                        if (tracker.bitsRRY == 0)
                                        {
                                            GUI.enabled = false;
                                        }

                                        tracker.minRRY = EditorGUILayout.IntField(tracker.minRRY, RelativeWidth((float)1 / 5 / 2));
                                        GUI.enabled = true;
                                    }
                                    else
                                    {
                                        GUILayout.Label("", RelativeWidth((float)1 / 5));
                                    }
                                }
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        tracker.maxLRY = EditorGUILayout.IntField(tracker.maxLRY, RelativeWidth((float)1 / 5 / 2));
                                        if (tracker.bitsRRY == 0)
                                        {
                                            GUI.enabled = false;
                                        }

                                        tracker.maxRRY = EditorGUILayout.IntField(tracker.maxRRY, RelativeWidth((float)1 / 5 / 2));
                                        GUI.enabled = true;
                                    }
                                    else
                                    {
                                        GUILayout.Label("", RelativeWidth((float)1 / 5));
                                    }
                                }
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        LabelAccuracy(tracker.maxLRY - tracker.minLRY, 32, "°", RelativeWidth((float)1 / 5 / 2));
                                        LabelAccuracy(tracker.maxRRY - tracker.minRRY, tracker.bitsRRY, "°", RelativeWidth((float)1 / 5 / 2));
                                    }
                                    else
                                    {
                                        LabelAccuracy(tracker.maxRRY - tracker.minRRY, tracker.bitsRRY, "°", RelativeWidth((float)1 / 5));
                                    }
                                }
                            }
                        }

                        using (new GUILayout.HorizontalScope("box"))
                        {
                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("Z:", RelativeWidth((float)1 / 5));
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                tracker.bitsRRZ = RangeNumberInputField(tracker.bitsRRZ, 0, 32, RelativeWidth((float)1 / 5));
                                tracker.bitsRRZ = SliderNumberInputField(tracker.bitsRRZ, 0, 32, RelativeWidth((float)1 / 5));
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        tracker.minLRZ = EditorGUILayout.IntField(tracker.minLRZ, RelativeWidth((float)1 / 5 / 2));
                                        if (tracker.bitsRRZ == 0)
                                        {
                                            GUI.enabled = false;
                                        }

                                        tracker.minRRZ = EditorGUILayout.IntField(tracker.minRRZ, RelativeWidth((float)1 / 5 / 2));
                                        GUI.enabled = true;
                                    }
                                    else
                                    {
                                        GUILayout.Label("", RelativeWidth((float)1 / 5));
                                    }
                                }
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        tracker.maxLRZ = EditorGUILayout.IntField(tracker.maxLRZ, RelativeWidth((float)1 / 5 / 2));
                                        if (tracker.bitsRRZ == 0)
                                        {
                                            GUI.enabled = false;
                                        }

                                        tracker.maxRRZ = EditorGUILayout.IntField(tracker.maxRRZ, RelativeWidth((float)1 / 5 / 2));
                                        GUI.enabled = true;
                                    }
                                    else
                                    {
                                        GUILayout.Label("", RelativeWidth((float)1 / 5));
                                    }
                                }
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    if (setup.mode == Utility.Modes.Expert)
                                    {
                                        LabelAccuracy(tracker.maxLRZ - tracker.minLRZ, 32, "°", RelativeWidth((float)1 / 5 / 2));
                                        LabelAccuracy(tracker.maxRRZ - tracker.minRRZ, tracker.bitsRRZ, "°", RelativeWidth((float)1 / 5 / 2));
                                    }
                                    else
                                    {
                                        LabelAccuracy(tracker.maxRRZ - tracker.minRRZ, tracker.bitsRRZ, "°", RelativeWidth((float)1 / 5));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        int positionBits = tracker.bitsRPX;
                        int positionRange = tracker.maxRPX - tracker.minRPX;
                        int rotationBits = tracker.bitsRRX;

                        using (new GUILayout.HorizontalScope())
                        {
                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("", RelativeWidth((float)1 / 4));
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("Bits per axe:", RelativeWidth((float)1 / 4));
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("Range (in m):", RelativeWidth((float)1 / 4));
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("Accuracy:", RelativeWidth((float)1 / 4));
                            }
                        }

                        using (new GUILayout.HorizontalScope())
                        {
                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("Position:", RelativeWidth((float)1 / 4));
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                positionBits = RangeNumberInputField(positionBits, 0, 32, RelativeWidth((float)1 / 4));
                                positionBits = SliderNumberInputField(positionBits, 0, 32, RelativeWidth((float)1 / 4));
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                positionRange = Math.Abs(EditorGUILayout.IntField(positionRange, RelativeWidth((float)1 / 4)));
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                LabelAccuracy(positionRange, positionBits, "m", RelativeWidth((float)1 / 4));
                            }
                        }

                        using (new GUILayout.HorizontalScope())
                        {
                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("Rotation:", RelativeWidth((float)1 / 4));
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                rotationBits = RangeNumberInputField(rotationBits, 0, 32, RelativeWidth((float)1 / 4));
                                rotationBits = SliderNumberInputField(rotationBits, 0, 32, RelativeWidth((float)1 / 4));
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                GUILayout.Label("", RelativeWidth((float)1 / 4));
                            }

                            using (new GUILayout.VerticalScope())
                            {
                                LabelAccuracy(360, rotationBits, "°", RelativeWidth((float)1 / 4));
                            }
                        }

                        tracker.bitsRPX = tracker.bitsRPY = tracker.bitsRPZ = positionBits;
                        if (positionBits > 0)
                        {
                            tracker.bitsRPY = positionBits - 1;
                        }

                        tracker.minRPX = tracker.minRPZ = positionRange / 2 * -1;
                        tracker.minRPY = 0;
                        tracker.maxRPX = tracker.maxRPY = tracker.maxRPZ = positionRange / 2;

                        tracker.bitsRRX = tracker.bitsRRY = tracker.bitsRRZ = rotationBits;
                    }


                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Remove '" + tracker.name + "'"))
                        {
                            setup.RemoveTracker(setup.trackers.IndexOf(tracker));
                            break;
                        }
                    }
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Tracker"))
                {
                    setup.AddTracker();
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Costs", RelativeWidth((float)1 / 2));
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label(GenerateCostsString("bits", setup.GetExpectedExpressionParameterCosts(), setup.GetUsedExpressionParameterCosts(), setup.GetUsedTotalExpressionParameterCosts(), Setup.GetMaxExpressionParameterCosts()));
                    GUILayout.Label(GenerateCostsString("parameters", setup.CountExpectedExpressionParameters(), setup.CountUsedExpressionParameters(), setup.CountUsedTotalExpressionParameters(), Setup.CountMaxExpressionParameters()));
                }
            }

            EditorGUILayout.Space();

            bool isTrackersValid = setup.trackers.Count != 0;
            string[] trackerNames = new string[setup.trackers.Count];
            foreach (SetupTracker tracker in setup.trackers)
            {
                string name = tracker.name;
                if (name.Length == 0 || Array.Exists(trackerNames, trackerName => trackerName == name))
                {
                    isTrackersValid = false;
                    break;
                }

                trackerNames[setup.trackers.IndexOf(tracker)] = name;
            }

            bool isParameterCostsViable = Utility.IsCostViable(setup.GetExpectedExpressionParameterCosts(), setup.GetUsedExpressionParameterCosts(), setup.GetUsedTotalExpressionParameterCosts(), Setup.GetMaxExpressionParameterCosts());
            bool isParametersViable = Utility.IsCostViable(setup.CountExpectedExpressionParameters(), setup.CountUsedExpressionParameters(), setup.CountUsedTotalExpressionParameters(), Setup.CountMaxExpressionParameters());
            bool isCreateable = true;
            using (new GUILayout.VerticalScope())
            {
                if (setup.rootGameObject == null)
                {
                    isCreateable = false;
                    GUILayout.Label("No root GameObject selected!", RelativeWidth((float)1 / 2));
                }

                if (setup.controller == null)
                {
                    isCreateable = false;
                    GUILayout.Label("No Animator Controller selected!", RelativeWidth((float)1 / 2));
                }

                if (setup.expressionParameters == null)
                {
                    isCreateable = false;
                    GUILayout.Label("No Expression Parameter List selected!", RelativeWidth((float)1 / 2));
                }

                if (!isParameterCostsViable)
                {
                    isCreateable = false;
                    GUILayout.Label("Expression Parameter Costs are too high!", RelativeWidth((float)1 / 2));
                }

                if (!isParametersViable)
                {
                    isCreateable = false;
                    GUILayout.Label("Expression Parameter Count is too high!", RelativeWidth((float)1 / 2));
                }

                if (!isTrackersValid)
                {
                    isCreateable = false;
                    GUILayout.Label("You have no Tracker or your Tracker names are not unique!", RelativeWidth((float)1 / 2));
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                if (!isCreateable)
                {
                    GUI.enabled = false;
                }

                if (GUILayout.Button(setup.IsInstalled() ? "Recreate" : "Create"))
                {
                    setup.Create();
                }

                GUI.enabled = true;
                if ((setup.rootGameObject == null && setup.controller == null && setup.expressionParameters == null) || !setup.IsInstalled())
                {
                    GUI.enabled = false;
                }

                if (GUILayout.Button("Remove"))
                {
                    setup.Remove();
                }

                GUI.enabled = true;
            }
        }
    }
}
#endif