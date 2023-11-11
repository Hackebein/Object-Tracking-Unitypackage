#if VRC_SDK_VRCSDK3
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Hackebein.ObjectTracking
{
    [CustomEditor(typeof(ObjectTrackingSetup))]
    public class ObjectTrackingEditor : Editor
    {
        private float realEyeHeight = 1.7f;
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
            double accuracy = (float)range / (1 << bits);
            if (bits == 32 && range == 42)
            {
                // ;)
                GUILayout.Label("^,....^ remember i love you", guiLayoutOption);
            }
            else if (bits >= 32)
            {
                GUILayout.Label("n/A", guiLayoutOption);
            }
            else if (suffix == "m" && accuracy < 0.001)
            {
                GUILayout.Label(accuracy.ToString("F4") + suffix + " o.O", guiLayoutOption);
            }
            else if (suffix == "°" && accuracy < 0.1)
            {
                GUILayout.Label(accuracy.ToString("F2") + suffix + " o.O", guiLayoutOption);
            }
            else if (bits > 0 && suffix == "m")
            {
                GUILayout.Label(accuracy.ToString("F4") + suffix, guiLayoutOption);
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
        
        private float? GetScaleFactorFromAvatar(GameObject avatar)
        {
            if (avatar != null)
            {
                Transform transform = avatar.transform.Find("ObjectTracking");
                if (transform != null)
                {
                    Vector3 scale = transform.localScale;
                    if (Math.Abs(scale.x - scale.y) < 0.00001 && Math.Abs(scale.x - scale.z) < 0.00001)
                    {
                        return scale.x;
                    }
                }
            }

            return null;
        }

        private GUILayoutOption[] RelativeWidth(float width)
        {
            return new[] { GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.95f * width) };
        }

        // TODO: Load everything from the avatar and fill the fields
        public override void OnInspectorGUI()
        {
            ObjectTrackingSetup setup = ((ObjectTrackingSetup)target);
            float avatarEyeHeight = 0;
            VRCAvatarDescriptor avatarDescriptor = null;

            setup.mode = (ObjectTrackingSetup.Modes)GUILayout.Toolbar(setup.mode.GetHashCode(), setup.modesText);
            // TODO: switch for avatar less descriptor (expert?)

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Avatar");
            // TODO: dropdown for avatar selection? or better filter for current solution like in VRCSDK3?
            setup.rootObjectOfAvatar =
                (GameObject)EditorGUILayout.ObjectField(setup.rootObjectOfAvatar,
                    typeof(GameObject), true, RelativeWidth((float)3 / 5));
            if (setup.rootObjectOfAvatar != null)
            {
                avatarDescriptor = (VRCAvatarDescriptor)setup.rootObjectOfAvatar.GetComponent(typeof(VRCAvatarDescriptor));
                if (avatarDescriptor == null)
                {
                    setup.rootObjectOfAvatar = null;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (avatarDescriptor != null)
            {
                if (setup.fxController == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("");
                    GUILayout.Label("No FX Controller found!", RelativeWidth((float)3 / 5));
                    EditorGUILayout.EndHorizontal();
                }
                if (setup.expressionParameters == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("");
                    GUILayout.Label("No Expression Parameter List found!", RelativeWidth((float)3 / 5));
                    EditorGUILayout.EndHorizontal();
                }
                if (avatarDescriptor)
                {
                    avatarEyeHeight = avatarDescriptor.ViewPosition.y;
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("");
                GUILayout.Label("No Avatar found!", RelativeWidth((float)3 / 5));
                EditorGUILayout.EndHorizontal();
            }

            float? scale = GetScaleFactorFromAvatar(setup.rootObjectOfAvatar);
            if (scale == null && avatarEyeHeight > 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Avatar eye height");
                GUILayout.Label(avatarEyeHeight + "m", RelativeWidth((float)3 / 5));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Real eye height (in m)");
                // TODO: measure with HMD over OpenVR?
                realEyeHeight = EditorGUILayout.FloatField(realEyeHeight, RelativeWidth((float)3 / 5));
                EditorGUILayout.EndHorizontal();
                if (realEyeHeight > 0)
                {
                    scale = avatarEyeHeight / realEyeHeight;
                }
            }
            
            if (scale != null)
            {
                setup.scale = (float)scale;
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Eye height ratio");
                GUILayout.Label(setup.scale.ToString("F4") + "x", RelativeWidth((float)3 / 5));
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Tracker serial number");
            // TODO: get tracker serial number from OpenVR
            setup.trackerSerialNumber = GUILayout.TextField(setup.trackerSerialNumber, RelativeWidth((float)3 / 5));
            if (setup.trackerSerialNumber.Length == 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("");
                GUILayout.Label("invalid name", RelativeWidth((float)3 / 5));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndHorizontal();

            if ((ObjectTrackingSetup.Modes.Expert & setup.mode) != 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Asset folder");
                // TODO: improve asset folder selection
                GUILayout.Label(setup.assetFolder, RelativeWidth((float)2 / 5));
                if (GUILayout.Button("Select", RelativeWidth((float)1 / 5)))
                {
                    setup.assetFolder = EditorUtility.OpenFolderPanel("Select asset folder", setup.assetFolder, "");
                }
                EditorGUILayout.EndHorizontal();
                if (setup.assetFolder.Length == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("");
                    GUILayout.Label("invalid path", RelativeWidth((float)3 / 5));
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space();

            if (((ObjectTrackingSetup.Modes.Advanced | ObjectTrackingSetup.Modes.Expert) & setup.mode) != 0)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Position", RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Bits:", RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Min (in m):", RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Max (in m):", RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Accuracy:", RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                GUILayout.Label("X:", RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                setup.bitsPX = RangeNumberInputField(setup.bitsPX, 0, 32, RelativeWidth((float)1 / 5));
                setup.bitsPX = SliderNumberInputField(setup.bitsPX, 0, 32, RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                if (setup.bitsPX == 0)
                {
                    GUI.enabled = false;
                }

                setup.minPX = EditorGUILayout.IntField(setup.minPX, RelativeWidth((float)1 / 5));
                GUI.enabled = true;
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                if (setup.bitsPX == 0)
                {
                    GUI.enabled = false;
                }

                setup.maxPX = EditorGUILayout.IntField(setup.maxPX, RelativeWidth((float)1 / 5));
                GUI.enabled = true;
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                LabelAccuracy(setup.maxPX - setup.minPX, setup.bitsPX, "m", RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Y:", RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                setup.bitsPY = RangeNumberInputField(setup.bitsPY, 0, 32, RelativeWidth((float)1 / 5));
                setup.bitsPY = SliderNumberInputField(setup.bitsPY, 0, 32, RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                if (setup.bitsPY == 0)
                {
                    GUI.enabled = false;
                }

                setup.minPY = EditorGUILayout.IntField(setup.minPY, RelativeWidth((float)1 / 5));
                GUI.enabled = true;
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                if (setup.bitsPY == 0)
                {
                    GUI.enabled = false;
                }

                setup.maxPY = EditorGUILayout.IntField(setup.maxPY, RelativeWidth((float)1 / 5));
                GUI.enabled = true;
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                LabelAccuracy(setup.maxPY - setup.minPY, setup.bitsPY, "m", RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Z:", RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                setup.bitsPZ = RangeNumberInputField(setup.bitsPZ, 0, 32, RelativeWidth((float)1 / 5));
                setup.bitsPZ = SliderNumberInputField(setup.bitsPZ, 0, 32, RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                if (setup.bitsPZ == 0)
                {
                    GUI.enabled = false;
                }

                setup.minPZ = EditorGUILayout.IntField(setup.minPZ, RelativeWidth((float)1 / 5));
                GUI.enabled = true;
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                if (setup.bitsPZ == 0)
                {
                    GUI.enabled = false;
                }

                setup.maxPZ = EditorGUILayout.IntField(setup.maxPZ, RelativeWidth((float)1 / 5));
                GUI.enabled = true;
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                LabelAccuracy(setup.maxPZ - setup.minPZ, setup.bitsPZ, "m", RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Rotation", RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Bits:", RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                if ((ObjectTrackingSetup.Modes.Expert & setup.mode) != 0)
                {
                    GUILayout.Label("Min (in °):", RelativeWidth((float)1 / 5));
                }
                else
                {
                    GUILayout.Label("", RelativeWidth((float)1 / 5));
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                if ((ObjectTrackingSetup.Modes.Expert & setup.mode) != 0)
                {
                    GUILayout.Label("Max (in °):", RelativeWidth((float)1 / 5));
                }
                else
                {
                    GUILayout.Label("", RelativeWidth((float)1 / 5));
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Accuracy:", RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                GUILayout.Label("X:", RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                setup.bitsRX = RangeNumberInputField(setup.bitsRX, 0, 32, RelativeWidth((float)1 / 5));
                setup.bitsRX = SliderNumberInputField(setup.bitsRX, 0, 32, RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                if ((ObjectTrackingSetup.Modes.Expert & setup.mode) != 0)
                {
                    if (setup.bitsRX == 0)
                    {
                        GUI.enabled = false;
                    }

                    setup.minRX = EditorGUILayout.IntField(setup.minRX, RelativeWidth((float)1 / 5));
                    GUI.enabled = true;
                }
                else
                {
                    GUILayout.Label("", RelativeWidth((float)1 / 5));
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                if ((ObjectTrackingSetup.Modes.Expert & setup.mode) != 0)
                {
                    if (setup.bitsRX == 0)
                    {
                        GUI.enabled = false;
                    }

                    setup.maxRX = EditorGUILayout.IntField(setup.maxRX, RelativeWidth((float)1 / 5));
                    GUI.enabled = true;
                }
                else
                {
                    GUILayout.Label("", RelativeWidth((float)1 / 5));
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                LabelAccuracy(setup.maxRX - setup.minRX, setup.bitsRX, "°", RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Y:", RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                setup.bitsRY = RangeNumberInputField(setup.bitsRY, 0, 32, RelativeWidth((float)1 / 5));
                setup.bitsRY = SliderNumberInputField(setup.bitsRY, 0, 32, RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                if ((ObjectTrackingSetup.Modes.Expert & setup.mode) != 0)
                {
                    if (setup.bitsRY == 0)
                    {
                        GUI.enabled = false;
                    }

                    setup.minRY = EditorGUILayout.IntField(setup.minRY, RelativeWidth((float)1 / 5));
                    GUI.enabled = true;
                }
                else
                {
                    GUILayout.Label("", RelativeWidth((float)1 / 5));
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                if ((ObjectTrackingSetup.Modes.Expert & setup.mode) != 0)
                {
                    if (setup.bitsRY == 0)
                    {
                        GUI.enabled = false;
                    }

                    setup.maxRY = EditorGUILayout.IntField(setup.maxRY, RelativeWidth((float)1 / 5));
                    GUI.enabled = true;
                }
                else
                {
                    GUILayout.Label("", RelativeWidth((float)1 / 5));
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                LabelAccuracy(setup.maxRY - setup.minRY, setup.bitsRY, "°", RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Z:", RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                setup.bitsRZ = RangeNumberInputField(setup.bitsRZ, 0, 32, RelativeWidth((float)1 / 5));
                setup.bitsRZ = SliderNumberInputField(setup.bitsRZ, 0, 32, RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                if ((ObjectTrackingSetup.Modes.Expert & setup.mode) != 0)
                {
                    if (setup.bitsRZ == 0)
                    {
                        GUI.enabled = false;
                    }

                    setup.minRZ = EditorGUILayout.IntField(setup.minRZ, RelativeWidth((float)1 / 5));
                    GUI.enabled = true;
                }
                else
                {
                    GUILayout.Label("", RelativeWidth((float)1 / 5));
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                if ((ObjectTrackingSetup.Modes.Expert & setup.mode) != 0)
                {
                    if (setup.bitsRZ == 0)
                    {
                        GUI.enabled = false;
                    }

                    setup.maxRZ = EditorGUILayout.IntField(setup.maxRZ, RelativeWidth((float)1 / 5));
                    GUI.enabled = true;
                }
                else
                {
                    GUILayout.Label("", RelativeWidth((float)1 / 5));
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                LabelAccuracy(setup.maxRZ - setup.minRZ, setup.bitsRZ, "°", RelativeWidth((float)1 / 5));
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                int positionBits = setup.bitsPX;
                int positionRange = setup.maxPX - setup.minPX;
                int rotationBits = setup.bitsRX;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                GUILayout.Label("", RelativeWidth((float)1 / 4));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Bits per axe:", RelativeWidth((float)1 / 4));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Range (in m):", RelativeWidth((float)1 / 4));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Accuracy:", RelativeWidth((float)1 / 4));
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Position:", RelativeWidth((float)1 / 4));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                positionBits = RangeNumberInputField(positionBits, 0, 32, RelativeWidth((float)1 / 4));
                positionBits = SliderNumberInputField(positionBits, 0, 32, RelativeWidth((float)1 / 4));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                positionRange = Math.Abs(EditorGUILayout.IntField(positionRange, RelativeWidth((float)1 / 4)));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                LabelAccuracy(positionRange, positionBits, "m", RelativeWidth((float)1 / 4));
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Rotation:", RelativeWidth((float)1 / 4));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                rotationBits = RangeNumberInputField(rotationBits, 0, 32, RelativeWidth((float)1 / 4));
                rotationBits = SliderNumberInputField(rotationBits, 0, 32, RelativeWidth((float)1 / 4));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                GUILayout.Label("", RelativeWidth((float)1 / 4));
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                LabelAccuracy(360, rotationBits, "°", RelativeWidth((float)1 / 4));
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                setup.bitsPX = setup.bitsPY = setup.bitsPZ = positionBits;
                if (positionBits > 0)
                {
                    setup.bitsPY = positionBits - 1;
                }

                setup.minPX = setup.minPZ = positionRange / 2 * -1;
                setup.minPY = 0;
                setup.maxPX = setup.maxPY = setup.maxPZ = positionRange / 2;

                setup.bitsRX = setup.bitsRY = setup.bitsRZ = rotationBits;
                setup.minRX = setup.minRY = setup.minRZ = -180;
                setup.maxRX = setup.maxRY = setup.maxRZ = 180;
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            // TODO: take removed parameters into account
            GUILayout.Label("Costs", RelativeWidth((float)1 / 2));
            int costs = setup.bitsPX + setup.bitsPY + setup.bitsPZ + setup.bitsRX + setup.bitsRY + setup.bitsRZ;
            int used = 0;
            if (setup.expressionParameters != null)
            {
                used = setup.expressionParameters.CalcTotalCost();
            }

            setup.expressionParameters = null;
            setup.fxController = null;
            if (avatarDescriptor)
            {
                setup.expressionParameters = avatarDescriptor.expressionParameters;
                VRCAvatarDescriptor.CustomAnimLayer[] customAnimLayers = avatarDescriptor.baseAnimationLayers;
                for (int i = 0; i < customAnimLayers.Length; i++)
                {
                    if (customAnimLayers[i].type == VRCAvatarDescriptor.AnimLayerType.FX &&
                        customAnimLayers[i].animatorController != null)
                    {
                        setup.fxController = (AnimatorController)customAnimLayers[i].animatorController;
                        break;
                    }
                }
            }

            GUILayout.Label(costs + " / " + (VRCExpressionParameters.MAX_PARAMETER_COST - used) + " bits", RelativeWidth((float)1 / 2));
            EditorGUILayout.EndHorizontal();
            using (new GUILayout.HorizontalScope())
            {
                // TODO: take removed parameters into account
                GUILayout.Label("Costs", RelativeWidth((float)1 / 2));
                GUILayout.Label(setup.bitsRX + setup.bitsRY + setup.bitsRZ + setup.bitsPX + setup.bitsPY + setup.bitsPZ + " / " + (256 - setup.expressionParameters.parameters.Length) + " parameters", RelativeWidth((float)1 / 2));
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            // TODO: better check than `setup.assetFolder.Length == 0`
            if (setup.rootObjectOfAvatar == null || setup.fxController == null || setup.expressionParameters == null || setup.trackerSerialNumber.Length == 0 || setup.assetFolder.Length == 0 || costs > VRCExpressionParameters.MAX_PARAMETER_COST - setup.expressionParameters.CalcTotalCost() || setup.expressionParameters.parameters.Length + setup.bitsRX + setup.bitsRY + setup.bitsRZ + setup.bitsPX + setup.bitsPY + setup.bitsPZ > 256 || scale == null)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("(Re)create"))
            {
                ((ObjectTrackingSetup)target).ValidateAndCreate();
            }

            GUI.enabled = true;
            if (setup.rootObjectOfAvatar == null || setup.fxController == null || setup.expressionParameters == null ||
                setup.trackerSerialNumber.Length == 0 || setup.assetFolder.Length == 0)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Remove " + setup.trackerSerialNumber))
            {
                ((ObjectTrackingSetup)target).ValidateAndRemove();
            }

            GUI.enabled = true;
            if (setup.rootObjectOfAvatar == null || setup.fxController == null || setup.expressionParameters == null ||
                setup.assetFolder.Length == 0)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Remove All"))
            {
                ((ObjectTrackingSetup)target).ValidateAndRemoveAll();
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif
