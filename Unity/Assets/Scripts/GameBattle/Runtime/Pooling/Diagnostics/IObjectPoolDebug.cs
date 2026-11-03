using System;

namespace AbilityKit.Core.Pooling
{
    internal interface IObjectPoolDebug
    {
        Type ElementType { get; }
        PoolStats Stats { get; }
        int MaxSize { get; }
        bool NeverTrim { get; }
    }
}
