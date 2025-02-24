using RZ.Foundation.Audit.Models;
using RZ.Foundation.MongoDb;

namespace RZ.Foundation.Audit.Mongo;

class MongoLogDispatcher(MongoAuditLogDbContext db) : IAuditLogDispatcher
{
    public Task Dispatch(AuditLog log)
        => db.GetCollection<AuditLog>().Add(log);
}