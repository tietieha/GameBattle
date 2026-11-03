#nullable enable

using System;
using AbilityKit.Protocol.Shooter;
using AbilityKit.World.Svelto;
using Svelto.DataStructures;
using Svelto.ECS;

namespace AbilityKit.Demo.Shooter.Runtime
{
    /// <summary>
    /// A deliberately pure Svelto-style bot AI example for comparison with <see cref="ShooterBotAiService" />.
    /// It owns no controller/FSM state and derives every command from the current ECS player collection.
    /// The default battle pipeline keeps using <see cref="ShooterBotAiServiceBattleSystem" /> because that path
    /// supports explicit Mount/Unmount semantics and profile-driven behaviours.
    /// </summary>
    internal sealed class ShooterPureSveltoBotAiBattleSystem : IShooterBattleSystem
    {
        private const float AttackRange = 5.5f;
        private const float StrafePhaseScale = 0.05f;
        private readonly ShooterBattleState _state;
        private readonly ISveltoWorldContext _context;

        public ShooterPureSveltoBotAiBattleSystem(IShooterBattleServiceResolver services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            _state = services.Resolve<ShooterBattleState>();
            _context = services.Resolve<ISveltoWorldContext>();
        }

        public int Order => ShooterBattleSystemOrder.PlayerBotAi;

        public string name => nameof(ShooterPureSveltoBotAiBattleSystem);

        public void Step(in float deltaTime)
        {
            if (_state.MatchState != ShooterBattleMatchState.Running)
            {
                return;
            }

            var playerCollection = _context.EntitiesDB.QueryEntities<ShooterSveltoPlayerComponent>((ExclusiveGroupStruct)ShooterSveltoGroups.Players);
            playerCollection.Deconstruct(out NB<ShooterSveltoPlayerComponent> players, out _, out var count);
            for (var i = 0; i < count; i++)
            {
                if (!players[i].Alive)
                {
                    _state.InputBuffer.RemoveLatestCommand(players[i].PlayerId);
                    continue;
                }

                var command = CreateCommandForPlayer(in players[i], players, count);
                _state.InputBuffer.SubmitCommand(_state.CurrentFrame, in command);
            }
        }

        private ShooterPlayerCommand CreateCommandForPlayer(
            in ShooterSveltoPlayerComponent self,
            NB<ShooterSveltoPlayerComponent> players,
            int count)
        {
            if (!TryFindNearestLiveOpponent(in self, players, count, out var target, out var distanceSq))
            {
                var wanderPhase = (_state.CurrentFrame + self.PlayerId * 17) * StrafePhaseScale;
                var wanderX = MathF.Cos(wanderPhase);
                var wanderY = MathF.Sin(wanderPhase);
                ShooterBotAiMath.Normalize(ref wanderX, ref wanderY);
                return new ShooterPlayerCommand(self.PlayerId, wanderX * 0.35f, wanderY * 0.35f, self.AimX, self.AimY, false);
            }

            var aimX = target.X - self.X;
            var aimY = target.Y - self.Y;
            ShooterBotAiMath.Normalize(ref aimX, ref aimY);

            var inAttackRange = distanceSq <= AttackRange * AttackRange;
            if (!inAttackRange)
            {
                return new ShooterPlayerCommand(self.PlayerId, aimX, aimY, aimX, aimY, false);
            }

            var strafePhase = (_state.CurrentFrame + self.PlayerId * 11) * StrafePhaseScale;
            var strafe = MathF.Sin(strafePhase) >= 0f ? 1f : -1f;
            var moveX = -aimY * strafe * 0.45f;
            var moveY = aimX * strafe * 0.45f;
            return new ShooterPlayerCommand(self.PlayerId, moveX, moveY, aimX, aimY, fire: true);
        }

        private static bool TryFindNearestLiveOpponent(
            in ShooterSveltoPlayerComponent self,
            NB<ShooterSveltoPlayerComponent> players,
            int count,
            out ShooterSveltoPlayerComponent target,
            out float distanceSq)
        {
            target = default;
            distanceSq = float.MaxValue;
            for (var i = 0; i < count; i++)
            {
                var candidate = players[i];
                if (!candidate.Alive || candidate.PlayerId == self.PlayerId)
                {
                    continue;
                }

                var dx = candidate.X - self.X;
                var dy = candidate.Y - self.Y;
                var candidateDistanceSq = dx * dx + dy * dy;
                if (candidateDistanceSq >= distanceSq)
                {
                    continue;
                }

                distanceSq = candidateDistanceSq;
                target = candidate;
            }

            return target.PlayerId != 0;
        }
    }
}
