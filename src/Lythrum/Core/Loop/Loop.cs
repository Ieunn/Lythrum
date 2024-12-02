using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;

namespace Lythrum.Core;

public sealed class GameLoop : ILoop
{
    private readonly World _world;
    private readonly Group<float> _systems;
    private readonly LoopSettings _settings;
    private readonly Stopwatch _stopwatch;
    
#if !ARCH_METRICS_DISABLED
    private readonly Meter _meter;
    private readonly Histogram<double> _frameTime;
    private readonly Histogram<double> _systemsTime;
#endif

    private bool _isRunning;
    private bool _isPaused;
    private float _accumulator;
    private float _interpolation;
    private LoopPhase _currentPhase;

    public LoopPhase CurrentPhase => _currentPhase;
    public bool IsRunning => _isRunning;

    public GameLoop(World world, Group<float> systems, LoopSettings? settings = null)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _systems = systems ?? throw new ArgumentNullException(nameof(systems));
        _settings = settings ?? LoopSettings.Default;
        _stopwatch = new Stopwatch();
        _currentPhase = LoopPhase.None;

#if !ARCH_METRICS_DISABLED
        _meter = new Meter("GameLoop");
        _frameTime = _meter.CreateHistogram<double>("FrameTime", unit: "milliseconds");
        _systemsTime = _meter.CreateHistogram<double>("SystemsTime", unit: "milliseconds");
#endif
    }

    public void Start()
    {
        if (_isRunning) return;

        _currentPhase = LoopPhase.Initializing;
        _systems.Initialize();
        
        _isRunning = true;
        _stopwatch.Start();
        
        Tick();
    }

    public void Stop()
    {
        if (!_isRunning) return;

        _isRunning = false;
        _stopwatch.Stop();
        
        _currentPhase = LoopPhase.Disposing;
        _systems.Dispose();
        _currentPhase = LoopPhase.None;
    }

    public void Pause() => _isPaused = true;
    public void Resume() => _isPaused = false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Tick()
    {
        float previousTime = 0;
        
        while (_isRunning)
        {
            if (_isPaused)
            {
                Thread.Sleep(1);
                continue;
            }

#if !ARCH_METRICS_DISABLED
            _stopwatch.Restart();
#endif
            float currentTime = (float)_stopwatch.Elapsed.TotalSeconds;
            float deltaTime = Math.Min(currentTime - previousTime, _settings.MaxAllowedDeltaTime);
            previousTime = currentTime;

            ExecuteFrame(deltaTime);

#if !ARCH_METRICS_DISABLED
            _stopwatch.Stop();
            _frameTime.Record(_stopwatch.Elapsed.TotalMilliseconds);
#endif

            if (_settings.TargetFrameRate > 0)
            {
                float frameTime = 1.0f / _settings.TargetFrameRate;
                float elapsed = (float)_stopwatch.Elapsed.TotalSeconds - currentTime;
                int sleepMs = (int)((frameTime - elapsed) * 1000);
                if (sleepMs > 0)
                    Thread.Sleep(sleepMs);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteFrame(float deltaTime)
    {
        _currentPhase = LoopPhase.BeforeUpdate;
        _systems.BeforeUpdate(deltaTime);

        if (_settings.UseFixedTimeStep)
        {
            _accumulator += deltaTime;
            _currentPhase = LoopPhase.FixedUpdate;
            
            while (_accumulator >= _settings.FixedTimeStep)
            {
                _systems.Update(_settings.FixedTimeStep);
                _accumulator -= _settings.FixedTimeStep;
            }

            _interpolation = _accumulator / _settings.FixedTimeStep;
        }
        else
        {
            _currentPhase = LoopPhase.Update;
            _systems.Update(deltaTime);
            _interpolation = 1.0f;
        }

        _currentPhase = LoopPhase.AfterUpdate;
        _systems.AfterUpdate(_interpolation);
    }

    public void Dispose()
    {
        if (_isRunning)
            Stop();
        
#if !ARCH_METRICS_DISABLED
        _meter.Dispose();
#endif
    }
}

public readonly struct LoopSettings
{
    public readonly float TargetFrameRate { get; init; }
    public readonly float FixedTimeStep { get; init; }
    public readonly float MaxAllowedDeltaTime { get; init; }
    public readonly bool UseFixedTimeStep { get; init; }

    public static LoopSettings Default => new()
    {
        TargetFrameRate = 60,
        FixedTimeStep = 1f / 60f,
        MaxAllowedDeltaTime = 0.25f,
        UseFixedTimeStep = true
    };
}