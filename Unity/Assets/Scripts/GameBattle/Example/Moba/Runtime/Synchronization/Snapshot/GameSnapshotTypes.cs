namespace AbilityKit.Demo.Moba.Services.Snapshot
{
    /// <summary>
    /// MOBA 战斗快照类型标识。
    /// </summary>
    public static class GameSnapshotTypes
    {
        /// <summary>进入游戏快照</summary>
        public const string EnterGame = "enter_game";

        /// <summary>实体生成快照</summary>
        public const string ActorSpawn = "actor_spawn";

        /// <summary>实体销毁快照</summary>
        public const string ActorDespawn = "actor_despawn";

        /// <summary>实体变换快照（位置、旋转）</summary>
        public const string ActorTransform = "actor_transform";

        /// <summary>实体伤害快照</summary>
        public const string ActorDamage = "actor_damage";

        /// <summary>实体死亡快照</summary>
        public const string ActorDead = "actor_dead";

        /// <summary>技能释放快照</summary>
        public const string SkillCast = "skill_cast";

        /// <summary>Buff 应用快照</summary>
        public const string BuffApply = "buff_apply";

        /// <summary>投射物击中快照</summary>
        public const string ProjectileHit = "projectile_hit";

        /// <summary>组合快照（多个快照合并）</summary>
        public const string Combined = "combined";
    }
}
