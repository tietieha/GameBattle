using System;
using AbilityKit.Ability.Flow.Blocks;
using AbilityKit.Ability.Flow.Nodes;

namespace AbilityKit.Ability.Flow
{
    /// <summary>
    /// Flow 官方推荐的轻量节点工厂入口，用于隐藏常用节点构造细节并稳定业务侧编排代码。
    /// </summary>
    public static class FlowGraph
    {
        /// <summary>
        /// 创建一个立即成功的空节点。
        /// </summary>
        public static IFlowNode Empty()
        {
            return new DoNode();
        }

        /// <summary>
        /// 创建一个委托节点。未提供 tick 时默认在首帧成功。
        /// </summary>
        public static IFlowNode Do(
            Action<FlowContext> onEnter = null,
            Func<FlowContext, float, FlowStatus> onTick = null,
            Action<FlowContext> onExit = null,
            Action<FlowContext> onInterrupt = null)
        {
            return new DoNode(onEnter, onTick, onExit, onInterrupt);
        }

        /// <summary>
        /// 创建按顺序执行的节点组。所有子节点成功才返回成功。
        /// </summary>
        public static IFlowNode Sequence(params IFlowNode[] nodes)
        {
            return new SequenceNode(nodes);
        }

        /// <summary>
        /// 创建全部并行完成的节点组。任意子节点失败则整体失败。
        /// </summary>
        public static IFlowNode ParallelAll(params IFlowNode[] nodes)
        {
            return new ParallelAllNode(nodes);
        }

        /// <summary>
        /// 创建竞态节点组。第一个完成的子节点决定整体结果，其他运行中节点会被中断。
        /// </summary>
        public static IFlowNode Race(params IFlowNode[] nodes)
        {
            return new RaceNode(nodes);
        }

        /// <summary>
        /// 创建条件分支节点。elseNode 为空时条件不满足会直接成功。
        /// </summary>
        public static IFlowNode If(Func<FlowContext, bool> predicate, IFlowNode thenNode, IFlowNode elseNode = null)
        {
            return new IfNode(predicate, thenNode, elseNode);
        }

        /// <summary>
        /// 创建按 deltaTime 累积的等待节点。
        /// </summary>
        public static IFlowNode WaitSeconds(float seconds)
        {
            return new WaitSecondsNode(seconds);
        }

        /// <summary>
        /// 创建超时节点。超时时会中断子节点并返回失败。
        /// </summary>
        public static IFlowNode Timeout(float seconds, IFlowNode child)
        {
            return new TimeoutNode(seconds, child);
        }
    }
}
