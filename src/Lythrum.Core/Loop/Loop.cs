using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using Arch.System;

namespace Lythrum.Core;

public sealed class Loop : ILoop
{
    private readonly Group<float> _systems;
    private readonly object _settingsLock = new();
    private volatile bool _isRunning;
    private volatile bool _isPaused;
    private float _accumulator;
    private float _interpolation;
    private LoopPhase _currentPhase;
    private ILoopSettings _settings;

    private readonly Stopwatch _stopwatch;
    
#if !ARCH_METRICS_DISABLED
    private readonly Meter _meter;
    private readonly Histogram<double> _frameTime;
    private readonly Histogram<double> _systemsTime;
#endif

    public LoopPhase CurrentPhase => _currentPhase;
    public bool IsRunning => _isRunning;
    public bool IsPaused => _isPaused;
    public ILoopSettings Settings => _settings;

    public Loop(Group<float> systems, ILoopSettings? settings = null)
    {
        _systems = systems ?? throw new ArgumentNullException(nameof(systems));
        _settings = settings ?? LoopSettings.Default;
        _stopwatch = new Stopwatch();
        _currentPhase = LoopPhase.None;

#if !ARCH_METRICS_DISABLED
        _meter = new Meter("Loop");
        _frameTime = _meter.CreateHistogram<double>("FrameTime", unit: "milliseconds");
        _systemsTime = _meter.CreateHistogram<double>("SystemsTime", unit: "milliseconds");
#endif
    }

    public void UpdateSettings(ILoopSettings settings)
    {
        if (settings == null) throw new ArgumentNullException(nameof(settings));
        lock (_settingsLock)
        {
            _settings = settings;
        }
    }

    public void Start()
    {
        if (_isRunning) return;

        _currentPhase = LoopPhase.Initializing;
        _systems.Initialize();
        
        _isRunning = true;
        _stopwatch.Start();
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

    public void Pause()
    {
        if (_isPaused) return;
        _isPaused = true;
        _stopwatch.Stop();
    }

    public void Resume()
    {
        if (!_isPaused) return;
        _isPaused = false;
        _stopwatch.Start();
    }

    public void Tick(float deltaTime)
    {
        if (!_isRunning || _isPaused) return;

        ILoopSettings settings;
        lock (_settingsLock)
        {
            settings = _settings;
        }

#if !ARCH_METRICS_DISABLED
        var tickStopwatch = Stopwatch.StartNew();
#endif

        deltaTime = Math.Min(deltaTime, settings.MaxAllowedDeltaTime);
        ExecuteFrame(deltaTime, settings);

#if !ARCH_METRICS_DISABLED
        tickStopwatch.Stop();
        _frameTime.Record(tickStopwatch.Elapsed.TotalMilliseconds);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ExecuteFrame(float deltaTime, ILoopSettings settings)
    {
        _currentPhase = LoopPhase.BeforeUpdate;
        _systems.BeforeUpdate(deltaTime);

        if (settings.UseFixedTimeStep)
        {
            _accumulator += deltaTime;
            _currentPhase = LoopPhase.FixedUpdate;
            
            while (_accumulator >= settings.FixedTimeStep)
            {
#if !ARCH_METRICS_DISABLED
                var systemStopwatch = Stopwatch.StartNew();
#endif

                _systems.Update(settings.FixedTimeStep);
                _accumulator -= settings.FixedTimeStep;

#if !ARCH_METRICS_DISABLED
                systemStopwatch.Stop();
                _systemsTime.Record(systemStopwatch.Elapsed.TotalMilliseconds);
#endif
            }

            _interpolation = _accumulator / settings.FixedTimeStep;
        }
        else
        {
            _currentPhase = LoopPhase.Update;
#if !ARCH_METRICS_DISABLED
            var systemStopwatch = Stopwatch.StartNew();
#endif

            _systems.Update(deltaTime);

#if !ARCH_METRICS_DISABLED
            systemStopwatch.Stop();
            _systemsTime.Record(systemStopwatch.Elapsed.TotalMilliseconds);
#endif

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

public readonly struct LoopSettings : ILoopSettings
{
    public float TargetFrameRate { get; init; }
    public float FixedTimeStep { get; init; }
    public float MaxAllowedDeltaTime { get; init; }
    public bool UseFixedTimeStep { get; init; }

    public static LoopSettings Default => new()
    {
        TargetFrameRate = 60,
        FixedTimeStep = 1f / 60f,
        MaxAllowedDeltaTime = 0.25f,
        UseFixedTimeStep = true
    };
}