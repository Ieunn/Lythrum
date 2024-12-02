namespace Lythrum.Core;

public enum LoopPhase
{
    None,
    Initializing,
    BeforeUpdate,
    FixedUpdate,
    Update,
    AfterUpdate,
    Disposing
}

public interface ILoop : IDisposable
{
    LoopPhase CurrentPhase { get; }
    bool IsRunning { get; }
    void Start();
    void Stop();
    void Pause();
    void Resume();
}