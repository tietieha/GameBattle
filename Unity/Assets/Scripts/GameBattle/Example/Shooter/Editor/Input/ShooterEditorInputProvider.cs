#nullable enable

using System.Collections.Generic;
using AbilityKit.Demo.Shooter.View;
using AbilityKit.Demo.Shooter.View.PlayMode;
using AbilityKit.Protocol.Shooter;
using UnityEngine;

namespace AbilityKit.Demo.Shooter.Editor.Input
{
    /// <summary>
    /// Collects keyboard input from the Editor window and produces <see cref="ShooterPlayerCommand"/>.
    /// Uses WASD or arrow keys for movement, mouse position for aiming (relative to player in SceneView),
    /// and Space for firing.
    /// </summary>
    public sealed class ShooterEditorInputProvider : IShooterPlayInputSource
    {
        public int ControlledPlayerId { get; set; } = 1;
        public bool EnableKeyboardInput { get; set; } = true;

        private readonly HashSet<KeyCode> _keysDown = new();

        /// <summary>
        /// Processes a key-down event. Call from OnGUI when EventType.KeyDown.
        /// Returns true if the event was consumed.
        /// </summary>
        public bool OnKeyDown(Event e)
        {
            if (!EnableKeyboardInput || e.type != EventType.KeyDown)
                return false;

            _keysDown.Add(e.keyCode);

            // Consume the event so it doesn't propagate
            return IsGameKey(e.keyCode);
        }

        /// <summary>
        /// Processes a key-up event. Call from OnGUI when EventType.KeyUp.
        /// </summary>
        public bool OnKeyUp(Event e)
        {
            if (e.type != EventType.KeyUp)
                return false;

            _keysDown.Remove(e.keyCode);
            return IsGameKey(e.keyCode);
        }

        /// <summary>
        /// Polls the current input state and produces a command if any input is active.
        /// Returns null when no meaningful input is detected.
        /// </summary>
        public ShooterPlayerCommand? PollInput()
        {
            if (!EnableKeyboardInput || _keysDown.Count == 0)
                return null;

            var moveX = 0f;
            var moveY = 0f;
            if (_keysDown.Contains(KeyCode.W) || _keysDown.Contains(KeyCode.UpArrow)) moveY += 1f;
            if (_keysDown.Contains(KeyCode.S) || _keysDown.Contains(KeyCode.DownArrow)) moveY -= 1f;
            if (_keysDown.Contains(KeyCode.A) || _keysDown.Contains(KeyCode.LeftArrow)) moveX -= 1f;
            if (_keysDown.Contains(KeyCode.D) || _keysDown.Contains(KeyCode.RightArrow)) moveX += 1f;

            var fire = _keysDown.Contains(KeyCode.Space);

            // Default aim direction: up (0, 1)
            var aimX = 0f;
            var aimY = 1f;

            if (moveX == 0f && moveY == 0f && !fire)
                return null;

            return ShooterClientInputBuilder.CreateCommand(
                ControlledPlayerId, moveX, moveY, aimX, aimY, fire);
        }

        public ShooterPlayFrameInput ReadInput(int controlledPlayerId)
        {
            ControlledPlayerId = controlledPlayerId;
            if (!EnableKeyboardInput || _keysDown.Count == 0)
            {
                return new ShooterPlayFrameInput(0f, 0f, 0f, 1f, false);
            }

            var moveX = 0f;
            var moveY = 0f;
            if (_keysDown.Contains(KeyCode.W) || _keysDown.Contains(KeyCode.UpArrow)) moveY += 1f;
            if (_keysDown.Contains(KeyCode.S) || _keysDown.Contains(KeyCode.DownArrow)) moveY -= 1f;
            if (_keysDown.Contains(KeyCode.A) || _keysDown.Contains(KeyCode.LeftArrow)) moveX -= 1f;
            if (_keysDown.Contains(KeyCode.D) || _keysDown.Contains(KeyCode.RightArrow)) moveX += 1f;

            return new ShooterPlayFrameInput(
                moveX,
                moveY,
                0f,
                1f,
                _keysDown.Contains(KeyCode.Space));
        }

        /// <summary>Clears all tracked key state.</summary>
        public void Reset()
        {
            _keysDown.Clear();
        }

        /// <summary>Returns a display string of currently held keys.</summary>
        public string GetDebugKeyString()
        {
            return _keysDown.Count == 0 ? "(none)" : string.Join(", ", _keysDown);
        }

        private static bool IsGameKey(KeyCode keyCode)
        {
            return keyCode is KeyCode.W or KeyCode.A or KeyCode.S or KeyCode.D
                or KeyCode.UpArrow or KeyCode.DownArrow or KeyCode.LeftArrow or KeyCode.RightArrow
                or KeyCode.Space or KeyCode.Q;
        }
    }
}
