using System;
using UnityEngine;
using VRC.SDKBase;
using hackebein.objecttracking.steamvr;

#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.Animations;
using Object = UnityEngine.Object;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Dynamics.Constraint.Components;
using hackebein.objecttracking.utility;
using UnityEngine.iOS;
#endif

namespace hackebein.objecttracking
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [Serializable]
    [AddComponentMenu("Hackebein/Hackebein's Object Tracking Tracker")]
    public class Tracker : MonoBehaviour, IEditorOnly
    {
        [Serializable]
        public class Settings
        {
            public string identifier;
            public Axes axes = new Axes();
            public float PositionDampingLocal = 0.05f;
            public float PositionDampingRemote = 0.05f;
            public float RotationDampingLocal = 0.03f;
            public float RotationDampingRemote = 0.03f;
            public bool hideBeyondLimits = true;
        }
        public Settings settings = new Settings();
        public TrackedDevice device
        {
            get => steamvr.TrackedDevices.List.Find(d => d.identifier == settings.identifier);
        }
#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
        public bool showDebugView
        {
            get => EditorPrefs.GetBool("Hackebein.ObjectTracking.ShowDebugView", false);
            set => EditorPrefs.SetBool("Hackebein.ObjectTracking.ShowDebugView", value);
        }
#endif
        public bool? updateContinuously = null;
        public bool showPossibleLocalPositions = true;
        public bool showPossibleRemotePositions = true;
        public Tracker(){}
#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (showPossibleLocalPositions)
            {
                DrawGizmos(Color.red, settings.axes.Position, true);
            }
            if (showPossibleRemotePositions)
            {
                DrawGizmos(Color.yellow, settings.axes.Position, false);
            }
        }

        void OnRenderObject()
        {
            gameObject.transform.localPosition = new Vector3(
                settings.axes.Position.X.Local.Bits > 0 ? gameObject.transform.localPosition.x : 0,
                settings.axes.Position.Y.Local.Bits > 0 ? gameObject.transform.localPosition.y : 0,
                settings.axes.Position.Z.Local.Bits > 0 ? gameObject.transform.localPosition.z : 0
            );
            gameObject.transform.localRotation = Quaternion.Euler(
                settings.axes.Rotation.X.Local.Bits > 0 ? gameObject.transform.localRotation.eulerAngles.x : 0,
                settings.axes.Rotation.Y.Local.Bits > 0 ? gameObject.transform.localRotation.eulerAngles.y : 0,
                settings.axes.Rotation.Z.Local.Bits > 0 ? gameObject.transform.localRotation.eulerAngles.z : 0
            );
            
            if (gameObject.transform.parent == null)
            {
                gameObject.transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                var baseComponent = gameObject.transform.parent.GetComponent<Base>();
                if (baseComponent != null)
                {
                    gameObject.transform.localScale = baseComponent.GetScaleVector();
                }
            }
        }
        
        private void DrawGizmos(Color color, AxeGroup axeGroup, bool local)
        {
            Gizmos.color = color;
            var X = local ? axeGroup.X.Local : axeGroup.X.Remote;
            var Y = local ? axeGroup.Y.Local : axeGroup.Y.Remote;
            var Z = local ? axeGroup.Z.Local : axeGroup.Z.Remote;
            var min = new Vector3(X.ValueMin, Y.ValueMin, Z.ValueMin);
            var max = new Vector3(X.ValueMax, Y.ValueMax, Z.ValueMax);
            var scale = gameObject.transform.localScale;
            min.Scale(scale);
            max.Scale(scale);
            var size = max - min;
            if (gameObject.transform.parent != null)
            {
                var origin = gameObject.transform.parent.transform.position;
                var start = origin + min;
                var center = start + size / 2;
                Gizmos.DrawWireCube(center, size);
            }
        }  
#endif
    }
}