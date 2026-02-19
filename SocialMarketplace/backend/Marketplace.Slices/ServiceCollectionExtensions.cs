using Microsoft.Extensions.DependencyInjection;
using Marketplace.Slices.AuthSlice;
using Marketplace.Slices.UserSlice.Repository;
using Marketplace.Slices.UserSlice.Services;

namespace Marketplace.Slices;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMarketplaceSlices(this IServiceCollection services)
    {
        // User Slice
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();

        // Auth Slice
        services.AddScoped<IAuthService, AuthService>();

        // TODO: Add more slices as they are implemented
        // Store Slice
        // Product Slice
        // Order Slice
        // Project Slice
        // Chat Slice

        return services;
    }
}
