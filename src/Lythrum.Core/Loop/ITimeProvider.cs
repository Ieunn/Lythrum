namespace Lythrum.Core;

public interface ITimeProvider
{
    float DeltaTime { get; }
    float TimeScale { get; set; }
    float UnscaledDeltaTime { get; }
    float RealtimeSinceStartup { get; }
    float Time { get; }
    float FixedTime { get; }
    int FrameCount { get; }
}