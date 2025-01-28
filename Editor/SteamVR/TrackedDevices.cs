using System;
using System.Collections.Generic;
using UnityEngine;

#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
using System.Linq;
using Unity.XR.OpenVR;
using UnityEditor;
using UnityEditor.Animations;
using Object = UnityEngine.Object;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Valve.VR;
#endif

namespace hackebein.objecttracking.steamvr
{
    [Serializable]
    public class TrackedDevice
    {
        private string _identifier;

        public string identifier
        {
            get => _identifier;
            set
            {
                _identifier = value;
#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
                Update();
#endif
            }
        }
        public string serialNumber { get; private set; }
        public string modelNumber { get; private set; }
        public string manufacturerName { get; private set; }
        public string trackingSystemName { get; private set; }
        public Vector3 position { get; }
        public Vector3 rotation { get; }

        public TrackedDevice() : this("", "", "", "", "", Vector3.zero, Vector3.zero) { }
        public TrackedDevice(string identifier, string serialNumber, string modelNumber, string manufacturerName, string trackingSystemName, Vector3 position, Vector3 rotation)
        {
            this._identifier = identifier;
            this.serialNumber = serialNumber;
            this.modelNumber = modelNumber;
            this.manufacturerName = manufacturerName;
            this.trackingSystemName = trackingSystemName;
            this.position = position;
            this.rotation = rotation;
        }
#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
        public void Update()
        {
            TrackedDevices.Update().ForEach(device =>
            {
                if (device.identifier == identifier)
                {
                    serialNumber = device.serialNumber;
                    modelNumber = device.modelNumber;
                    manufacturerName = device.manufacturerName;
                    trackingSystemName = device.trackingSystemName;
                }
            });
        }
#endif
    }
    
    [Serializable]
    public static class TrackedDevices
    {
        public static bool allowConnectingToSteamVR = true;
        public static List<TrackedDevice> List = new List<TrackedDevice>();
#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
        static TrackedDevices()
        {
            Update();
        }
	    private static string GetTrackedDeviceString(uint deviceIndex, ETrackedDeviceProperty prop)
	    {
	        ETrackedPropertyError error = ETrackedPropertyError.TrackedProp_Success;
	        uint size = OpenVR.System.GetStringTrackedDeviceProperty(deviceIndex, prop, null, 0, ref error);
            if (size == 0)
        	{
	            return string.Empty;
	        }

    	    System.Text.StringBuilder result = new System.Text.StringBuilder((int)size);
	        OpenVR.System.GetStringTrackedDeviceProperty(deviceIndex, prop, result, size, ref error);
    
        	return result.ToString();
    	}
        
        public static CVRSystem CheckSystem()
        {
            if (!allowConnectingToSteamVR) return null;
            
            CVRSystem system = OpenVR.System;
            if (system == null)
            {
                EVRInitError peError = EVRInitError.None;
                OpenVR.Init(ref peError, EVRApplicationType.VRApplication_Other, "");
                if (peError == EVRInitError.Init_HmdNotFound)
                {
                    Debug.LogWarning("[Hackebein's Object Tracking] SteamVR is not running!");
                    allowConnectingToSteamVR = false;
                }
                else if (peError != EVRInitError.None)
                    EditorUtility.DisplayDialog("[Hackebein's Object Tracking] SteamVR/OpenVR: Unknown Error", "Error: " + peError, "OK");
            }
            return system;
        }
        
        public static List<TrackedDevice> Update()
        {
            // We'll build a new list from the Python-like logic
            List = new List<TrackedDevice>();
        
            if (CheckSystem() != null)
            {
                var poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
                OpenVR.System.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, poses);
        
                // Store original meta (Model, Manufacturer, etc.) keyed by serial
                var metaData = new Dictionary<string, (string serialNumber, string modelNumber, string manufacturerName, string trackingSystemName)>();
                // Raw transforms
                Dictionary<string, Matrix4x4> trackingRefsRaw = new Dictionary<string, Matrix4x4>();
                Dictionary<string, Matrix4x4> trackingObjsRaw = new Dictionary<string, Matrix4x4>();
                Matrix4x4? hmdRaw = null;
        
                // Collect Python-like data
                for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
                {
                    if (!poses[i].bPoseIsValid || poses[i].eTrackingResult != ETrackingResult.Running_OK)
                        continue;
        
                    var devClass = OpenVR.System.GetTrackedDeviceClass(i);
                    if (devClass == ETrackedDeviceClass.Invalid) 
                        continue;
        
                    // Convert to Matrix4x4
                    var poseMatrix = poses[i].mDeviceToAbsoluteTracking;
                    Matrix4x4 mat44 = ConvertMatrix34ToMatrix44(poseMatrix);
        
                    // Grab string properties
                    string identifier = GetTrackedDeviceString(i, ETrackedDeviceProperty.Prop_SerialNumber_String);
                    string serialNumber = GetTrackedDeviceString(i, ETrackedDeviceProperty.Prop_SerialNumber_String);
                    string modelNumber = GetTrackedDeviceString(i, ETrackedDeviceProperty.Prop_ModelNumber_String);
                    string manufacturerName = GetTrackedDeviceString(i, ETrackedDeviceProperty.Prop_ManufacturerName_String);
                    string trackingSystemName = GetTrackedDeviceString(i, ETrackedDeviceProperty.Prop_TrackingSystemName_String);
                    metaData[identifier] = (serialNumber, modelNumber, manufacturerName, trackingSystemName);
        
                    // Fill references+HMD
                    if (devClass == ETrackedDeviceClass.TrackingReference)
                        trackingRefsRaw[identifier] = mat44;
                    if (devClass == ETrackedDeviceClass.HMD)
                        hmdRaw = mat44;
        
                    // Everything in tracking objects
                    trackingObjsRaw[identifier] = mat44;
                }
        
                // compute tracking reference
                Matrix4x4 trackingRef = ComputeTrackingReferencePosition(trackingRefsRaw);
        
                // dynamic Playspace
                if (trackingRefsRaw.Count > 0)
                {
                    string[] order = trackingRefsRaw.Keys.OrderBy(k => k).ToArray();
                    var pos = trackingRef.GetColumn(3);
                    pos.y = 0f;
                    float yaw = QuaternionFromMatrix(trackingRefsRaw[order[0]]).eulerAngles.y;
                    trackingObjsRaw["PlaySpace"] = Matrix4x4.Rotate(Quaternion.Euler(0f, yaw, 0f));
                    trackingObjsRaw["PlaySpace"].SetColumn(3, pos);
                    metaData["PlaySpace"] = ("SteamVRPlayArea", "PlaySpace", "Hackebein", "openvr");
                }
                
                // zero out Y+rotation
                trackingRef = SetYAndRotationToZero(trackingRef);
        
                var trackingObjects = new Dictionary<string, Matrix4x4>();
                foreach (var kvp in trackingObjsRaw)
                    trackingObjects[kvp.Key] = RelativeMatrix(trackingRef, kvp.Value);
                
                // optional pill from HMD
                Matrix4x4 pill = Matrix4x4.identity;
                if (hmdRaw.HasValue)
                {
                    pill = RelativeMatrix(trackingRef, SetYAndXZRotationToZero(hmdRaw.Value));
                    foreach (var kvp in trackingObjects)
                    {
                        string id = kvp.Key;
                        Matrix4x4 trackerMat = kvp.Value;
        
                        Matrix4x4 pos = RelativeMatrix(pill, trackerMat);
                        //pos = RotateMatrixXZ(pos, pill);
        
                        List.Add(new TrackedDevice(
                            id,
                            metaData[id].serialNumber,
                            metaData[id].modelNumber,
                            metaData[id].manufacturerName,
                            metaData[id].trackingSystemName,
                            pos.GetColumn(3),
                            QuaternionFromMatrix(pos).eulerAngles));
                    }
                    
                }
            }
        
            return List;
        }
        
        private static Matrix4x4 ConvertMatrix34ToMatrix44(HmdMatrix34_t m)
        {
            Matrix4x4 mat = Matrix4x4.identity;
            mat.m00 = m.m0;   mat.m01 = m.m1;   mat.m02 = -m.m2;  mat.m03 = m.m3;
            mat.m10 = m.m4;   mat.m11 = m.m5;   mat.m12 = -m.m6;  mat.m13 = m.m7;
            mat.m20 = -m.m8;  mat.m21 = -m.m9;  mat.m22 = m.m10;  mat.m23 = -m.m11;
            return mat;
        }
        
        private static Matrix4x4 ComputeTrackingReferencePosition(Dictionary<string, Matrix4x4> refs)
        {
            if (refs.Count == 0) return Matrix4x4.identity;
        
            Vector3 sum = Vector3.zero;
            foreach (var mat in refs.Values)
            {
                Vector4 column = mat.GetColumn(3);
                sum += new Vector3(column.x, column.y, column.z);
            }
        
            Vector3 avg = sum / refs.Count;
            Matrix4x4 result = Matrix4x4.identity;
            result.SetColumn(3, new Vector4(avg.x, avg.y, avg.z, 1f));
            return result;
        }
        
        private static Matrix4x4 SetYAndXZRotationToZero(Matrix4x4 mat)
        {
            Vector3 pos = mat.GetColumn(3);
            pos.y = 0f;
            mat.SetColumn(3, new Vector4(pos.x, pos.y, pos.z, 1f));
        
            float theta = Mathf.Atan2(mat.m20, mat.m00);
            mat.m00 = Mathf.Cos(theta);  mat.m01 = 0f; mat.m02 = Mathf.Sin(theta);
            mat.m10 = 0f;                mat.m11 = 1f; mat.m12 = 0f;
            mat.m20 = -Mathf.Sin(theta); mat.m21 = 0f; mat.m22 = Mathf.Cos(theta);
        
            return mat;
        }
        
        private static Matrix4x4 SetYAndRotationToZero(Matrix4x4 mat)
        {
            Vector3 pos = mat.GetColumn(3);
            pos.y = 0f;
            mat.SetColumn(3, new Vector4(pos.x, pos.y, pos.z, 1f));
        
            mat.m00 = 1f; mat.m01 = 0f; mat.m02 = 0f;
            mat.m10 = 0f; mat.m11 = 1f; mat.m12 = 0f;
            mat.m20 = 0f; mat.m21 = 0f; mat.m22 = 1f;
            return mat;
        }
        
        private static Matrix4x4 RelativeMatrix(Matrix4x4 parent, Matrix4x4 child)
        {
            return parent.inverse * child;
        }
        
        private static Matrix4x4 RotateMatrixXZ(Matrix4x4 mat, Matrix4x4 pill)
        {
            Quaternion q = QuaternionFromMatrix(pill);
            float deg = q.eulerAngles.y;
            Matrix4x4 rot = Matrix4x4.Rotate(Quaternion.Euler(0f, deg, 0f));
        
            // move position
            Vector3 newPos = rot.MultiplyPoint3x4(mat.GetColumn(3));
            mat.SetColumn(3, new Vector4(newPos.x, newPos.y, newPos.z, 1f));
        
            // double multiply
            mat = rot * mat;
            return mat;
        }
        
        private static Quaternion QuaternionFromMatrix(Matrix4x4 m)
        {
            return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
        }
#endif
    }
}
