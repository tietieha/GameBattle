using System;
using AbilityKit.Core.Mathematics;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 战斗视图接口
    /// 定义战斗视图的基础契约，供不同平台实现（Unity GameObject / Console UI）
    /// </summary>
    public interface IBattleView
    {
        /// <summary>
        /// 获取视图 ID
        /// </summary>
        int ViewId { get; }

        /// <summary>
        /// 获取视图类型
        /// </summary>
        BattleViewType ViewType { get; }

        /// <summary>
        /// 是否有效
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// 获取位置
        /// </summary>
        Vec3 GetPosition();

        /// <summary>
        /// 设置位置
        /// </summary>
        void SetPosition(Vec3 position);

        /// <summary>
        /// 设置旋转（欧拉角）
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
        /// 销毁视图
        /// </summary>
        void Destroy();
    }

    /// <summary>
    /// 战斗视图类型
    /// </summary>
    public enum BattleViewType
    {
        /// <summary>
        /// 角色/单位视图
        /// </summary>
        Character = 0,

        /// <summary>
        /// 特效视图
        /// </summary>
        Vfx = 1,

        /// <summary>
        /// 弹道视图
        /// </summary>
        Projectile = 2,

        /// <summary>
        /// 区域视图
        /// </summary>
        Area = 3,

        /// <summary>
        /// 指示器视图
        /// </summary>
        Indicator = 4,
    }

    /// <summary>
    /// 角色视图接口
    /// 专用于角色/单位的视图
    /// </summary>
    public interface ICharacterView : IBattleView
    {
        /// <summary>
        /// 角色 ID
        /// </summary>
        int ActorId { get; }

        /// <summary>
        /// 模型 ID
        /// </summary>
        int ModelId { get; }

        /// <summary>
        /// 设置跟随目标
        /// </summary>
        void SetFollowTarget(ICharacterView target);

        /// <summary>
        /// 设置父级
        /// </summary>
        void SetParent(ICharacterView parent);

        /// <summary>
        /// 播放动画
        /// </summary>
        void PlayAnimation(string animName, float crossFade = 0f);

        /// <summary>
        /// 停止动画
        /// </summary>
        void StopAnimation(string animName);
    }
}
