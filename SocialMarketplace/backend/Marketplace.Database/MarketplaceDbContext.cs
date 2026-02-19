using Microsoft.EntityFrameworkCore;
using Marketplace.Database.Entities;

namespace Marketplace.Database;

public class MarketplaceDbContext : DbContext
{
    public MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options) : base(options)
    {
    }

    // Identity & Authorization
    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();

    // Feature Flags
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
    public DbSet<UserFeatureFlag> UserFeatureFlags => Set<UserFeatureFlag>();
    public DbSet<RoleFeatureFlag> RoleFeatureFlags => Set<RoleFeatureFlag>();

    // Localization
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Translation> Translations => Set<Translation>();

    // Categories
    public DbSet<Category> Categories => Set<Category>();

    // Stores
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<StoreEmployee> StoreEmployees => Set<StoreEmployee>();
    public DbSet<StoreCategory> StoreCategories => Set<StoreCategory>();

    // Products
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();

    // Services
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServicePackage> ServicePackages => Set<ServicePackage>();
    public DbSet<ServiceImage> ServiceImages => Set<ServiceImage>();

    // Skills & Testing
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<UserSkill> UserSkills => Set<UserSkill>();
    public DbSet<SkillTest> SkillTests => Set<SkillTest>();
    public DbSet<SkillTestQuestion> SkillTestQuestions => Set<SkillTestQuestion>();
    public DbSet<SkillTestAttempt> SkillTestAttempts => Set<SkillTestAttempt>();
    public DbSet<SkillTestAnswer> SkillTestAnswers => Set<SkillTestAnswer>();
    public DbSet<SkillCertificate> SkillCertificates => Set<SkillCertificate>();

    // Projects
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectBid> ProjectBids => Set<ProjectBid>();
    public DbSet<ProjectMilestone> ProjectMilestones => Set<ProjectMilestone>();
    public DbSet<ProjectContract> ProjectContracts => Set<ProjectContract>();

    // Tenders
    public DbSet<Tender> Tenders => Set<Tender>();
    public DbSet<TenderBid> TenderBids => Set<TenderBid>();
    public DbSet<TenderDocument> TenderDocuments => Set<TenderDocument>();
    public DbSet<TenderAward> TenderAwards => Set<TenderAward>();

    // Orders
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    // Payments
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentGateway> PaymentGateways => Set<PaymentGateway>();
    public DbSet<Escrow> Escrows => Set<Escrow>();
    public DbSet<EscrowRelease> EscrowReleases => Set<EscrowRelease>();
    public DbSet<Payout> Payouts => Set<Payout>();

    // Wallet & Transactions
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    // Reviews
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReviewResponse> ReviewResponses => Set<ReviewResponse>();

    // Notifications
    public DbSet<Notification> Notifications => Set<Notification>();

    // Chat
    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();
    public DbSet<ChatParticipant> ChatParticipants => Set<ChatParticipant>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<ChatMessageRead> ChatMessageReads => Set<ChatMessageRead>();

    // Companies
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanyEmployee> CompanyEmployees => Set<CompanyEmployee>();

    // Agencies
    public DbSet<Agency> Agencies => Set<Agency>();
    public DbSet<AgencyMember> AgencyMembers => Set<AgencyMember>();

    // Wishlist
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();

    // Cart
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    // Discounts
    public DbSet<Discount> Discounts => Set<Discount>();
    public DbSet<DiscountUsage> DiscountUsages => Set<DiscountUsage>();

    // Audit & Outbox
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MarketplaceDbContext).Assembly);

        // Configure soft delete filter for all entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var filter = System.Linq.Expressions.Expression.Lambda(
                    System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false)),
                    parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.Id = entry.Entity.Id == Guid.Empty ? Guid.NewGuid() : entry.Entity.Id;
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
