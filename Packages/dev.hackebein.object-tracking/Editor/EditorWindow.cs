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
        [MenuItem("Tools/Hackebein/Object Tracking Setup")]
        public static void GenerateGameObjectAndSelect()
        {
            GameObject gameObject = Utility.FindOrCreateEmptyGameObject("Object Tracking Setup");
            gameObject.tag = "EditorOnly";
            gameObject.AddComponent<Setup>();
            Selection.activeGameObject = gameObject;

            // try to find the avatar
            try
            {
                GameObject avatar = FindObjectsOfType<GameObject>()
                    .SingleOrDefault(obj => obj.GetComponent(typeof(VRCAvatarDescriptor)));
                gameObject.GetComponent<Setup>().rootGameObject = avatar;
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}
#endif
