using System;

namespace AbilityKit.Demo.Shooter.Runtime
{
    public static class ShooterBattleMath
    {
        public static float Normalize(ref float x, ref float y)
        {
            var length = (float)Math.Sqrt(x * x + y * y);
            if (length <= 0.0001f)
            {
                x = 0f;
                y = 0f;
                return 0f;
            }

            x /= length;
            y /= length;
            return length;
        }
    }
}
