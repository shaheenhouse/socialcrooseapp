using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Marketplace.Core.Performance;

public interface IComputePool
{
    Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken cancellationToken = default);
    Task ExecuteAsync(Func<CancellationToken, Task> work, CancellationToken cancellationToken = default);
}

public sealed class ComputePool : IComputePool, IDisposable
{
    private readonly Channel<Func<Task>> _workChannel;
    private readonly Task[] _workers;
    private readonly CancellationTokenSource _cts;
    private readonly ILogger<ComputePool> _logger;

    public ComputePool(ILogger<ComputePool> logger, int? workerCount = null)
    {
        _logger = logger;
        _cts = new CancellationTokenSource();

        var options = new BoundedChannelOptions(10000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = false,
            SingleReader = false
        };

        _workChannel = Channel.CreateBounded<Func<Task>>(options);

        var count = workerCount ?? Environment.ProcessorCount * 2;
        _workers = new Task[count];

        for (int i = 0; i < count; i++)
        {
            _workers[i] = ProcessWorkAsync(_cts.Token);
        }

        _logger.LogInformation("ComputePool initialized with {WorkerCount} workers", count);
    }

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<T>();
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);

        await _workChannel.Writer.WriteAsync(async () =>
        {
            try
            {
                var result = await work(linkedCts.Token);
                tcs.TrySetResult(result);
            }
            catch (OperationCanceledException)
            {
                tcs.TrySetCanceled(linkedCts.Token);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }, linkedCts.Token);

        return await tcs.Task;
    }

    public async Task ExecuteAsync(Func<CancellationToken, Task> work, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<bool>();
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);

        await _workChannel.Writer.WriteAsync(async () =>
        {
            try
            {
                await work(linkedCts.Token);
                tcs.TrySetResult(true);
            }
            catch (OperationCanceledException)
            {
                tcs.TrySetCanceled(linkedCts.Token);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }, linkedCts.Token);

        await tcs.Task;
    }

    private async Task ProcessWorkAsync(CancellationToken cancellationToken)
    {
        await foreach (var work in _workChannel.Reader.ReadAllAsync(cancellationToken))
        {
            try
            {
                await work();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing work item");
            }
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _workChannel.Writer.Complete();
        Task.WhenAll(_workers).Wait(TimeSpan.FromSeconds(5));
        _cts.Dispose();
    }
}
