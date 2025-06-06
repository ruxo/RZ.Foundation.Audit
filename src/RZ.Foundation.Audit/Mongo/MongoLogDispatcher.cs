using RZ.Foundation.Audit.Models;
using RZ.Foundation.MongoDb;

namespace RZ.Foundation.Audit.Mongo;

public class MongoLogDispatcher(MongoAuditLogDbContext db) : IAuditLogDispatcher
{
    public Task Dispatch(AuditLog log)
        => db.GetCollection<AuditLog>().Add(log);
}