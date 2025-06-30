using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RZ.Foundation.Audit.Mongo;

[PublicAPI]
public static class MongoAuditLogSettings
{
    public static IServiceCollection AddMongoAuditLog(this IServiceCollection services, string configKey = "ConnectionStrings:AuditLog", ServiceLifetime lifetime = ServiceLifetime.Singleton)
        => lifetime switch {
            ServiceLifetime.Scoped => services.AddScoped<IAuditLog, MongoAuditLog>()
                                              .AddSingleton<AuditLogDispatcher>()
                                              .AddScoped(CreateDbContext(configKey))
                                              .AddScoped<IAuditLogDispatcher, MongoLogDispatcher>(),
            ServiceLifetime.Singleton => services.AddSingleton<IAuditLog, MongoAuditLog>()
                                                 .AddSingleton<AuditLogDispatcher>()
                                                 .AddSingleton(CreateDbContext(configKey))
                                                 .AddSingleton<IAuditLogDispatcher, MongoLogDispatcher>(),
            ServiceLifetime.Transient => services.AddSingleton<IAuditLog, MongoAuditLog>()
                                                 .AddSingleton<AuditLogDispatcher>()
                                                 .AddTransient(CreateDbContext(configKey))
                                                 .AddSingleton<IAuditLogDispatcher, MongoLogDispatcher>(),

            _ => throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null)
        };

    static Func<IServiceProvider, MongoAuditLogDbContext> CreateDbContext(string configKey) => sp => {
        var config = sp.GetRequiredService<IConfiguration>();
        var logger = sp.GetRequiredService<ILogger<MongoAuditLogDbContext>>();
        var connection = config[configKey] ?? NoConfig(logger, configKey);
        return new MongoAuditLogDbContext(connection);
    };

    [DoesNotReturn]
    static string NoConfig(ILogger logger, string configKey) {
        logger.LogWarning("No connection string key `{ConfigKey}` found in configuration", configKey);
        Environment.Exit(-1);
        throw new Exception(); // never reached
    }
}