using System;
using System.Collections.Generic;
using AbilityKit.Ability.Config;
using AbilityKit.Demo.Moba.Config.BattleDemo;

namespace AbilityKit.Demo.Moba.Config.Core
{
    /// <summary>
    /// Luban 配置组。
    /// 封装 Luban 导出的 JSON 配置加载和反序列化逻辑。
    /// 
    /// 使用方式：
    /// ```csharp
    /// var group = LubanConfigGroup.Create(textAssetLoader, "luban/moba");
    /// var db = new MobaConfigDatabase(MobaConfigRegistry.Instance);
    /// db.LoadFromGroups(new[] { group });
    /// ```
    /// </summary>
    public sealed class LubanConfigGroup : IConfigGroup
    {
        public string Name { get; }
        public IConfigGroupLoader Loader { get; }
        public IConfigGroupDeserializer Deserializer => LubanConfigGroupDeserializer.Instance;
        public IReadOnlyList<ConfigTableDefinition> Tables { get; }

        private LubanConfigGroup(
            string name,
            IConfigGroupLoader loader,
            IReadOnlyList<ConfigTableDefinition> tables)
        {
            Name = name;
            Loader = loader;
            Tables = tables;
        }

        /// <summary>
        /// 创建 Luban 配置组
        /// </summary>
        /// <param name="textAssetLoader">平台相关的文本加载器</param>
        /// <param name="resourcesDir">Luban 导出 JSON 的资源目录</param>
        /// <returns>Luban 配置组</returns>
        public static LubanConfigGroup Create(ITextAssetLoader textAssetLoader, string resourcesDir = "luban/moba")
        {
            if (textAssetLoader == null) throw new ArgumentNullException(nameof(textAssetLoader));

            var loader = new LubanConfigGroupLoader(textAssetLoader, resourcesDir);
            return new LubanConfigGroup(
                name: "Luban",
                loader: loader,
                tables: MobaRuntimeConfigTableRegistry.Tables);
        }

        /// <summary>
        /// 创建带有自定义名称的 Luban 配置组
        /// </summary>
        public static LubanConfigGroup Create(
            string name,
            ITextAssetLoader textAssetLoader,
            string resourcesDir = "luban/moba")
        {
            if (textAssetLoader == null) throw new ArgumentNullException(nameof(textAssetLoader));

            var loader = new LubanConfigGroupLoader(textAssetLoader, resourcesDir);
            return new LubanConfigGroup(
                name: name,
                loader: loader,
                tables: MobaRuntimeConfigTableRegistry.Tables);
        }
    }
}
