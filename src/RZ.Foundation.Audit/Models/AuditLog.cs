using System.Diagnostics;
using System.Net;
using FluentValidation;
using JetBrains.Annotations;
using RZ.Foundation.MongoDb;
using RZ.Foundation.Types;
using RZ.Foundation.Validation;

namespace RZ.Foundation.Audit.Models;

[PublicAPI]
public sealed record AuditLog : IHaveValidator<AuditLog>, IHaveKey<Guid>
{
    public static AuditLog New(Actor actor, string action, string message, DateTimeOffset now, string? traceId = null, object? data = null, IndexInfo[]? indexes = null, Guid? id = null) =>
        new() {
            Id = id ?? Guid.CreateVersion7(now),
            Actor = actor,
            Action = action,
            Message = Sanitize(message),
            Data = data,
            Indexes = indexes,
            Timestamp = now,
            TraceId = traceId ?? Activity.Current?.Id ?? throw new ErrorInfoException(StandardErrorCodes.InvalidRequest, "TraceId is required"),
            Service = ((AuditKey)action).Service
        };

    public Guid Id { get; init; }
    public int Version { get; init; } = 1;
    public required Actor Actor { get; init; }
    public required string TraceId { get; init; }

    /// <summary>
    /// Must conform <see cref="AuditKey"/> format.
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// Service name, comes from the service name in <see cref="Action"/>.
    /// </summary>
    public required string Service { get; init; }

    /// <summary>
    /// This field contains a string that is designed to be formatted, providing a meaningful interpretation of this log entry.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// A custom object as an application data. This object will be serialized into a JSON object, which its size must not exceed 1000 characters!
    /// </summary>
    public object? Data { get; init; }

    /// <summary>
    /// Define indexes key and value for searching this log entry.
    /// </summary>
    public IndexInfo[]? Indexes { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    public const int MaxMessageLength = 499;
    public const char HorizontalEllipsis = '…';

    public AuditLog Sanitize() =>
        Message.Length <= MaxMessageLength ? this : this with { Message = Message[..MaxMessageLength] + HorizontalEllipsis };

    static string Sanitize(string message) =>
        message.Length <= MaxMessageLength ? message : message[..MaxMessageLength] + HorizontalEllipsis;

    #region Validator

    public static IValidator<AuditLog> Validator { get; } = new ValidatorType();

    public class ValidatorType : AbstractValidator<AuditLog>
    {
        public ValidatorType() {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Actor).SetValidator(Actor.Validator);
            RuleFor(x => x.TraceId).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Action).NotEmpty();
            RuleFor(x => x.Service).NotEmpty();
            RuleFor(pl => pl.Message).NotEmpty().WithErrorCode(StandardErrorCodes.InvalidRequest).WithMessage("Message must be non-empty string.");
            RuleFor(x => x).Must(x => ((AuditKey)x.Action).Service == x.Service).WithErrorCode(StandardErrorCodes.InvalidRequest).WithMessage("Service must be same as Action's service.");
        }
    }

    #endregion
}

[PublicAPI]
public readonly record struct Actor(string UserId, string Channel, string? UserName = null, string? Ip = null, string[]? Roles = null) : IHaveValidator<Actor>
{
    public static IValidator<Actor> Validator { get; } = new ValidatorType();

    public class ValidatorType : AbstractValidator<Actor>
    {
        public ValidatorType() {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Channel).NotEmpty();
            RuleFor(x => x.Ip).Must(ip => ip is null || IPAddress.TryParse(ip, out _)).WithErrorCode(StandardErrorCodes.InvalidRequest).WithMessage("Incorrect IP Address format.");
        }
    }
}

/// <summary>
/// Channel is a string that indicates the source of the request.
/// </summary>
[PublicAPI]
public static class Channels
{
    public const string Browser = "browser";
    public const string Mobile = "mobile";
    public const string Android = "android";
    public const string Apple = "ios";
    public const string Desktop = "desktop";
    public const string Facebook = "facebook";
    public const string Telegram = "telegram";
    public const string WhatsApp = "whatsapp";
    public const string Email = "email";
    public const string LINE = "line";
}

/// <summary>
/// Index key/value pairs.
/// </summary>
/// <param name="Key">
/// The <b>key</b> field within the <b>indexes</b> array refers to
/// the name of an index key. Much like the <b>action</b> field,
/// this key should maintain uniqueness across
///     the entire banking system. The key should follow
/// the format <b>&lt;system&gt;:&lt;key-name&gt;</b>. Here, <b>&lt;system&gt;</b>
/// symbolizes the distinct system that provides
/// the key, while <b>&lt;key-name&gt;</b> stands for a unique key
///     originating from that specific system.
/// </param>
/// <param name="Value">Index value name</param>
[PublicAPI]
public readonly record struct IndexInfo(string Key, string Value)
{
    public static implicit operator IndexInfo((string Key, string Value) tuple) => new(tuple.Key, tuple.Value);
}