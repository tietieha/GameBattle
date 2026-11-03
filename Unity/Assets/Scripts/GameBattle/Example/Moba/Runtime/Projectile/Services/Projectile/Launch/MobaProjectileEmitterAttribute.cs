using System;
using AbilityKit.Demo.Moba;

namespace AbilityKit.Demo.Moba.Services.Projectile.Launch
{
    /// <summary>
    /// 将一个投射物发射序列标记为指定投射物发射器类型的实现。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class MobaProjectileEmitterAttribute : Attribute
    {
        public MobaProjectileEmitterAttribute(ProjectileEmitterType emitterType)
        {
            EmitterType = emitterType;
        }

        public ProjectileEmitterType EmitterType { get; }
        public int Priority { get; set; }
        public bool IsDefault { get; set; }
    }
}
