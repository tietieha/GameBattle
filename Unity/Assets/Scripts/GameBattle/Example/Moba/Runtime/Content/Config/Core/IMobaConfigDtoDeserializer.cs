using System;
using AbilityKit.Ability.Config;

namespace AbilityKit.Demo.Moba.Config.Core
{
    /// <summary>
    /// MOBA DTO 反序列化器接口，扩展通用 IConfigDeserializer。
    /// </summary>
    public interface IMobaConfigDtoDeserializer : IConfigDeserializer
    {
        /// <summary>
        /// 从文本反序列化 DTO 数组，供 MOBA 配置加载管线使用。
        /// </summary>
        Array DeserializeDtoArray(string text, Type dtoType);
    }
}
