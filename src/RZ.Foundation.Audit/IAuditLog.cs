using RZ.Foundation.Audit.Models;
using RZ.Foundation.Types;

namespace RZ.Foundation.Audit;

public interface IAuditLog
{
    void Log(AuditLog log);

    Task<IReadOnlyList<AuditLog>> SearchLogs(DateRange period, string service, string key, string value);
    Task<IReadOnlyList<AuditLog>> SearchLogs(DateRange period, string service, string action);
    Task<IReadOnlyList<AuditLog>> SearchLogs(DateRange period, string service);
}