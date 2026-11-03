using System;
using AbilityKit.Core.Mathematics;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 视图工厂接口
    /// 负责创建和管理视图对象（GameObject/Console UI）
    /// 不同平台提供各自的实现
    /// </summary>
    public interface IViewFactory
    {
        /// <summary>
        /// 创建角色视图
        /// </summary>
        IViewHandle CreateCharacterView(int actorId, int modelId);

        /// <summary>
        /// 创建特效视图
        /// </summary>
        IViewHandle CreateVfxView(int vfxId);

        /// <summary>
        /// 创建弹道视图
        /// </summary>
        IViewHandle CreateProjectileView(int projectileId, int templateId);

        /// <summary>
        /// 销毁视图
        /// </summary>
        void DestroyView(IViewHandle handle);
    }

    /// <summary>
    /// 视图句柄接口
    /// 封装平台特定的视图对象引用
    /// </summary>
    public interface IViewHandle : IDisposable
    {
        /// <summary>
        /// 设置位置
        /// </summary>
        void SetPosition(Vec3 position);

        /// <summary>
        /// 获取位置
        /// </summary>
        Vec3 GetPosition();

        /// <summary>
        /// 设置跟随目标
        /// </summary>
        void SetFollowTarget(IViewHandle target);

        /// <summary>
        /// 设置父级
        /// </summary>
        void SetParent(IViewHandle parent);

        /// <summary>
        /// 设置旋转
        /// </summary>
        void SetRotation(Vec3 eulerAngles);

        /// <summary>
        /// 设置缩放
        /// </summary>
        void SetScale(Vec3 scale);

        /// <summary>
        /// 设置激活状态
        /// </summary>
        void SetActive(bool active);

        /// <summary>
        /// 是否有效
        /// </summary>
        bool IsValid { get; }
    }
}
