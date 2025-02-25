using JetBrains.Annotations;
using RZ.Foundation.Audit.Models;
using RZ.Foundation.Types;

namespace RZ.Foundation.Audit;

[PublicAPI]
public interface IAuditLog
{
    void Log(AuditLog log);

    IAsyncEnumerable<AuditLog> SearchLogs(DateRange period, string service, string key, string value, CancellationToken cancelToken = default);
    IAsyncEnumerable<AuditLog> SearchLogs(DateRange period, string service, string action, CancellationToken cancelToken = default);
    IAsyncEnumerable<AuditLog> SearchLogs(DateRange period, string service, CancellationToken cancelToken = default);
}