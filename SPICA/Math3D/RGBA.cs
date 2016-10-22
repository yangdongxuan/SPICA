﻿using SPICA.Serialization.Attributes;

namespace SPICA.Math3D
{
    [Inline]
    public class RGBA
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public RGBA() { }

        public RGBA(byte R, byte G, byte B, byte A)
        {
            this.R = R;
            this.G = G;
            this.B = B;
            this.A = A;
        }

        public uint ToUInt32()
        {
            uint Param = 0;

            Param |= (uint)R << 0;
            Param |= (uint)G << 8;
            Param |= (uint)B << 16;
            Param |= (uint)A << 24;

            return Param;
        }

        public override string ToString()
        {
            return string.Format("R: {0} G: {1} B: {2} A: {3}", R, G, B, A);
        }
    }
}
