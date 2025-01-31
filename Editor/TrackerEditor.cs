﻿#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.OpenVR;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Valve.VR;
using VRC.SDKBase;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace hackebein.objecttracking
{
    [CustomEditor(typeof(Tracker))]
    public class TrackerEditor : Editor
    {
        private string GetTrackedDeviceString(uint deviceIndex, ETrackedDeviceProperty prop)
        {
            ETrackedPropertyError error = ETrackedPropertyError.TrackedProp_Success;
            uint capacity = OpenVR.System.GetStringTrackedDeviceProperty(deviceIndex, prop, null, 0, ref error);
        
            // If error occurs or no capacity, return an empty string
            if (capacity == 0)
            {
                return string.Empty;
            }

            System.Text.StringBuilder result = new System.Text.StringBuilder((int)capacity);
            OpenVR.System.GetStringTrackedDeviceProperty(deviceIndex, prop, result, capacity, ref error);
        
            return result.ToString();
        }
        
        public override void OnInspectorGUI()
        {
            var tracker = (Tracker)target;
            var halfWidth = utility.UnityHelper.RelativeWidth((float)1/2, false);
            var quarterWidth = utility.UnityHelper.RelativeWidth((float)1/4, false);
            
            // basic checks
            if (tracker.transform.parent == null)
            {
                EditorGUILayout.HelpBox("Parent Object must be Hackebein's Object Tracking Base Component", MessageType.Error);
                return;
            }
            Base baseComponent = tracker.transform.parent.GetComponent<Base>();
            if (baseComponent == null)
            {
                var avatarDescriptor = tracker.transform.parent.GetComponent<VRCAvatarDescriptor>();
                if (avatarDescriptor == null)
                {
                    EditorGUILayout.HelpBox("Parent Object must be Hackebein's Object Tracking Base Component", MessageType.Error);
                    return;
                }

                baseComponent = avatarDescriptor.GetComponentInChildren<Base>();
                if (baseComponent == null)
                {
                    var ObjectTrackingGameObject = Utility.FindOrCreateEmptyGameObject("ObjectTracking", avatarDescriptor.gameObject);
                    baseComponent = ObjectTrackingGameObject.AddComponent<Base>();
                    tracker.transform.parent = ObjectTrackingGameObject.transform;
                }
                else
                {
                    tracker.transform.parent = baseComponent.gameObject.transform;                    
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                GUI.backgroundColor = tracker.tag == "Untagged" ? Color.green : Color.white;
                if (GUILayout.Button(tracker.tag == "Untagged" ? "Active" : "Enable", halfWidth))
                {
                    tracker.tag = tracker.tag == "Untagged" ? "EditorOnly" : "Untagged";
                }

                GUI.backgroundColor = Color.white; // Reset to default color

                if (GUILayout.Button("Remove", halfWidth))
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

            var foldoutInfo = false;
            using (new GUILayout.HorizontalScope())
            {
                foldoutInfo = utility.UnityHelper.Foldout("Hackebein.ObjectTracking.TrackerEditor.TrackerInfoFoldout", "Tracker Identifier", true);
                //GUILayout.Label("Tracker Identifier", halfWidth);
                tracker.settings.identifier = EditorGUILayout.TextField(tracker.settings.identifier, halfWidth);
            }
            GUILayout.Space(2); // Somehow this is needed to prevent overlapping
            
            var availableIdentifiers = steamvr.TrackedDevices.List.Select(device => device.identifier).ToArray();
            if (!availableIdentifiers.Contains(tracker.settings.identifier))
            {
                availableIdentifiers = availableIdentifiers.Append(tracker.settings.identifier).ToArray();
            }
            if (availableIdentifiers.Length > 1)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("", halfWidth);
                    tracker.settings.identifier =
                        availableIdentifiers[EditorGUILayout.Popup("", Array.IndexOf(availableIdentifiers, tracker.settings.identifier), availableIdentifiers, halfWidth)];
                }
            }
            if (tracker.device == null)
            {
                
                if (string.IsNullOrEmpty(tracker.settings.identifier))
                {
                    EditorGUILayout.HelpBox("Please enter a valid identifier", MessageType.Error);
                }
            }
            if (foldoutInfo)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Serial Number", halfWidth);
                    GUI.enabled = false;
                    EditorGUILayout.TextField(tracker.device != null ? tracker.device.serialNumber : "", halfWidth);
                    GUI.enabled = true;
                }
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Model Number", halfWidth);
                    GUI.enabled = false;
                    EditorGUILayout.TextField(tracker.device != null ? tracker.device.modelNumber : "", halfWidth);
                    GUI.enabled = true;
                }
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Manufacturer Name", halfWidth);
                    GUI.enabled = false;
                    EditorGUILayout.TextField(tracker.device != null ? tracker.device.manufacturerName : "", halfWidth);
                    GUI.enabled = true;
                }
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Tracking System Name", halfWidth);
                    GUI.enabled = false;
                    EditorGUILayout.TextField(tracker.device != null ? tracker.device.trackingSystemName : "", halfWidth);
                    GUI.enabled = true;
                }
                GUILayout.Space(5);
            }
            using (new GUILayout.HorizontalScope())
            {
                // TODO: info 0.00 (0%) - 1.00 (100%)
                GUILayout.Label("Position Damping (local/remote) | 0.00 (0%) - 1.00 (100%)", halfWidth);
                tracker.settings.PositionDampingLocal = utility.Input.RangeNumberField(tracker.settings.PositionDampingLocal, 0f, 1f, quarterWidth);
                //tracker.settings.PositionDampingRemote = utility.Input.RangeNumberField(tracker.settings.PositionDampingRemote, 0f, 1f, quarterWidth);
            }

            using (new GUILayout.HorizontalScope())
            {
                // TODO: info 0.00 (0%) - 1.00 (100%)
                GUILayout.Label("Rotation Damping (local/remote) | 0.00 (0%) - 1.00 (100%)", halfWidth);
                tracker.settings.RotationDampingLocal = utility.Input.RangeNumberField(tracker.settings.RotationDampingLocal, 0f, 1f, quarterWidth);
                //tracker.settings.RotationDampingRemote = utility.Input.RangeNumberField(tracker.settings.RotationDampingRemote, 0f, 1f, quarterWidth);
            }
                
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Hide Beyond Limits", halfWidth);
                tracker.settings.hideBeyondLimits = EditorGUILayout.Toggle(tracker.settings.hideBeyondLimits, halfWidth);
            }
            
            InspectorGuiAxeGroup(tracker.settings.axes.Position, "Position", 1f, "m");
            InspectorGuiAxeGroup(tracker.settings.axes.Rotation, "Rotation", 1f, "°");

            if (tracker.showDebugView)
            {
                if (utility.UnityHelper.Foldout("Hackebein.ObjectTracking.TrackerEditor.DebugFoldout", "Debug Settings"))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Show Possible Local Positions", halfWidth);
                        tracker.showPossibleLocalPositions = EditorGUILayout.Toggle(tracker.showPossibleLocalPositions, halfWidth);
                    }
                    
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Show Possible Remote Positions", halfWidth);
                        tracker.showPossibleRemotePositions = EditorGUILayout.Toggle(tracker.showPossibleRemotePositions, halfWidth);
                    }
                }
            }
        }

        private void InspectorGuiAxeGroup(AxeGroup axeGroup, string name, float baseWith, string suffix)
        {
            var tracker = (Tracker)target;
            var witdth = (float)(baseWith / 8);
            var options = utility.UnityHelper.RelativeWidth(witdth, false);
            GUILayout.Label(name);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("", options);
                GUILayout.Label("<color=yellow>Min Local</color>", new GUIStyle() { richText = true }, options);
                GUILayout.Label("<color=yellow>Max Local</color>", new GUIStyle() { richText = true }, options);
                GUILayout.Label("<color=yellow>Accuracy Local</color>", new GUIStyle() { richText = true }, options);
                GUILayout.Label("Bits", options);
                GUILayout.Label("<color=red>Min Remote</color>", new GUIStyle() { richText = true }, options);
                GUILayout.Label("<color=red>Max Remote</color>", new GUIStyle() { richText = true }, options);
                GUILayout.Label("<color=red>Accuracy Remote</color>", new GUIStyle() { richText = true }, options);
            }
            InspectorGuiAxe(axeGroup.X, "X", witdth, suffix);
            InspectorGuiAxe(axeGroup.Y, "Y", witdth, suffix);
            InspectorGuiAxe(axeGroup.Z, "Z", witdth, suffix);
        }
        
        private void InspectorGuiAxe(Axe axe, string name, float baseWith, string suffix)
        {
            var tracker = (Tracker)target;
            var options = utility.UnityHelper.RelativeWidth(baseWith, false);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(name, options);
                axe.Local.ValueMin = utility.Input.RangeNumberField(axe.Local.ValueMin, axe.Local.ValueLimitMin, axe.Local.ValueLimitMax, options);
                axe.Local.ValueMax = utility.Input.RangeNumberField(axe.Local.ValueMax, axe.Local.ValueLimitMin, axe.Local.ValueLimitMax, options);
                utility.UnityHelper.LabelAccuracy(axe.Local.ValueMax - axe.Local.ValueMin, axe.Local.Bits, suffix, options);
                axe.Remote.Bits = utility.Input.RangeNumberField(axe.Remote.Bits, axe.Remote.BitsLimitMin, axe.Remote.BitsLimitMax, options);
                axe.Remote.ValueMin = utility.Input.RangeNumberField(axe.Remote.ValueMin, axe.Remote.ValueLimitMin, axe.Remote.ValueLimitMax, options);
                axe.Remote.ValueMax = utility.Input.RangeNumberField(axe.Remote.ValueMax, axe.Remote.ValueLimitMin, axe.Remote.ValueLimitMax, options);
                utility.UnityHelper.LabelAccuracy(axe.Remote.ValueMax - axe.Remote.ValueMin, axe.Remote.Bits, suffix, options);
            }
        }
    }
}
#endif
