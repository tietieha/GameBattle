using System;

namespace AbilityKit.Ability.Config
{
    /// <summary>
    /// 空实现的 TextAsset 加载器，用于 .NET 测试环境。
    /// 在 Unity 运行时，View 层应提供真正的 Resources 加载实现。
    /// </summary>
    public sealed class NullTextAssetLoader : ITextAssetLoader
    {
        public static readonly NullTextAssetLoader Instance = new NullTextAssetLoader();

        private NullTextAssetLoader() { }

        public bool TryLoadText(string path, out string text)
        {
            text = null;
            return false;
        }

        public bool TryLoadBytes(string path, out byte[] bytes)
        {
            bytes = null;
            return false;
        }
    }
}
