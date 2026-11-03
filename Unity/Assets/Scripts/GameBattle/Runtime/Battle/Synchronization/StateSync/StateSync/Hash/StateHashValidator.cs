using System;

namespace AbilityKit.Ability.StateSync.Hash
{
    public enum DesyncType
    {
        None = 0,
        Unknown = 99
    }

    public readonly struct ValidationResult
    {
        public bool IsValid { get; }
        public int Frame { get; }
        public StateHash ClientHash { get; }
        public StateHash ServerHash { get; }
        public DesyncType DesyncKind { get; }
        public long Timestamp { get; }

        public ValidationResult(
            bool isValid,
            int frame,
            StateHash clientHash,
            StateHash serverHash,
            DesyncType desyncKind)
        {
            IsValid = isValid;
            Frame = frame;
            ClientHash = clientHash;
            ServerHash = serverHash;
            DesyncKind = desyncKind;
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public static ValidationResult Valid(int frame) => new ValidationResult(true, frame, StateHash.Invalid, StateHash.Invalid, DesyncType.None);
        public static ValidationResult Invalid(int frame, StateHash client, StateHash server, DesyncType kind) =>
            new ValidationResult(false, frame, client, server, kind);

        public override string ToString() => IsValid
            ? $"Valid(frame={Frame})"
            : $"Desync(frame={Frame}, kind={DesyncKind}, client={ClientHash}, server={ServerHash})";
    }

    public sealed class StateHashValidator
    {
        public Action<ValidationResult> OnValidationResult;

        public ValidationResult Validate(int frame, Snapshot.WorldStateSnapshot clientState, Snapshot.WorldStateSnapshot serverState)
        {
            if (clientState == null || serverState == null)
                return ValidationResult.Valid(frame);

            var clientHash = clientState.ComputeHash();
            var serverHash = serverState.ComputeHash();

            if (clientHash == serverHash)
            {
                return ValidationResult.Valid(frame);
            }

            var result = ValidationResult.Invalid(frame, clientHash, serverHash, DesyncType.Unknown);

            OnValidationResult?.Invoke(result);

            return result;
        }

        public ValidationResult Validate(int frame, StateHash clientHash, StateHash serverHash)
        {
            if (clientHash == serverHash)
            {
                return ValidationResult.Valid(frame);
            }

            return ValidationResult.Invalid(frame, clientHash, serverHash, DesyncType.Unknown);
        }
    }
}
