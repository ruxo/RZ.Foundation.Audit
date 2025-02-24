using JetBrains.Annotations;
using MongoDB.Driver;
using RZ.Foundation.Audit.Models;
using RZ.Foundation.MongoDb.Migration;

namespace RZ.Foundation.Audit.Mongo;

/// <summary>
/// Migration script for setting up AuditLog
/// </summary>
[PublicAPI]
public static class Migrations
{
    public static void Up(IMongoDatabase database, IClientSessionHandle session) {
        database.Build<AuditLog>()
                .Index("ix_application_timeline",
                       b => b.Ascending(m => m.Service)
                             .Ascending("Indexes.Key")
                             .Ascending("Indexes.Value")
                             .Descending(m => m.Timestamp))
                .Index("ix_action", b => b.Ascending(m => m.Service).Ascending(m => m.Action).Descending(m => m.Timestamp))
                .Index("ix_timeline", b => b.Ascending(m => m.Service).Descending(m => m.Timestamp))
                .Run(session);
    }
    public static void Down(IMongoDatabase database, IClientSessionHandle session) {
        database.DropCollection(session, nameof(AuditLog));
    }
}