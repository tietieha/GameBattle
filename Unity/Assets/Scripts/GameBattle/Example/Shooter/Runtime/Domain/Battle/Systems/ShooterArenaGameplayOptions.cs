using System;

namespace AbilityKit.Demo.Shooter.Runtime
{
    public sealed class ShooterArenaGameplayOptions
    {
        public static ShooterArenaGameplayOptions Disabled { get; } = new ShooterArenaGameplayOptions(false);

        public static ShooterArenaGameplayOptions CreateCircular(float radius)
        {
            return new ShooterArenaGameplayOptions(true, radius);
        }

        public ShooterArenaGameplayOptions(bool enabled)
            : this(enabled, 0f)
        {
        }

        public ShooterArenaGameplayOptions(bool enabled, float radius)
        {
            Enabled = enabled && radius > 0f;
            Radius = Enabled ? radius : 0f;
        }

        public bool Enabled { get; }

        public float Radius { get; }

        public float RadiusSquared => Radius * Radius;
    }

    internal static class ShooterCircularArenaMath
    {
        public static bool IsEnabled(ShooterArenaGameplayOptions? options)
        {
            return options != null && options.Enabled && options.Radius > 0f;
        }

        public static bool IsInside(float x, float y, ShooterArenaGameplayOptions? options)
        {
            if (!IsEnabled(options))
            {
                return true;
            }

            return x * x + y * y <= options!.RadiusSquared;
        }

        public static void Clamp(ref float x, ref float y, ShooterArenaGameplayOptions? options)
        {
            if (!IsEnabled(options))
            {
                return;
            }

            Clamp(ref x, ref y, options!.Radius);
        }

        public static void Clamp(ref float x, ref float y, float radius)
        {
            if (radius <= 0f)
            {
                return;
            }

            var distanceSquared = x * x + y * y;
            var radiusSquared = radius * radius;
            if (distanceSquared <= radiusSquared)
            {
                return;
            }

            if (distanceSquared <= 0.000001f)
            {
                x = 0f;
                y = 0f;
                return;
            }

            var scale = radius / MathF.Sqrt(distanceSquared);
            x *= scale;
            y *= scale;
        }

        public static float ClampSpawnRadius(float requestedRadius, ShooterArenaGameplayOptions? options)
        {
            if (!IsEnabled(options))
            {
                return requestedRadius;
            }

            return MathF.Max(0f, MathF.Min(requestedRadius, options!.Radius));
        }
    }
}
