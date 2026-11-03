using System;
using System.Collections.Generic;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 战斗实体查询器接口
    /// 定义查询战斗实体的契约
    /// </summary>
    public interface IBattleEntityQuery
    {
        /// <summary>
        /// 获取关联的世界对象
        /// </summary>
        object World { get; }

        /// <summary>
        /// 尝试获取变换组件
        /// </summary>
        bool TryGetTransform(BattleNetId id, out ITransformHandle transform);

        /// <summary>
        /// 尝试解析实体
        /// </summary>
        bool TryResolve(BattleNetId id, out IEntityHandle entity);

        /// <summary>
        /// 获取所有脏实体
        /// </summary>
        IReadOnlyList<int> GetDirtyEntities();

        /// <summary>
        /// 获取实体数量
        /// </summary>
        int EntityCount { get; }
    }

    /// <summary>
    /// 战斗网络 ID
    /// 唯一标识战斗中的实体
    /// </summary>
    public readonly struct BattleNetId : IEquatable<BattleNetId>
    {
        public readonly int Value;

        public BattleNetId(int value)
        {
            Value = value;
        }

        public bool IsValid => Value > 0;
        public static BattleNetId Invalid => default;

        public bool Equals(BattleNetId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is BattleNetId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public static bool operator ==(BattleNetId left, BattleNetId right) => left.Equals(right);
        public static bool operator !=(BattleNetId left, BattleNetId right) => !left.Equals(right);
        public override string ToString() => $"BattleNetId({Value})";
    }

    /// <summary>
    /// 变换组件句柄接口
    /// 提供实体的变换信息
    /// </summary>
    public interface ITransformHandle
    {
        /// <summary>
        /// 获取位置
        /// </summary>
        AbilityKit.Core.Mathematics.Vec3 Position { get; }

        /// <summary>
        /// 获取旋转（Y 轴欧拉角）
        /// </summary>
        float RotationY { get; }

        /// <summary>
        /// 获取缩放
        /// </summary>
        float Scale { get; }

        /// <summary>
        /// 获取网络 ID
        /// </summary>
        BattleNetId NetId { get; }

        /// <summary>
        /// 是否有效
        /// </summary>
        bool IsValid { get; }
    }

    /// <summary>
    /// 实体句柄接口
    /// 提供实体的通用访问
    /// </summary>
    public interface IEntityHandle
    {
        /// <summary>
        /// 获取实体 ID
        /// </summary>
        int Id { get; }

        /// <summary>
        /// 获取网络 ID
        /// </summary>
        BattleNetId NetId { get; }

        /// <summary>
        /// 获取实体类型
        /// </summary>
        EntityKind Kind { get; }

        /// <summary>
        /// 获取模板 ID
        /// </summary>
        int TemplateId { get; }

        /// <summary>
        /// 获取拥有者 ID
        /// </summary>
        int OwnerId { get; }

        /// <summary>
        /// 是否有效
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// 尝试获取组件
        /// </summary>
        bool TryGetComponent<T>(out T component) where T : struct;
    }

    /// <summary>
    /// 实体类型
    /// </summary>
    public enum EntityKind
    {
        /// <summary>
        /// 无效
        /// </summary>
        None = 0,

        /// <summary>
        /// 角色
        /// </summary>
        Character = 1,

        /// <summary>
        /// 弹道
        /// </summary>
        Projectile = 2,

        /// <summary>
        /// 区域（AOE）
        /// </summary>
        Area = 3,

        /// <summary>
        /// 野怪
        /// </summary>
        Neutral = 4,

        /// <summary>
        /// 防御塔
        /// </summary>
        Tower = 5,

        /// <summary>
        /// 水晶/基地
        /// </summary>
        Nexus = 6,
    }

    /// <summary>
    /// 视图绑定器接口
    /// 定义 ECS 实体与视图对象的绑定关系
    /// </summary>
    public interface IViewBinder
    {
        /// <summary>
        /// 绑定实体到视图
        /// </summary>
        void Bind(BattleNetId netId, IViewHandle view);

        /// <summary>
        /// 解除绑定
        /// </summary>
        void Unbind(BattleNetId netId);

        /// <summary>
        /// 同步实体到视图
        /// </summary>
        void Sync(IEntityHandle entity);

        /// <summary>
        /// 尝试获取绑定的视图
        /// </summary>
        bool TryGetView(BattleNetId netId, out IViewHandle view);

        /// <summary>
        /// 清空所有绑定
        /// </summary>
        void ClearAll();
    }
}
