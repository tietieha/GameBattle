using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace AbilityKit.Game.Test.UnitTest
{
    public static class MobaAcceptanceTraceAssert
    {
        public static void AssertTraceNodeKindInRoot(MobaAcceptanceTraceRecord[] records, long rootId, string kind, int configId, string message)
        {
            Assert.IsNotNull(records, "Trace records are required for strict root trace validation.");

            for (var i = 0; i < records.Length; i++)
            {
                var record = records[i];
                if (!string.Equals(record.kind, kind, StringComparison.OrdinalIgnoreCase)) continue;
                if (record.configId != configId) continue;
                if (rootId > 0 && record.rootId != rootId) continue;
                return;
            }

            Assert.Fail(message + $" kind={kind}, configId={configId}, rootId={rootId}.");
        }

        public static void AssertSingleTargetDamageInRoot(MobaAcceptanceTraceRecord[] records, long rootId, int configId, int expectedHitCount, string message)
        {
            var hits = CollectTraceNodesInRoot(records, rootId, "DamageApply", configId);
            Assert.AreEqual(expectedHitCount, hits.Count, message + $" configId={configId}, rootId={rootId}, actual={hits.Count}, expected={expectedHitCount}.");
            Assert.Greater(hits[0].targetActorId, 0, message + " hit must target a valid actor.");

            for (var i = 1; i < hits.Count; i++)
            {
                Assert.AreEqual(hits[0].targetActorId, hits[i].targetActorId, message + $" hitIndex={i} should remain on the same target actor.");
            }
        }

        public static void AssertRepeatHitDamagePattern(MobaAcceptanceTraceRecord[] records, long rootId, int configId, int expectedHitCount, string message)
        {
            var hits = CollectTraceNodesInRoot(records, rootId, "DamageApply", configId);
            Assert.AreEqual(expectedHitCount, hits.Count, message + $" configId={configId}, rootId={rootId}, actual={hits.Count}, expected={expectedHitCount}.");

            var targetActorId = hits[0].targetActorId;
            Assert.Greater(targetActorId, 0, message + " first hit must target a valid actor.");

            for (var i = 0; i < hits.Count; i++)
            {
                Assert.AreEqual(targetActorId, hits[i].targetActorId, message + $" hitIndex={i} should remain on the same target actor.");
                if (i == 0) continue;
                Assert.GreaterOrEqual(hits[i].timeMs, hits[i - 1].timeMs, message + $" hitIndex={i} should not occur earlier than the previous hit.");
                Assert.Greater(hits[i].frame, hits[i - 1].frame, message + $" hitIndex={i} should occur on a later frame than the previous hit.");
            }
        }

        public static void AssertEffectOccursAfter(MobaAcceptanceTraceRecord[] records, int laterEffectId, int earlierEffectId, string message)
        {
            Assert.IsNotNull(records, "Trace records are required for effect order validation.");
            Assert.IsTrue(TryGetFirstTraceTime(records, "EffectExecution", earlierEffectId, out var earlierTimeMs), message + $" Missing earlier effect={earlierEffectId}.");
            Assert.IsTrue(TryGetFirstTraceTime(records, "EffectExecution", laterEffectId, out var laterTimeMs), message + $" Missing later effect={laterEffectId}.");
            Assert.Greater(laterTimeMs, earlierTimeMs, message + $" laterEffect={laterEffectId}, earlierEffect={earlierEffectId}, laterTimeMs={laterTimeMs}, earlierTimeMs={earlierTimeMs}.");
        }

        public static List<MobaAcceptanceTraceRecord> CollectTraceNodesInRoot(MobaAcceptanceTraceRecord[] records, long rootId, string kind, int configId)
        {
            Assert.IsNotNull(records, "Trace records are required for strict root trace validation.");

            var matches = new List<MobaAcceptanceTraceRecord>();
            for (var i = 0; i < records.Length; i++)
            {
                var record = records[i];
                if (!string.Equals(record.kind, kind, StringComparison.OrdinalIgnoreCase)) continue;
                if (record.configId != configId) continue;
                if (rootId > 0 && record.rootId != rootId) continue;
                matches.Add(record);
            }

            return matches;
        }

        public static List<long> CollectEffectRootIds(MobaAcceptanceTraceRecord[] records, int effectId)
        {
            Assert.IsNotNull(records, "Trace records are required for strict effect-root validation.");

            var rootIds = new List<long>();
            for (var i = 0; i < records.Length; i++)
            {
                var record = records[i];
                if (!string.Equals(record.kind, "EffectExecution", StringComparison.OrdinalIgnoreCase)) continue;
                if (record.configId != effectId) continue;
                if (record.rootId <= 0) continue;
                if (rootIds.Contains(record.rootId)) continue;
                rootIds.Add(record.rootId);
            }

            return rootIds;
        }

        private static bool TryGetFirstTraceTime(MobaAcceptanceTraceRecord[] records, string kind, int configId, out int timeMs)
        {
            timeMs = 0;
            if (records == null) return false;

            var found = false;
            var earliest = int.MaxValue;
            for (var i = 0; i < records.Length; i++)
            {
                var record = records[i];
                if (!string.Equals(record.kind, kind, StringComparison.OrdinalIgnoreCase)) continue;
                if (record.configId != configId) continue;
                if (record.timeMs < earliest) earliest = record.timeMs;
                found = true;
            }

            if (!found) return false;
            timeMs = earliest;
            return true;
        }
    }
}
