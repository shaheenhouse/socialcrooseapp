using Marketplace.Workers.Workers;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

// Redis connection
var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection));

// Register workers
builder.Services.AddHostedService<NotificationWorker>();
builder.Services.AddHostedService<EmailWorker>();
builder.Services.AddHostedService<SearchIndexingWorker>();
builder.Services.AddHostedService<PaymentWorker>();
builder.Services.AddHostedService<RealtimePushWorker>();

var host = builder.Build();
host.Run();
