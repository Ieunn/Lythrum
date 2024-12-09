namespace Lythrum.Core;

public interface ILoop : IDisposable
{
    bool IsRunning { get; }
    bool IsPaused { get; }

    void Start();
    void Stop();
    void Pause();
    void Resume();
    void Tick(float deltaTime);
}