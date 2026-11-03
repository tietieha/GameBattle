#nullable enable

using System;
using System.Collections.Generic;

namespace AbilityKit.Demo.Shooter.Runtime
{
    public readonly struct ShooterSveltoGameplayScenarioJsonDefinition
    {
        public ShooterSveltoGameplayScenarioJsonDefinition(string id, string displayName, string json)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Scenario id is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Scenario display name is required.", nameof(displayName));
            if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("Scenario json is required.", nameof(json));

            Id = id;
            DisplayName = displayName;
            Json = json;
        }

        public string Id { get; }

        public string DisplayName { get; }

        public string Json { get; }

        public ShooterSveltoGameplayScenarioConfig Parse()
        {
            return ShooterSveltoGameplayScenarioJsonParser.ParseScenario(Json);
        }
    }

    public interface IShooterSveltoGameplayScenarioSource
    {
        IReadOnlyList<ShooterSveltoGameplayScenarioJsonDefinition> Definitions { get; }

        bool TryGetDefinition(string id, out ShooterSveltoGameplayScenarioJsonDefinition definition);

        bool TryGetScenario(string id, out ShooterSveltoGameplayScenarioConfig scenario);
    }

    public sealed class ShooterSveltoGameplayScenarioJsonSource : IShooterSveltoGameplayScenarioSource
    {
        private readonly ShooterSveltoGameplayScenarioJsonDefinition[] _definitions;
        private readonly Dictionary<string, ShooterSveltoGameplayScenarioJsonDefinition> _byId;

        public ShooterSveltoGameplayScenarioJsonSource(IEnumerable<ShooterSveltoGameplayScenarioJsonDefinition> definitions)
        {
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));

            var list = new List<ShooterSveltoGameplayScenarioJsonDefinition>();
            _byId = new Dictionary<string, ShooterSveltoGameplayScenarioJsonDefinition>(StringComparer.OrdinalIgnoreCase);

            foreach (var definition in definitions)
            {
                if (string.IsNullOrWhiteSpace(definition.Id)) continue;
                list.Add(definition);
                _byId[definition.Id] = definition;
            }

            _definitions = list.ToArray();
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
            if (string.IsNullOrWhiteSpace(id))
            {
                definition = default;
                return false;
            }

            return _byId.TryGetValue(id, out definition);
        }

        public bool TryGetScenario(string id, out ShooterSveltoGameplayScenarioConfig scenario)
        {
            if (!TryGetDefinition(id, out var definition))
            {
                scenario = default;
                return false;
            }

            scenario = definition.Parse();
            return true;
        }

        public ShooterSveltoGameplayScenarioConfig GetScenarioOrDefault(string id, in ShooterSveltoGameplayScenarioConfig fallback)
        {
            return TryGetScenario(id, out var scenario) ? scenario : fallback;
        }
    }
}
