#nullable enable

using System.Collections.Generic;
using AbilityKit.Demo.Shooter.Runtime;
using AbilityKit.Demo.Shooter.View;
using AbilityKit.Demo.Shooter.View.Hosting;
using AbilityKit.Protocol.Shooter;
using UnityEditor;
using UnityEngine;

namespace AbilityKit.Demo.Shooter.Editor.Sink
{
    /// <summary>
    /// Implements <see cref="IShooterSnapshotViewSink"/> for the Editor SceneView.
    /// Applies snapshot batches through the shared projection/store layer, caches draw-only
    /// entity data from the projected stores, then draws Gizmos during <c>SceneView.duringSceneGui</c>.
    /// </summary>
    public sealed class ShooterEditorSceneViewSink : IShooterSnapshotViewSink, IShooterHostViewSink
    {
        private readonly ShooterProjectedSnapshotViewSink _clientProjectionSink;
        private readonly ShooterProjectedSnapshotViewSink _authorityProjectionSink;
        private readonly List<EntityDrawData> _clientEntities = new(32);
        private readonly List<EntityDrawData> _authorityEntities = new(32);
        private readonly List<ShooterEventSnapshot> _pendingEvents = new(16);

        private ShooterSnapshotViewBatch _clientBatch;
        private ShooterSnapshotViewBatch _authorityBatch;
        private ShooterLagCompensationTelemetry? _lagCompensationTelemetry;
        private ShooterLagCompensationEvaluation? _lagCompensationEvaluation;
        private bool _hasAuthorityBatch;
        private bool _showDivergence;

        public ShooterEditorSceneViewSink()
        {
            _clientProjectionSink = new ShooterProjectedSnapshotViewSink(new ProjectedViewSinkAdapter(this, isAuthority: false));
            _authorityProjectionSink = new ShooterProjectedSnapshotViewSink(new ProjectedViewSinkAdapter(this, isAuthority: true));
        }

        /// <summary>Whether to draw the authoritative world overlay.</summary>
        public bool ShowAuthorityWorld
        {
            get => _hasAuthorityBatch;
            set => _hasAuthorityBatch = value;
        }

        /// <summary>Whether to draw divergence lines between client and authority entities.</summary>
        public bool ShowDivergence
        {
            get => _showDivergence;
            set => _showDivergence = value;
        }

        public void Render(in ShooterHostPresentationFrame frame)
        {
            _lagCompensationTelemetry = frame.LagCompensationTelemetry;
            _lagCompensationEvaluation = frame.LagCompensationEvaluation;
            var clientBatch = frame.ClientBatch;
            ApplySnapshot(in clientBatch);
            if (frame.HasAuthorityBatch)
            {
                var authorityBatch = frame.AuthorityBatch;
                ApplyAuthoritySnapshot(in authorityBatch);
                _hasAuthorityBatch = true;
            }
            else
            {
                _authorityBatch = default;
                _authorityProjectionSink.Clear();
                _hasAuthorityBatch = false;
            }
        }

        public void ApplySnapshot(in ShooterSnapshotViewBatch batch)
        {
            _clientBatch = batch;
            _clientProjectionSink.ApplySnapshot(in batch);
            CacheEvents(in batch);
        }

        /// <summary>
        /// Applies the authoritative world snapshot for overlay rendering.
        /// Separate from <see cref="ApplySnapshot"/> to keep client and authority data independent.
        /// </summary>
        public void ApplyAuthoritySnapshot(in ShooterSnapshotViewBatch batch)
        {
            _authorityBatch = batch;
            _authorityProjectionSink.ApplySnapshot(in batch);
        }

        public void Clear()
        {
            _clientBatch = default;
            _authorityBatch = default;
            _lagCompensationTelemetry = null;
            _lagCompensationEvaluation = null;
            _hasAuthorityBatch = false;
            _clientProjectionSink.Clear();
            _authorityProjectionSink.Clear();
            _pendingEvents.Clear();
        }

        /// <summary>
        /// Draws all cached entities into the SceneView. Called from
        /// <c>SceneView.duringSceneGui</c> by the Editor window.
        /// </summary>
        public void DrawSceneView()
        {
            DrawGrid();

            // Draw client world entities (solid)
            for (int i = 0; i < _clientEntities.Count; i++)
            {
                var entity = _clientEntities[i];
                DrawEntity(in entity, isAuthority: false);
            }

            // Draw events (hit flashes, fire effects)
            DrawEvents();

            // Draw authority world entities (transparent overlay)
            if (_hasAuthorityBatch)
            {
                for (int i = 0; i < _authorityEntities.Count; i++)
                {
                    var entity = _authorityEntities[i];
                    DrawEntity(in entity, isAuthority: true);
                }

                // Draw divergence lines
                if (_showDivergence)
                {
                    DrawDivergenceLines();
                }
            }

            DrawTelemetryOverlay();
        }

        /// <summary>Gets the cached client entity data for external diagnostics.</summary>
        public IReadOnlyList<EntityDrawData> ClientEntities => _clientEntities;

        /// <summary>Gets the cached authority entity data for external diagnostics.</summary>
        public IReadOnlyList<EntityDrawData> AuthorityEntities => _authorityEntities;

        /// <summary>Gets the latest lag compensation telemetry cached from PlayMode frames.</summary>
        public ShooterLagCompensationTelemetry? LagCompensationTelemetry => _lagCompensationTelemetry;

        /// <summary>Gets the latest lag compensation shot evaluation cached from PlayMode frames.</summary>
        public ShooterLagCompensationEvaluation? LagCompensationEvaluation => _lagCompensationEvaluation;

        private void ApplyProjectedViewState(ShooterViewEntityStore store, bool isAuthority)
        {
            ExtractEntities(store, isAuthority ? _authorityEntities : _clientEntities);
        }

        private void ClearProjectedViewState(bool isAuthority)
        {
            if (isAuthority)
            {
                _authorityEntities.Clear();
            }
            else
            {
                _clientEntities.Clear();
            }
        }

        private static void ExtractEntities(ShooterViewEntityStore store, List<EntityDrawData> target)
        {
            target.Clear();
            foreach (var entity in store.Entities.Values)
            {
                if (!entity.Alive || !store.TryGetTransform(entity.Key, out var transform))
                {
                    continue;
                }

                var data = new EntityDrawData
                {
                    EntityId = entity.EntityId,
                    Kind = entity.Kind,
                    OwnerEntityId = entity.OwnerEntityId,
                    X = transform.X,
                    Y = transform.Y,
                    FacingX = transform.FacingX,
                    FacingY = transform.FacingY,
                    VelocityX = transform.VelocityX,
                    VelocityY = transform.VelocityY,
                };

                if (store.TryGetHealth(entity.Key, out var health))
                {
                    data.Hp = health.Hp;
                }

                if (store.TryGetScore(entity.Key, out var score))
                {
                    data.Score = score.Score;
                }

                if (store.TryGetProjectileLifetime(entity.Key, out var projectileLifetime))
                {
                    data.RemainingFrames = projectileLifetime.RemainingFrames;
                }

                target.Add(data);
            }
        }

        private void CacheEvents(in ShooterSnapshotViewBatch batch)
        {
            _pendingEvents.Clear();
            if (batch.Events == null) return;
            for (int i = 0; i < batch.Events.Count; i++)
            {
                _pendingEvents.Add(batch.Events[i]);
            }
        }

        private static void DrawGrid()
        {
            var prevColor = HandlesColor;
            HandlesColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);

            // Draw battlefield boundary (-10 to 10)
            const float size = 10f;
            DrawLine(-size, -size, size, -size); // bottom
            DrawLine(size, -size, size, size);    // right
            DrawLine(size, size, -size, size);    // top
            DrawLine(-size, size, -size, -size);  // left

            // Draw grid lines every 2 units
            HandlesColor = new Color(0.2f, 0.2f, 0.2f, 0.15f);
            for (float x = -size; x <= size; x += 2f)
            {
                DrawLine(x, -size, x, size);
            }
            for (float y = -size; y <= size; y += 2f)
            {
                DrawLine(-size, y, size, y);
            }

            HandlesColor = prevColor;
        }

        private static void DrawEntity(in EntityDrawData data, bool isAuthority)
        {
            // Map 2D game coords to SceneView: X→X, Y→Z (top-down view)
            var pos = new Vector3(data.X, 0f, data.Y);

            if (data.Kind == ShooterViewEntityKind.Player)
            {
                DrawPlayer(pos, data.FacingX, data.FacingY, data.Hp, data.Score, data.EntityId, isAuthority);
            }
            else if (data.Kind == ShooterViewEntityKind.Bullet)
            {
                DrawBullet(pos, data.VelocityX, data.VelocityY, data.OwnerEntityId, data.RemainingFrames, isAuthority);
            }
            else if (data.Kind == ShooterViewEntityKind.Enemy)
            {
                DrawEnemy(pos, data.VelocityX, data.VelocityY, data.Hp, data.EntityId, isAuthority);
            }
        }

        private static void DrawPlayer(
            Vector3 pos, float facingX, float facingY, int hp, int score, int playerId, bool isAuthority)
        {
            var prevColor = HandlesColor;

            if (isAuthority)
            {
                // Authority: blue transparent disc
                HandlesColor = new Color(0.3f, 0.5f, 1f, 0.4f);
                DrawDisc(pos, 0.4f);
                HandlesColor = new Color(0.3f, 0.5f, 1f, 0.6f);
                DrawDiscOutline(pos, 0.4f);
            }
            else
            {
                // Client: green disc
                HandlesColor = hp > 0 ? new Color(0.2f, 0.8f, 0.2f, 0.8f) : new Color(0.5f, 0.5f, 0.5f, 0.4f);
                DrawDisc(pos, 0.4f);
                HandlesColor = Color.white;
                DrawDiscOutline(pos, 0.4f);

                // Facing direction arrow
                var facingDir = new Vector3(facingX, 0f, facingY).normalized;
                if (facingDir.magnitude > 0.01f)
                {
                    HandlesColor = Color.white;
                    DrawLine(pos, pos + facingDir * 0.6f);
                }

                // HP label
                HandlesColor = Color.white;
                DrawLabel(pos + Vector3.up * 0.8f, $"P{playerId} HP:{hp} S:{score}");
            }

            HandlesColor = prevColor;
        }

        private static void DrawBullet(
            Vector3 pos, float velX, float velY, int ownerId, int remainingFrames, bool isAuthority)
        {
            var prevColor = HandlesColor;

            if (isAuthority)
            {
                // Authority: blue transparent
                HandlesColor = new Color(0.3f, 0.5f, 1f, 0.3f);
            }
            else
            {
                // Client: yellow
                HandlesColor = new Color(1f, 0.9f, 0.2f, 0.9f);
            }

            // Draw bullet as a small sphere
            DrawDisc(pos, 0.1f);

            // Draw velocity direction
            var vel = new Vector3(velX, 0f, velY);
            if (vel.magnitude > 0.01f)
            {
                DrawLine(pos, pos + vel.normalized * 0.4f);
            }

            if (!isAuthority)
            {
                HandlesColor = new Color(1f, 0.9f, 0.2f, 0.6f);
                DrawLabel(pos + Vector3.up * 0.3f, $"B(owner:{ownerId} f:{remainingFrames})");
            }

            HandlesColor = prevColor;
        }

        private static void DrawEnemy(
            Vector3 pos, float velX, float velY, int hp, int enemyId, bool isAuthority)
        {
            var prevColor = HandlesColor;

            if (isAuthority)
            {
                HandlesColor = new Color(1f, 0.25f, 0.25f, 0.35f);
            }
            else
            {
                HandlesColor = hp > 0 ? new Color(1f, 0.15f, 0.15f, 0.85f) : new Color(0.45f, 0.2f, 0.2f, 0.4f);
            }

            DrawDiamond(pos, 0.34f);
            HandlesColor = isAuthority ? new Color(1f, 0.5f, 0.5f, 0.55f) : Color.white;
            DrawDiamondOutline(pos, 0.34f);

            var vel = new Vector3(velX, 0f, velY);
            if (vel.magnitude > 0.01f)
            {
                DrawLine(pos, pos + vel.normalized * 0.45f);
            }

            if (!isAuthority)
            {
                HandlesColor = new Color(1f, 0.85f, 0.85f, 0.9f);
                DrawLabel(pos + Vector3.up * 0.65f, $"E{enemyId} HP:{hp}");
            }

            HandlesColor = prevColor;
        }

        private void DrawEvents()
        {
            var prevColor = HandlesColor;
            for (int i = 0; i < _pendingEvents.Count; i++)
            {
                var evt = _pendingEvents[i];
                var pos = new Vector3(evt.X, 0f, evt.Y);

                if (evt.EventType == (int)ShooterEventType.Hit)
                {
                    HandlesColor = new Color(1f, 0.2f, 0.2f, 0.8f);
                    DrawDiscOutline(pos, 0.5f);
                    DrawDiscOutline(pos, 0.3f);
                }
                else if (evt.EventType == (int)ShooterEventType.Fire)
                {
                    HandlesColor = new Color(1f, 0.6f, 0.1f, 0.6f);
                    DrawDisc(pos, 0.08f);
                }
            }
            HandlesColor = prevColor;
        }

        private void DrawTelemetryOverlay()
        {
            if (!_lagCompensationTelemetry.HasValue) return;

            var telemetry = _lagCompensationTelemetry.Value;
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(12f, 12f, 260f, 82f), EditorStyles.helpBox);
            GUILayout.Label("Lag Compensation", EditorStyles.boldLabel);
            GUILayout.Label($"History: {telemetry.CapturedFrameCount} frames");
            GUILayout.Label($"Range: {telemetry.OldestFrame} → {telemetry.LatestFrame}");
            GUILayout.EndArea();
            Handles.EndGUI();
        }

        private void DrawDivergenceLines()
        {
            var prevColor = HandlesColor;
            HandlesColor = new Color(1f, 0.2f, 0.2f, 0.6f);

            // Match client and authority entities by EntityId
            for (int i = 0; i < _clientEntities.Count; i++)
            {
                var client = _clientEntities[i];
                for (int j = 0; j < _authorityEntities.Count; j++)
                {
                    var auth = _authorityEntities[j];
                    if (auth.EntityId == client.EntityId && auth.Kind == client.Kind)
                    {
                        var clientPos = new Vector3(client.X, 0f, client.Y);
                        var authPos = new Vector3(auth.X, 0f, auth.Y);
                        var distance = Vector3.Distance(clientPos, authPos);

                        if (distance > 0.01f)
                        {
                            DrawDashedLine(clientPos, authPos);
                            DrawLabel(
                                (clientPos + authPos) * 0.5f + Vector3.up * 0.5f,
                                $"{distance:F2}");
                        }
                        break;
                    }
                }
            }

            HandlesColor = prevColor;
        }

        // --- Handle drawing utilities ---

        private static UnityEngine.Color HandlesColor
        {
            get => Handles.color;
            set => Handles.color = value;
        }

        private static void DrawLine(Vector3 a, Vector3 b)
        {
            Handles.DrawLine(a, b);
        }

        private static void DrawLine(float x1, float y1, float x2, float y2)
        {
            Handles.DrawLine(new Vector3(x1, 0f, y1), new Vector3(x2, 0f, y2));
        }

        private static void DrawDisc(Vector3 center, float radius)
        {
            Handles.DrawSolidDisc(center, Vector3.up, radius);
        }

        private static void DrawDiscOutline(Vector3 center, float radius)
        {
            Handles.DrawWireDisc(center, Vector3.up, radius);
        }

        private static void DrawDiamond(Vector3 center, float radius)
        {
            var points = GetDiamondPoints(center, radius);
            Handles.DrawAAConvexPolygon(points);
        }

        private static void DrawDiamondOutline(Vector3 center, float radius)
        {
            var points = GetDiamondPoints(center, radius);
            Handles.DrawLine(points[0], points[1]);
            Handles.DrawLine(points[1], points[2]);
            Handles.DrawLine(points[2], points[3]);
            Handles.DrawLine(points[3], points[0]);
        }

        private static Vector3[] GetDiamondPoints(Vector3 center, float radius)
        {
            return new[]
            {
                center + new Vector3(0f, 0f, radius),
                center + new Vector3(radius, 0f, 0f),
                center + new Vector3(0f, 0f, -radius),
                center + new Vector3(-radius, 0f, 0f)
            };
        }

        private static void DrawLabel(Vector3 position, string text)
        {
            Handles.Label(position, text);
        }

        private static void DrawDashedLine(Vector3 a, Vector3 b)
        {
            // Simple dashed line using short segments
            var dir = (b - a);
            var length = dir.magnitude;
            if (length < 0.01f) return;
            dir /= length;

            const float dashLen = 0.15f;
            const float gapLen = 0.1f;
            var drawn = 0f;
            var on = true;

            while (drawn < length)
            {
                var segLen = on ? dashLen : gapLen;
                if (drawn + segLen > length) segLen = length - drawn;

                if (on)
                {
                    var start = a + dir * drawn;
                    var end = a + dir * (drawn + segLen);
                    Handles.DrawLine(start, end);
                }

                drawn += segLen;
                on = !on;
            }
        }

        private sealed class ProjectedViewSinkAdapter : IShooterProjectedViewSink
        {
            private readonly ShooterEditorSceneViewSink _owner;
            private readonly bool _isAuthority;

            public ProjectedViewSinkAdapter(ShooterEditorSceneViewSink owner, bool isAuthority)
            {
                _owner = owner;
                _isAuthority = isAuthority;
            }

            public void ApplyViewState(
                ShooterViewEntityStore store,
                in ShooterSnapshotViewBatch sourceBatch,
                in ShooterViewProjectionApplyResult applyResult)
            {
                _owner.ApplyProjectedViewState(store, _isAuthority);
            }

            public void Clear()
            {
                _owner.ClearProjectedViewState(_isAuthority);
            }
        }

        /// <summary>
        /// Cached entity data extracted from a <see cref="ShooterViewEntityStore"/>
        /// for efficient Gizmo drawing.
        /// </summary>
        public struct EntityDrawData
        {
            public int EntityId;
            public ShooterViewEntityKind Kind;
            public float X;
            public float Y;
            public float FacingX;
            public float FacingY;
            public float VelocityX;
            public float VelocityY;
            public int Hp;
            public int Score;
            public int OwnerEntityId;
            public int RemainingFrames;
        }
    }
}
