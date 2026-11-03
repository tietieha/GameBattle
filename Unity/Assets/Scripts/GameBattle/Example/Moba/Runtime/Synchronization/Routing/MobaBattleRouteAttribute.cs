using System;
using AbilityKit.Core.Markers;

namespace AbilityKit.Demo.Moba.Services
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public class MobaBattleRouteAttribute : MarkerAttribute
    {
        public MobaBattleRouteAttribute(int opCode, MobaBattleRouteKind kind)
        {
            OpCode = opCode;
            Kind = kind;
        }

        public int OpCode { get; }

        public MobaBattleRouteKind Kind { get; }

        public Type PayloadType { get; set; }

        public Type HandlerType { get; set; }

        public string Name { get; set; }

        public override void OnScanned(Type implType, IMarkerRegistry registry)
        {
            if (registry is MobaBattleRouteRegistry routeRegistry)
            {
                routeRegistry.Register(CreateDescriptor(implType));
            }
        }

        protected virtual MobaBattleRouteDescriptor CreateDescriptor(Type ownerType)
        {
            return new MobaBattleRouteDescriptor(
                OpCode,
                Kind,
                ownerType,
                PayloadType,
                HandlerType,
                Name);
        }
    }
}
