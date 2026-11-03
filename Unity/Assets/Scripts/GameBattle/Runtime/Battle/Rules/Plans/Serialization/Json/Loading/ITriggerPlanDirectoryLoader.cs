using System;
using System.Collections.Generic;

namespace AbilityKit.Triggering.Runtime.Plan.Json
{
    /// <summary>
    /// 触发器计划目录加载器接口
    /// 支持多文件分离的触发器配置加载
    /// </summary>
    public interface ITriggerPlanDirectoryLoader
    {
        /// <summary>
        /// 加载指定目录下的所有触发器文件
        /// </summary>
        /// <param name="directory">目录路径</param>
        /// <param name="pattern">文件匹配模式，如 "*.json" 或 "skills/*.json"</param>
        /// <returns>合并后的触发器数据库</returns>
        TriggerPlanJsonDatabase LoadDirectory(string directory, string pattern = "*.json");

        TriggerPlanJsonDatabase LoadDirectory(string directory, string pattern, TriggerPlanDirectoryLoadOptions options);

        /// <summary>
        /// 加载多个目录下的触发器文件
        /// </summary>
        TriggerPlanJsonDatabase LoadDirectories(IEnumerable<string> directories, string pattern = "*.json");

        TriggerPlanJsonDatabase LoadDirectories(IEnumerable<string> directories, string pattern, TriggerPlanDirectoryLoadOptions options);

        /// <summary>
        /// 加载主索引文件 + 模块目录
        /// 主索引文件包含所有触发器 ID 的引用，模块目录包含实际配置
        /// </summary>
        /// <param name="manifestPath">主索引文件路径（如 trigger_plans_manifest.json）</param>
        /// <param name="moduleDirectory">模块目录路径</param>
        /// <returns>合并后的触发器数据库</returns>
        TriggerPlanJsonDatabase LoadWithManifest(string manifestPath, string moduleDirectory);

        TriggerPlanJsonDatabase LoadWithManifest(string manifestPath, string moduleDirectory, TriggerPlanDirectoryLoadOptions options);
    }

    /// <summary>
    /// 触发器计划目录加载器接口的文本加载器扩展
    /// </summary>
    public interface ITriggerPlanFileEnumerator
    {
        /// <summary>
        /// 获取目录下所有匹配的文件路径
        /// </summary>
        IEnumerable<string> GetFiles(string directory, string pattern);

        /// <summary>
        /// 读取文件内容
        /// </summary>
        bool TryReadFile(string path, out string content);
    }
}
