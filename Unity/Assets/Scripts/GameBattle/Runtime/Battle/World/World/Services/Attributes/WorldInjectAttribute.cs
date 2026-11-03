using System;
using AbilityKit.Core.Markers;

namespace AbilityKit.Ability.World.Services.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class WorldInjectAttribute : MarkerAttribute
    {
        public Type ServiceType { get; }
        public bool Required { get; }

        public WorldInjectAttribute(bool required = true)
        {
            Required = required;
        }

        public WorldInjectAttribute(Type serviceType, bool required = true)
        {
            ServiceType = serviceType;
            Required = required;
        }
    }
}
