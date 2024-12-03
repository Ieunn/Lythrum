using Arch.System;

namespace Lythrum.Core;

public abstract class LoopAdapter : ILoop
{
    protected readonly Loop CoreLoop;
    protected readonly ITimeProvider TimeProvider;
    
    protected LoopAdapter(Group<float> systems, ITimeProvider timeProvider, ILoopSettings? settings = null)
    {
        CoreLoop = new Loop(systems, settings);
        TimeProvider = timeProvider;
    }

    public LoopPhase CurrentPhase => CoreLoop.CurrentPhase;
    public bool IsRunning => CoreLoop.IsRunning;
    public bool IsPaused => CoreLoop.IsPaused;
    public ILoopSettings Settings => CoreLoop.Settings;

    public virtual void Start()
    {
        OnInitialize();
        CoreLoop.Start();
    }

    public virtual void Stop()
    {
        OnShutdown();
        CoreLoop.Stop();
    }

    public virtual void Pause()
    {
        OnPause();
        CoreLoop.Pause();
    }

    public virtual void Resume()
    {
        OnResume();
        CoreLoop.Resume();
    }

    public virtual void Tick(float deltaTime)
    {
        if (!IsRunning || IsPaused) return;
        
        PreUpdate();
        CoreLoop.Tick(TimeProvider.DeltaTime * TimeProvider.TimeScale);
        PostUpdate();
    }

    public virtual void UpdateSettings(ILoopSettings settings)
    {
        CoreLoop.UpdateSettings(settings);
    }

    protected abstract void OnInitialize();
    protected abstract void OnShutdown();
    protected abstract void OnPause();
    protected abstract void OnResume();
    
    protected virtual void PreUpdate() { }
    protected virtual void PostUpdate() { }
    
    public virtual void Dispose()
    {
        CoreLoop.Dispose();
    }
}