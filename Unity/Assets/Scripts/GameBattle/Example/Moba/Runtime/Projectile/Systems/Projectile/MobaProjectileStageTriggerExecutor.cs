using System;
using System.Collections.Generic;
using AbilityKit.Ability.Share.Effect;
using AbilityKit.Combat.Projectile;
using AbilityKit.Core.Eventing;
using AbilityKit.Core.Logging;
using AbilityKit.Demo.Moba.Services;

namespace AbilityKit.Demo.Moba.Runtime.Application.Systems.Projectile
{
    internal static class MobaProjectileStageTriggerExecutor
    {
        public static void ExecuteSpawn(MobaProjectileSyncSystem sys, in ProjectileSpawnEvent evt)
        {
            Publish(sys, ProjectileTriggering.Events.Spawn, evt);
            sys.StageTriggers?.ExecuteProjectileSpawn(evt);
        }

        public static void ExecuteTick(MobaProjectileSyncSystem sys, in ProjectileTickEvent evt)
        {
            Publish(sys, ProjectileTriggering.Events.Tick, evt);
            sys.StageTriggers?.ExecuteProjectileTick(evt);
        }

        public static void ExecuteExit(MobaProjectileSyncSystem sys, in ProjectileExitEvent evt)
        {
            Publish(sys, ProjectileTriggering.Events.Exit, evt);
            sys.StageTriggers?.ExecuteProjectileExit(evt);
        }

        public static void ExecuteHit(MobaProjectileSyncSystem sys, in ProjectileHitEvent evt, int hitActorId)
        {
            Publish(sys, ProjectileTriggering.Events.Hit, evt);
            sys.StageTriggers?.ExecuteProjectileHit(evt, hitActorId);
        }

        private static void Publish<T>(MobaProjectileSyncSystem sys, string eventId, T evt)
        {
            var eventBus = sys.EventBus;
            if (eventBus == null) return;

            try
            {
                var eid = TriggeringIdUtil.GetEventEid(eventId);
                eventBus.Publish(new EventKey<T>(eid), in evt);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, $"[MobaProjectileStageTriggerExecutor] Publish projectile event failed. eventId={eventId}");
                throw;
            }
        }
    }
}
