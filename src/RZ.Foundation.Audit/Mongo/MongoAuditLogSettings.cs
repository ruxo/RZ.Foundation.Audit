using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace RZ.Foundation.Audit.Mongo;

[PublicAPI]
public static class MongoAuditLogSettings
{
    public static IServiceCollection AddMongoAuditLog(this IServiceCollection services)
        => services.AddSingleton<IAuditLog, MongoAuditLog>()
                   .AddSingleton<AuditLogDispatcher>()
                   .AddScoped<MongoAuditLogDbContext>()
                   .AddScoped<IAuditLogDispatcher, MongoLogDispatcher>();
}