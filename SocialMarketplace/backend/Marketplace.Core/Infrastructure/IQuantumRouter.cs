namespace Marketplace.Core.Infrastructure;

public interface IQuantumRouter
{
    Task<T> RouteAsync<T>(Func<Task<T>> fastPath, Func<Task<T>> safePath);
    bool ShouldUseFastPath();
}
