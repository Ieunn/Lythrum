namespace Lythrum.Core;

public interface ILoopSettings
{
    float TargetFrameRate { get; }
    float FixedTimeStep { get; }
    float MaxAllowedDeltaTime { get; }
    bool UseFixedTimeStep { get; }
}