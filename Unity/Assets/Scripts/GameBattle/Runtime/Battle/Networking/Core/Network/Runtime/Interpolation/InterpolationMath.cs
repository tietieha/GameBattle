#nullable enable

namespace AbilityKit.Network.Runtime
{
    /// <summary>
    /// Gameplay-agnostic interpolation primitives shared by
    /// <see cref="NetworkSyncModel.AuthoritativeInterpolation"/> style projectors. Keeps the math for
    /// blending remote snapshot fields (scalars and angles) in one place so individual game layers do
    /// not reimplement subtly wrong versions (most notably angle interpolation across the wrap point).
    /// </summary>
    public static class InterpolationMath
    {
        /// <summary>A full turn in radians (2π).</summary>
        public const float TwoPiRadians = 6.283185307179586f;

        /// <summary>A full turn in degrees.</summary>
        public const float FullTurnDegrees = 360f;

        /// <summary>Linearly interpolates a scalar between <paramref name="from"/> and <paramref name="to"/>.</summary>
        public static float Lerp(float from, float to, float t)
        {
            return from + (to - from) * t;
        }

        /// <summary>
        /// Interpolates an angle along the shortest arc, taking the wrap-around at the period boundary
        /// into account so values straddling the seam (e.g. 350° → 10°) rotate the short way instead of
        /// spinning almost all the way around. The result is returned within <c>[0, period)</c>.
        /// </summary>
        /// <param name="fromAngle">Start angle in the same unit as <paramref name="period"/>.</param>
        /// <param name="toAngle">End angle in the same unit as <paramref name="period"/>.</param>
        /// <param name="t">Interpolation factor in [0,1].</param>
        /// <param name="period">The full-turn period (<see cref="TwoPiRadians"/> for radians, <see cref="FullTurnDegrees"/> for degrees).</param>
        public static float LerpAngle(float fromAngle, float toAngle, float t, float period)
        {
            if (period <= 0f)
            {
                return Lerp(fromAngle, toAngle, t);
            }

            float delta = Repeat(toAngle - fromAngle, period);
            float half = period * 0.5f;
            if (delta > half)
            {
                delta -= period;
            }

            return Repeat(fromAngle + delta * t, period);
        }

        /// <summary>Interpolates an angle expressed in radians along the shortest arc.</summary>
        public static float LerpAngleRadians(float fromRadians, float toRadians, float t)
        {
            return LerpAngle(fromRadians, toRadians, t, TwoPiRadians);
        }

        /// <summary>Interpolates an angle expressed in degrees along the shortest arc.</summary>
        public static float LerpAngleDegrees(float fromDegrees, float toDegrees, float t)
        {
            return LerpAngle(fromDegrees, toDegrees, t, FullTurnDegrees);
        }

        /// <summary>
        /// Wraps <paramref name="value"/> into the range <c>[0, period)</c>, mirroring the behaviour of
        /// Unity's <c>Mathf.Repeat</c> without taking a UnityEngine dependency.
        /// </summary>
        public static float Repeat(float value, float period)
        {
            if (period <= 0f)
            {
                return value;
            }

            float result = value - (float)System.Math.Floor(value / (double)period) * period;
            // Guard against floating point landing exactly on the period.
            if (result >= period)
            {
                result -= period;
            }
            else if (result < 0f)
            {
                result += period;
            }

            return result;
        }
    }
}
