namespace AbilityKit.Context
{
    /// <summary>
    /// 按键提供上下文值的接口。
    /// 属性或快照实现该接口后，可被上下文值解析器统一读取。
    /// </summary>
    public interface IContextValueProvider
    {
        bool TryGetValue<T>(string key, out T value);
    }

    /// <summary>
    /// 指定某个上下文属性类型的值读取请求。
    /// </summary>
    public readonly struct ContextValueRequest
    {
        public ContextValueRequest(long contextId, int propertyTypeId, string key = null)
        {
            ContextId = contextId;
            PropertyTypeId = propertyTypeId;
            Key = key;
        }

        public long ContextId { get; }
        public int PropertyTypeId { get; }
        public string Key { get; }
        public bool HasPropertyType => PropertyTypeId > 0;

        public static ContextValueRequest ForProperty<TProperty>(long contextId, string key = null)
            where TProperty : IProperty
        {
            var type = PropertyTypeRegistry.Instance.Get<TProperty>() ?? PropertyTypeRegistry.Instance.Register<TProperty>();
            return new ContextValueRequest(contextId, type.Id, key);
        }
    }
}
