namespace AbilityKit.Demo.Moba.Services
{
    public readonly struct MobaTriggerExecutionRequest<TPayload>
    {
        public MobaTriggerExecutionRequest(int triggerId, TPayload payload, string source = null)
        {
            TriggerId = triggerId;
            Payload = payload;
            Source = source;
        }

        public int TriggerId { get; }
        public TPayload Payload { get; }
        public string Source { get; }
        public bool IsValid => TriggerId > 0 && Payload != null;
        public string PayloadTypeName => Payload != null ? Payload.GetType().Name : null;

        public static MobaTriggerExecutionRequest<TPayload> Create(int triggerId, TPayload payload, string source = null)
        {
            return new MobaTriggerExecutionRequest<TPayload>(triggerId, payload, source);
        }
    }
}
