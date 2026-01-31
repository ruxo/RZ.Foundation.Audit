using RZ.Foundation.Audit.Models;
using RZ.Foundation.MongoDb;

namespace RZ.Foundation.Audit.Mongo;

public class MongoLogDispatcher(MongoAuditLogDbContext db) : IAuditLogDispatcher
{
    public async ValueTask<Outcome<Unit>> Dispatch(AuditLog log)
        => (await db.GetCollection<AuditLog>().Add(log).ConfigureAwait(false)).Map(_ => unit);
}