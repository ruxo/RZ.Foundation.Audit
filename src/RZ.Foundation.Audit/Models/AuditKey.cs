using System.Diagnostics;
using System.Text.RegularExpressions;
using FluentValidation;
using JetBrains.Annotations;
using RZ.Foundation.Validation;

namespace RZ.Foundation.Audit.Models;

[PublicAPI]
public readonly partial record struct AuditKey : IHaveValidator<AuditKey>
{
    const char Delimiter = ':';

    public string Value { get; private init; }
    public string Service { get; private init; }
    public string Identifier { get; private init; }

    public static Regex ValidPattern => ActionRegex();

    public static implicit operator AuditKey (string action) {
        if (!ValidPattern.IsMatch(action))
            throw new FormatException($"Invalid format for Audit Action: {action[..Math.Min(20, action.Length)]}");
        var separator = action.IndexOf(Delimiter);
        Debug.Assert(separator != -1);
        var service = action[..separator];
        var identifier = action[(separator + 1)..];
        return new(){ Value = action, Service = service, Identifier = identifier };
    }

    [GeneratedRegex("^[a-z0-9-]+:(?!-)[a-z0-9-:]+[a-z0-9]$", RegexOptions.None, "en-US")]
    private static partial Regex ActionRegex();

    public static IValidator<AuditKey> Validator { get; } = new ValidatorType();

    public class ValidatorType : AbstractValidator<AuditKey>
    {
        public ValidatorType() {
            RuleFor(x => x.Value).NotEmpty().Matches(ValidPattern).WithMessage("Incorrect AuditKey format");
        }
    }
}