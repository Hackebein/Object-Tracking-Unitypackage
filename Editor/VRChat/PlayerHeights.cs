#if VRC_SDK_VRCSDK3 && UNITY_EDITOR
using System;
using System.Collections.Generic;
using hackebein.objecttracking.utility;

namespace hackebein.objecttracking.vrchat
{
    public class PlayerHeight
    {
        public int Index { get; }
        public string DisplayText { get; }
        public double CmValue { get; }

        public PlayerHeight(int index, string displayText, double cmValue)
        {
            Index = index;
            DisplayText = displayText;
            CmValue = cmValue;
        }
    }

    public static class PlayerHeights
    {
        public static readonly List<PlayerHeight> List;
        static PlayerHeights()
        {
            List = new List<PlayerHeight>();
            int index = 0;

            // Add inch-based measurements (3' 0" to 8' 0")
            for (int inches = 3 * 12; inches <= 8 * 12; inches++)
            {
                int feet = inches / 12;
                int remainingInches = inches % 12;
                double cm = Math.Round(inches * 2.54, 2);
                string display = $"{feet}' {remainingInches}\"";
                List.Add(new PlayerHeight(index++, display, cm));
            }

            // Add cm-based measurements (92 to 243 cm)
            for (int cmInt = 92; cmInt <= 243; cmInt++)
            {
                string display = $"{cmInt} cm";
                List.Add(new PlayerHeight(index++, display, cmInt));
            }
        }
        
        public static PlayerHeight GetClosest(double cmValue)
        {
            PlayerHeight closest = List[0];
            double closestDiff = Math.Abs(closest.CmValue - cmValue);
            for (int i = 1; i < List.Count; i++)
            {
                double diff = Math.Abs(List[i].CmValue - cmValue);
                if (diff < closestDiff)
                {
                    closest = List[i];
                    closestDiff = diff;
                }
            }
            return closest;
        }
        public static PlayerHeight GetCurrentHeight()
        {
            double cmValue = RegistryReader.ReadRegistryRawQword(
                RegistryReader.HKEY_CURRENT_USER,
                @"Software\VRChat\VRChat",
                UnityHelper.AddHashToKeyName("PlayerHeight"),
                1.7
            ) * 100;
            return GetClosest(cmValue);
        }
    }
}
#endif
