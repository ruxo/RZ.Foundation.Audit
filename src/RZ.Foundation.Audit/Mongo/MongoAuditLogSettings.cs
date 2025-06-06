using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace RZ.Foundation.Audit.Mongo;

[PublicAPI]
public static class MongoAuditLogSettings
{
    public static IServiceCollection AddMongoAuditLog(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        => lifetime switch {
            ServiceLifetime.Scoped => services.AddScoped<IAuditLog, MongoAuditLog>()
                                              .AddSingleton<AuditLogDispatcher>()
                                              .AddScoped<MongoAuditLogDbContext>()
                                              .AddScoped<IAuditLogDispatcher, MongoLogDispatcher>(),
            ServiceLifetime.Singleton => services.AddSingleton<IAuditLog, MongoAuditLog>()
                                                 .AddSingleton<AuditLogDispatcher>()
                                                 .AddSingleton<MongoAuditLogDbContext>()
                                                 .AddSingleton<IAuditLogDispatcher, MongoLogDispatcher>(),
            ServiceLifetime.Transient => services.AddSingleton<IAuditLog, MongoAuditLog>()
                                                 .AddSingleton<AuditLogDispatcher>()
                                                 .AddTransient<MongoAuditLogDbContext>()
                                                 .AddSingleton<IAuditLogDispatcher, MongoLogDispatcher>(),

            _ => throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null)
        };
}