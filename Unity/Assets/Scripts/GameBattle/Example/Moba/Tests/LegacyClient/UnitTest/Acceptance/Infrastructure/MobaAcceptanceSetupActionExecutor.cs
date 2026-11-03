using System;
using NUnit.Framework;

namespace AbilityKit.Game.Test.UnitTest
{
    /// <summary>
    /// Test-project-only executor for scenario setupActions.
    /// It standardizes environment preparation commands while delegating runtime mutations to the test harness,
    /// so production gameplay services remain the single source of runtime behavior.
    /// </summary>
    public static class MobaAcceptanceSetupActionExecutor
    {
        public static void Execute(MobaSkillConfigTestHarness harness, MobaAcceptanceSetupActionExpectation action)
        {
            if (harness == null) throw new ArgumentNullException(nameof(harness));
            if (action == null) return;
            if (action.enabled == false && string.Equals(action.action, "disabled", StringComparison.OrdinalIgnoreCase)) return;

            if (IsWaitAction(action.action))
            {
                harness.TickMilliseconds(action.durationMs);
                return;
            }

            var command = Normalize(action.action);
            switch (command)
            {
                case "spawn_actor":
                    ExecuteSpawnActor(harness, action);
                    return;
                case "set_attr":
                    ExecuteSetAttr(harness, action);
                    return;
                case "move_to":
                    ExecuteMoveTo(harness, action);
                    return;
                case "add_buff":
                    ExecuteAddBuff(harness, action);
                    return;
                case "remove_buff":
                    ExecuteRemoveBuff(harness, action);
                    return;
                default:
                    Assert.Fail($"Unsupported setup action: {action.action}");
                    return;
            }
        }

        public static bool IsEnvironmentCommand(string action)
        {
            var command = Normalize(action);
            return string.Equals(command, "spawn_actor", StringComparison.Ordinal)
                || string.Equals(command, "set_attr", StringComparison.Ordinal)
                || string.Equals(command, "move_to", StringComparison.Ordinal)
                || string.Equals(command, "add_buff", StringComparison.Ordinal)
                || string.Equals(command, "remove_buff", StringComparison.Ordinal);
        }

        public static bool IsWaitAction(string action)
        {
            return string.Equals(action, "wait", StringComparison.OrdinalIgnoreCase)
                || string.Equals(action, "tick", StringComparison.OrdinalIgnoreCase);
        }

        private static void ExecuteSpawnActor(MobaSkillConfigTestHarness harness, MobaAcceptanceSetupActionExpectation action)
        {
            var alias = FirstNonEmpty(action.alias, action.actorAlias, action.targetAlias);
            Assert.IsFalse(string.IsNullOrEmpty(alias), "spawn_actor requires alias or actorAlias.");

            var actorId = ParseActorId(action.actorId, 0);
            var ownerActorId = action.ownerActorId > 0 ? action.ownerActorId : ResolveOptionalActorId(harness, action.sourceAlias, action.sourceActorId);
            harness.SpawnScenarioActor(
                alias,
                actorId,
                action.kind,
                action.teamId,
                action.heroId,
                action.attributeTemplateId,
                action.level,
                action.unitSubType,
                action.mainType,
                action.playerId,
                ownerActorId,
                action.sourceKind,
                action.sourceId,
                action.position);
        }

        private static void ExecuteSetAttr(MobaSkillConfigTestHarness harness, MobaAcceptanceSetupActionExpectation action)
        {
            var actorId = ResolveRequiredActorId(harness, action.actorAlias, action.actorId, action.targetActorId, "set_attr");
            var value = ResolveValue(action);
            harness.SetScenarioActorAttribute(actorId, action.property, value);
        }

        private static void ExecuteMoveTo(MobaSkillConfigTestHarness harness, MobaAcceptanceSetupActionExpectation action)
        {
            var actorId = ResolveRequiredActorId(harness, action.actorAlias, action.actorId, action.targetActorId, "move_to");
            Assert.IsNotNull(action.position, "move_to requires position.");
            harness.MoveScenarioActor(actorId, action.position);
        }

        private static void ExecuteAddBuff(MobaSkillConfigTestHarness harness, MobaAcceptanceSetupActionExpectation action)
        {
            var targetActorId = ResolveRequiredActorId(harness, FirstNonEmpty(action.targetAlias, action.actorAlias), action.actorId, action.targetActorId, "add_buff");
            var sourceActorId = ResolveOptionalActorId(harness, action.sourceAlias, action.sourceActorId);
            if (sourceActorId <= 0) sourceActorId = targetActorId;
            harness.AddScenarioBuff(targetActorId, action.buffId, sourceActorId, action.durationOverrideMs);
        }

        private static void ExecuteRemoveBuff(MobaSkillConfigTestHarness harness, MobaAcceptanceSetupActionExpectation action)
        {
            var targetActorId = ResolveRequiredActorId(harness, FirstNonEmpty(action.targetAlias, action.actorAlias), action.actorId, action.targetActorId, "remove_buff");
            var sourceActorId = ResolveOptionalActorId(harness, action.sourceAlias, action.sourceActorId);
            harness.RemoveScenarioBuff(targetActorId, action.buffId, sourceActorId, action.removeAll);
        }

        private static int ResolveRequiredActorId(MobaSkillConfigTestHarness harness, string alias, string actorIdText, int explicitActorId, string command)
        {
            var actorId = ResolveOptionalActorId(harness, alias, explicitActorId);
            if (actorId <= 0) actorId = ParseActorId(actorIdText, 0);
            Assert.Greater(actorId, 0, command + " requires actorAlias, actorId or targetActorId.");
            return actorId;
        }

        private static int ResolveOptionalActorId(MobaSkillConfigTestHarness harness, string alias, int explicitActorId)
        {
            if (explicitActorId > 0) return explicitActorId;
            if (!string.IsNullOrEmpty(alias) && harness.TryGetActorId(alias, out var actorId)) return actorId;
            return 0;
        }

        private static int ParseActorId(string actorIdText, int fallback)
        {
            return int.TryParse(actorIdText, out var actorId) && actorId > 0 ? actorId : fallback;
        }

        private static float ResolveValue(MobaAcceptanceSetupActionExpectation action)
        {
            if (Math.Abs(action.value) > float.Epsilon) return action.value;
            if (action.intValue != 0) return action.intValue;
            return 0f;
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null) return null;
            for (var i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrEmpty(values[i])) return values[i];
            }

            return null;
        }

        private static string Normalize(string action)
        {
            return string.IsNullOrEmpty(action) ? string.Empty : action.Trim().ToLowerInvariant();
        }
    }
}
