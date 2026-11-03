using System;

namespace AbilityKit.Ability.StateSync
{

/// <summary>
/// 输入命令接口
/// 空的，不假设任何输入结构，由具体业务实现
/// </summary>
public interface IInputCommand
{
}

/// <summary>
/// 帧号
/// </summary>
public readonly struct Frame : IEquatable<Frame>, IComparable<Frame>
{
    public int Value { get; }

    public Frame(int value) => Value = value;

    public static Frame operator +(Frame a, int b) => new Frame(a.Value + b);
    public static Frame operator -(Frame a, int b) => new Frame(a.Value - b);
    public static bool operator ==(Frame a, Frame b) => a.Value == b.Value;
    public static bool operator !=(Frame a, Frame b) => a.Value != b.Value;
    public static bool operator <(Frame a, Frame b) => a.Value < b.Value;
    public static bool operator >(Frame a, Frame b) => a.Value > b.Value;
    public static bool operator <=(Frame a, Frame b) => a.Value <= b.Value;
    public static bool operator >=(Frame a, Frame b) => a.Value >= b.Value;

    public bool Equals(Frame other) => Value == other.Value;
    public override bool Equals(object obj) => obj is Frame other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public int CompareTo(Frame other) => Value.CompareTo(other.Value);
    public override string ToString() => $"F{Value}";

    public static Frame Invalid => new Frame(-1);
    public static Frame Zero => new Frame(0);
}

/// <summary>
/// 预测策略
/// </summary>
public enum PredictionStrategy
{
    None,
    Optimistic,
    OptimisticWithRollback
}

/// <summary>
/// 冲突级别
/// </summary>
public enum ConflictLevel
{
    None,
    Minor,
    Major,
    Critical
}

/// <summary>
/// 预测结果
/// </summary>
public readonly struct PredictionResult
{
    public bool Success { get; }
    public ConflictLevel Level { get; }
    public string Message { get; }

    private PredictionResult(bool success, ConflictLevel level, string message)
    {
        Success = success;
        Level = level;
        Message = message;
    }

    public static PredictionResult Ok() => new PredictionResult(true, ConflictLevel.None, string.Empty);
    public static PredictionResult Minor(string msg) => new PredictionResult(false, ConflictLevel.Minor, msg);
    public static PredictionResult Major(string msg) => new PredictionResult(false, ConflictLevel.Major, msg);
    public static PredictionResult Critical(string msg) => new PredictionResult(false, ConflictLevel.Critical, msg);
}

/// <summary>
/// 槽位值 - 通用值类型
/// </summary>
public readonly struct SlotValue
{
    public object Value { get; }
    public Type ValueType { get; }

    public SlotValue(object value)
    {
        Value = value;
        ValueType = value != null ? value.GetType() : typeof(object);
    }

    public T As<T>() where T : class => Value is T t ? t : default;

    public static implicit operator SlotValue(int v) => new SlotValue(v);
    public static implicit operator SlotValue(float v) => new SlotValue(v);
    public static implicit operator SlotValue(bool v) => new SlotValue(v);
    public static implicit operator SlotValue(string v) => new SlotValue(v);
}

/// <summary>
/// 向量类型（独立于引擎，用于预测系统内部计算）
/// 与 AbilityKit.Core.Mathematics.Vec3 和 AbilityKit.Ability.StateSync.Snapshot.Vec3 是不同的类型
/// - AbilityKit.Core.Mathematics.Vec3: 完整数学库，适合内部计算
/// - AbilityKit.Ability.StateSync.Snapshot.Vec3: MemoryPackable，适合序列化
/// - AbilityKit.Ability.StateSync.Vector3: 简化版，适合预测系统内部快速计算
/// </summary>
public readonly struct Vector3
{
    public float X { get; }
    public float Y { get; }
    public float Z { get; }

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vector3 Zero => new Vector3(0, 0, 0);
    public float Length => MathF.Sqrt(X * X + Y * Y + Z * Z);
    public float LengthXZ => MathF.Sqrt(X * X + Z * Z);

    public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vector3 operator *(Vector3 a, float scalar) => new Vector3(a.X * scalar, a.Y * scalar, a.Z * scalar);
}

/// <summary>
/// 四元数类型（独立于引擎，用于预测系统内部计算）
/// 与 AbilityKit.Core.Mathematics.Quat 和 AbilityKit.Ability.StateSync.Snapshot.Quat 是不同的类型
/// - AbilityKit.Core.Mathematics.Quat: 完整数学库，适合内部计算
/// - AbilityKit.Ability.StateSync.Snapshot.Quat: MemoryPackable，适合序列化
/// - AbilityKit.Ability.StateSync.Quaternion: 简化版，适合预测系统内部快速计算
/// </summary>
public readonly struct Quaternion
{
    public float X { get; }
    public float Y { get; }
    public float Z { get; }
    public float W { get; }

    public Quaternion(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public static Quaternion Identity => new Quaternion(0, 0, 0, 1);
}

/// <summary>
/// 槽位定义 - 声明需要的槽位
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class NeedsSlotAttribute : Attribute
{
    public string SlotName { get; }
    public bool Required { get; }

    public NeedsSlotAttribute(string slotName, bool required)
    {
        SlotName = slotName;
        Required = required;
    }
}

/// <summary>
/// 提供槽位定义 - 槽位从哪里来
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ProvidesSlotAttribute : Attribute
{
    public string SlotName { get; }
    public string Description { get; }

    public ProvidesSlotAttribute(string slotName, string description)
    {
        SlotName = slotName;
        Description = description;
    }
}

/// <summary>
/// 槽位模式 - 用于匹配槽位名称
/// </summary>
public static class SlotPattern
{
    public static bool Matches(string pattern, string slotName)
    {
        if (pattern == slotName) return true;
        if (pattern.EndsWith(".*"))
        {
            var prefix = pattern.Substring(0, pattern.Length - 1);
            return slotName.StartsWith(prefix);
        }
        return false;
    }

    public static System.Collections.Generic.IEnumerable<string> GetMatchingSlots(string pattern, System.Collections.Generic.IEnumerable<string> allSlots)
    {
        foreach (var slot in allSlots)
        {
            if (Matches(pattern, slot))
                yield return slot;
        }
    }
}

}
