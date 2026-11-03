using System;

namespace AbilityKit.Network.Abstractions
{
    public readonly struct AbilityKitConnectionRole : IEquatable<AbilityKitConnectionRole>
    {
        public static readonly AbilityKitConnectionRole GatewayReliable = new AbilityKitConnectionRole("gateway.reliable");
        public static readonly AbilityKitConnectionRole BattleRealtime = new AbilityKitConnectionRole("battle.realtime");
        public static readonly AbilityKitConnectionRole BattleReliable = new AbilityKitConnectionRole("battle.reliable");

        public AbilityKitConnectionRole(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Connection role is required.", nameof(value));
            Value = value;
        }

        public string Value { get; }

        public bool Equals(AbilityKitConnectionRole other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is AbilityKitConnectionRole other && Equals(other);

        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);

        public override string ToString() => Value ?? string.Empty;

        public static bool operator ==(AbilityKitConnectionRole left, AbilityKitConnectionRole right) => left.Equals(right);

        public static bool operator !=(AbilityKitConnectionRole left, AbilityKitConnectionRole right) => !left.Equals(right);
    }
}
