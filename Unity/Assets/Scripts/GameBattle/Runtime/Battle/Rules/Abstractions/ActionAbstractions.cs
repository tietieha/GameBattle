using System;

namespace AbilityKit.Triggering.Runtime.Abstractions
{
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float X => x;
        public float Y => y;
        public float Z => z;
    }

    public interface IBlackboardResolver
    {
        bool TryResolve(int boardId, out IBlackboard board);
        IBlackboard GetOrCreate(int boardId);
        bool TryGetDouble(int boardId, int keyId, out double value);
    }

    public interface IBlackboard
    {
        bool TryGetDouble(int keyId, out double value);
        void Set(int keyId, double value);
        double this[int keyId] { get; set; }
        bool Contains(int keyId);
    }

    public interface IPayloadAccessor
    {
        bool TryGetPayloadDouble(in object args, int fieldId, out double value);
        bool TryGetPayloadObject(in object args, int fieldId, out object value);
        bool TryGetDouble(int fieldId, out double value);
        object Target { get; }
    }

    public interface IVariableRepository
    {
        double GetNumeric(string domainId, string key);
        void SetNumeric(string domainId, string key, double value);
        bool Has(string domainId, string key);
        bool TryGet(string domainId, string key, out double value);
    }

    public interface IVarResolvable
    {
        bool TryResolveVarValue(string domainId, string key, out double value);
    }

    public interface ITimeService
    {
        float DeltaTimeMs { get; }
        float TotalTimeMs { get; }
        long CurrentTimestampMs { get; }
    }

    public interface IEventBus
    {
        void Publish<T>(T evt);
        void Subscribe<T>(Action<T> handler);
        void Unsubscribe<T>(Action<T> handler);
    }

    public interface ILogger
    {
        void Log(string message);
        void Warn(string message);
        void Error(string message);
    }

}
