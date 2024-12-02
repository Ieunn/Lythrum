using Arch.Core;
using Arch.System;

namespace Lythrum.Core;

public abstract class LoopAdapter : ILoop
{
    protected readonly GameLoop CoreLoop;
    
    protected LoopAdapter(World world, Group<float> systems, LoopSettings? settings = null)
    {
        CoreLoop = new GameLoop(world, systems, settings);
    }

    public LoopPhase CurrentPhase => CoreLoop.CurrentPhase;
    public bool IsRunning => CoreLoop.IsRunning;
    
    public abstract void Start();
    public abstract void Stop();
    public abstract void Pause();
    public abstract void Resume();
    
    public virtual void Dispose()
    {
        CoreLoop.Dispose();
    }
}