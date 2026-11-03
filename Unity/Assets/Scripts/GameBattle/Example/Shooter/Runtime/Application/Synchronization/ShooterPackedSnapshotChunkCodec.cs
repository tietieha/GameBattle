using System;

namespace AbilityKit.Demo.Shooter.Runtime
{
    internal static class ShooterPackedSnapshotChunkCodec
    {
        public static int GetInt(int[] values, int index, int fallback = 0)
        {
            return values != null && index >= 0 && index < values.Length ? values[index] : fallback;
        }

        public static float GetFloat(float[]? values, int index, float fallback = 0f)
        {
            return values != null && index >= 0 && index < values.Length ? values[index] : fallback;
        }

        public static byte GetByte(byte[] values, int index, byte fallback = 0)
        {
            return values != null && index >= 0 && index < values.Length ? values[index] : fallback;
        }

        public static int[] PackPairValues(float[]? left, float[]? right)
        {
            var count = Math.Max(left?.Length ?? 0, right?.Length ?? 0);
            if (count == 0)
            {
                return Array.Empty<int>();
            }

            var values = new int[count * 2];
            for (int i = 0; i < count; i++)
            {
                SetPackedPairValue(values, i, GetFloat(left, i), GetFloat(right, i));
            }

            return values;
        }

        public static void SetPackedPairValue(int[] values, int index, float left, float right)
        {
            values[index * 2] = ShooterRuntimeSnapshotUtility.Quantize(left);
            values[(index * 2) + 1] = ShooterRuntimeSnapshotUtility.Quantize(right);
        }

        public static float GetPackedPairValue(int[] values, int index, int slot)
        {
            return GetInt(values, (index * 2) + slot) / 10000f;
        }
    }
}
