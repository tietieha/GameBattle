using System;

namespace AbilityKit.Network.Runtime.Sync
{
/// <summary>
/// 捕获预测、插值、回溯和演示播放共用的时间坐标。
/// </summary>
public readonly struct SyncTimeAnchor : IEquatable<SyncTimeAnchor>
{
    public SyncTimeAnchor(
        int localFrame,
        long timelineTicks,
        double elapsedSeconds,
        int authoritativeFrame = 0,
        bool hasAuthoritativeFrame = false,
        long serverTicks = 0L,
        bool hasServerTicks = false)
    {
        if (localFrame < 0) throw new ArgumentOutOfRangeException(nameof(localFrame));
        if (timelineTicks < 0L) throw new ArgumentOutOfRangeException(nameof(timelineTicks));
        if (elapsedSeconds < 0d) throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
        if (hasAuthoritativeFrame && authoritativeFrame < 0) throw new ArgumentOutOfRangeException(nameof(authoritativeFrame));
        if (hasServerTicks && serverTicks < 0L) throw new ArgumentOutOfRangeException(nameof(serverTicks));

        LocalFrame = localFrame;
        TimelineTicks = timelineTicks;
        ElapsedSeconds = elapsedSeconds;
        AuthoritativeFrame = authoritativeFrame;
        HasAuthoritativeFrame = hasAuthoritativeFrame;
        ServerTicks = serverTicks;
        HasServerTicks = hasServerTicks;
    }

    public int LocalFrame { get; }

    public long TimelineTicks { get; }

    public double ElapsedSeconds { get; }

    public int AuthoritativeFrame { get; }

    public bool HasAuthoritativeFrame { get; }

    public long ServerTicks { get; }

    public bool HasServerTicks { get; }

    public static SyncTimeAnchor FromLocalFrame(int localFrame, long timelineTicks, double elapsedSeconds)
    {
        return new SyncTimeAnchor(localFrame, timelineTicks, elapsedSeconds);
    }

    public SyncTimeAnchor WithAuthoritativeFrame(int authoritativeFrame)
    {
        return new SyncTimeAnchor(LocalFrame, TimelineTicks, ElapsedSeconds, authoritativeFrame, true, ServerTicks, HasServerTicks);
    }

    public SyncTimeAnchor WithServerTicks(long serverTicks)
    {
        return new SyncTimeAnchor(LocalFrame, TimelineTicks, ElapsedSeconds, AuthoritativeFrame, HasAuthoritativeFrame, serverTicks, true);
    }

    public bool Equals(SyncTimeAnchor other)
    {
        return LocalFrame == other.LocalFrame
            && TimelineTicks == other.TimelineTicks
            && ElapsedSeconds.Equals(other.ElapsedSeconds)
            && AuthoritativeFrame == other.AuthoritativeFrame
            && HasAuthoritativeFrame == other.HasAuthoritativeFrame
            && ServerTicks == other.ServerTicks
            && HasServerTicks == other.HasServerTicks;
    }

    public override bool Equals(object? obj)
    {
        return obj is SyncTimeAnchor other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(LocalFrame, TimelineTicks, ElapsedSeconds, AuthoritativeFrame, HasAuthoritativeFrame, ServerTicks, HasServerTicks);
    }

    public static bool operator ==(SyncTimeAnchor left, SyncTimeAnchor right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SyncTimeAnchor left, SyncTimeAnchor right)
    {
        return !left.Equals(right);
    }
}
}
