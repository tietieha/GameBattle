using System;
using System.Collections.Generic;

namespace AbilityKit.Ability.Config
{
    /// <summary>
    /// TextAsset 加载器接口，用于抽象化 TextAsset 资源的加载方式。
    /// View 层负责实现此接口（如 Unity Resources 加载），Logic 层仅依赖此接口。
    /// </summary>
    public interface ITextAssetLoader
    {
        /// <summary>
        /// 尝试加载 TextAsset 并获取其文本内容
        /// </summary>
        /// <param name="path">资源路径（不含扩展名）</param>
        /// <param name="text">输出的文本内容</param>
        /// <returns>是否成功加载</returns>
        bool TryLoadText(string path, out string text);

        /// <summary>
        /// 尝试加载 TextAsset 并获取其二进制内容
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="bytes">输出的二进制内容</param>
        /// <returns>是否成功加载</returns>
        bool TryLoadBytes(string path, out byte[] bytes);
    }

    /// <summary>
    /// TextAsset 加载器的可选能力，用于枚举目录下的文件。
    /// </summary>
    public interface ITextAssetDirectoryLoader
    {
        IEnumerable<string> GetTextAssetPaths(string directory, string pattern);
    }
}
