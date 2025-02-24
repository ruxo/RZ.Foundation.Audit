using MongoDB.Driver;
using RZ.Foundation.Audit.Models;
using RZ.Foundation.Types;
using RZ.Foundation.Validation;

namespace RZ.Foundation.Audit.Mongo;

class MongoAuditLog(MongoAuditLogDbContext db, AuditLogDispatcher dispatcher) : IAuditLog
{
    public void Log(AuditLog log)
        => dispatcher.Dispatch(log.Validate());

    public async Task<IReadOnlyList<AuditLog>> SearchLogs(DateRange period, string service, string key, string value) {
        ValidatePeriod(period);
        return await db.GetCollection<AuditLog>()
                       .Find(x => x.Service == service &&
                                  x.Indexes!.Any(i => i.Key == key && i.Value == value) &&
                                  x.Timestamp >= period.Begin && x.Timestamp < period.End)
                       .ToListAsync();
    }
    public async Task<IReadOnlyList<AuditLog>> SearchLogs(DateRange period, string service, string action) {
        ValidatePeriod(period);
        return await db.GetCollection<AuditLog>().Find(x => x.Service == service && x.Action == action && x.Timestamp >= period.Begin && x.Timestamp < period.End).ToListAsync();
    }
    public async Task<IReadOnlyList<AuditLog>> SearchLogs(DateRange period, string service) {
        ValidatePeriod(period);
        return await db.GetCollection<AuditLog>().Find(x => x.Service == service && x.Timestamp >= period.Begin && x.Timestamp < period.End).ToListAsync();
    }

    static void ValidatePeriod(DateRange period) {
        if (period.Begin is null || period.End is null)
            throw new ErrorInfoException(StandardErrorCodes.InvalidRequest, "Period's begin/end cannot be required");
    }
}