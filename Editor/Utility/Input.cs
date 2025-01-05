#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.OpenVR;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Valve.VR;

namespace hackebein.objecttracking.utility
{
    public static class Input
    {
        public static string MakeNameSafe(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-";

            var sb = new StringBuilder(fileName.Length);

            foreach (char c in fileName)
            {
                // If it's in the allowed set, keep it
                if (allowedChars.IndexOf(c) >= 0)
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append('_');
                }
            }
            string result = Regex.Replace(sb.ToString(), "_+", "_").Trim('_').ToLower();
            return result;
        }
        public static int RangeNumberField(int value, int min, int max, GUILayoutOption[] guiLayoutOption)
        {
            return (int)RangeNumberField((float)value, min, max, guiLayoutOption);
        }

        public static float RangeNumberField(float value, float min, float max, GUILayoutOption[] guiLayoutOption)
        {
            //TODO: add +/- buttons
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

        public static int SliderNumberField(int value, int min, int max, GUILayoutOption[] guiLayoutOption)
        {
            return (int)GUILayout.HorizontalSlider(value, min, max, guiLayoutOption.Append(GUILayout.Height(20)).ToArray());
        }

        public static float SliderNumberField(float value, float min, float max, GUILayoutOption[] guiLayoutOption)
        {
            return GUILayout.HorizontalSlider(value, min, max, guiLayoutOption.Append(GUILayout.Height(20)).ToArray());
        }
    }
}
#endif