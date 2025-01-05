#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
using System;
using System.Collections.Generic;
using hackebein.objecttracking.utility;
using VRC.SDK3.Avatars.ScriptableObjects;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace hackebein.objecttracking.vrchat
{
    public static class ExpressionMenu
    {
        public static void CreateAsset(Object objectToSave)
        {
            var path = "Assets/Hackebein/Generated/" + objectToSave.GetInstanceID() + ".asset";
            Utility.CreatePathRecursive(System.IO.Path.GetDirectoryName(path));
            AssetDatabase.CreateAsset(objectToSave, path);
        }

        public static VRCExpressionsMenu CreateMenu()
        {
            var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            if (menu.controls == null)
            {
                menu.controls = new List<VRCExpressionsMenu.Control>();
            }

            EditorUtility.SetDirty(menu);
            CreateAsset(menu);
            return menu;
        }

        public static VRCExpressionsMenu CreateSubMenu(VRCExpressionsMenu expressionsMenu, string name, Texture2D icon = null)
        {
            var newMenu = CreateMenu();
            expressionsMenu.controls.Add(new VRCExpressionsMenu.Control()
            {
                name = name,
                icon = icon,
                type = VRCExpressionsMenu.Control.ControlType.SubMenu,
                subMenu = newMenu
            });
            EditorUtility.SetDirty(expressionsMenu);
            return newMenu;
        }

        public static VRCExpressionsMenu CreateIfNeededMoreMenu(VRCExpressionsMenu expressionsMenu)
        {
            if (expressionsMenu.controls.Count >= 8)
            {
                Debug.LogWarning("[Hackebein's Object Tracking] The maximum number of controls in an expression menu is reached. Uploading the avatar might fail.");
                return expressionsMenu;
            }

            if (expressionsMenu.controls.Count == 7)
            {
                return CreateSubMenu(expressionsMenu, "More ...");
            }

            return expressionsMenu;
        }
    }
}
#endif