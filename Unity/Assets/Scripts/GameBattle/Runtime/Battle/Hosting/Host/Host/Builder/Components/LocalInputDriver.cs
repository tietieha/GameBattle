using System;
using System.Collections.Generic;
using AbilityKit.Ability.FrameSync;
using AbilityKit.Ability.Host;
using AbilityKit.Ability.Host.Framework;
using AbilityKit.Ability.Host.Transport;
using AbilityKit.Ability.World.Abstractions;

namespace AbilityKit.Ability.Host.Builder.Components
{
    /// <summary>
    /// 本地输入驱动
    /// 用于单机游戏或测试环境，直接在本地提交输入
    /// </summary>
    public sealed class LocalInputDriver : IInputDriver
    {
        private HostRuntime _runtime;
        private HostRuntimeOptions _options;
        private readonly Dictionary<WorldId, List<PlayerInputCommand>> _pendingInputs = new Dictionary<WorldId, List<PlayerInputCommand>>();
        private readonly List<Action<WorldId, FrameIndex, PlayerInputCommand[]>> _handlers = new List<Action<WorldId, FrameIndex, PlayerInputCommand[]>>();
        private readonly Action<float> _onPreTick;

        public LocalInputDriver()
        {
            _onPreTick = OnPreTick;
        }

        public void Attach(HostRuntime runtime, HostRuntimeOptions options)
        {
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _options.PreTick.Add(_onPreTick);
        }

        public void Detach()
        {
            if (_options != null)
            {
                _options.PreTick.Remove(_onPreTick);
            }

            _pendingInputs.Clear();
            _handlers.Clear();
            _runtime = null;
            _options = null;
        }

        public bool SubmitInput(ServerClientId clientId, WorldId worldId, PlayerInputCommand input)
        {
            if (_runtime == null) return false;

            if (!_pendingInputs.TryGetValue(worldId, out var list))
            {
                list = new List<PlayerInputCommand>();
                _pendingInputs[worldId] = list;
            }

            list.Add(input);
            return true;
        }

        public void AddInputsFlushed(Action<WorldId, FrameIndex, PlayerInputCommand[]> handler)
        {
            if (handler != null)
            {
                _handlers.Add(handler);
            }
        }

        public void RemoveInputsFlushed(Action<WorldId, FrameIndex, PlayerInputCommand[]> handler)
        {
            if (handler != null)
            {
                _handlers.Remove(handler);
            }
        }

        private void OnPreTick(float deltaTime)
        {
            if (_runtime == null) return;

            foreach (var kv in _pendingInputs)
            {
                var worldId = kv.Key;
                var inputs = kv.Value;

                if (inputs.Count == 0) continue;

                var frame = new FrameIndex(0);
                var inputArray = inputs.ToArray();
                inputs.Clear();

                foreach (var handler in _handlers)
                {
                    handler?.Invoke(worldId, frame, inputArray);
                }
            }
        }
    }
}
