#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
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
            
            var avatar = tracker.transform.parent.GetComponent<VRCAvatarDescriptor>();
            // basic checks
            if(avatar == null)
            {
                EditorGUILayout.HelpBox("Parent Object must be Avatar root", MessageType.Error);
                return;
            }

            Base baseComponent = avatar.GetComponentInChildren<Base>();
            if(baseComponent == null)
            {
                var ObjectTrackingGameObject = Utility.FindOrCreateEmptyGameObject("ObjectTracking", tracker.transform.parent.gameObject);
                baseComponent = ObjectTrackingGameObject.AddComponent<Base>();
            }
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Base Component", halfWidth);
                GUI.enabled = false;
                EditorGUILayout.ObjectField(baseComponent, typeof(Base), true, halfWidth);
                GUI.enabled = true;
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
                    Utility.ResetGameObject(tracker.gameObject);
                }
            }
            
            if (string.IsNullOrEmpty(tracker.device.serialNumber) && string.IsNullOrEmpty(tracker.device.modelNumber) && string.IsNullOrEmpty(tracker.device.manufacturerName) && string.IsNullOrEmpty(tracker.device.trackingSystemName))
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Tracker identifier", halfWidth);
                    tracker.device.identifier = EditorGUILayout.TextField(tracker.device.identifier, halfWidth);
                }
                
                if (string.IsNullOrEmpty(tracker.device.identifier))
                {
                    EditorGUILayout.HelpBox("Please enter a valid identifier", MessageType.Error);
                    GUI.enabled = false;
                }
                
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Update Meta Data", halfWidth))
                    {
                        tracker.device.Update();
                    }
                }

                GUI.enabled = true;
            }
            else
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Serial Number", halfWidth);
                    GUI.enabled = false;
                    EditorGUILayout.TextField(tracker.device.serialNumber, halfWidth);
                    GUI.enabled = true;
                }
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Model Number", halfWidth);
                    GUI.enabled = false;
                    EditorGUILayout.TextField(tracker.device.modelNumber, halfWidth);
                    GUI.enabled = true;
                }
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Manufacturer Name", halfWidth);
                    GUI.enabled = false;
                    EditorGUILayout.TextField(tracker.device.manufacturerName, halfWidth);
                    GUI.enabled = true;
                }
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Tracking System Name", halfWidth);
                    GUI.enabled = false;
                    EditorGUILayout.TextField(tracker.device.trackingSystemName, halfWidth);
                    GUI.enabled = true;
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                // TODO: info 0.00 (0%) - 1.00 (100%)
                GUILayout.Label("Position Damping", halfWidth);
                tracker.settings.PositionDamping = utility.Input.RangeNumberField(tracker.settings.PositionDamping, 0f, 1f, halfWidth);
            }

            using (new GUILayout.HorizontalScope())
            {
                // TODO: info 0.00 (0%) - 1.00 (100%)
                GUILayout.Label("Rotation Damping", halfWidth);
                tracker.settings.RotationDamping = utility.Input.RangeNumberField(tracker.settings.RotationDamping, 0f, 1f, halfWidth);
            }
                
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Hide Beyond Limits", halfWidth);
                tracker.settings.hideBeyondLimits = EditorGUILayout.Toggle(tracker.settings.hideBeyondLimits, halfWidth);
            }
                
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Update In Edit Mode", halfWidth);
                tracker.updateInEditMode = EditorGUILayout.Toggle(tracker.updateInEditMode, halfWidth);
            }
            
            InspectorGuiAxeGroup(tracker.settings.axes.Position, "Position", 1f);
            InspectorGuiAxeGroup(tracker.settings.axes.Rotation, "Rotation", 1f);

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

        private void InspectorGuiAxeGroup(AxeGroup axeGroup, string name, float baseWith)
        {
            var tracker = (Tracker)target;
            var witdth = (float)(baseWith / (tracker.showDebugView ? 5 : 3));
            var options = utility.UnityHelper.RelativeWidth(witdth, false);
            GUILayout.Label(name);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Bits", options);
                if (tracker.showDebugView)
                {
                    GUILayout.Label("Min Local", options);
                    GUILayout.Label("Max Local", options);
                }
                GUILayout.Label("Min Remote", options);
                GUILayout.Label("Max Remote", options);
            }
            InspectorGuiAxe(axeGroup.X, "X", witdth);
            InspectorGuiAxe(axeGroup.Y, "Y", witdth);
            InspectorGuiAxe(axeGroup.Z, "Z", witdth);
        }
        
        private void InspectorGuiAxe(Axe axe, string name, float baseWith)
        {
            var tracker = (Tracker)target;
            var witdth = (float)(baseWith);
            var options = utility.UnityHelper.RelativeWidth(witdth, false);
            using (new GUILayout.HorizontalScope())
            {
                //GUILayout.Label(name);
                axe.Remote.Bits = utility.Input.RangeNumberField(axe.Remote.Bits, axe.Remote.BitsLimitMin, axe.Remote.BitsLimitMax, options);
                if (tracker.showDebugView)
                {
                    axe.Local.ValueMin = utility.Input.RangeNumberField(axe.Local.ValueMin, axe.Local.ValueLimitMin, axe.Local.ValueLimitMax, options);
                    axe.Local.ValueMax = utility.Input.RangeNumberField(axe.Local.ValueMax, axe.Local.ValueLimitMin, axe.Local.ValueLimitMax, options);
                }
                axe.Remote.ValueMin = utility.Input.RangeNumberField(axe.Remote.ValueMin, axe.Remote.ValueLimitMin, axe.Remote.ValueLimitMax, options);
                axe.Remote.ValueMax = utility.Input.RangeNumberField(axe.Remote.ValueMax, axe.Remote.ValueLimitMin, axe.Remote.ValueLimitMax, options);
            }
        }
    }
}
#endif
