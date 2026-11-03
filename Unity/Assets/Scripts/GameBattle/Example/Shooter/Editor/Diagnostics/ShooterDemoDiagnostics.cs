#nullable enable

using System.Collections.Generic;
using AbilityKit.Demo.Shooter.View;
using AbilityKit.Demo.Shooter.View.Hosting;
using AbilityKit.Protocol.Shooter;

namespace AbilityKit.Demo.Shooter.Editor.Diagnostics
{
    /// <summary>
    /// Captured diagnostic snapshot from a running Shooter session.
    /// Updated each Editor tick and displayed in the diagnostics panel.
    /// </summary>
    public sealed class ShooterDemoDiagnostics
    {
        public int Frame { get; set; }
        public int PlayerCount { get; set; }
        public int BulletCount { get; set; }
        public int EnemyCount { get; set; }
        public int RollbackCount { get; set; }
        public double MaxDivergence { get; set; }
        public float TickDurationMs { get; set; }
        public bool IsRunning { get; set; }
        public bool IsPaused { get; set; }

        /// <summary>Per-entity divergence data (empty when comparison mode is off).</summary>
        public IReadOnlyList<ShooterWorldDivergence> Divergences { get; set; } = System.Array.Empty<ShooterWorldDivergence>();

        /// <summary>Recent events from the last applied snapshot.</summary>
        public IReadOnlyList<ShooterEventSnapshot> RecentEvents { get; set; } = System.Array.Empty<ShooterEventSnapshot>();

        /// <summary>Total events seen since session start.</summary>
        public int TotalEvents { get; set; }

        /// <summary>Total rollbacks since session start.</summary>
        public int TotalRollbacks { get; set; }

        /// <summary>Formatted status string for the toolbar.</summary>
        public string StatusText
        {
            get
            {
                if (!IsRunning) return "Not Started";
                if (IsPaused) return "Running (Paused)";
                return "Running";
            }
        }

        public void Apply(in ShooterHostDiagnosticsSnapshot snapshot)
        {
            Frame = snapshot.Frame;
            PlayerCount = snapshot.PlayerCount;
            BulletCount = snapshot.BulletCount;
            EnemyCount = snapshot.EnemyCount;
            MaxDivergence = snapshot.MaxDivergence;
            Divergences = snapshot.Divergences;
            RecentEvents = snapshot.RecentEvents;
            TotalEvents = snapshot.TotalEvents;
        }

        public void Reset()
        {
            Frame = 0;
            PlayerCount = 0;
            BulletCount = 0;
            EnemyCount = 0;
            RollbackCount = 0;
            MaxDivergence = 0;
            TickDurationMs = 0f;
            IsRunning = false;
            IsPaused = false;
            Divergences = System.Array.Empty<ShooterWorldDivergence>();
            RecentEvents = System.Array.Empty<ShooterEventSnapshot>();
            TotalEvents = 0;
            TotalRollbacks = 0;
        }
    }
}
