﻿namespace ConcreteUI.Graphics.Native.Direct3D
{
    public enum D3DDriverType
    {
        Unknown = 0,
        Hardware,
        Reference,
        Null,
        Software,
        Warp
    }

    public enum D3DFeatureLevel
    {
        Level_1_0_Core = 0x1000,
        Level_9_1 = 0x9100,
        Level_9_2 = 0x9200,
        Level_9_3 = 0x9300,
        Level_10_0 = 0xa000,
        Level_10_1 = 0xa100,
        Level_11_0 = 0xb000,
        Level_11_1 = 0xb100,
        Level_12_0 = 0xc000,
        Level_12_1 = 0xc100,
        Level_12_2 = 0xc200
    }
}
