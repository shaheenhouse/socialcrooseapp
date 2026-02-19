using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Marketplace.Core.Infrastructure;

public sealed class QuantumRouter : IQuantumRouter
{
    private readonly ILogger<QuantumRouter> _logger;
    private readonly double _loadFactorThreshold;
    private readonly Random _random = new();
    private volatile double _currentLoadFactor;
    private long _fastPathSuccesses;
    private long _fastPathFailures;
    private long _totalRequests;

    public QuantumRouter(IConfiguration configuration, ILogger<QuantumRouter> logger)
    {
        _logger = logger;
        _loadFactorThreshold = configuration.GetValue("Router:LoadFactorThreshold", 0.7);
        _currentLoadFactor = 0.0;
    }

    public async Task<T> RouteAsync<T>(Func<Task<T>> fastPath, Func<Task<T>> safePath)
    {
        Interlocked.Increment(ref _totalRequests);

        if (!ShouldUseFastPath())
        {
            return await safePath();
        }

        try
        {
            var sw = Stopwatch.StartNew();
            var result = await fastPath();
            sw.Stop();

            Interlocked.Increment(ref _fastPathSuccesses);
            UpdateLoadFactor(sw.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _fastPathFailures);
            _logger.LogWarning(ex, "Fast path failed, falling back to safe path");

            return await safePath();
        }
    }

    public bool ShouldUseFastPath()
    {
        // Probabilistic routing based on load factor
        var probability = 1.0 - (_currentLoadFactor / _loadFactorThreshold);
        return _random.NextDouble() < Math.Max(0.1, probability);
    }

    private void UpdateLoadFactor(long responseTimeMs)
    {
        // Exponential moving average
        const double alpha = 0.1;
        var normalizedTime = Math.Min(1.0, responseTimeMs / 100.0);
        _currentLoadFactor = (alpha * normalizedTime) + ((1 - alpha) * _currentLoadFactor);
    }

    public (long Total, long FastPathSuccesses, long FastPathFailures, double LoadFactor) GetStats()
    {
        return (_totalRequests, _fastPathSuccesses, _fastPathFailures, _currentLoadFactor);
    }
}
