namespace AbilityKit.Context
{
    /// <summary>
    /// 上下文事件数据
    /// </summary>
    public readonly struct ContextEvent
    {
        public ContextEventType Type { get; }
        public long EntityId { get; }
        public long FlowId { get; }
        public long ParentFlowId { get; }
        public long OwnerEntityId { get; }
        public FlowContextPhase FlowPhase { get; }
        public int PropertyTypeId { get; }
        public string ChangedKey { get; }
        public object OldValue { get; }
        public object NewValue { get; }

        public ContextEvent(
            ContextEventType type,
            long entityId,
            long flowId = 0,
            long parentFlowId = 0,
            long ownerEntityId = 0,
            FlowContextPhase flowPhase = FlowContextPhase.Created,
            int propertyTypeId = 0,
            string changedKey = null,
            object oldValue = null,
            object newValue = null)
        {
            Type = type;
            EntityId = entityId;
            FlowId = flowId;
            ParentFlowId = parentFlowId;
            OwnerEntityId = ownerEntityId;
            FlowPhase = flowPhase;
            PropertyTypeId = propertyTypeId;
            ChangedKey = changedKey;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public static ContextEvent Created(long entityId, long flowId = 0)
        {
            return new ContextEvent(ContextEventType.Created, entityId, flowId);
        }

        public static ContextEvent Updated(
            long entityId,
            long flowId,
            int propertyTypeId,
            string key,
            object oldValue,
            object newValue)
        {
            return new ContextEvent(ContextEventType.Updated, entityId, flowId, propertyTypeId: propertyTypeId, changedKey: key, oldValue: oldValue, newValue: newValue);
        }

        public static ContextEvent Updated(long entityId, int propertyTypeId, string key, object oldValue, object newValue)
        {
            return Updated(entityId, 0, propertyTypeId, key, oldValue, newValue);
        }

        public static ContextEvent Destroying(long entityId, long flowId = 0)
        {
            return new ContextEvent(ContextEventType.Destroying, entityId, flowId);
        }

        public static ContextEvent Destroyed(long entityId, long flowId = 0)
        {
            return new ContextEvent(ContextEventType.Destroyed, entityId, flowId);
        }

        public static ContextEvent FlowCreated(long flowId, long ownerEntityId, long parentFlowId, FlowContextPhase phase)
        {
            return new ContextEvent(ContextEventType.FlowCreated, 0, flowId, parentFlowId, ownerEntityId, phase);
        }

        public static ContextEvent FlowPhaseChanged(long flowId, long ownerEntityId, long parentFlowId, FlowContextPhase phase)
        {
            return new ContextEvent(ContextEventType.FlowPhaseChanged, 0, flowId, parentFlowId, ownerEntityId, phase);
        }
    }
}
