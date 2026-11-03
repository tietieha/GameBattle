#nullable enable

using AbilityKit.Demo.Shooter.View.Hosting;
using AbilityKit.Protocol.Shooter;

namespace AbilityKit.Demo.Shooter.View.PlayMode
{
    public readonly struct ShooterPlayFrameInput
    {
        private readonly ShooterHostFrameInput _input;

        public ShooterPlayFrameInput(float moveX, float moveY, float aimX, float aimY, bool fire)
            : this(new ShooterHostFrameInput(moveX, moveY, aimX, aimY, fire))
        {
        }

        public ShooterPlayFrameInput(float moveX, float moveY, float aimX, float aimY, bool fire, int attackSlot)
            : this(new ShooterHostFrameInput(moveX, moveY, aimX, aimY, fire, attackSlot))
        {
        }

        public ShooterPlayFrameInput(ShooterHostFrameInput input)
        {
            _input = input;
        }

        public float MoveX => _input.MoveX;
        public float MoveY => _input.MoveY;
        public float AimX => _input.AimX;
        public float AimY => _input.AimY;
        public bool Fire => _input.Fire;
        public int AttackSlot => _input.AttackSlot;

        public ShooterHostFrameInput ToHostInput()
        {
            return _input;
        }
    }

    public interface IShooterPlayInputSource : IShooterHostInputSource
    {
        new ShooterPlayFrameInput ReadInput(int controlledPlayerId);

        ShooterHostFrameInput IShooterHostInputSource.ReadInput(int controlledPlayerId)
        {
            return ReadInput(controlledPlayerId).ToHostInput();
        }
    }

    public static class ShooterPlayInputMapping
    {
        public static ShooterHostFrameInput CreateFrameInput(
            float moveX,
            float moveY,
            bool primaryFire,
            bool spreadFire,
            bool twinFire)
        {
            moveX = Clamp(moveX, -1f, 1f);
            moveY = Clamp(moveY, -1f, 1f);
            var aimX = moveX;
            var aimY = moveY;
            if (aimX * aimX + aimY * aimY <= 0.000001f)
            {
                aimX = 0f;
                aimY = 1f;
            }

            var attackSlot = ShooterPlayerAttackSlots.Primary;
            var fire = primaryFire;
            if (twinFire)
            {
                attackSlot = ShooterPlayerAttackSlots.Twin;
                fire = true;
            }
            else if (spreadFire)
            {
                attackSlot = ShooterPlayerAttackSlots.Spread;
                fire = true;
            }

            return new ShooterHostFrameInput(moveX, moveY, aimX, aimY, fire, attackSlot);
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }

            return value > max ? max : value;
        }
    }

    public interface IShooterPlayViewSink : IShooterHostViewSink
    {
    }
}
