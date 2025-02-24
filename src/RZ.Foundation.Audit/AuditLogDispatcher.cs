using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using RZ.Foundation.Akka;
using RZ.Foundation.Audit.Models;

namespace RZ.Foundation.Audit;

public interface IAuditLogDispatcher
{
    Task Dispatch(AuditLog log);
}

sealed class AuditLogDispatcher(ActorSystem system, IAuditLogDispatcher dispatcher)
{
    readonly IActorRef dispatcherRef = system.CreateActor<Dispatcher>("audit-log-dispatcher", dispatcher);

    public void Dispatch(AuditLog log)
        => dispatcherRef.Tell(log);

    [UsedImplicitly]
    class Dispatcher(ILogger<Dispatcher> logger, IAuditLogDispatcher dispatcher) : UntypedActor
    {
        protected override void OnReceive(object message) {
            switch (message){
                case AuditLog log:
                    RunTask(async () => {
                        try{
                            await dispatcher.Dispatch(log);
                        }
                        catch (Exception e){
                            logger.LogError(e, "Audit log failed. It is lost: {Log}", log);
                        }
                    });
                    break;
                default:
                    Unhandled(message);
                    break;
            }
        }
    }
}