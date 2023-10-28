#if VRC_SDK_VRCSDK3
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Hackebein.ObjectTracking
{
    public class ObjectTrackingEditorWindow : EditorWindow
    {
        [MenuItem("Tools/Object Tracking Setup")]
        public static void GenerateGameObjectAndSelect()
        {
            GameObject gameObject = new GameObject();
            gameObject.name = "Object Tracking Setup";
            gameObject.AddComponent<ObjectTrackingSetup>();
            gameObject.tag = "EditorOnly";
            Selection.activeGameObject = gameObject;

            // try to find the avatar
            try
            {
                GameObject avatar = FindObjectsOfType<GameObject>()
                    .SingleOrDefault(obj => obj.GetComponent(typeof(VRCAvatarDescriptor)));
                gameObject.GetComponent<ObjectTrackingSetup>().rootObjectOfAvatar = avatar;
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}
#endif
