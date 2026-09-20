using System.ComponentModel.DataAnnotations;

namespace Ludokino.Api.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class HttpUrlListAttribute : ValidationAttribute
{
    private static ValidationResult BuildError(string message, ValidationContext validationContext)
    {
        var memberName = validationContext.MemberName;
        return string.IsNullOrWhiteSpace(memberName)
            ? new ValidationResult(message)
            : new ValidationResult(message, [memberName]);
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (value is not IEnumerable<string> values)
        {
            return BuildError("Le champ doit être une liste d'URL.", validationContext);
        }

        foreach (var item in values)
        {
            if (string.IsNullOrWhiteSpace(item))
            {
                return BuildError("Chaque URL de la liste doit être renseignée.", validationContext);
            }

            if (!Uri.TryCreate(item, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return BuildError("Chaque URL de la liste doit être une URL HTTP/HTTPS valide.", validationContext);
            }
        }

        return ValidationResult.Success;
    }
}
