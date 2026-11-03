using System;
using AbilityKit.Ability.Config;

namespace AbilityKit.Demo.Moba.Config.Core
{
    /// <summary>
    /// Luban 配置组加载器。
    /// 从平台无关的 ITextAssetLoader 加载 Luban 导出的 JSON 配置。
    /// </summary>
    public sealed class LubanConfigGroupLoader : IConfigGroupLoader
    {
        private readonly ITextAssetLoader _textAssetLoader;
        private readonly string _resourcesDir;

        public string ResourcesDir => _resourcesDir;

        public LubanConfigGroupLoader(ITextAssetLoader textAssetLoader, string resourcesDir = "luban/moba")
        {
            _textAssetLoader = textAssetLoader ?? throw new ArgumentNullException(nameof(textAssetLoader));
            _resourcesDir = resourcesDir ?? "luban/moba";
        }

        public bool TryLoad(string tableName, out byte[] bytes, out string text)
        {
            bytes = null;
            text = null;

            // 尝试多种路径格式
            var paths = new[]
            {
                $"{_resourcesDir}/{tableName}",
                $"{_resourcesDir}/{ToUnderscoreCase(tableName)}",
                tableName
            };

            foreach (var path in paths)
            {
                if (_textAssetLoader.TryLoadText(path, out text) && !string.IsNullOrEmpty(text))
                {
                    return true;
                }
            }

            return false;
        }

        private static string ToUnderscoreCase(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            var result = new System.Text.StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (char.IsUpper(c) && i > 0)
                {
                    result.Append('_');
                }
                result.Append(char.ToLower(c));
            }
            return result.ToString();
        }
    }
}
