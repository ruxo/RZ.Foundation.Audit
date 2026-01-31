using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RZ.Foundation.Audit.Models;
using RZ.Foundation.MongoDb;
using RZ.Foundation.Types;
using RZ.Foundation.Validation;

namespace RZ.Foundation.Audit.Mongo;

public class MongoAuditLog(ILogger<MongoAuditLog> logger, MongoAuditLogDbContext db, AuditLogDispatcher dispatcher) : IAuditLog
{
    public void Log(AuditLog log) {
        if (Success(log.Validate(), out var content, out var e))
            dispatcher.Dispatch(content);
        else
            logger.LogWarning("Audit log failed. It is lost: {Log}, Error: {@Error}", log, e);
    }

    public IAsyncEnumerable<Outcome<AuditLog>> SearchLogs(DateRange period, string service, string key, string value, CancellationToken cancelToken = default) {
        var b = Builders<AuditLog>.Filter;
        var exp = b.And(b.Eq(x => x.Service, service), b.Eq("Indexes.Key", key), b.Eq("Indexes.Value", value));
        return Query(exp, period, cancelToken);
    }

    public IAsyncEnumerable<Outcome<AuditLog>> SearchLogs(DateRange period, string service, string action, CancellationToken cancelToken = default) {
        var b = Builders<AuditLog>.Filter;
        var exp = b.And(b.Eq(x => x.Service, service), b.Eq(x => x.Action, action));
        return Query(exp, period, cancelToken);
    }

    public IAsyncEnumerable<Outcome<AuditLog>> SearchLogs(DateRange period, string service, CancellationToken cancelToken = default) {
        var exp = Builders<AuditLog>.Filter.Eq(x => x.Service, service);
        return Query(exp, period, cancelToken);
    }

    IAsyncEnumerable<Outcome<AuditLog>> Query(FilterDefinition<AuditLog> exp, DateRange period, CancellationToken cancelToken) {
        var b = Builders<AuditLog>.Filter;
        if (period.Begin is not null)
            exp = b.And(exp, b.Gte(x => x.Timestamp, period.Begin!.Value));
        if (period.End is not null)
            exp = b.And(exp, b.Lt(x => x.Timestamp, period.End!.Value));
        return Query(exp, cancelToken);
    }

    async IAsyncEnumerable<Outcome<AuditLog>> Query(FilterDefinition<AuditLog> exp, [EnumeratorCancellation] CancellationToken cancelToken = default) {
        if (Fail(await TryCatch(db.GetCollection<AuditLog>().FindAsync(exp, cancellationToken: cancelToken)).ConfigureAwait(false), out var e, out var cursor)){
            yield return e;
            yield break;
        }
        while (!cancelToken.IsCancellationRequested && Success(await cursor.TryMoveNext(cancelToken), out var cont, out e) && cont)
            foreach (var log in cursor.Current)
                yield return log;
        if (e is not null) yield return e;
    }
}