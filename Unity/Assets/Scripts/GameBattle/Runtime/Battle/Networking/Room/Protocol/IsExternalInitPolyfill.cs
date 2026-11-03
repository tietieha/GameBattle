#if !UNITY_5_3_OR_NEWER
using System;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Polyfill for C# 9.0 init accessor in .NET Framework 4.x.
    /// Required for MemoryPack generated code compatibility.
    /// </summary>
    internal static class IsExternalInit { }
}

#endif
