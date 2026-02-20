using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Marketplace.Core;
using Marketplace.Database;
using Marketplace.Slices;
using Marketplace.Realtime;
using Marketplace.Realtime.Hubs;
using Marketplace.Api.Endpoints;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/marketplace-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info.Title = "Social Marketplace API";
        document.Info.Version = "v1";
        document.Info.Description = "A comprehensive social marketplace API with multi-role support, escrow payments, and project management";
        return Task.CompletedTask;
    });
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:3000"])
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Database Context (for EF Core operations)
builder.Services.AddDbContext<MarketplaceDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Add Core infrastructure
builder.Services.AddMarketplaceCore(builder.Configuration);

// Add Slices (repositories and services)
builder.Services.AddMarketplaceSlices();

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("SocialMarketplace.Api"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddMeter("Marketplace.Api")
            .AddMeter("Marketplace.Core");
    });

// SignalR Real-time services
var redisConn = builder.Configuration.GetConnectionString("Redis");
builder.Services.AddRealtimeServices(redisConn);

// Health checks (Redis is optional)
var healthChecks = builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection") ?? "", name: "database");

if (!string.IsNullOrWhiteSpace(redisConn))
{
    healthChecks.AddRedis(redisConn, name: "redis");
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/openapi/v1.json", "Social Marketplace API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseSerilogRequestLogging();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// Prometheus metrics
app.UseHttpMetrics();
app.MapMetrics();

// Health check endpoint
app.MapHealthChecks("/health");

// Map API endpoints
app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapStoreEndpoints();
app.MapProductEndpoints();
app.MapOrderEndpoints();
app.MapProjectEndpoints();
app.MapReviewEndpoints();
app.MapWalletEndpoints();
app.MapNotificationEndpoints();
app.MapCompanyEndpoints();
app.MapSearchEndpoints();
app.MapMessageEndpoints();
app.MapFollowEndpoints();
app.MapConnectionEndpoints();
app.MapPortfolioEndpoints();
app.MapResumeEndpoints();
app.MapDesignEndpoints();
app.MapKhataEndpoints();
app.MapPostEndpoints();
app.MapCartEndpoints();
app.MapWishlistEndpoints();
app.MapDiscountEndpoints();
app.MapTenderEndpoints();
app.MapInventoryEndpoints();
app.MapInvoiceEndpoints();

// SignalR Hubs
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<PresenceHub>("/hubs/presence");

// Root endpoint
app.MapGet("/", () => Results.Ok(new
{
    Name = "Social Marketplace API",
    Version = "2.0.0",
    Status = "Running",
    Documentation = "/swagger",
    Features = new[]
    {
        "Authentication & Authorization (JWT + Refresh Tokens)",
        "User & Profile Management",
        "Social Feed (Posts, Reactions, Comments)",
        "Marketplace (Stores, Products, Services)",
        "Orders & Transactions",
        "Projects & Freelancing",
        "Government Tenders",
        "HR & Company Management",
        "Reviews & Ratings",
        "Wallet & Escrow Payments",
        "Real-time Messaging (SignalR)",
        "Social Networking (Connections, Follows)",
        "LinkedIn-level Search",
        "Notifications (Real-time + Push)",
        "Skills & Certifications",
        "Portfolio Builder",
        "Resume Management",
        "Design Studio (Canva-like)",
        "Khata / Ledger (Credit-Debit Tracking)",
        "Invoicing & Billing",
        "Inventory Management",
        "Analytics & Reporting",
        "Multi-language (EN, UR, AR, ZH, ES)",
        "Dark/Light Theme",
        "Feature Flags"
    }
}));

app.MapPost("/api/admin/seed", async (MarketplaceDbContext db) =>
{
    await DatabaseSeeder.SeedAsync(db);
    await DatabaseSeeder.SeedSuperAdminAsync(db);
    return Results.Ok(new { message = "Database seeded successfully" });
}).WithTags("Admin");

app.MapPost("/api/admin/seed-superadmin", async (MarketplaceDbContext db) =>
{
    await DatabaseSeeder.SeedSuperAdminAsync(db);
    return Results.Ok(new { message = "Super admin seeded successfully" });
}).WithTags("Admin");

app.Run();

internal sealed class BearerSecuritySchemeTransformer(
    IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var authSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (!authSchemes.Any(s => s.Name == "Bearer"))
            return;

        var bearerScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme."
        };

        document.Components ??= new OpenApiComponents();
        document.AddComponent("Bearer", bearerScheme);

        var securityRequirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        };

        foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
        {
            operation.Value.Security ??= new List<OpenApiSecurityRequirement>();
            operation.Value.Security.Add(securityRequirement);
        }
    }
}
