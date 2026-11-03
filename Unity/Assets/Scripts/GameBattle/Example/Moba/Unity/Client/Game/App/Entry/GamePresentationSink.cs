using AbilityKit.Core.Logging;
using AbilityKit.Game.Flow;
using UnityEngine;

namespace AbilityKit.Game
{
    /// <summary>
    /// Unity 宿主的 <see cref="IPresentationSink"/> 实现。
    /// 接收 Flow 编排层推送的阶段变化 / 战斗开始 / 战斗结束 / 错误事件，
    /// 当前以日志输出为主，后续可扩展为 UI 状态切换、场景加载触发等。
    /// </summary>
    public sealed class GamePresentationSink : IPresentationSink
    {
        public void OnPhaseChanged(MobaRootState root, MobaBattleState battle)
        {
            Log.Info($"[PresentationSink] PhaseChanged: Root={root}, Battle={battle}");
        }

        public void OnBattleStart()
        {
            Log.Info("[PresentationSink] BattleStart");
        }

        public void OnBattleEnd()
        {
            Log.Info("[PresentationSink] BattleEnd");
        }

        public void OnError(string message)
        {
            Log.Error($"[PresentationSink] Error: {message}");
        }
    }
}
