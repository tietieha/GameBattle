using System;
using AbilityKit.Modifiers;

namespace AbilityKit.Triggering.Runtime.Executable
{
    public sealed class DecoratorBuilder
    {
        private ISimpleExecutable _inner;

        public DecoratorBuilder(ISimpleExecutable inner)
        {
            _inner = inner;
        }

        public DecoratorBuilder WithDuration(float durationMs, bool autoStart = true)
        {
            var deco = DecoratorRegistry.CreateDuration(durationMs);
            deco.AutoStart = autoStart;
            deco.Inner = _inner;
            _inner = deco;
            return this;
        }

        public DecoratorBuilder WithTags(params string[] tagNames)
        {
            var tagDeco = DecoratorRegistry.CreateTag(tagNames);
            tagDeco.Inner = _inner;
            _inner = tagDeco;
            return this;
        }

        public DecoratorBuilder WithTags(string required, string ignore = "")
        {
            var tagDeco = DecoratorRegistry.CreateTag();
            tagDeco.RequiredTags = required ?? string.Empty;
            tagDeco.IgnoreTags = ignore ?? string.Empty;
            tagDeco.Inner = _inner;
            _inner = tagDeco;
            return this;
        }

        public DecoratorBuilder WithModifiers(params ModifierData[] modifiers)
        {
            var modDeco = DecoratorRegistry.CreateModifier(modifiers);
            modDeco.Inner = _inner;
            _inner = modDeco;
            return this;
        }

        public DecoratorBuilder WithStack(int initialStack = 1, float stackMultiplier = 1f)
        {
            var stackDeco = DecoratorRegistry.CreateStack(initialStack, stackMultiplier);
            stackDeco.Inner = _inner;
            _inner = stackDeco;
            return this;
        }

        public DecoratorBuilder WithHierarchy(int? parentId = null)
        {
            var hierDeco = DecoratorRegistry.CreateHierarchy(parentId);
            hierDeco.Inner = _inner;
            _inner = hierDeco;
            return this;
        }

        public DecoratorBuilder WithContinuous(string continuationId = null)
        {
            var deco = DecoratorRegistry.CreateContinuous(continuationId);
            deco.Inner = _inner;
            _inner = deco;
            return this;
        }

        public DecoratorBuilder WithCapability(CapabilityId capabilityId = default)
        {
            var deco = DecoratorRegistry.CreateCapability(capabilityId);
            deco.Inner = _inner;
            _inner = deco;
            return this;
        }

        public ISimpleExecutable Build() => _inner;
    }
}
