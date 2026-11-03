using System;
using AbilityKit.Coordinator;
using AbilityKit.Core.Logging;
using AbilityKit.Demo.Moba.Config.Core;
using AbilityKit.Demo.Moba.Services;

namespace AbilityKit.Demo.Moba.Session
{
    /// <summary>
    /// 从玩家生成数据构建 Coordinator 首帧快照
    /// </summary>
    public static class MobaEnterGameSnapshotBuilder
    {
        public static FrameSnapshotData BuildEnterGameSnapshot(
            PlayerSpawnData[] spawns,
            MobaConfigDatabase config,
            MobaSessionDefaults defaults = null,
            IMobaBattleDiagnosticsService diagnostics = null)
        {
            defaults = MobaSessionDefaults.OrDefault(defaults);
            if (spawns == null || spawns.Length == 0)
            {
                return new FrameSnapshotData(0, 0, SnapshotType.Full, Array.Empty<SnapshotEntityState>());
            }

            var entities = new EntityState[spawns.Length];
            for (int i = 0; i < spawns.Length; i++)
            {
                var spawn = spawns[i];
                ResolveHp(spawn.CharacterId, config, diagnostics, out var hp, out var hpMax);
                entities[i] = new EntityState
                {
                    EntityId = spawn.PlayerId,
                    X = spawn.X,
                    Y = spawn.Y,
                    Z = spawn.Z,
                    Rotation = defaults.SnapshotRotation,
                    Hp = hp,
                    HpMax = hpMax,
                    TeamId = spawn.TeamId,
                    IsDead = false
                };
            }

            return new FrameSnapshotData(0, 0, SnapshotType.Full, entities);
        }

        public static SnapshotEntityState[] ToEntityStates(
            PlayerSpawnData[] spawns,
            MobaConfigDatabase config,
            MobaSessionDefaults defaults = null,
            IMobaBattleDiagnosticsService diagnostics = null)
        {
            var snapshot = BuildEnterGameSnapshot(spawns, config, defaults, diagnostics);
            return snapshot.Entities;
        }

        private static void ResolveHp(
            int characterId,
            MobaConfigDatabase config,
            IMobaBattleDiagnosticsService diagnostics,
            out float hp,
            out float hpMax)
        {
            hp = 0f;
            hpMax = 0f;

            if (characterId <= 0)
            {
                ThrowInvalidSnapshotConfig(diagnostics, "snapshot.hp.invalidCharacter", $"Snapshot HP config is invalid because characterId is invalid. characterId={characterId}");
            }

            if (config == null)
            {
                ThrowInvalidSnapshotConfig(diagnostics, "snapshot.hp.missingConfig", $"Snapshot HP config is invalid because config database is missing. characterId={characterId}");
            }

            try
            {
                if (!config.TryGetCharacter(characterId, out var character) || character == null)
                {
                    ThrowInvalidSnapshotConfig(diagnostics, "snapshot.hp.missingCharacter", $"Snapshot HP config is invalid because character config is missing. characterId={characterId}");
                }

                var attributeTemplateId = character.AttributeTemplateId;
                if (attributeTemplateId <= 0)
                {
                    ThrowInvalidSnapshotConfig(diagnostics, "snapshot.hp.invalidAttributeTemplate", $"Snapshot HP config is invalid because character attribute template id is invalid. characterId={characterId} attributeTemplateId={attributeTemplateId}");
                }

                if (!config.TryGetAttributeTemplate(attributeTemplateId, out var template) || template == null)
                {
                    ThrowInvalidSnapshotConfig(diagnostics, "snapshot.hp.missingAttributeTemplate", $"Snapshot HP config is invalid because attribute template config is missing. characterId={characterId} attributeTemplateId={attributeTemplateId}");
                }

                if (template.Hp <= 0f)
                {
                    ThrowInvalidSnapshotConfig(diagnostics, "snapshot.hp.invalidHp", $"Snapshot HP config is invalid because template HP must be positive. characterId={characterId} attributeTemplateId={attributeTemplateId} hp={template.Hp}");
                }

                if (template.MaxHp <= 0f)
                {
                    ThrowInvalidSnapshotConfig(diagnostics, "snapshot.hp.invalidMaxHp", $"Snapshot HP config is invalid because template MaxHp must be positive. characterId={characterId} attributeTemplateId={attributeTemplateId} maxHp={template.MaxHp}");
                }

                hp = template.Hp;
                hpMax = template.MaxHp;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ThrowSnapshotException(diagnostics, ex, $"Snapshot HP config lookup failed. characterId={characterId}");
            }
        }

        private static void ThrowInvalidSnapshotConfig(IMobaBattleDiagnosticsService diagnostics, string key, string message)
        {
            var exception = new InvalidOperationException(message);
            if (diagnostics != null)
            {
                diagnostics.Exception(key, exception, message);
            }
            else
            {
                Log.Exception(exception, "[MobaEnterGameSnapshotBuilder] " + message);
            }

            throw exception;
        }

        private static void ThrowSnapshotException(IMobaBattleDiagnosticsService diagnostics, Exception ex, string message)
        {
            var exception = new InvalidOperationException(message, ex);
            if (diagnostics != null)
            {
                diagnostics.Exception("snapshot.hp.exception", exception, message);
            }
            else
            {
                Log.Exception(exception, "[MobaEnterGameSnapshotBuilder] " + message);
            }

            throw exception;
        }
    }
}
