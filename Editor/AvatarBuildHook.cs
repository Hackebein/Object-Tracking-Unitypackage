#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using VRC.SDKBase.Editor.BuildPipeline;

namespace hackebein.objecttracking
{
    [InitializeOnLoad]
    public class AvatarBuildHook : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => -10042;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            var baseComponent = avatarGameObject.GetComponentInChildren<Base>();
            if (baseComponent == null)
            {
                return true;
            }
            try
            {
                baseComponent.Apply();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("[Hackebein's Object Tracking] Failed to apply object tracking to avatar: " + e.Message);
                return false;
            }
        }
    }
}
#endif