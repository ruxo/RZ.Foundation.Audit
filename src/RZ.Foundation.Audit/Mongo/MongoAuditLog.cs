using System.Runtime.CompilerServices;
using MongoDB.Driver;
using RZ.Foundation.Audit.Models;
using RZ.Foundation.Types;
using RZ.Foundation.Validation;

namespace RZ.Foundation.Audit.Mongo;

class MongoAuditLog(MongoAuditLogDbContext db, AuditLogDispatcher dispatcher) : IAuditLog
{
    public void Log(AuditLog log)
        => dispatcher.Dispatch(log.Validate());

    public IAsyncEnumerable<AuditLog> SearchLogs(DateRange period, string service, string key, string value, CancellationToken cancelToken = default) {
        var b = Builders<AuditLog>.Filter;
        var exp = b.And(b.Eq(x => x.Service, service), b.Eq("Indexes.Key", key), b.Eq("Indexes.Value", value));
        return Query(exp, period, cancelToken);
    }

    public IAsyncEnumerable<AuditLog> SearchLogs(DateRange period, string service, string action, CancellationToken cancelToken = default) {
        var b = Builders<AuditLog>.Filter;
        var exp = b.And(b.Eq(x => x.Service, service), b.Eq(x => x.Action, action));
        return Query(exp, period, cancelToken);
    }

    public IAsyncEnumerable<AuditLog> SearchLogs(DateRange period, string service, CancellationToken cancelToken = default) {
        var exp = Builders<AuditLog>.Filter.Eq(x => x.Service, service);
        return Query(exp, period, cancelToken);
    }

    IAsyncEnumerable<AuditLog> Query(FilterDefinition<AuditLog> exp, DateRange period, CancellationToken cancelToken) {
        var b = Builders<AuditLog>.Filter;
        if (period.Begin is not null)
            exp = b.And(exp, b.Gte(x => x.Timestamp, period.Begin!.Value));
        if (period.End is not null)
            exp = b.And(exp, b.Lt(x => x.Timestamp, period.End!.Value));
        return Query(exp, cancelToken);
    }

    async IAsyncEnumerable<AuditLog> Query(FilterDefinition<AuditLog> exp, [EnumeratorCancellation] CancellationToken cancelToken = default) {
        var cursor = await db.GetCollection<AuditLog>().FindAsync(exp, cancellationToken: cancelToken);
        while (!cancelToken.IsCancellationRequested && await cursor.MoveNextAsync(cancelToken))
            foreach (var log in cursor.Current)
                yield return log;
    }
}