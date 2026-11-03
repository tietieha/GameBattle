using System;
using AbilityKit.Ability.Config;

namespace AbilityKit.Demo.Moba.Config.Core
{
    /// <summary>
    /// MOBA 二进制 DTO 反序列化器接口。
    /// </summary>
    public interface IMobaConfigDtoBytesDeserializer : IConfigDeserializer
    {
        /// <summary>
        /// 从二进制内容反序列化 DTO 数组，供 MOBA 配置加载管线使用。
        /// </summary>
        Array DeserializeDtoArray(byte[] bytes, Type dtoType);
    }
}
