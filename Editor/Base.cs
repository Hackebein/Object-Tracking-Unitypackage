using System;
using UnityEngine;
using VRC.SDKBase;
using Debug = UnityEngine.Debug;

#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.Animations;
using Object = UnityEngine.Object;
using UnityEngine.Rendering;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Dynamics.Constraint.Components;
using Valve.VR;
#endif

namespace hackebein.objecttracking
{
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [Serializable]
    [AddComponentMenu("Hackebein/Hackebein's Object Tracking Base Component")]
    [HelpURL("https://github.com/Hackebein/Object-Tracking-Unitypackage/blob/main/README.md")]
    public class Base : MonoBehaviour, IEditorOnly
    {
        [Serializable]
	    public class Settings
	    {
            public string assetFolder = "Assets/Hackebein/ObjectTracking/Generated";
            //TODO: generate new id on duplication
            public string uuid = Guid.NewGuid().ToString();
            public bool addLazyStabilization = true;
            public bool addStabilization = true;
            public bool addMenu = true;
            public bool addDebugMenu = false;
            public string[] ignoreTrackeridentifiers = new string[] { };
        }
        public Settings settings = new Settings();
#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
        public bool showDebugView
        {
            get => EditorPrefs.GetBool("Hackebein.ObjectTracking.ShowDebugView", false);
            set => EditorPrefs.SetBool("Hackebein.ObjectTracking.ShowDebugView", value);
        }
#endif
        public bool ignoreNewTrackers = false;

        public bool updateInEditMode
        {
            get => steamvr.TrackedDevices.allowConnectingToSteamVR;
            set => steamvr.TrackedDevices.allowConnectingToSteamVR = value;
        }
        public bool updateContinuously = false;
        public static float magicNumber = 1.074f;
        // https://sketchfab.com/3d-models/transform-gizmo-8d1edffdedda4898b3fb1c3c4c08113c
        public static readonly string gizmoPath = "Packages/hackebein.objecttracking/Prefab/GizmoUnity.fbx";
#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
        public Tracker[] GetTrackers(bool all = false)
        {
            return gameObject.transform.parent.GetComponentsInChildren<Tracker>().Where(tracker => all || (tracker.tag != "EditorOnly" && tracker.settings.identifier != "")).ToArray();
        }
        
        void OnRenderObject()
        {
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;
            if (updateContinuously)
            {
                ApplyPreview();
            }
        }

        public Vector3 GetScaleVector()
        {
            var avatarDescriptor = gameObject.transform.parent.GetComponent<VRCAvatarDescriptor>();
            if (avatarDescriptor == null)
            {
                throw new NullReferenceException("Parent GameObject must have a VRC_AvatarDescriptor component");
            }
            float scale = (avatarDescriptor.ViewPosition.y / (float)vrchat.PlayerHeights.GetCurrentHeight().CmValue * 100) * magicNumber;
            scale = 1f;
            return new Vector3(scale, scale, scale);
        }
        
        public void ApplyPreview()
        {
            // avatar descriptor
            var avatarDescriptor = gameObject.transform.parent.GetComponent<VRCAvatarDescriptor>();
            if (avatarDescriptor == null)
            {
                throw new NullReferenceException("Parent GameObject must have a VRC_AvatarDescriptor component");
            }
            
            // scale
            var scaleVector = GetScaleVector();

            // tracker
            steamvr.TrackedDevices.Update().ForEach(device =>
            {
                var isCreated = GetTrackers().Any(tracker => tracker.settings.identifier == device.identifier);
                var isIgnored = settings.ignoreTrackeridentifiers.Contains(device.identifier);
                if (isIgnored && !isCreated)
                {
                    return;
                }
                // tracker object
                GameObject trackerObject = Utility.FindOrCreateEmptyGameObject(device.identifier, gameObject.transform.parent.gameObject, new List<Type> { typeof(Tracker) });
                var isNewComponent = trackerObject.GetComponent<Tracker>() == null;
                if (isNewComponent)
                {
                    Tracker trackerComponent = trackerObject.AddComponent<Tracker>();
                    trackerComponent.settings.identifier = device.identifier;
                    trackerComponent.updateInEditMode = updateContinuously;

                    if (device.serialNumber == "SteamVRPlayArea")
                    {
                        trackerComponent.settings.axes.Position.Y.Local.Bits = 0;
                        trackerComponent.settings.axes.Position.Y.Local.ValueMin = 0f;
                        trackerComponent.settings.axes.Position.Y.Local.ValueMax = 0f;
                        trackerComponent.settings.axes.Rotation.X.Local.Bits = 0;
                        trackerComponent.settings.axes.Rotation.X.Local.ValueMin = 0f;
                        trackerComponent.settings.axes.Rotation.X.Local.ValueMax = 0f;
                        trackerComponent.settings.axes.Rotation.Z.Local.Bits = 0;
                        trackerComponent.settings.axes.Rotation.Z.Local.ValueMin = 0f;
                        trackerComponent.settings.axes.Rotation.Z.Local.ValueMax = 0f;
                        trackerComponent.settings.axes.Position.Y.Remote.Bits = 0;
                        trackerComponent.settings.axes.Position.Y.Remote.ValueMin = 0f;
                        trackerComponent.settings.axes.Position.Y.Remote.ValueMax = 0f;
                        trackerComponent.settings.axes.Rotation.X.Remote.Bits = 0;
                        trackerComponent.settings.axes.Rotation.X.Remote.ValueMin = 0f;
                        trackerComponent.settings.axes.Rotation.X.Remote.ValueMax = 0f;
                        trackerComponent.settings.axes.Rotation.Z.Remote.Bits = 0;
                        trackerComponent.settings.axes.Rotation.Z.Remote.ValueMin = 0f;
                        trackerComponent.settings.axes.Rotation.Z.Remote.ValueMax = 0f;
                        
                        /*
                        HmdQuad_t playArea = new HmdQuad_t();
                        var color = Color.cyan;
                        var chaperone = OpenVR.Chaperone;
                        bool success = (chaperone != null) && chaperone.GetPlayAreaRect(ref playArea) && chaperone.GetCalibrationState() == ChaperoneCalibrationState.OK;
                        if (!success)
                        {
                            Debug.LogError("[Hackebein's Object Tracking] Failed to retrieve the play area rectangle.");
                            return;
                        }
                        var corners = new HmdVector3_t[] { playArea.vCorners0, playArea.vCorners1, playArea.vCorners2, playArea.vCorners3 };
                        Debug.Log($"[Hackebein's Object Tracking] Play Area Corners: {corners[0]}, {corners[1]}, {corners[2]}, {corners[3]}");
                        
		                var vertices = new Vector3[corners.Length * 2];
		                for (int i = 0; i < corners.Length; i++)
		                {
			                var c = corners[i];
			                vertices[i] = new Vector3(c.v0, 0.01f, c.v2);
		                }

		                for (int i = 0; i < corners.Length; i++)
		                {
			                int next = (i + 1) % corners.Length;
			                int prev = (i + corners.Length - 1) % corners.Length;

			                var nextSegment = (vertices[next] - vertices[i]).normalized;
			                var prevSegment = (vertices[prev] - vertices[i]).normalized;

			                var vert = vertices[i];
			                vert += Vector3.Cross(nextSegment, Vector3.up) * 0.01f;
			                vert += Vector3.Cross(prevSegment, Vector3.down) * 0.01f;

			                vertices[corners.Length + i] = vert;
		                }

		                var triangles = new int[]
		                {
			                0, 1, 4,
			                1, 5, 4,
			                1, 2, 5,
			                2, 6, 5,
			                2, 3, 6,
			                3, 7, 6,
			                3, 0, 7,
			                0, 4, 7
		                };

		                var uv = new Vector2[]
		                {
			                new Vector2(0.0f, 0.0f),
			                new Vector2(1.0f, 0.0f),
			                new Vector2(0.0f, 0.0f),
			                new Vector2(1.0f, 0.0f),
			                new Vector2(0.0f, 1.0f),
			                new Vector2(1.0f, 1.0f),
			                new Vector2(0.0f, 1.0f),
			                new Vector2(1.0f, 1.0f)
		                };

		                var colors = new Color[]
		                {
			                color,
			                color,
			                color,
			                color,
			                new Color(color.r, color.g, color.b, 0.0f),
			                new Color(color.r, color.g, color.b, 0.0f),
			                new Color(color.r, color.g, color.b, 0.0f),
			                new Color(color.r, color.g, color.b, 0.0f)
		                };
                        
                        var mesh = new Mesh();
                        mesh.vertices = vertices;
                        mesh.uv = uv;
                        mesh.colors = colors;
                        mesh.triangles = triangles;
                        
                        Utility.RemoveGameObjects("SteamVR PlayArea", trackerObject);
                        var boundryObject = Utility.FindOrCreateEmptyGameObject("SteamVR PlayArea", trackerObject);
                        boundryObject.transform.localRotation = Quaternion.Euler(new Vector3(0, 180f, 0));
                        
                        var meshFilter = boundryObject.AddComponent<MeshFilter>();
                        meshFilter.mesh = mesh;

		                var renderer = boundryObject.AddComponent<MeshRenderer>();
		                //renderer.material = Resources.GetBuiltinResource<Material>("Sprites-Default.mat");
		                renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
		                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		                renderer.receiveShadows = false;
		                renderer.lightProbeUsage = LightProbeUsage.Off;
                        */
                    }
                    // tracker model
                    Utility.RemoveGameObjects(device.identifier, trackerObject);
                    string[] paths = new string[]
                    {
                        $"Assets/Hackebein/ObjectTracking/Prefab/BySerialNumber/{utility.Input.MakeNameSafe(device.serialNumber)}.fbx",
                        $"Assets/Hackebein/ObjectTracking/Prefab/ByModelNumber/{utility.Input.MakeNameSafe(device.modelNumber)}.fbx",
                        $"Assets/Hackebein/ObjectTracking/Prefab/ByManufacturerName/{utility.Input.MakeNameSafe(device.manufacturerName)}.fbx",
                        $"Assets/Hackebein/ObjectTracking/Prefab/ByTrackingSystemName/{utility.Input.MakeNameSafe(device.trackingSystemName)}.fbx",
                        $"Packages/hackebein.objecttracking/Prefab/BySerialNumber/{utility.Input.MakeNameSafe(device.serialNumber)}.fbx",
                        $"Packages/hackebein.objecttracking/Prefab/ByModelNumber/{utility.Input.MakeNameSafe(device.modelNumber)}.fbx",
                        $"Packages/hackebein.objecttracking/Prefab/ByManufacturerName/{utility.Input.MakeNameSafe(device.manufacturerName)}.fbx",
                        $"Packages/hackebein.objecttracking/Prefab/ByTrackingSystemName/{utility.Input.MakeNameSafe(device.trackingSystemName)}.fbx",
                    };
                    GameObject model = null;
                    foreach (string path in paths)
                    {
                        if (File.Exists(path))
                        {
                            model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                            break;
                        }
                        else
                        {
                            Debug.Log($"[Hackebein's Object Tracking] Searching for Tracker Model at `{path}`");
                        }
                    }

                    if (model != null)
                    {
                        GameObject modelInstance = Object.Instantiate(model, Vector3.zero, Quaternion.identity, trackerObject.transform);
                        modelInstance.transform.localPosition = Vector3.zero;
                        modelInstance.transform.localRotation = Quaternion.Euler(new Vector3(0, 180f, 0));
                        modelInstance.transform.localScale = Vector3.one;
                        modelInstance.name = trackerObject.name + " (Model)";
                    }
                    else
                    {
                        GameObject fallback = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(gizmoPath), Vector3.zero, Quaternion.identity, trackerObject.transform);
                        fallback.transform.localPosition = Vector3.zero;
                        fallback.transform.localRotation = Quaternion.Euler(new Vector3(0, 180f, 0));
                        fallback.transform.localScale = new Vector3(0.04f, 0.04f, -0.04f);
                        fallback.name = trackerObject.name + " (Gizmo)";
                    }
                }
                Tracker tracker = trackerObject.GetComponent<Tracker>();
                if (isNewComponent || tracker.updateInEditMode)
                {
                    var devicePosition = new Vector3(device.position.x, device.position.y, device.position.z);
                    devicePosition.Scale(scaleVector);
                    trackerObject.transform.localPosition = devicePosition;
                    trackerObject.transform.localRotation = Quaternion.Euler(device.rotation);
                    trackerObject.transform.localScale = scaleVector;
                }
            });
        }
        
        public void Apply()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            // Basic checks
            if (gameObject.tag == "EditorOnly")
            {
                return;
            }
            if (GetTrackers().Length == 0)
            {
                Debug.LogWarning("[Hackebein's Object Tracking] No trackers found");
                return;
            }
            
            // Cleanup
            Cleanup();
            Utility.ResetGameObject(gameObject, new List<Type>{typeof(Base)});
            for(int i = 0; i < gameObject.transform.childCount; i++)
            {
                GameObject.DestroyImmediate(gameObject.transform.GetChild(i).gameObject);
            }
            
            // avatar descriptor
            var avatarDescriptor = gameObject.transform.parent.GetComponent<VRCAvatarDescriptor>();
            if (avatarDescriptor == null)
            {
                throw new NullReferenceException("Parent GameObject must have a VRC_AvatarDescriptor component");
            }
            avatarDescriptor.customizeAnimationLayers = true;
            avatarDescriptor.customExpressions = true;
            
            // expression Menu
            var expressionMenu = avatarDescriptor.expressionsMenu;
            if (expressionMenu == null)
            {
                expressionMenu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
                AssetDatabase.CreateAsset(expressionMenu, settings.assetFolder + "/" + settings.uuid + "/Menu/Root.asset");
            }
            if (expressionMenu.controls == null)
            {
                expressionMenu.controls = new List<VRCExpressionsMenu.Control>();
            }
            
            // expression parameters
            VRCExpressionParameters expressionParameters = avatarDescriptor.expressionParameters;
            
            // scale
            float scale = (avatarDescriptor.ViewPosition.y / (float)vrchat.PlayerHeights.GetCurrentHeight().CmValue * 100) * magicNumber;
            gameObject.transform.localPosition = new Vector3(0, 0, avatarDescriptor.ViewPosition.z);
            gameObject.transform.localScale = new Vector3(scale, scale, scale);

            // animation controller
            // Layer Design:
            //   x:   30, y: 190
            //   x: +240, y: +80
            AnimatorController animatorController = null;
            VRCAvatarDescriptor.CustomAnimLayer[] customAnimLayers = avatarDescriptor.baseAnimationLayers;
            for (int i = 0; i < customAnimLayers.Length; i++)
            {
                if (customAnimLayers[i].type == VRCAvatarDescriptor.AnimLayerType.FX &&
                    customAnimLayers[i].animatorController != null)
                {
                    animatorController = (AnimatorController)customAnimLayers[i].animatorController;
                    break;
                }
            }
            if (animatorController == null)
            {
                for (int i = 0; i < customAnimLayers.Length; i++)
                {
                    if (customAnimLayers[i].type == VRCAvatarDescriptor.AnimLayerType.FX)
                    {
                        Utility.CreatePathRecursive(System.IO.Path.GetDirectoryName(settings.assetFolder + "/" + settings.uuid + "/FX.controller"));
                        var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(settings.assetFolder + "/" + settings.uuid + "/FX.controller");
                        customAnimLayers[i].animatorController = controller;
                        customAnimLayers[i].isDefault = false;
                        avatarDescriptor.baseAnimationLayers = customAnimLayers;
                        animatorController = controller;
                        break;
                    }
                }
                if (animatorController == null)
                {
                    throw new NullReferenceException("[Hackebein's Object Tracking] Failed to create FX animator controller");
                }
            }
            animatorController = Utility.CreateBoolParameterAndAddToAnimator(animatorController, "IsLocal");
            
            // ignore animation
            AnimationClip ignoreClip = Utility.CreateClip("ignore", "_ignore", "GameObject.m_IsActive", 0, 0, settings.assetFolder);
            
            // Animator Controller Layers
            (animatorController, expressionParameters) = CreateProcessingLayer(animatorController, expressionParameters, ignoreClip);
            
            // Player Height Layer
            (animatorController, expressionParameters) = CreatePlayerHeightLayer(animatorController, expressionParameters);

            // Stabilization Layer
            (animatorController, expressionParameters) = CreateStabilizationLayer(animatorController, expressionParameters, ignoreClip);
            
            // Add Menu Items
            expressionMenu = CreateExpressionMenu(expressionMenu);
            
            // Tracker Components
            foreach (var tracker in GetTrackers()) 
            {
                (animatorController, expressionParameters) = CreateHideBeyondLimitsLayer(animatorController, expressionParameters, tracker);
                (animatorController, expressionParameters) = CreateTransitionLayer(animatorController, expressionParameters, tracker, tracker.settings.axes.Position.X, "PX", "VRCParentConstraint.Sources.source0.ParentPositionOffset.x");
                (animatorController, expressionParameters) = CreateTransitionLayer(animatorController, expressionParameters, tracker, tracker.settings.axes.Position.Y, "PY", "VRCParentConstraint.Sources.source0.ParentPositionOffset.y");
                (animatorController, expressionParameters) = CreateTransitionLayer(animatorController, expressionParameters, tracker, tracker.settings.axes.Position.Z, "PZ", "VRCParentConstraint.Sources.source0.ParentPositionOffset.z");
                (animatorController, expressionParameters) = CreateTransitionLayer(animatorController, expressionParameters, tracker, tracker.settings.axes.Rotation.X, "RX", "VRCParentConstraint.Sources.source0.ParentRotationOffset.x");
                (animatorController, expressionParameters) = CreateTransitionLayer(animatorController, expressionParameters, tracker, tracker.settings.axes.Rotation.Y, "RY", "VRCParentConstraint.Sources.source0.ParentRotationOffset.y");
                (animatorController, expressionParameters) = CreateTransitionLayer(animatorController, expressionParameters, tracker, tracker.settings.axes.Rotation.Z, "RZ", "VRCParentConstraint.Sources.source0.ParentRotationOffset.z");
                Utility.MarkDirty(expressionParameters);
                
                GameObject trackerPositionRaw = Utility.FindOrCreateEmptyGameObject(tracker.settings.identifier, gameObject);
                trackerPositionRaw.tag = "Untagged";
                
                VRCParentConstraint trackerPositionContraint = trackerPositionRaw.AddComponent<VRCParentConstraint>();
                trackerPositionContraint.IsActive = true;
                trackerPositionContraint.SolveInLocalSpace = true;
                trackerPositionContraint.Locked = true;
                trackerPositionContraint.Sources.Add(new VRCConstraintSource(gameObject.transform, 1, Vector3.zero, Vector3.zero));

                VRCPositionConstraint trackerPositionDampingConstraint = tracker.gameObject.AddComponent<VRCPositionConstraint>();
                trackerPositionDampingConstraint.IsActive = true;
                trackerPositionDampingConstraint.Locked = true;
                trackerPositionDampingConstraint.Sources.Add(new VRCConstraintSource(tracker.transform, 1, Vector3.zero, Vector3.zero));
                trackerPositionDampingConstraint.Sources.Add(new VRCConstraintSource(trackerPositionRaw.transform, tracker.settings.PositionDamping, Vector3.zero, Vector3.zero));
                
                VRCRotationConstraint trackerRotationDampingConstraint = tracker.gameObject.AddComponent<VRCRotationConstraint>();
                trackerRotationDampingConstraint.IsActive = true;
                trackerRotationDampingConstraint.Locked = true;
                trackerRotationDampingConstraint.Sources.Add(new VRCConstraintSource(tracker.transform, 1, Vector3.zero, Vector3.zero));
                trackerRotationDampingConstraint.Sources.Add(new VRCConstraintSource(trackerPositionRaw.transform, tracker.settings.RotationDamping, Vector3.zero, Vector3.zero));
            }
            AssetDatabase.SaveAssets();
            stopwatch.Stop();
            Debug.Log("[Hackebein's Object Tracking] Apply Time: " + stopwatch.ElapsedMilliseconds + "ms");
        }

        private VRCExpressionsMenu CreateExpressionMenu(VRCExpressionsMenu expressionMenu)
        {
            if (settings.addMenu)
            {
                var expressionSubMenu = vrchat.ExpressionMenu.CreateMenu();
                expressionSubMenu.controls.Add(new VRCExpressionsMenu.Control()
                {
                    name = "Stabalize Objects",
                    icon = null,
                    type = VRCExpressionsMenu.Control.ControlType.Toggle,
                    parameter = new VRCExpressionsMenu.Control.Parameter()
                    {
                        name = "ObjectTracking/goStabilized"
                    },
                    value = 1
                });

                foreach (var tracker in GetTrackers())
                {
                    expressionSubMenu = vrchat.ExpressionMenu.CreateIfNeededMoreMenu(expressionSubMenu);
                    expressionSubMenu.controls.Add(new VRCExpressionsMenu.Control()
                    {
                        name = tracker.name,
                        icon = null,
                        type = VRCExpressionsMenu.Control.ControlType.Toggle,
                        parameter = new VRCExpressionsMenu.Control.Parameter()
                        {
                            name = "ObjectTracking/" + tracker.settings.identifier + "/enabled"
                        },
                    });
                }
                expressionMenu.controls.Add(new VRCExpressionsMenu.Control()
                {
                    name = "Object Tracking",
                    icon = null,
                    type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                    subMenu = expressionSubMenu
                });
            }

            if (settings.addDebugMenu)
            {
                var expressionDebugSubMenu = vrchat.ExpressionMenu.CreateMenu();
                expressionDebugSubMenu.controls.Add(new VRCExpressionsMenu.Control()
                {
                    name = "go Stabilized",
                    icon = null,
                    type = VRCExpressionsMenu.Control.ControlType.Toggle,
                    parameter = new VRCExpressionsMenu.Control.Parameter()
                    {
                        name = "ObjectTracking/goStabilized"
                    },
                    value = 1
                });
                expressionDebugSubMenu.controls.Add(new VRCExpressionsMenu.Control()
                {
                    name = "Is Stabilized",
                    icon = null,
                    type = VRCExpressionsMenu.Control.ControlType.Toggle,
                    parameter = new VRCExpressionsMenu.Control.Parameter()
                    {
                        name = "ObjectTracking/isStabilized"
                    },
                    value = 1
                });
                expressionDebugSubMenu.controls.Add(new VRCExpressionsMenu.Control()
                {
                    name = "Is Lazy Stabilized",
                    icon = null,
                    type = VRCExpressionsMenu.Control.ControlType.Toggle,
                    parameter = new VRCExpressionsMenu.Control.Parameter()
                    {
                        name = "ObjectTracking/isLazyStabilized"
                    },
                    value = 1
                });
                expressionDebugSubMenu.controls.Add(new VRCExpressionsMenu.Control()
                {
                    name = "Remote Preview",
                    icon = null,
                    type = VRCExpressionsMenu.Control.ControlType.Toggle,
                    parameter = new VRCExpressionsMenu.Control.Parameter()
                    {
                        name = "ObjectTracking/isRemotePreview"
                    },
                    value = 1
                });
                expressionDebugSubMenu.controls.Add(new VRCExpressionsMenu.Control()
                {
                    name = "Re-Send Config",
                    icon = null,
                    type = VRCExpressionsMenu.Control.ControlType.Toggle,
                    parameter = new VRCExpressionsMenu.Control.Parameter()
                    {
                        name = "ObjectTracking/config/global"
                    },
                    value = 1
                });

                foreach (var tracker in GetTrackers())
                {
                    var expressionDebugSubMenuTracker = vrchat.ExpressionMenu.CreateMenu();
                    expressionDebugSubMenuTracker.controls.Add(new VRCExpressionsMenu.Control()
                    {
                        name = "Disable",
                        icon = null,
                        type = VRCExpressionsMenu.Control.ControlType.Toggle,
                        parameter = new VRCExpressionsMenu.Control.Parameter()
                        {
                            name = "ObjectTracking/" + tracker.settings.identifier + "/enabled"
                        },
                        value = 0
                    });
                    expressionDebugSubMenuTracker.controls.Add(new VRCExpressionsMenu.Control()
                    {
                        name = "Position X",
                        icon = null,
                        type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                        subParameters = new VRCExpressionsMenu.Control.Parameter[]
                        {
                            new VRCExpressionsMenu.Control.Parameter()
                            {
                                name = "ObjectTracking/" + tracker.settings.identifier + "/LPX"
                            }
                        }
                    });
                    expressionDebugSubMenuTracker.controls.Add(new VRCExpressionsMenu.Control()
                    {
                        name = "Position Y",
                        icon = null,
                        type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                        subParameters = new VRCExpressionsMenu.Control.Parameter[]
                        {
                            new VRCExpressionsMenu.Control.Parameter()
                            {
                                name = "ObjectTracking/" + tracker.settings.identifier + "/LPY"
                            }
                        }
                    });
                    expressionDebugSubMenuTracker.controls.Add(new VRCExpressionsMenu.Control()
                    {
                        name = "Position Z",
                        icon = null,
                        type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                        subParameters = new VRCExpressionsMenu.Control.Parameter[]
                        {
                            new VRCExpressionsMenu.Control.Parameter()
                            {
                                name = "ObjectTracking/" + tracker.settings.identifier + "/LPZ"
                            }
                        }
                    });
                    expressionDebugSubMenuTracker.controls.Add(new VRCExpressionsMenu.Control()
                    {
                        name = "Rotation X",
                        icon = null,
                        type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                        subParameters = new VRCExpressionsMenu.Control.Parameter[]
                        {
                            new VRCExpressionsMenu.Control.Parameter()
                            {
                                name = "ObjectTracking/" + tracker.settings.identifier + "/LRX"
                            }
                        }
                    });
                    expressionDebugSubMenuTracker.controls.Add(new VRCExpressionsMenu.Control()
                    {
                        name = "Rotation Y",
                        icon = null,
                        type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                        subParameters = new VRCExpressionsMenu.Control.Parameter[]
                        {
                            new VRCExpressionsMenu.Control.Parameter()
                            {
                                name = "ObjectTracking/" + tracker.settings.identifier + "/LRY"
                            }
                        }
                    });
                    expressionDebugSubMenuTracker.controls.Add(new VRCExpressionsMenu.Control()
                    {
                        name = "Rotation Z",
                        icon = null,
                        type = VRCExpressionsMenu.Control.ControlType.RadialPuppet,
                        subParameters = new VRCExpressionsMenu.Control.Parameter[]
                        {
                            new VRCExpressionsMenu.Control.Parameter()
                            {
                                name = "ObjectTracking/" + tracker.settings.identifier + "/LRZ"
                            }
                        }
                    });
                    expressionDebugSubMenuTracker.controls.Add(new VRCExpressionsMenu.Control()
                    {
                        name = "Enable",
                        icon = null,
                        type = VRCExpressionsMenu.Control.ControlType.Toggle,
                        parameter = new VRCExpressionsMenu.Control.Parameter()
                        {
                            name = "ObjectTracking/" + tracker.settings.identifier + "/enabled"
                        },
                        value = 1
                    });
                    expressionDebugSubMenu = vrchat.ExpressionMenu.CreateIfNeededMoreMenu(expressionDebugSubMenu);
                    expressionDebugSubMenu.controls.Add(new VRCExpressionsMenu.Control()
                    {
                        name = tracker.name != tracker.settings.identifier ? tracker.name + "<br>" + tracker.settings.identifier + "" : tracker.settings.identifier,
                        icon = null,
                        type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                        subMenu = expressionDebugSubMenuTracker
                    });
                }
                expressionMenu.controls.Add(new VRCExpressionsMenu.Control()
                {
                    name = "Object Tracking Debug",
                    icon = null,
                    type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                    subMenu = expressionDebugSubMenu
                });
            }

            return expressionMenu;
        }

        private (AnimatorController, VRCExpressionParameters) CreateTransitionLayer(AnimatorController animatorController, VRCExpressionParameters expressionParameters, Tracker tracker, Axe axe, string name, string property)
        {
            // Animation State Local
            AnimatorState stateLocal = new AnimatorState
            {
                name = "Local",
                writeDefaultValues = false,
                timeParameterActive = true,
                timeParameter = "ObjectTracking/" + tracker.settings.identifier + "/L" + name
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
                timeParameter = "ObjectTracking/" + tracker.settings.identifier + "/R" + name
            };

            ChildAnimatorState stateRemoteChild = new ChildAnimatorState
            {
                state = stateRemote,
                position = new Vector3(270, 190, 0)
            };

            // Layer
            AnimatorControllerLayer layer = Utility.CreateLayer("ObjectTracking/" + tracker.settings.identifier + "/" + name);
            layer.stateMachine.states = new[] { stateLocalChild, stateRemoteChild };
            animatorController.layers = animatorController.layers.Append(layer).ToArray();

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
                Utility.CreateClip(tracker.settings.identifier + "/L" + name, gameObject.name + "/" + tracker.settings.identifier, property, axe.Local.ValueMin, axe.Local.ValueMax, settings.assetFolder + "/" + settings.uuid + "/Tracker");
            stateRemote.motion =
                Utility.CreateClip(tracker.settings.identifier + "/R" + name, gameObject.name + "/" + tracker.settings.identifier, property, axe.Remote.ValueMin, axe.Remote.ValueMax, settings.assetFolder + "/" + settings.uuid + "/Tracker");

            Utility.AddSubAssetsToDatabase(layer, animatorController);
            return (animatorController, expressionParameters);
        }

        public (AnimatorController, VRCExpressionParameters) CreateHideBeyondLimitsLayer(AnimatorController animatorController, VRCExpressionParameters expressionParameters, Tracker tracker)
        {
            if (!tracker.settings.hideBeyondLimits)
            {
                return (animatorController, expressionParameters);
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
            AnimatorControllerLayer layer = Utility.CreateLayer("ObjectTracking/" + tracker.settings.identifier + "/HideBeyondLimits");
            layer.stateMachine.states = new[] { stateHideChild, stateShowChild };
            animatorController.layers = animatorController.layers.Append(layer).ToArray();

            // Transition Conditions
            /// Show
            var conditionsToShowLocalBool = new Tuple<string, bool>[]
            {
                Tuple.Create("IsLocal", true),
                Tuple.Create("ObjectTracking/isRemotePreview", false),
            };
            var conditionsToShowRemotePreviewBool = new Tuple<string, bool>[]
            {
                Tuple.Create("IsLocal", true),
                Tuple.Create("ObjectTracking/isRemotePreview", true),
            };
            var conditionsToShowRemoteBool = new Tuple<string, bool>[]
            {
                Tuple.Create("IsLocal", false),
            };
            var conditionsToShowLocalFloat = new List<Tuple<string, AnimatorConditionMode, float>>
            {
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LPX", AnimatorConditionMode.Greater, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.X.Local.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LPX", AnimatorConditionMode.Less, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.X.Local.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LPY", AnimatorConditionMode.Greater, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.Y.Local.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LPY", AnimatorConditionMode.Less, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.Y.Local.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LPZ", AnimatorConditionMode.Greater, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.Z.Local.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LPZ", AnimatorConditionMode.Less, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.Z.Local.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LRX", AnimatorConditionMode.Greater, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.X.Local.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LRX", AnimatorConditionMode.Less, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.X.Local.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LRY", AnimatorConditionMode.Greater, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.Y.Local.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LRY", AnimatorConditionMode.Less, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.Y.Local.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LRZ", AnimatorConditionMode.Greater, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.Z.Local.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LRZ", AnimatorConditionMode.Less, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.Z.Local.Bits)))),
            };
            
            var conditionsToShowRemoteFloat = new List<Tuple<string, AnimatorConditionMode, float>>
            {
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RPX", AnimatorConditionMode.Greater, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.X.Remote.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RPX", AnimatorConditionMode.Less, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.X.Remote.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RPY", AnimatorConditionMode.Greater, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.Y.Remote.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RPY", AnimatorConditionMode.Less, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.Y.Remote.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RPZ", AnimatorConditionMode.Greater, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.Z.Remote.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RPZ", AnimatorConditionMode.Less, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.Z.Remote.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RRX", AnimatorConditionMode.Greater, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.X.Remote.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RRX", AnimatorConditionMode.Less, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.X.Remote.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RRY", AnimatorConditionMode.Greater, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.Y.Remote.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RRY", AnimatorConditionMode.Less, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.Y.Remote.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RRZ", AnimatorConditionMode.Greater, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.Z.Remote.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RRZ", AnimatorConditionMode.Less, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.Z.Remote.Bits)))),
            };
            
            var conditionsToHideLocalBool = new Tuple<string, bool>[]
            {
                Tuple.Create("IsLocal", true),
                Tuple.Create("ObjectTracking/isRemotePreview", false),
            };
            var conditionsToHideRemotePreviewBool = new Tuple<string, bool>[]
            {
                Tuple.Create("IsLocal", true),
                Tuple.Create("ObjectTracking/isRemotePreview", true),
            };
            var conditionsToHideRemoteBool = new Tuple<string, bool>[]
            {
                Tuple.Create("IsLocal", false),
            };
            var conditionsToHideLocalFloat = new List<Tuple<string, AnimatorConditionMode, float>>
            {
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LPX", AnimatorConditionMode.Less, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.X.Local.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LPX", AnimatorConditionMode.Greater, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.X.Local.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LPY", AnimatorConditionMode.Less, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.Y.Local.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LPY", AnimatorConditionMode.Greater, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.Y.Local.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LPZ", AnimatorConditionMode.Less, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.Z.Local.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LPZ", AnimatorConditionMode.Greater, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.Z.Local.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LRX", AnimatorConditionMode.Less, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.X.Local.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LRX", AnimatorConditionMode.Greater, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.X.Local.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LRY", AnimatorConditionMode.Less, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.Y.Local.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LRY", AnimatorConditionMode.Greater, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.Y.Local.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LRZ", AnimatorConditionMode.Less, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.Z.Local.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/LRZ", AnimatorConditionMode.Greater, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.Z.Local.Bits)))),
            };
            var conditionsToHideRemoteFloat = new List<Tuple<string, AnimatorConditionMode, float>>
            {
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RPX", AnimatorConditionMode.Less, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.X.Remote.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RPX", AnimatorConditionMode.Greater, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.X.Remote.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RPY", AnimatorConditionMode.Less, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.Y.Remote.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RPY", AnimatorConditionMode.Greater, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.Y.Remote.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RPZ", AnimatorConditionMode.Less, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.Z.Remote.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RPZ", AnimatorConditionMode.Greater, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Position.Z.Remote.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RRX", AnimatorConditionMode.Less, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.X.Remote.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RRX", AnimatorConditionMode.Greater, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.X.Remote.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RRY", AnimatorConditionMode.Less, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.Y.Remote.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RRY", AnimatorConditionMode.Greater, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.Y.Remote.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RRZ", AnimatorConditionMode.Less, (float)(1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.Z.Remote.Bits)))),
                Tuple.Create("ObjectTracking/" + tracker.settings.identifier + "/RRZ", AnimatorConditionMode.Greater, (float)(1 - 1 / Math.Pow(2, Math.Min(12, tracker.settings.axes.Rotation.Z.Remote.Bits)))),
            };
            
            stateHide.transitions = new[]
            {
                Utility.CreateTransition("isLocal", conditionsToShowLocalBool, conditionsToShowLocalFloat.ToArray(), stateShow),
                Utility.CreateTransition("isRemotePreview", conditionsToShowRemotePreviewBool, conditionsToShowRemoteFloat.ToArray(), stateShow),
                Utility.CreateTransition("isRemote", conditionsToShowRemoteBool, conditionsToShowRemoteFloat.ToArray(), stateShow),
                Utility.CreateTransition("isLocal", conditionsToHideLocalBool, conditionsToHideLocalFloat.ToArray(), stateHide),
                Utility.CreateTransition("isRemotePreview", conditionsToHideRemotePreviewBool, conditionsToHideRemoteFloat.ToArray(), stateHide),
                Utility.CreateTransition("isRemote", conditionsToHideRemoteBool, conditionsToHideRemoteFloat.ToArray(), stateHide),
            };
            layer.stateMachine.entryTransitions = new[]
            {
                Utility.CreateEntryTransition("isLocal", conditionsToShowLocalBool, conditionsToShowLocalFloat.ToArray(), stateShow),
                Utility.CreateEntryTransition("isRemotePreview", conditionsToShowRemotePreviewBool, conditionsToShowRemoteFloat.ToArray(), stateShow),
                Utility.CreateEntryTransition("isRemote", conditionsToShowRemoteBool, conditionsToShowRemoteFloat.ToArray(), stateShow),
            };
            stateShow.transitions = new[]
            {
                Utility.CreateTransition("isLocal", conditionsToHideLocalBool, conditionsToHideLocalFloat.ToArray(), stateHide),
                Utility.CreateTransition("isRemotePreview", conditionsToHideRemotePreviewBool, conditionsToHideRemoteFloat.ToArray(), stateHide),
                Utility.CreateTransition("isRemote", conditionsToHideRemoteBool, conditionsToHideRemoteFloat.ToArray(), stateHide),
            };

            // Clip
            stateShow.motion =
                Utility.CreateClip(tracker.settings.identifier + "/ShowInsideLimits", tracker.name, "GameObject.m_IsActive", 1, 1, settings.assetFolder + "/" + settings.uuid + "/Tracker");
            stateHide.motion =
                Utility.CreateClip(tracker.settings.identifier + "/HideBeyondLimits", tracker.name, "GameObject.m_IsActive", 0, 0, settings.assetFolder + "/" + settings.uuid + "/Tracker");

            Utility.AddSubAssetsToDatabase(layer, animatorController);
            return (animatorController, expressionParameters);
        }

        private (AnimatorController, VRCExpressionParameters) CreateProcessingLayer(AnimatorController animatorController, VRCExpressionParameters expressionParameters, AnimationClip ignoreClip)
        {
            //        [ Entry ]
            //         ||    \\
            //         v      v
            // [ Local ] <=> [ Remote ] <
            //
            // Transitions:
            //   Entry => Local: (default)
            //   Entry => Remote: IsLocal = False
            //   Local => Remote: IsLocal = False
            //   Remote => Local: IsLocal = True
            //   Remote => Remote: IsLocal = False
            //
            // Animations:
            //   Local: Empty (2 Frames)
            //   Remote: BlendTree
            //     DirectBlendTree: AAP (binary addition)
            //
            // Components:
            //   Remote: Parameter Driver
            //     Copy every bool/int to float (0-1/0-255 => 0-1)
            //
            // Layer Design:
            //   x:   30, y: 190
            //   x: +240, y: +80
            
            // Layer
            AnimatorControllerLayer layer = Utility.CreateLayer("ObjectTracking/Processing");
            animatorController.layers = animatorController.layers.Append(layer).ToArray();
            
            // Parameters
            expressionParameters = Utility.CreateBoolParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/isRemotePreview", false, false, false);
            expressionParameters = Utility.CreateBoolParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/config/global", false, false, false);
            expressionParameters = Utility.CreateIntParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/config/index", 0, false, false);
            expressionParameters = Utility.CreateIntParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/config/value", 0, false, false);
            animatorController = Utility.CreateBoolParameterAndAddToAnimator(animatorController, "ObjectTracking/isRemotePreview");
            animatorController = Utility.CreateBoolParameterAndAddToAnimator(animatorController, "ObjectTracking/config/global");
            animatorController = Utility.CreateIntParameterAndAddToAnimator(animatorController, "ObjectTracking/config/index");
            animatorController = Utility.CreateFloatParameterAndAddToAnimator(animatorController, "ObjectTracking/config/value");
            
            // Animation State Local
            var stateLocal = new AnimatorState
            {
                name = "Local",
                writeDefaultValues = false,
                motion = ignoreClip
            };
            
            List<VRCAvatarParameterDriver.Parameter> parameterDriverParametersLocal = new List<VRCAvatarParameterDriver.Parameter>
            {
                Utility.ParameterDriverParameterSet("ObjectTracking/config/index", 0),
                Utility.ParameterDriverParameterSet("ObjectTracking/config/value", 0f),
            };
            Utility.AddTrackerStart(parameterDriverParametersLocal, "global");
            Utility.AddConfigValue(parameterDriverParametersLocal, 1, 1f); // version number
            Utility.AddTrackerEnd(parameterDriverParametersLocal, "global");
            foreach(Tracker tracker in GetTrackers())
            {
                expressionParameters = Utility.CreateBoolParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/" + tracker.settings.identifier + "/enabled", true, true, false);
                expressionParameters = Utility.CreateBoolParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/config/" + tracker.settings.identifier, false, false, false);
                animatorController = Utility.CreateBoolParameterAndAddToAnimator(animatorController, "ObjectTracking/config/" + tracker.settings.identifier);
                Utility.AddTrackerStart(parameterDriverParametersLocal, tracker.settings.identifier);
                int i = 1;
                // TODO: verify if inputs are between limits
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Position.X.Remote.Bits);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Position.Y.Remote.Bits);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Position.Z.Remote.Bits);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Rotation.X.Remote.Bits);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Rotation.Y.Remote.Bits);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Rotation.Z.Remote.Bits);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Position.X.Local.ValueMin);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Position.Y.Local.ValueMin);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Position.Z.Local.ValueMin);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Rotation.X.Local.ValueMin);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Rotation.Y.Local.ValueMin);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Rotation.Z.Local.ValueMin);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Position.X.Remote.ValueMin);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Position.Y.Remote.ValueMin);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Position.Z.Remote.ValueMin);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Rotation.X.Remote.ValueMin);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Rotation.Y.Remote.ValueMin);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Rotation.Z.Remote.ValueMin);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Position.X.Local.ValueMax);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Position.Y.Local.ValueMax);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Position.Z.Local.ValueMax);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Rotation.X.Local.ValueMax);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Rotation.Y.Local.ValueMax);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Rotation.Z.Local.ValueMax);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Position.X.Remote.ValueMax);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Position.Y.Remote.ValueMax);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Position.Z.Remote.ValueMax);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Rotation.X.Remote.ValueMax);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Rotation.Y.Remote.ValueMax);
                Utility.AddConfigValue(parameterDriverParametersLocal, i++, tracker.settings.axes.Rotation.Z.Remote.ValueMax);
                Utility.AddTrackerEnd(parameterDriverParametersLocal, tracker.settings.identifier);
            }
            
            VRCAvatarParameterDriver parameterDriverLocal = ScriptableObject.CreateInstance<VRCAvatarParameterDriver>();
            parameterDriverLocal.parameters = parameterDriverParametersLocal;
            parameterDriverLocal.localOnly = false;
            stateLocal.behaviours = new StateMachineBehaviour[] { parameterDriverLocal };

            ChildAnimatorState stateLocalChild = new ChildAnimatorState
            {
                state = stateLocal,
                position = new Vector3(30, 190, 0)
            };

            layer.stateMachine.states = layer.stateMachine.states.Append(stateLocalChild).ToArray();
            
            // Animation State Remote
            AnimatorState stateRemote = new AnimatorState
            {
                name = "Remote",
                writeDefaultValues = true
            };
            
            List<VRCAvatarParameterDriver.Parameter> parameterDriverParametersRemote = new List<VRCAvatarParameterDriver.Parameter>{};
            Dictionary<string, AnimationClip> motionsRemote = new Dictionary<string, AnimationClip>();
            foreach (Tracker tracker in GetTrackers())
            {
                (AxeTarget target, string name)[] axeTargetsLocal = new[]
                {
                    (tracker.settings.axes.Position.X.Remote, "LPX"),
                    (tracker.settings.axes.Position.Y.Remote, "LPY"),
                    (tracker.settings.axes.Position.Z.Remote, "LPZ"),
                    (tracker.settings.axes.Rotation.X.Remote, "LRX"),
                    (tracker.settings.axes.Rotation.Y.Remote, "LRY"),
                    (tracker.settings.axes.Rotation.Z.Remote, "LRZ"),
                };
                foreach ((AxeTarget target, string name) axe in axeTargetsLocal)
                {
                    animatorController = Utility.CreateFloatParameterAndAddToAnimator(animatorController, "ObjectTracking/" + tracker.settings.identifier + "/" + axe.name);
                    expressionParameters = Utility.CreateFloatParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/" + tracker.settings.identifier + "/" + axe.name, 0f, false, false);
                }
                
                (AxeTarget target, string name)[] axeTargetsRemote = new[]
                {
                    (tracker.settings.axes.Position.X.Remote, "RPX"),
                    (tracker.settings.axes.Position.Y.Remote, "RPY"),
                    (tracker.settings.axes.Position.Z.Remote, "RPZ"),
                    (tracker.settings.axes.Rotation.X.Remote, "RRX"),
                    (tracker.settings.axes.Rotation.Y.Remote, "RRY"),
                    (tracker.settings.axes.Rotation.Z.Remote, "RRZ"),
                };
                foreach ((AxeTarget target, string name) axe in axeTargetsRemote)
                {
                    int accuracy = axe.target.Bits;
                    int accuracyBytes = accuracy / 8;
                    int accuracyBits = accuracy - (accuracyBytes * 8);
                    animatorController = Utility.CreateFloatParameterAndAddToAnimator(animatorController, "ObjectTracking/" + tracker.settings.identifier + "/" + axe.name);
                    for (int i = 0; i < accuracyBytes; i++)
                    {
                        float multiplicator = Utility.GetAAPMultiplicator(accuracy, 8 * i, 8);
                        expressionParameters = Utility.CreateIntParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/" + tracker.settings.identifier + "/" + axe.name + "-Byte" + i, 0, false, true);
                        animatorController = Utility.CreateIntParameterAndAddToAnimator(animatorController, "ObjectTracking/" + tracker.settings.identifier + "/" + axe.name + "-Byte" + i);
                        animatorController = Utility.CreateFloatParameterAndAddToAnimator(animatorController, "ObjectTracking/" + tracker.settings.identifier + "/" + axe.name + "-Byte" + i + "-Float");
                        parameterDriverParametersRemote.Add(Utility.ParameterDriverParameterIntToFloat(
                            "ObjectTracking/" + tracker.settings.identifier + "/" + axe.name + "-Byte" + i,
                            "ObjectTracking/" + tracker.settings.identifier + "/" + axe.name + "-Byte" + i + "-Float"));
                        motionsRemote.Add("ObjectTracking/" + tracker.settings.identifier + "/" + axe.name + "-Byte" + i + "-Float",
                            Utility.CreateClip(tracker.settings.identifier + "/" + axe.name + "-Byte" + i, "", "Animator.ObjectTracking/" + tracker.settings.identifier + "/" + axe.name, multiplicator, multiplicator, settings.assetFolder + "/" + settings.uuid + "/Tracker"));
                    }
                    for (int i = 0; i < accuracyBits; i++)
                    {
                        float multiplicator = Utility.GetAAPMultiplicator(accuracy, 8 * accuracyBytes + i, 1);
                        expressionParameters = Utility.CreateBoolParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/" + tracker.settings.identifier + "/" + axe.name + "-Bit" + i, false, false, true);
                        animatorController = Utility.CreateBoolParameterAndAddToAnimator(animatorController, "ObjectTracking/" + tracker.settings.identifier + "/" + axe.name + "-Bit" + i);
                        animatorController = Utility.CreateFloatParameterAndAddToAnimator(animatorController, "ObjectTracking/" + tracker.settings.identifier + "/" + axe.name + "-Bit" + i + "-Float");
                        parameterDriverParametersRemote.Add(Utility.ParameterDriverParameterBoolToFloat(
                            "ObjectTracking/" + tracker.settings.identifier + "/" + axe.name + "-Bit" + i,
                            "ObjectTracking/" + tracker.settings.identifier + "/" + axe.name + "-Bit" + i + "-Float"));
                        motionsRemote.Add("ObjectTracking/" + tracker.settings.identifier + "/" + axe.name + "-Bit" + i + "-Float",
                            Utility.CreateClip(tracker.settings.identifier + "/" + axe.name + "-Bit" + i, "", "Animator.ObjectTracking/" + tracker.settings.identifier + "/" + axe.name, multiplicator, multiplicator, settings.assetFolder + "/" + settings.uuid + "/Tracker"));
                    }
                }                
            }

            stateRemote.motion = Utility.CreateDirectBlendTree("processing", motionsRemote);

            VRCAvatarParameterDriver parameterDriverRemote = ScriptableObject.CreateInstance<VRCAvatarParameterDriver>();
            parameterDriverRemote.parameters = parameterDriverParametersRemote;
            parameterDriverRemote.localOnly = false;
            stateRemote.behaviours = new StateMachineBehaviour[] { parameterDriverRemote };

            ChildAnimatorState stateRemoteChild = new ChildAnimatorState
            {
                state = stateRemote,
                position = new Vector3(270, 190, 0)
            };

            layer.stateMachine.states = layer.stateMachine.states.Append(stateRemoteChild).ToArray();

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
            Tuple<string, bool>[] conditionsReload = new Tuple<string, bool>[]
            {
                Tuple.Create("ObjectTracking/config/global", true),
            };
            
            layer.stateMachine.entryTransitions = new[]
            {
                Utility.CreateEntryTransition("isRemote", conditionsToRemote, stateRemote),
                Utility.CreateEntryTransition("isRemotePreview", conditionsToRemotePreview, stateRemote),
                Utility.CreateEntryTransition("isLocal", conditionsToLocal, stateLocal)
            };
            stateLocal.transitions = new[]
            {
                Utility.CreateTransition("isRemote", conditionsToRemote, stateRemote),
                Utility.CreateTransition("isRemotePreview", conditionsToRemotePreview, stateRemote),
                Utility.CreateTransition("reload", conditionsReload, stateLocal)
            };
            stateRemote.transitions = new[]
            {
                Utility.CreateTransition("isRemote", conditionsToRemote, stateRemote),
                Utility.CreateTransition("isRemotePreview", conditionsToRemotePreview, stateRemote),
                Utility.CreateTransition("isLocal", conditionsToLocal, stateLocal),
            };

            Utility.AddSubAssetsToDatabase(layer, animatorController);
            return (animatorController, expressionParameters);
        }
        
        public (AnimatorController, VRCExpressionParameters) CreatePlayerHeightLayer(AnimatorController animatorController, VRCExpressionParameters expressionParameters)
        {
            animatorController = Utility.CreateIntParameterAndAddToAnimator(animatorController, "ObjectTracking/PlayerHeightIndex");
            expressionParameters = Utility.CreateIntParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/PlayerHeightIndex", vrchat.PlayerHeights.GetCurrentHeight().Index, true, true);
            // Layer
            AnimatorControllerLayer layer = Utility.CreateLayer("ObjectTracking/PlayerHeight");
            animatorController.layers = animatorController.layers.Append(layer).ToArray();
            var avatarDescriptor = gameObject.transform.parent.GetComponent<VRCAvatarDescriptor>();
            foreach (vrchat.PlayerHeight playerHeight in vrchat.PlayerHeights.List)
            {
                // Animation State
                float scale = (avatarDescriptor.ViewPosition.y / (float)vrchat.PlayerHeights.GetCurrentHeight().CmValue * 100) * magicNumber;
                Dictionary<string[], float[]> props = new Dictionary<string[], float[]>
                {
                    { new[] { gameObject.name, "Transform.m_LocalScale.x" }, new[] { scale, scale } },
                    { new[] { gameObject.name, "Transform.m_LocalScale.y" }, new[] { scale, scale } },
                    { new[] { gameObject.name, "Transform.m_LocalScale.z" }, new[] { scale, scale } },
                };
                foreach (var tracker in GetTrackers())
                {
                    props.Add(new[] { tracker.name, "Transform.m_LocalScale.x" }, new[] { scale, scale });
                    props.Add(new[] { tracker.name, "Transform.m_LocalScale.y" }, new[] { scale, scale });
                    props.Add(new[] { tracker.name, "Transform.m_LocalScale.z" }, new[] { scale, scale });
                }
                
                AnimatorState state = new AnimatorState
                {
                    name = playerHeight.DisplayText,
                    writeDefaultValues = false,
                    motion = Utility.CreateClip("PlayerHeightIndex/" + playerHeight.Index.ToString(), props, settings.assetFolder + "/" + settings.uuid),
                };

                ChildAnimatorState stateChild = new ChildAnimatorState
                {
                    state = state,
                    position = new Vector3(410, 190 + (playerHeight.Index * 80), 0),
                };

                layer.stateMachine.states = layer.stateMachine.states.Append(stateChild).ToArray();
                
                // Transition Conditions
                Tuple<string, AnimatorConditionMode, int>[] conditionsToHeight = new Tuple<string, AnimatorConditionMode, int>[]
                {
                    Tuple.Create("ObjectTracking/PlayerHeightIndex", AnimatorConditionMode.Equals, playerHeight.Index),
                };
                
                layer.stateMachine.entryTransitions = layer.stateMachine.entryTransitions.Append(Utility.CreateEntryTransition("isIndex", conditionsToHeight, state)).ToArray();
                AnimatorStateTransition exitTransition = state.AddExitTransition();
                exitTransition.conditions = new[] {
                    new AnimatorCondition
                    {
                        mode = AnimatorConditionMode.NotEqual,
                        parameter = "ObjectTracking/PlayerHeightIndex",
                        threshold = playerHeight.Index
                    }
                };
            }

            Utility.AddSubAssetsToDatabase(layer, animatorController);
            return (animatorController, expressionParameters);
        }

        public (AnimatorController, VRCExpressionParameters) CreateStabilizationLayer(AnimatorController animatorController, VRCExpressionParameters expressionParameters, AnimationClip ignoreClip)
        {
            if (settings.addStabilization == false && settings.addLazyStabilization == false)
            {
                return (animatorController, expressionParameters);
            }
            animatorController = Utility.CreateFloatParameterAndAddToAnimator(animatorController, "VelocityX");
            animatorController = Utility.CreateFloatParameterAndAddToAnimator(animatorController, "VelocityY");
            animatorController = Utility.CreateFloatParameterAndAddToAnimator(animatorController, "VelocityZ");
            if (settings.addStabilization)
            {
                animatorController = Utility.CreateBoolParameterAndAddToAnimator(animatorController, "ObjectTracking/goStabilized");
                animatorController = Utility.CreateBoolParameterAndAddToAnimator(animatorController, "ObjectTracking/isStabilized");
                expressionParameters = Utility.CreateBoolParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/goStabilized", false, false, false);
                expressionParameters = Utility.CreateBoolParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/isStabilized", false, false, false);
            }
            if (settings.addLazyStabilization)
            {
                animatorController = Utility.CreateBoolParameterAndAddToAnimator(animatorController, "ObjectTracking/isLazyStabilized");
                expressionParameters = Utility.CreateBoolParameterAndAddToExpressionParameters(expressionParameters, "ObjectTracking/isLazyStabilized", false, false, true);
                
                VRCParentConstraint WorldDropConstraint = gameObject.AddComponent<VRCParentConstraint>();
                WorldDropConstraint.IsActive = true;
                WorldDropConstraint.Locked = true;
                WorldDropConstraint.Sources.Add(new VRCConstraintSource(gameObject.transform.parent.transform, 1, Vector3.zero, Vector3.zero));
            }

            // Layer
            AnimatorControllerLayer layer = Utility.CreateLayer("ObjectTracking/Stabilization");
            animatorController.layers = animatorController.layers.Append(layer).ToArray();
            
            // Animation State Off
            AnimatorState stateOff = new AnimatorState
            {
                name = "Off",
                writeDefaultValues = false,
                motion = Utility.CreateClip("Stabilization/Off", gameObject.name, "VRCParentConstraint.FreezeToWorld", 0, 0, settings.assetFolder + "/" + settings.uuid)
            };

            VRCAnimatorLocomotionControl locomotionControlOff = ScriptableObject.CreateInstance<VRCAnimatorLocomotionControl>();
            locomotionControlOff.disableLocomotion = false;
            
            VRCAvatarParameterDriver parameterDriverOff = ScriptableObject.CreateInstance<VRCAvatarParameterDriver>();
            var parameters = new List<VRCAvatarParameterDriver.Parameter>();
            if (settings.addStabilization)
            {
                parameters.Add(Utility.ParameterDriverParameterSet("ObjectTracking/isStabilized", false));
            }
            if (settings.addLazyStabilization)
            {
                parameters.Add(Utility.ParameterDriverParameterSet("ObjectTracking/isLazyStabilized", false));
            }
            parameterDriverOff.parameters = parameters;
            parameterDriverOff.localOnly = false;
            stateOff.behaviours = new StateMachineBehaviour[] { locomotionControlOff, parameterDriverOff };

            ChildAnimatorState stateOffChild = new ChildAnimatorState
            {
                state = stateOff,
                position = new Vector3(30, 190, 0)
            };

            layer.stateMachine.states = layer.stateMachine.states.Append(stateOffChild).ToArray();

            // Transition Conditions
            var transitionsOff = new List<AnimatorStateTransition>();
            
            // Animation State On
            if (settings.addStabilization)
            {
                AnimatorState stateOn = new AnimatorState
                {
                    name = "On",
                    writeDefaultValues = false,
                    motion = ignoreClip
                };

                VRCAnimatorLocomotionControl locomotionControlOn =
                    ScriptableObject.CreateInstance<VRCAnimatorLocomotionControl>();
                locomotionControlOn.disableLocomotion = true;

                VRCAvatarParameterDriver parameterDriverOn =
                    ScriptableObject.CreateInstance<VRCAvatarParameterDriver>();
                parameterDriverOn.parameters = new List<VRCAvatarParameterDriver.Parameter>
                {
                    Utility.ParameterDriverParameterSet("ObjectTracking/isStabilized", true),
                    Utility.ParameterDriverParameterSet("ObjectTracking/isLazyStabilized", false)
                };
                parameterDriverOn.localOnly = false;
                stateOn.behaviours = new StateMachineBehaviour[] { locomotionControlOn, parameterDriverOn };

                ChildAnimatorState stateOnChild = new ChildAnimatorState
                {
                    state = stateOn,
                    position = new Vector3(270, 190, 0)
                };

                layer.stateMachine.states = layer.stateMachine.states.Append(stateOnChild).ToArray();
                
                Tuple<string, bool>[] conditionsToOnBools = new Tuple<string, bool>[]
                {
                    Tuple.Create("IsLocal", true),
                    Tuple.Create("ObjectTracking/goStabilized", true),
                };
                Tuple<string, AnimatorConditionMode, float>[] conditionsToOnFloats = new Tuple<string, AnimatorConditionMode, float>[]
                {
                    Tuple.Create("VelocityZ", AnimatorConditionMode.Greater, (float)0),
                };
                transitionsOff.Add(Utility.CreateTransition("isStabilization", conditionsToOnBools, conditionsToOnFloats, stateOn));

                Tuple<string, bool>[] conditionsOnToOff = new Tuple<string, bool>[]
                {
                    Tuple.Create("ObjectTracking/goStabilized", false)
                };
                stateOn.transitions = new[]
                {
                    Utility.CreateTransition("isUnstabilization", conditionsOnToOff, stateOff)
                };
            }
            
            // Animation State Lazy On
            if (settings.addLazyStabilization)
            {
                AnimatorState stateLazyOn = new AnimatorState
                {
                    name = "Lazy On",
                    writeDefaultValues = false,
                    motion = Utility.CreateClip("Stabilization/LazyOn", gameObject.name, "VRCParentConstraint.FreezeToWorld", 1, 1, settings.assetFolder + "/" + settings.uuid)
                };

                VRCAvatarParameterDriver parameterDriverLazyOn =
                    ScriptableObject.CreateInstance<VRCAvatarParameterDriver>();
                parameterDriverLazyOn.parameters = new List<VRCAvatarParameterDriver.Parameter>
                {
                    Utility.ParameterDriverParameterSet("ObjectTracking/isLazyStabilized", false),
                    Utility.ParameterDriverParameterSet("ObjectTracking/isLazyStabilized", true)
                };
                parameterDriverLazyOn.localOnly = false;
                stateLazyOn.behaviours = new StateMachineBehaviour[] { parameterDriverLazyOn };

                ChildAnimatorState stateLazyOnChild = new ChildAnimatorState
                {
                    state = stateLazyOn,
                    position = new Vector3(30, 270, 0)
                };

                layer.stateMachine.states = layer.stateMachine.states.Append(stateLazyOnChild).ToArray();
                
                Tuple<string, bool>[] conditionsToLazyOnBools = new Tuple<string, bool>[]
                {
                    Tuple.Create("IsLocal", true),
                    Tuple.Create("ObjectTracking/goStabilized", false),
                };
                Tuple<string, AnimatorConditionMode, float>[] conditionsToLazyOnFloats = new Tuple<string, AnimatorConditionMode, float>[]
                {
                    Tuple.Create("VelocityX", AnimatorConditionMode.Greater, (float)-1 / 127),
                    Tuple.Create("VelocityX", AnimatorConditionMode.Less, (float)1 / 127),
                    Tuple.Create("VelocityY", AnimatorConditionMode.Greater, (float)-1 / 127),
                    Tuple.Create("VelocityY", AnimatorConditionMode.Less, (float)1 / 127),
                    Tuple.Create("VelocityZ", AnimatorConditionMode.Greater, (float)-1 / 127),
                    Tuple.Create("VelocityZ", AnimatorConditionMode.Less, (float)1 / 127),
                };

                transitionsOff.Add(Utility.CreateTransition("isLazyStabilization", conditionsToLazyOnBools, conditionsToLazyOnFloats, stateLazyOn));
                
                Tuple<string, bool>[] conditionsLazyOnToOffBools = new Tuple<string, bool>[]
                {
                    Tuple.Create("ObjectTracking/goStabilized", true)
                };
                stateLazyOn.transitions = new[]
                {
                    Utility.CreateTransition("isUnstabilization", conditionsLazyOnToOffBools, stateOff)
                };
                
                Tuple<string, AnimatorConditionMode, float>[][] conditionsLazyOnToOffFloatsArray = new Tuple<string, AnimatorConditionMode, float>[][]
                {
                    new [] { Tuple.Create("VelocityX", AnimatorConditionMode.Less, (float)-1 / 127) },
                    new [] { Tuple.Create("VelocityX", AnimatorConditionMode.Greater, (float)1 / 127) },
                    new [] { Tuple.Create("VelocityY", AnimatorConditionMode.Less, (float)-1 / 127) },
                    new [] { Tuple.Create("VelocityY", AnimatorConditionMode.Greater, (float)1 / 127) },
                    new [] { Tuple.Create("VelocityZ", AnimatorConditionMode.Less, (float)-1 / 127) },
                    new [] { Tuple.Create("VelocityZ", AnimatorConditionMode.Greater, (float)1 / 127) },
                };
                foreach (Tuple<string, AnimatorConditionMode, float>[] conditionsLazyOnToOffFloats in conditionsLazyOnToOffFloatsArray)
                {
                    stateLazyOn.transitions = stateLazyOn.transitions.Append(Utility.CreateTransition("isUnstabilization", conditionsLazyOnToOffFloats, stateOff)).ToArray();
                }

            }
            stateOff.transitions = transitionsOff.ToArray();

            Utility.AddSubAssetsToDatabase(layer, animatorController);
            return (animatorController, expressionParameters);
        }
        
        public void Cleanup()
        {
            if (Directory.Exists(settings.assetFolder))
            {
                Utility.RemoveAssets(settings.assetFolder + "/" + settings.uuid);
                if (Directory.GetDirectories(settings.assetFolder, "*", SearchOption.AllDirectories).Length == 0)
                {
                    AssetDatabase.DeleteAsset(settings.assetFolder);
                }
            }
            
            var avatarDescriptor = gameObject.transform.parent.GetComponent<VRCAvatarDescriptor>();
            AnimatorController animatorController = null;
            VRCAvatarDescriptor.CustomAnimLayer[] customAnimLayers = avatarDescriptor.baseAnimationLayers;
            for (int i = 0; i < customAnimLayers.Length; i++)
            {
                if (customAnimLayers[i].type == VRCAvatarDescriptor.AnimLayerType.FX &&
                    customAnimLayers[i].animatorController != null)
                {
                    animatorController = (AnimatorController)customAnimLayers[i].animatorController;
                    break;
                }
            }
            if (animatorController != null)
            {
                Utility.RemoveLayerStartingWith("ObjectTracking/", animatorController);
                Utility.RemoveAnimatorParametersStartingWith("ObjectTracking/", animatorController);
            }

            var expressionParameters = avatarDescriptor.expressionParameters;
            if (expressionParameters != null)
            {
                Utility.RemoveExpressionParametersStartingWith("ObjectTracking/", expressionParameters);
            }
        }
#endif
    }
}
