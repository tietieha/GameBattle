using System;
using AbilityKit.Core.Mathematics;

namespace AbilityKit.Demo.Moba.Share
{
    /// <summary>
    /// 共享实体唯一标识符
    /// 跨平台统一的实体 ID 类型
    /// </summary>
    public readonly struct ShareEntityId : IEquatable<ShareEntityId>, IComparable<ShareEntityId>
    {
        public readonly int Value;

        public ShareEntityId(int value)
        {
            Value = value;
        }

        public static ShareEntityId Invalid => default;
        public static ShareEntityId Zero => default;

        public bool IsValid => Value > 0;

        public int CompareTo(ShareEntityId other) => Value.CompareTo(other.Value);
        public bool Equals(ShareEntityId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ShareEntityId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();

        public static bool operator ==(ShareEntityId left, ShareEntityId right) => left.Equals(right);
        public static bool operator !=(ShareEntityId left, ShareEntityId right) => !left.Equals(right);
        public static bool operator <(ShareEntityId left, ShareEntityId right) => left.Value < right.Value;
        public static bool operator >(ShareEntityId left, ShareEntityId right) => left.Value > right.Value;
        public static bool operator <=(ShareEntityId left, ShareEntityId right) => left.Value <= right.Value;
        public static bool operator >=(ShareEntityId left, ShareEntityId right) => left.Value >= right.Value;

        public override string ToString() => Value.ToString();
    }

    /// <summary>
    /// 共享实体接口
    /// 定义跨平台的 ECS 实体操作契约
    /// </summary>
    public interface IShareEntity
    {
        /// <summary>
        /// 实体唯一标识符
        /// </summary>
        ShareEntityId Id { get; }

        /// <summary>
        /// 实体名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 所属的世界
        /// </summary>
        IShareEntityWorld World { get; }

        /// <summary>
        /// 实体是否有效（未销毁）
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// 实体是否存活
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// 获取网络 ID
        /// </summary>
        BattleNetId NetId { get; }

        /// <summary>
        /// 尝试获取组件引用
        /// </summary>
        bool TryGetRef<T>(out T component) where T : struct;

        /// <summary>
        /// 设置组件
        /// </summary>
        void SetRef<T>(in T component) where T : struct;

        /// <summary>
        /// 移除组件
        /// </summary>
        bool Remove<T>() where T : struct;

        /// <summary>
        /// 销毁实体
        /// </summary>
        void Destroy();
    }

    /// <summary>
    /// 共享实体世界接口
    /// 定义跨平台的 ECS 世界操作契约
    /// </summary>
    public interface IShareEntityWorld
    {
        /// <summary>
        /// 创建空实体
        /// </summary>
        IShareEntity Create();

        /// <summary>
        /// 创建带名称的实体
        /// </summary>
        IShareEntity Create(string name);

        /// <summary>
        /// 检查实体是否存活
        /// </summary>
        bool IsAlive(ShareEntityId id);

        /// <summary>
        /// 尝试获取实体
        /// </summary>
        bool TryGetEntity(ShareEntityId id, out IShareEntity entity);

        /// <summary>
        /// 获取实体数量
        /// </summary>
        int AliveCount { get; }

        /// <summary>
        /// 清空所有实体
        /// </summary>
        void Clear();

        /// <summary>
        /// 销毁所有实体
        /// </summary>
        void DestroyAll();
    }

    /// <summary>
    /// 实体查找器接口
    /// 管理 BattleNetId 到 IShareEntity 的映射
    /// </summary>
    public interface IShareEntityLookup
    {
        /// <summary>
        /// 绑定网络 ID 到实体
        /// </summary>
        void Bind(BattleNetId netId, IShareEntity entity);

        /// <summary>
        /// 尝试解析网络 ID 获取实体
        /// </summary>
        bool TryResolve(IShareEntityWorld world, BattleNetId netId, out IShareEntity entity);

        /// <summary>
        /// 尝试解析网络 ID 获取实体 ID
        /// </summary>
        bool TryResolveId(BattleNetId netId, out ShareEntityId entityId);

        /// <summary>
        /// 解除绑定
        /// </summary>
        bool Unbind(BattleNetId netId);

        /// <summary>
        /// 清空所有绑定
        /// </summary>
        void Clear();

        /// <summary>
        /// 绑定数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 获取所有绑定的网络 ID
        /// </summary>
        ShareEntityId GetAllNetIds();
    }

    /// <summary>
    /// 实体工厂接口
    /// 定义创建战斗实体的契约
    /// </summary>
    public interface IShareEntityFactory
    {
        /// <summary>
        /// 获取关联的世界
        /// </summary>
        IShareEntityWorld World { get; }

        /// <summary>
        /// 创建角色实体
        /// </summary>
        IShareEntity CreateCharacter(BattleNetId netId, int templateId = 0);

        /// <summary>
        /// 创建弹道实体
        /// </summary>
        IShareEntity CreateProjectile(BattleNetId netId, BattleNetId ownerNetId, int templateId = 0);

        /// <summary>
        /// 创建区域实体（AOE）
        /// </summary>
        IShareEntity CreateArea(BattleNetId netId, BattleNetId ownerNetId, int templateId = 0);

        /// <summary>
        /// 创建野怪实体
        /// </summary>
        IShareEntity CreateNeutral(BattleNetId netId, int templateId = 0);

        /// <summary>
        /// 创建防御塔实体
        /// </summary>
        IShareEntity CreateTower(BattleNetId netId, int templateId = 0);

        /// <summary>
        /// 创建水晶/基地实体
        /// </summary>
        IShareEntity CreateNexus(BattleNetId netId, int templateId = 0);
    }

    /// <summary>
    /// 角色组件接口
    /// 定义角色实体的数据契约
    /// </summary>
    public interface ICharacterComponent
    {
        /// <summary>
        /// 队伍 ID
        /// </summary>
        int TeamId { get; }

        /// <summary>
        /// 当前生命值
        /// </summary>
        float Hp { get; }

        /// <summary>
        /// 最大生命值
        /// </summary>
        float HpMax { get; }

        /// <summary>
        /// 模型 ID
        /// </summary>
        int ModelId { get; }

        /// <summary>
        /// 等级
        /// </summary>
        int Level { get; }

        /// <summary>
        /// 是否已死亡
        /// </summary>
        bool IsDead { get; }

        /// <summary>
        /// 是否无敌
        /// </summary>
        bool IsInvincible { get; }
    }

    /// <summary>
    /// 弹道组件接口
    /// 定义弹道实体的数据契约
    /// </summary>
    public interface IProjectileComponent
    {
        /// <summary>
        /// 拥有者网络 ID
        /// </summary>
        BattleNetId OwnerNetId { get; }

        /// <summary>
        /// 目标网络 ID
        /// </summary>
        BattleNetId TargetNetId { get; }

        /// <summary>
        /// 移动速度
        /// </summary>
        float Speed { get; }

        /// <summary>
        /// 半径
        /// </summary>
        float Radius { get; }

        /// <summary>
        /// 是否已完成
        /// </summary>
        bool IsFinished { get; }

        /// <summary>
        /// 是否为追踪弹
        /// </summary>
        bool IsTracking { get; }
    }

    /// <summary>
    /// 区域组件接口
    /// 定义 AOE 区域实体的数据契约
    /// </summary>
    public interface IAreaComponent
    {
        /// <summary>
        /// 拥有者网络 ID
        /// </summary>
        BattleNetId OwnerNetId { get; }

        /// <summary>
        /// 区域类型
        /// </summary>
        AreaKind Kind { get; }

        /// <summary>
        /// 半径
        /// </summary>
        float Radius { get; }

        /// <summary>
        /// 持续时间（秒）
        /// </summary>
        float Duration { get; }

        /// <summary>
        /// 剩余时间（秒）
        /// </summary>
        float RemainingTime { get; }

        /// <summary>
        /// 是否已完成
        /// </summary>
        bool IsFinished { get; }
    }

    /// <summary>
    /// 区域类型
    /// </summary>
    public enum AreaKind
    {
        None = 0,
        Circle = 1,
        Rectangle = 2,
        Ring = 3,
    }

    /// <summary>
    /// 变换组件接口
    /// 定义实体的位置和朝向
    /// </summary>
    public interface IShareTransformComponent
    {
        /// <summary>
        /// 位置
        /// </summary>
        Vec3 Position { get; set; }

        /// <summary>
        /// 朝向（前向向量）
        /// </summary>
        Vec3 Forward { get; set; }

        /// <summary>
        /// 上向量
        /// </summary>
        Vec3 Up { get; set; }

        /// <summary>
        /// 旋转（Y 轴欧拉角，弧度）
        /// </summary>
        float RotationY { get; set; }

        /// <summary>
        /// 缩放
        /// </summary>
        float Scale { get; set; }

        /// <summary>
        /// 设置位置
        /// </summary>
        void SetPosition(float x, float y, float z);

        /// <summary>
        /// 设置旋转
        /// </summary>
        void SetRotation(float rotationY);

        /// <summary>
        /// 朝向指定目标
        /// </summary>
        void LookAt(Vec3 target);

        /// <summary>
        /// 移动指定距离
        /// </summary>
        void Translate(Vec3 delta);

        /// <summary>
        /// 获取位置距离
        /// </summary>
        float DistanceTo(Vec3 position);

        /// <summary>
        /// 获取位置距离平方
        /// </summary>
        float SqrDistanceTo(Vec3 position);
    }

    /// <summary>
    /// 实体元数据组件接口
    /// 定义实体的基本元信息
    /// </summary>
    public interface IShareMetaComponent
    {
        /// <summary>
        /// 实体类型
        /// </summary>
        EntityKind Kind { get; }

        /// <summary>
        /// 模板 ID
        /// </summary>
        int TemplateId { get; }

        /// <summary>
        /// 拥有者 ID（网络 ID）
        /// </summary>
        BattleNetId OwnerNetId { get; }
    }
}
