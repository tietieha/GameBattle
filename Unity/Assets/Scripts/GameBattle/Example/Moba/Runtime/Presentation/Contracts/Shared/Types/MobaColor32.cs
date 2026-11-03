using System;

namespace AbilityKit.Demo.Moba.View.Abstractions.Shared.Types
{
    public readonly struct MobaColor32 : IEquatable<MobaColor32>
    {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;
        public readonly byte A;

        public MobaColor32(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public bool Equals(MobaColor32 other)
        {
            return R == other.R && G == other.G && B == other.B && A == other.A;
        }

        public override bool Equals(object obj)
        {
            return obj is MobaColor32 other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)R;
                hashCode = (hashCode * 397) ^ G;
                hashCode = (hashCode * 397) ^ B;
                hashCode = (hashCode * 397) ^ A;
                return hashCode;
            }
        }
    }
}
