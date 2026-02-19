using Microsoft.Extensions.DependencyInjection;
using Marketplace.Orchestrator.Workflows;

namespace Marketplace.Orchestrator;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrchestrator(this IServiceCollection services)
    {
        // Register workflows
        services.AddScoped<OrderWorkflow>();
        services.AddScoped<ProjectWorkflow>();
        services.AddScoped<TenderWorkflow>();
        services.AddScoped<NotificationWorkflow>();
        services.AddScoped<EscrowWorkflow>();

        return services;
    }
}
