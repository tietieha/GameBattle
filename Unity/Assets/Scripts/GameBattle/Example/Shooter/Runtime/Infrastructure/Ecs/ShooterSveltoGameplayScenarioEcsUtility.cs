#nullable enable

using System;
using System.Collections.Generic;
using AbilityKit.World.Svelto;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.Internal;

namespace AbilityKit.Demo.Shooter.Runtime.Infrastructure.Ecs
{
    internal static class ShooterSveltoGameplayScenarioEcsUtility
    {
        public static bool RemoveGroupIfExists(ISveltoWorldContext context, ExclusiveGroupStruct group)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            context.EntityFunctions.RemoveEntitiesFromGroup(group);
            context.SubmitEntities();
            return true;
        }

        public static void RebuildIndex(Dictionary<uint, int> indexByEntityId, NativeEntityIDs ids, int count)
        {
            indexByEntityId.Clear();
            for (var i = 0; i < count; i++)
            {
                indexByEntityId[ids[i]] = i;
            }
        }

        public static bool IsAlive(uint entityId, NB<ShooterSveltoHealthComponent> healths, int count, Dictionary<uint, int> indexByEntityId)
        {
            if (!indexByEntityId.TryGetValue(entityId, out var index) || index < 0 || index >= count)
            {
                return false;
            }

            return healths[index].Alive != 0 && healths[index].Current > 0;
        }

        public static bool TryGetTransform(uint entityId, NB<ShooterSveltoTransformComponent> transforms, int count, Dictionary<uint, int> indexByEntityId, out ShooterSveltoTransformComponent transform)
        {
            if (!indexByEntityId.TryGetValue(entityId, out var index) || index < 0 || index >= count)
            {
                transform = default;
                return false;
            }

            transform = transforms[index];
            return true;
        }

        public static bool TryGetLiveTarget(uint entityId, NB<ShooterSveltoTransformComponent> transforms, NB<ShooterSveltoHealthComponent> healths, int count, Dictionary<uint, int> indexByEntityId, out ShooterSveltoTransformComponent transform)
        {
            if (!IsAlive(entityId, healths, count, indexByEntityId))
            {
                transform = default;
                return false;
            }

            return TryGetTransform(entityId, transforms, count, indexByEntityId, out transform);
        }

        public static void Normalize(ref float x, ref float y)
        {
            var length = MathF.Sqrt(x * x + y * y);
            if (length <= 0f)
            {
                x = 0f;
                y = 0f;
                return;
            }

            x /= length;
            y /= length;
        }
    }
}
