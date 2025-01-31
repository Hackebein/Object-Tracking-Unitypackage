﻿#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.XR.OpenVR;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Valve.VR;

namespace hackebein.objecttracking
{
    [CustomEditor(typeof(Base))]
    public class BaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Base baseComponent = (Base)target;
            var fullWidth = utility.UnityHelper.RelativeWidth(1, false);
            var halfWidth = utility.UnityHelper.RelativeWidth((float)1 / 2, false);
            var quarterWidth = utility.UnityHelper.RelativeWidth((float)1 / 4, false);
            var width4of6 = utility.UnityHelper.RelativeWidth((float)4 / 6, false);
            var width2of6 = utility.UnityHelper.RelativeWidth((float)2 / 6, false);
            var width1of6 = utility.UnityHelper.RelativeWidth((float)1 / 6, false);
            
            // basic checks
            if (baseComponent.transform.parent == null)
            {
                EditorGUILayout.HelpBox("Parent Object must be Avatar root", MessageType.Error);
                return;
            }
            if(baseComponent.transform.parent.GetComponent<VRCAvatarDescriptor>() == null)
            {
                EditorGUILayout.HelpBox("Parent Object must be Avatar root", MessageType.Error);
                return;
            }
            foreach (Transform child in baseComponent.transform)
            {
                if (child.name == "_")
                {
                    EditorGUILayout.HelpBox("Child GameObject named \"_\" detected. This name is reserved for internal use.", MessageType.Warning);
                }
                else if (child.GetComponent<Tracker>() == null)
                {
                    EditorGUILayout.HelpBox("Child GameObject named \"" + child.name + "\" is missing a Tracker component.", MessageType.Error);
                }
            }
            var avatar = baseComponent.transform.parent.GetComponent<VRCAvatarDescriptor>();
            if (avatar.expressionsMenu != null && avatar.expressionsMenu.controls.Count >= (baseComponent.settings.addDebugMenu ? 7 : 8))
            {
                EditorGUILayout.HelpBox("No Space in your Expression Menu. Upload might fail.", MessageType.Warning);
            }
            if (steamvr.TrackedDevices.CheckSystem() == null)
            {
                EditorGUILayout.HelpBox("Start SteamVR, to load Tracker informations", MessageType.Warning);
                if (GUILayout.Button("Retry"))
                {
                    baseComponent.updateInEditMode = true;
                }
            }
            else
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Playspace", halfWidth);
                    if (GUILayout.Button("Generate (missing) Playspace", halfWidth))
                    {
                        baseComponent.GeneratePlayspace();
                        if (baseComponent.settings.addDebugMenu)
                        {
                            EditorPrefs.SetBool("Hackebein.ObjectTracking.ShowDebugView", true);
                        }
                    }
                }
            }
            
            SceneView sceneView = SceneView.lastActiveSceneView;

            if (sceneView != null)
            {
                BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
                var sceneViewStateField = typeof(SceneView).GetField("m_SceneViewState", bindingFlags);
                if (sceneViewStateField != null)
                {
                    var sceneViewState = sceneViewStateField.GetValue(sceneView);
                    if (sceneViewState != null)
                    {
                        var alwaysRefreshField = sceneViewState.GetType().GetField("m_AlwaysRefresh", bindingFlags);
                        if (alwaysRefreshField != null)
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                GUILayout.Label("In Editor FPS", halfWidth);
                                bool currentValue = (bool)alwaysRefreshField.GetValue(sceneViewState);
                                bool newValue = currentValue;
                                GUI.backgroundColor = !currentValue ? Color.green : Color.white;
                                if (GUILayout.Button("Limited", quarterWidth))
                                {
                                    alwaysRefreshField.SetValue(sceneViewState, !currentValue);
                                    sceneView.Repaint();
                                }
                                GUI.backgroundColor = currentValue ? Color.green : Color.white;
                                if (GUILayout.Button("Unlimted", quarterWidth))
                                {
                                    alwaysRefreshField.SetValue(sceneViewState, !currentValue);
                                    sceneView.Repaint();
                                }
                                GUI.backgroundColor = Color.white; // Reset to default color
                            }
                        }
                    }
                }
            }
                    
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Add Menu", halfWidth);
                baseComponent.settings.addMenu = EditorGUILayout.Toggle(baseComponent.settings.addMenu, halfWidth);
            }

            var trackersFoldout = utility.UnityHelper.Foldout("Hackebein.ObjectTracking.BaseEditor.TrackersFoldout", "Trackers", true);
            if (baseComponent.GetTrackers().Length == 0)
            {
                EditorGUILayout.HelpBox("No active Trackers detected in the scene. Object Tracking will be disabled if no trackers are present on upload.", MessageType.Warning);
            }
            if (trackersFoldout)
            {
                foreach (var tracker in baseComponent.GetTrackers(true))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUI.enabled = false;
                        EditorGUILayout.ObjectField(tracker, typeof(Tracker), true, halfWidth);
                        GUI.enabled = true;
                        if (string.IsNullOrEmpty(tracker.settings.identifier) || baseComponent.settings.ignoreTrackeridentifiers.Contains(tracker.settings.identifier))
                        {
                            GUI.enabled = false;
                        }
                        if (GUILayout.Button("Ignore", width1of6))
                        {
                            baseComponent.settings.ignoreTrackeridentifiers =
                                baseComponent.settings.ignoreTrackeridentifiers.Append(tracker.settings.identifier).ToArray();
                        }
                        GUI.enabled = true;

                        GUI.backgroundColor = tracker.tag == "Untagged" && !string.IsNullOrEmpty(tracker.settings.identifier) ? Color.green : Color.white;
                        if (GUILayout.Button(tracker.tag == "Untagged" ? "Active" : "Enable", width1of6))
                        {
                            if (string.IsNullOrEmpty(tracker.settings.identifier))
                            {
                                // TODO: this shouldn't be necessary, neither should it be here
                                Selection.activeObject = tracker;
                            }
                            else
                            {
                                tracker.tag = tracker.tag == "Untagged" ? "EditorOnly" : "Untagged";
                            }
                        }
                        GUI.backgroundColor = Color.white; // Reset to default color
                        
                        if (GUILayout.Button("Remove", width1of6))
                        {
                            var child = tracker.transform.Find(tracker.settings.identifier);
                            if (child != null)
                            {
                                DestroyImmediate(child.gameObject);
                            }
                            if (tracker.transform.childCount == 0)
                            {
                                DestroyImmediate(tracker.gameObject);
                            }
                            else
                            {
                                Utility.ResetGameObject(tracker.gameObject);
                            }
                        }
                    }
                    
                    if (steamvr.TrackedDevices.CheckSystem() != null)
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            var availableIdentifiers = steamvr.TrackedDevices.List.Select(device => device.identifier).ToArray();
                            if (!availableIdentifiers.Contains(tracker.settings.identifier))
                            {
                                availableIdentifiers = availableIdentifiers.Append(tracker.settings.identifier).ToArray();
                            }
                            if (availableIdentifiers.Length > 1)
                            {
                                tracker.settings.identifier =
                                    availableIdentifiers[EditorGUILayout.Popup("", Array.IndexOf(availableIdentifiers, tracker.settings.identifier), availableIdentifiers, halfWidth)];
                            }
                            else
                            {
                                GUILayout.Label(tracker.settings.identifier, halfWidth);
                            }
                            
                            if (string.IsNullOrEmpty(tracker.settings.identifier))
                            {
                                GUI.enabled = false;
                            }
                            if (tracker.updateContinuously != null)
                            {
                                GUI.enabled = false;
                            }
                            if (GUILayout.Button("Update Once", quarterWidth))
                            {
                                tracker.updateContinuously = false;
                            }
                            GUI.enabled = true;
                            if (string.IsNullOrEmpty(tracker.settings.identifier))
                            {
                                GUI.enabled = false;
                            }
                            GUI.backgroundColor = tracker.updateContinuously == true ? Color.green : Color.white;
                            if (GUILayout.Button("Update Continuously", quarterWidth))
                            {
                                tracker.updateContinuously = (tracker.updateContinuously == true) ? null : true;
                            }
                            GUI.enabled = true;
                            GUI.backgroundColor = Color.white; // Reset to default color
                        }
                        GUILayout.Space(5);
                    }
                }
            }

            if (utility.UnityHelper.Foldout("Hackebein.ObjectTracking.BaseEditor.IgnoredTrackersFoldout", "Ignore Trackers", false))
            {
                foreach (var identifier in baseComponent.settings.ignoreTrackeridentifiers)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        // Show as disabled if found in baseComponent.GetTrackers()[].settings.identifier
                        GUILayout.Label(identifier, halfWidth);
                        if (GUILayout.Button("Remove", halfWidth))
                        {
                            baseComponent.settings.ignoreTrackeridentifiers = baseComponent.settings.ignoreTrackeridentifiers.Where(id => id != identifier).ToArray();
                        }
                    }
                }
            }

            if (baseComponent.showDebugView)
            {
                if (utility.UnityHelper.Foldout("Hackebein.ObjectTracking.BaseEditor.DebugFoldout", "Debug Settings", false))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Show Debug View", halfWidth);
                        baseComponent.showDebugView = EditorGUILayout.Toggle(baseComponent.showDebugView, halfWidth);
                    }
                    
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Add Debug Menu", halfWidth);
                        baseComponent.settings.addDebugMenu = EditorGUILayout.Toggle(baseComponent.settings.addDebugMenu, halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Player Height", halfWidth);
                        EditorGUILayout.Popup(vrchat.PlayerHeights.GetCurrentHeight().Index, vrchat.PlayerHeights.List.Select(p => p.DisplayText).ToArray(), halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Add Stabilization", halfWidth);
                        baseComponent.settings.addStabilization =
                            EditorGUILayout.Toggle(baseComponent.settings.addStabilization, halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Add Lazy Stabilization", halfWidth);
                        baseComponent.settings.addLazyStabilization = EditorGUILayout.Toggle(baseComponent.settings.addLazyStabilization, halfWidth);
                    }
                    
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Prediction (local/remote)", halfWidth);
                        baseComponent.settings.predictionLocal = EditorGUILayout.FloatField(baseComponent.settings.predictionLocal, quarterWidth);
                        baseComponent.settings.predictionRemote = EditorGUILayout.FloatField(baseComponent.settings.predictionRemote, halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Magic Number", halfWidth);
                        Base.magicNumber = EditorGUILayout.FloatField(Base.magicNumber, halfWidth);
                    }
                    
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Default Child GameObject", halfWidth);
                        Utility.DefaultChildGameObject = EditorGUILayout.ObjectField(Utility.DefaultChildGameObject, typeof(GameObject), false, halfWidth) as GameObject;
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Default Min Bits Per Position Axis On Local", halfWidth);
                        Axes.DefaultMinBitsPerPositionAxisOnLocal = EditorGUILayout.IntField(Axes.DefaultMinBitsPerPositionAxisOnLocal, halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Default Max Bits Per Position Axis On Local", halfWidth);
                        Axes.DefaultMaxBitsPerPositionAxisOnLocal = EditorGUILayout.IntField(Axes.DefaultMaxBitsPerPositionAxisOnLocal, halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Default Bits Per Position Axis On Local", halfWidth);
                        Axes.DefaultBitsPerPositionAxisOnLocal = EditorGUILayout.IntField(Axes.DefaultBitsPerPositionAxisOnLocal, halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Default Min Position Axis On Local", halfWidth);
                        Axes.DefaultMinPositionAxisOnLocal = EditorGUILayout.FloatField(Axes.DefaultMinPositionAxisOnLocal, halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Default Max Position Axis On Local", halfWidth);
                        Axes.DefaultMaxPositionAxisOnLocal = EditorGUILayout.FloatField(Axes.DefaultMaxPositionAxisOnLocal, halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Default Min Bits Per Rotation Axis On Local", halfWidth);
                        Axes.DefaultMinBitsPerRotationAxisOnLocal = EditorGUILayout.IntField(Axes.DefaultMinBitsPerRotationAxisOnLocal, halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Default Max Bits Per Rotation Axis On Local", halfWidth);
                        Axes.DefaultMaxBitsPerRotationAxisOnLocal = EditorGUILayout.IntField(Axes.DefaultMaxBitsPerRotationAxisOnLocal, halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Default Bits Per Rotation Axis On Local", halfWidth);
                        Axes.DefaultBitsPerRotationAxisOnLocal = EditorGUILayout.IntField(Axes.DefaultBitsPerRotationAxisOnLocal, halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Default Min Rotation Axis On Local", halfWidth);
                        Axes.DefaultMinRotationAxisOnLocal = EditorGUILayout.FloatField(Axes.DefaultMinRotationAxisOnLocal, halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Default Max Rotation Axis On Local", halfWidth);
                        Axes.DefaultMaxRotationAxisOnLocal = EditorGUILayout.FloatField(Axes.DefaultMaxRotationAxisOnLocal, halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Default Min Bits Per Position Axis On Remote", halfWidth);
                        Axes.DefaultMinBitsPerPositionAxisOnRemote = EditorGUILayout.IntField(Axes.DefaultMinBitsPerPositionAxisOnRemote, halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Default Max Bits Per Position Axis On Remote", halfWidth);
                        Axes.DefaultMaxBitsPerPositionAxisOnRemote = EditorGUILayout.IntField(Axes.DefaultMaxBitsPerPositionAxisOnRemote, halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Default Bits Per Position Axis On Remote", halfWidth);
                        Axes.DefaultBitsPerPositionAxisOnRemote = EditorGUILayout.IntField(Axes.DefaultBitsPerPositionAxisOnRemote, halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Default Min Position Axis On Remote", halfWidth);
                        Axes.DefaultMinPositionAxisOnRemote = EditorGUILayout.FloatField(Axes.DefaultMinPositionAxisOnRemote, halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Default Max Position Axis On Remote", halfWidth);
                        Axes.DefaultMaxPositionAxisOnRemote = EditorGUILayout.FloatField(Axes.DefaultMaxPositionAxisOnRemote, halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Default Min Bits Per Rotation Axis On Remote", halfWidth);
                        Axes.DefaultMinBitsPerRotationAxisOnRemote = EditorGUILayout.IntField(Axes.DefaultMinBitsPerRotationAxisOnRemote, halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Default Max Bits Per Rotation Axis On Remote", halfWidth);
                        Axes.DefaultMaxBitsPerRotationAxisOnRemote = EditorGUILayout.IntField(Axes.DefaultMaxBitsPerRotationAxisOnRemote, halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Default Bits Per Rotation Axis On Remote", halfWidth);
                        Axes.DefaultBitsPerRotationAxisOnRemote = EditorGUILayout.IntField(Axes.DefaultBitsPerRotationAxisOnRemote, halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Default Min Rotation Axis On Remote", halfWidth);
                        Axes.DefaultMinRotationAxisOnRemote = EditorGUILayout.FloatField(Axes.DefaultMinRotationAxisOnRemote, halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Default Max Rotation Axis On Remote", halfWidth);
                        Axes.DefaultMaxRotationAxisOnRemote = EditorGUILayout.FloatField(Axes.DefaultMaxRotationAxisOnRemote, halfWidth);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Apply?", halfWidth))
                        {
                            baseComponent.Apply();
                        }

                        if (GUILayout.Button("Remove! DA)=hoawdl-.", halfWidth))
                        {
                            baseComponent.Cleanup();
                        }
                    }
                }
            }
        }
    }
}
#endif