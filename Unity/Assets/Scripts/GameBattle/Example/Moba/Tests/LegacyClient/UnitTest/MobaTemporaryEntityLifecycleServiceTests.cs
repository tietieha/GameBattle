using AbilityKit.Demo.Moba.Services;
using NUnit.Framework;

namespace AbilityKit.Game.Test.UnitTest
{
    public sealed class MobaTemporaryEntityLifecycleServiceTests
    {
        [Test]
        public void LifecycleCounters_RecordProjectileEvents_KeepHealthConsistent()
        {
            var service = new MobaTemporaryEntityLifecycleService();

            service.RecordSpawn(MobaTemporaryEntityKind.Projectile, activeCount: 2, frame: 10, count: 2L);
            service.RecordHitEvents(MobaTemporaryEntityKind.Projectile, count: 3L, frame: 11);
            service.RecordExpireEvents(MobaTemporaryEntityKind.Projectile, count: 1L, frame: 12);
            service.RecordDespawn(MobaTemporaryEntityKind.Projectile, activeCount: 1, frame: 13);

            var health = service.GetHealth(MobaTemporaryEntityKind.Projectile);

            Assert.AreEqual(MobaTemporaryEntityKind.Projectile, health.Kind);
            Assert.AreEqual(1, health.ActiveCount);
            Assert.AreEqual(2L, health.SpawnedCount);
            Assert.AreEqual(1L, health.DespawnedCount);
            Assert.AreEqual(3L, health.HitEventCount);
            Assert.AreEqual(1L, health.ExpireEventCount);
            Assert.AreEqual(13, health.LastFrame);
        }

        [Test]
        public void LifecycleCounters_RejectNegativeActiveCount_ClampsToZero()
        {
            var service = new MobaTemporaryEntityLifecycleService();

            service.RecordRejected(MobaTemporaryEntityKind.Summon, activeCount: -5, frame: 21);
            service.RecordReplaced(MobaTemporaryEntityKind.Summon, activeCount: -1, frame: 22);

            var health = service.GetHealth(MobaTemporaryEntityKind.Summon);

            Assert.AreEqual(0, health.ActiveCount);
            Assert.AreEqual(1L, health.RejectedCount);
            Assert.AreEqual(1L, health.ReplacedCount);
            Assert.AreEqual(22, health.LastFrame);
        }

        [Test]
        public void LifecycleCounters_GetAllHealth_WritesAllTemporaryKinds()
        {
            var service = new MobaTemporaryEntityLifecycleService();
            var results = new MobaTemporaryEntityLifecycleHealth[3];

            service.RecordSpawn(MobaTemporaryEntityKind.Projectile, activeCount: 1, frame: 1);
            service.RecordSpawn(MobaTemporaryEntityKind.Area, activeCount: 2, frame: 2, count: 2L);
            service.RecordSpawn(MobaTemporaryEntityKind.Summon, activeCount: 3, frame: 3, count: 3L);

            service.GetAllHealth(results);

            Assert.AreEqual(MobaTemporaryEntityKind.Projectile, results[0].Kind);
            Assert.AreEqual(1L, results[0].SpawnedCount);
            Assert.AreEqual(MobaTemporaryEntityKind.Area, results[1].Kind);
            Assert.AreEqual(2L, results[1].SpawnedCount);
            Assert.AreEqual(MobaTemporaryEntityKind.Summon, results[2].Kind);
            Assert.AreEqual(3L, results[2].SpawnedCount);
        }
    }
}
