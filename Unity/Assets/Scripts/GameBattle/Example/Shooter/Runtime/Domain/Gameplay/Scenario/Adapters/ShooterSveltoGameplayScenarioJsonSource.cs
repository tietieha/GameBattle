#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;

namespace AbilityKit.Demo.Shooter.Runtime.Domain.Gameplay.Scenario.Adapters
{
    public readonly struct ShooterSveltoGameplayScenarioJsonDefinition
    {
        public ShooterSveltoGameplayScenarioJsonDefinition(string id, string displayName, string json)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            Json = json ?? throw new ArgumentNullException(nameof(json));
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Json { get; }
    }

    public interface IShooterSveltoGameplayScenarioSource
    {
        IReadOnlyList<ShooterSveltoGameplayScenarioJsonDefinition> Definitions { get; }

        bool TryGetDefinition(string id, out ShooterSveltoGameplayScenarioJsonDefinition definition);
    }

    public sealed class ShooterSveltoGameplayScenarioJsonSource : IShooterSveltoGameplayScenarioSource, IEnumerable<ShooterSveltoGameplayScenarioJsonDefinition>
    {
        private readonly IReadOnlyList<ShooterSveltoGameplayScenarioJsonDefinition> _definitions;
        private readonly Dictionary<string, ShooterSveltoGameplayScenarioJsonDefinition> _byId;

        public ShooterSveltoGameplayScenarioJsonSource(IEnumerable<ShooterSveltoGameplayScenarioJsonDefinition> definitions)
        {
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));

            var list = new List<ShooterSveltoGameplayScenarioJsonDefinition>();
            _byId = new Dictionary<string, ShooterSveltoGameplayScenarioJsonDefinition>(StringComparer.OrdinalIgnoreCase);

            foreach (var definition in definitions)
            {
                list.Add(definition);
                _byId[definition.Id] = definition;
            }

            _definitions = list;
        }

        public IReadOnlyList<ShooterSveltoGameplayScenarioJsonDefinition> Definitions => _definitions;

        public static ShooterSveltoGameplayScenarioJsonSource BuiltIn { get; } = new ShooterSveltoGameplayScenarioJsonSource(new[]
        {
            new ShooterSveltoGameplayScenarioJsonDefinition(
                "svelto-projectile-storm",
                "Svelto Projectile Storm",
                ShooterSveltoGameplayScenarioJsonCatalog.ProjectileStormJson),
            new ShooterSveltoGameplayScenarioJsonDefinition(
                "svelto-wave-survival",
                "Svelto Wave Survival",
                ShooterSveltoGameplayScenarioJsonCatalog.WaveSurvivalJson)
        });

        public static ShooterSveltoGameplayScenarioJsonSource FromJson(string id, string displayName, string json)
        {
            return new ShooterSveltoGameplayScenarioJsonSource(new[]
            {
                new ShooterSveltoGameplayScenarioJsonDefinition(id, displayName, json)
            });
        }

        public bool TryGetDefinition(string id, out ShooterSveltoGameplayScenarioJsonDefinition definition)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            return _byId.TryGetValue(id, out definition);
        }

        public IEnumerator<ShooterSveltoGameplayScenarioJsonDefinition> GetEnumerator()
        {
            return _definitions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public static class ShooterSveltoGameplayScenarioJsonCatalog
    {
        public static string ProjectileStormJson => "{}";
        public static string WaveSurvivalJson => "{}";
    }
}
