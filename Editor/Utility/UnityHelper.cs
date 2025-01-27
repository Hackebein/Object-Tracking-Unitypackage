#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.OpenVR;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Valve.VR;

namespace hackebein.objecttracking.utility
{
    public static class UnityHelper
    {
	    public static string AddHashToKeyName(string key)
    	{
	        uint hashVal = 5381;
	        
	        foreach (char c in key)
	        {
	            // Multiply by 33 and XOR with the character's code
	            hashVal = (hashVal * 33) ^ c;
	        }
        
        	return $"{key}_h{hashVal}";
    	}
	     
	    /* AvatarDescriptorEditor3.Foldout */
		public static bool Foldout(string editorPrefsKey, string label, bool deft = false)
		{
		    bool prevState = EditorPrefs.GetBool(editorPrefsKey, deft);
		    bool state = EditorGUILayout.Foldout(prevState, label);
		    if (state != prevState)
		        EditorPrefs.SetBool(editorPrefsKey, state);
		    return state;
		}

	    public static GUILayoutOption[] RelativeWidth(float width, bool boxed = false, float offset = 0)
	    {
		    float taken = 0;
		    if (width != 0f)
		    {
			     taken -= Mathf.Max(((1f / width) - 1f) * 3f);
		    }
		    if (boxed) taken -= 6f;
		    return new[] { GUILayout.Width((EditorGUIUtility.currentViewWidth - 22f + taken) * width + offset) };
	    }

	    public static void LabelAccuracy(float range, int bits, string suffix, GUILayoutOption[] guiLayoutOption)
	    {
		    using (new GUILayout.VerticalScope())
		    {
			    float accuracy = range / (1L << bits);
			    if (suffix == "m" && accuracy < 0.001)
			    {
				    GUILayout.Label("<0.001" + suffix, guiLayoutOption);
				    GUILayout.Label("<0.04in", guiLayoutOption);
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
    }
}
#endif
