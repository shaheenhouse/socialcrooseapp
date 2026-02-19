using Microsoft.Extensions.DependencyInjection;
using Marketplace.Slices.AuthSlice;
using Marketplace.Slices.UserSlice.Repository;
using Marketplace.Slices.UserSlice.Services;
using Marketplace.Slices.StoreSlice;
using Marketplace.Slices.ProductSlice;
using Marketplace.Slices.OrderSlice;
using Marketplace.Slices.ProjectSlice;
using Marketplace.Slices.ReviewSlice;
using Marketplace.Slices.WalletSlice;
using Marketplace.Slices.NotificationSlice;
using Marketplace.Slices.CompanySlice;
using Marketplace.Slices.Social.Search;
using Marketplace.Slices.Social.Messaging;
using Marketplace.Slices.Social.Follows;
using Marketplace.Slices.Social.Connections;

namespace Marketplace.Slices;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMarketplaceSlices(this IServiceCollection services)
    {
        // Auth
        services.AddScoped<IAuthService, AuthService>();

        // User
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();

        // Store
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<IStoreService, StoreService>();

        // Product
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductService, ProductService>();

        // Order
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();

        // Project
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IProjectService, ProjectService>();

        // Review
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IReviewService, ReviewService>();

        // Wallet
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IWalletService, WalletService>();

        // Notification
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationService, NotificationService>();

        // Company & HR
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ICompanyService, CompanyService>();

        // Search
        services.AddScoped<ISearchRepository, SearchRepository>();
        services.AddScoped<ISearchService, SearchService>();

        // Messaging
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageService, MessageService>();

        // Follows
        services.AddScoped<IFollowRepository, FollowRepository>();
        services.AddScoped<IFollowService, FollowService>();

        // Connections
        services.AddScoped<IConnectionRepository, ConnectionRepository>();
        services.AddScoped<IConnectionService, ConnectionService>();

        return services;
    }
}
