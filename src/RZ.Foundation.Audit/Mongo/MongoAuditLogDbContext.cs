using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RZ.Foundation.MongoDb;

namespace RZ.Foundation.Audit.Mongo;

class MongoAuditLogDbContext(ILogger<MongoAuditLogDbContext> logger, IConfiguration configuration)
    : RzMongoDbContext(configuration["AuditLog"] ?? NoConfig(logger))
{
    [DoesNotReturn]
    static string NoConfig(ILogger logger) {
        logger.LogWarning("No configuration for AuditLog");
        Environment.Exit(-1);
        throw new Exception(); // never reached
    }
}