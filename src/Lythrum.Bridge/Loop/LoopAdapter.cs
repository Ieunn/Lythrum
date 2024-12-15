using Lythrum.Core;

namespace Lythrum.Bridge;

public abstract class LoopAdapter : ILoop
{
    private bool _isInitialized;

    protected readonly IEnumerable<GroupConfig> GroupConfigs;

    protected Loop CoreLoop;

    public bool IsRunning => CoreLoop.IsRunning;
    public bool IsPaused => CoreLoop.IsPaused;

    protected LoopAdapter()
    {
        GroupConfigs = ConfigureGroups();
    }

    protected abstract IEnumerable<GroupConfig> ConfigureGroups();

    protected abstract void OnInitialize();
    protected abstract void OnShutdown();
    protected abstract void OnPause();
    protected abstract void OnResume();
    
    protected virtual void PreUpdate() { }
    protected virtual void PostUpdate() { }
    
    public SystemRegistry CreateRegistry()
    {
        if (_isInitialized)
        {
            throw new InvalidOperationException("Loop is already initialized");
        }
        return new SystemRegistry(GroupConfigs);
    }

    public void Initialize(SystemRegistry registry)
    {
        if (_isInitialized)
        {
            throw new InvalidOperationException("Loop is already initialized");
        }

        CoreLoop = new Loop(registry.Build());
        _isInitialized = true;
    }

    public virtual void Start()
    {
        EnsureInitialized();
        OnInitialize();
        CoreLoop.Start();
    }

    public virtual void Stop()
    {
        EnsureInitialized();
        OnShutdown();
        CoreLoop.Stop();
    }

    public virtual void Pause()
    {
        EnsureInitialized();
        OnPause();
        CoreLoop.Pause();
    }

    public virtual void Resume()
    {
        EnsureInitialized();
        OnResume();
        CoreLoop.Resume();
    }

    public virtual void Tick(float deltaTime)
    {
        EnsureInitialized();
        if (!IsRunning || IsPaused) return;
        
        PreUpdate();
        CoreLoop.Tick(deltaTime);
        PostUpdate();
    }

    private void EnsureInitialized()
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("Loop must be initialized before use");
        }
    }
    
    public virtual void Dispose()
    {
        CoreLoop?.Dispose();
    }
}