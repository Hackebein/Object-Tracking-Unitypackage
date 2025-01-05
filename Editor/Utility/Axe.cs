using System;

namespace hackebein.objecttracking
{
    [Serializable]
    public class AxeTarget
    {
        public int Bits;
        public int BitsLimitMin { get; }
        public int BitsLimitMax { get; }
        public float ValueLimitMin { get; }
        public float ValueLimitMax { get; }
        public float ValueMin;
        public float ValueMax;
        
        public AxeTarget(int bits, int bitsLimitMin, int bitsLimitMax, float valueLimitMin, float valueLimitMax, float valueMin, float valueMax)
        {
            Bits = bits;
            BitsLimitMin = bitsLimitMin;
            BitsLimitMax = bitsLimitMax;
            ValueLimitMin = valueLimitMin;
            ValueLimitMax = valueLimitMax;
            ValueMin = valueMin;
            ValueMax = valueMax;
        }
            
    }
    
    [Serializable]
    public class Axe
    {
        public AxeTarget Local { get; }
        public AxeTarget Remote { get; }
        
        public Axe(AxeTarget local, AxeTarget remote)
        {
            Local = local;
            Remote = remote;
        }
    }

    [Serializable]
    public class AxeGroup
    {
        public Axe X { get; }
        public Axe Y { get; }
        public Axe Z { get; }

        public AxeGroup(Axe x, Axe y, Axe z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
    
    [Serializable]
    public class Axes
    {
        public AxeGroup Position { get; }
        public AxeGroup Rotation { get; }
        
        public static int DefaultMinBitsPerPositionAxisOnLocal = 0;
        public static int DefaultMaxBitsPerPositionAxisOnLocal = 32;
        public static int DefaultBitsPerPositionAxisOnLocal = 32;
        
        public static float DefaultMinPositionAxisOnLocal = -12;
        public static float DefaultMaxPositionAxisOnLocal = 12;
        
        public static int DefaultMinBitsPerRotationAxisOnLocal = 0;
        public static int DefaultMaxBitsPerRotationAxisOnLocal = 32;
        public static int DefaultBitsPerRotationAxisOnLocal = 32;
        
        public static float DefaultMinRotationAxisOnLocal = -180;
        public static float DefaultMaxRotationAxisOnLocal = 180;
        
        public static int DefaultMinBitsPerPositionAxisOnRemote = 0;
        public static int DefaultMaxBitsPerPositionAxisOnRemote = 32;
        public static int DefaultBitsPerPositionAxisOnRemote = 10;
        
        public static float DefaultMinPositionAxisOnRemote = -5;
        public static float DefaultMaxPositionAxisOnRemote = 5;
        
        public static int DefaultMinBitsPerRotationAxisOnRemote = 0;
        public static int DefaultMaxBitsPerRotationAxisOnRemote = 32;
        public static int DefaultBitsPerRotationAxisOnRemote = 6;
        
        public static float DefaultMinRotationAxisOnRemote = -180;
        public static float DefaultMaxRotationAxisOnRemote = 180;
        public static readonly Axes Default;

        public Axes()
        {
            Position = new AxeGroup(
                new Axe(
                    new AxeTarget(DefaultBitsPerPositionAxisOnLocal, DefaultMinBitsPerPositionAxisOnLocal, DefaultMaxBitsPerPositionAxisOnLocal, DefaultMinPositionAxisOnLocal, DefaultMaxPositionAxisOnLocal, DefaultMinPositionAxisOnLocal, DefaultMaxPositionAxisOnLocal),
                    new AxeTarget(DefaultBitsPerPositionAxisOnRemote, DefaultMinBitsPerPositionAxisOnRemote, DefaultMaxBitsPerPositionAxisOnRemote, DefaultMinPositionAxisOnRemote, DefaultMaxPositionAxisOnRemote, DefaultMinPositionAxisOnRemote, DefaultMaxPositionAxisOnRemote)
                ),
                new Axe(
                    new AxeTarget(DefaultBitsPerPositionAxisOnLocal, DefaultMinBitsPerPositionAxisOnLocal, DefaultMaxBitsPerPositionAxisOnLocal, DefaultMinPositionAxisOnLocal, DefaultMaxPositionAxisOnLocal, DefaultMinPositionAxisOnLocal, DefaultMaxPositionAxisOnLocal),
                    new AxeTarget(DefaultBitsPerPositionAxisOnRemote - 1, DefaultMinBitsPerPositionAxisOnRemote, DefaultMaxBitsPerPositionAxisOnRemote, DefaultMinPositionAxisOnRemote, DefaultMaxPositionAxisOnRemote, 0, DefaultMaxPositionAxisOnRemote)
                ),
                new Axe(
                    new AxeTarget(DefaultBitsPerPositionAxisOnLocal, DefaultMinBitsPerPositionAxisOnLocal, DefaultMaxBitsPerPositionAxisOnLocal, DefaultMinPositionAxisOnLocal, DefaultMaxPositionAxisOnLocal, DefaultMinPositionAxisOnLocal, DefaultMaxPositionAxisOnLocal),
                    new AxeTarget(DefaultBitsPerPositionAxisOnRemote, DefaultMinBitsPerPositionAxisOnRemote, DefaultMaxBitsPerPositionAxisOnRemote, DefaultMinPositionAxisOnRemote, DefaultMaxPositionAxisOnRemote, DefaultMinPositionAxisOnRemote, DefaultMaxPositionAxisOnRemote)
                )
            );
            Rotation = new AxeGroup(
                new Axe(
                    new AxeTarget(DefaultBitsPerRotationAxisOnLocal, DefaultMinBitsPerRotationAxisOnLocal, DefaultMaxBitsPerRotationAxisOnLocal, DefaultMinRotationAxisOnLocal, DefaultMaxRotationAxisOnLocal, DefaultMinRotationAxisOnLocal, DefaultMaxRotationAxisOnLocal),
                    new AxeTarget(DefaultBitsPerRotationAxisOnRemote, DefaultMinBitsPerRotationAxisOnRemote, DefaultMaxBitsPerRotationAxisOnRemote, DefaultMinRotationAxisOnRemote, DefaultMaxRotationAxisOnRemote, DefaultMinRotationAxisOnRemote, DefaultMaxRotationAxisOnRemote)
                ),
                new Axe(
                    new AxeTarget(DefaultBitsPerRotationAxisOnLocal, DefaultMinBitsPerRotationAxisOnLocal, DefaultMaxBitsPerRotationAxisOnLocal, DefaultMinRotationAxisOnLocal, DefaultMaxRotationAxisOnLocal, DefaultMinRotationAxisOnLocal, DefaultMaxRotationAxisOnLocal),
                    new AxeTarget(DefaultBitsPerRotationAxisOnRemote, DefaultMinBitsPerRotationAxisOnRemote, DefaultMaxBitsPerRotationAxisOnRemote, DefaultMinRotationAxisOnRemote, DefaultMaxRotationAxisOnRemote, DefaultMinRotationAxisOnRemote, DefaultMaxRotationAxisOnRemote)
                ),
                new Axe(
                    new AxeTarget(DefaultBitsPerRotationAxisOnLocal, DefaultMinBitsPerRotationAxisOnLocal, DefaultMaxBitsPerRotationAxisOnLocal, DefaultMinRotationAxisOnLocal, DefaultMaxRotationAxisOnLocal, DefaultMinRotationAxisOnLocal, DefaultMaxRotationAxisOnLocal),
                    new AxeTarget(DefaultBitsPerRotationAxisOnRemote, DefaultMinBitsPerRotationAxisOnRemote, DefaultMaxBitsPerRotationAxisOnRemote, DefaultMinRotationAxisOnRemote, DefaultMaxRotationAxisOnRemote, DefaultMinRotationAxisOnRemote, DefaultMaxRotationAxisOnRemote)
                )
            );

        }

        public Axes(AxeGroup position, AxeGroup rotation)
        {
            Position = position;
            Rotation = rotation;

        }
    }
}