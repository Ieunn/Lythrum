using Arch.System;
using Lythrum.Core;

namespace Lythrum.Bridge;

public sealed class Loop : ILoop
{
    private readonly Groups _groups;
    private volatile bool _isRunning;
    private volatile bool _isPaused;

    public bool IsRunning => _isRunning;
    public bool IsPaused => _isPaused;

    public Loop(Groups groups)
    {
        _groups = groups ?? throw new ArgumentNullException(nameof(groups));
    }

    public void Start()
    {
        if (_isRunning) return;
        _groups.Initialize();
        _isRunning = true;
    }

    public void Stop()
    {
        if (!_isRunning) return;
        _isRunning = false;
        _groups.Dispose();
    }

    public void Pause()
    {
        if (_isPaused) return;
        _isPaused = true;
    }

    public void Resume()
    {
        if (!_isPaused) return;
        _isPaused = false;
    }

    public void Tick(float deltaTime)
    {
        if (!_isRunning || _isPaused) return;

        _groups.BeforeUpdate(deltaTime);
        _groups.Update(deltaTime);
        _groups.AfterUpdate(deltaTime);
    }

    public void Dispose()
    {
        if (_isRunning)
            Stop();
    }
}