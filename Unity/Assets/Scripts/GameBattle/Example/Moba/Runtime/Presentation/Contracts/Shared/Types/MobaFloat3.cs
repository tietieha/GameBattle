using System;

namespace AbilityKit.Demo.Moba.View.Abstractions.Shared.Types
{
    public readonly struct MobaFloat3 : IEquatable<MobaFloat3>
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public MobaFloat3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool Equals(MobaFloat3 other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        public override bool Equals(object obj)
        {
            return obj is MobaFloat3 other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }
    }
}
