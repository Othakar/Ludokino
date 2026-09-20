namespace Ludokino.Api.Security;

public static class StartupSecurityValidator
{
    private const string DevelopmentSecretKey = "Ludokino-Development-Secret-Key-1234567890";
    private const string DevelopmentIssuer = "LudokinoApi";
    private const string DevelopmentAudience = "LudokinoClient";

    public static string[] GetCorsOrigins(IConfiguration configuration, IHostEnvironment environment)
    {
        var configuredOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()?
            .Where(static origin => !string.IsNullOrWhiteSpace(origin))
            .Select(static origin => origin.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? Array.Empty<string>();

        if (configuredOrigins.Length == 0)
        {
            if (environment.IsDevelopment())
            {
                return ["http://localhost:3000"];
            }

            throw new InvalidOperationException("Cors:AllowedOrigins doit être configuré en production.");
        }

        if (environment.IsProduction() && configuredOrigins.Any(static origin => !origin.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Cors:AllowedOrigins doit utiliser HTTPS en production.");
        }

        return configuredOrigins;
    }

    public static JwtConfiguration GetJwtConfiguration(IConfiguration configuration, IHostEnvironment environment)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]?.Trim();
        var issuer = jwtSettings["Issuer"]?.Trim();
        var audience = jwtSettings["Audience"]?.Trim();

        if (string.IsNullOrWhiteSpace(secretKey) ||
            string.IsNullOrWhiteSpace(issuer) ||
            string.IsNullOrWhiteSpace(audience))
        {
            if (!environment.IsProduction())
            {
                return new JwtConfiguration(DevelopmentSecretKey, DevelopmentIssuer, DevelopmentAudience);
            }

            throw new InvalidOperationException("JwtSettings (SecretKey, Issuer, Audience) est obligatoire en production.");
        }

        if (secretKey.Length < 32)
        {
            throw new InvalidOperationException("JwtSettings:SecretKey doit contenir au moins 32 caractères.");
        }

        return new JwtConfiguration(secretKey, issuer, audience);
    }

    public static JwtConfiguration GetJwtConfiguration(IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]?.Trim();
        var issuer = jwtSettings["Issuer"]?.Trim();
        var audience = jwtSettings["Audience"]?.Trim();

        if (string.IsNullOrWhiteSpace(secretKey) ||
            string.IsNullOrWhiteSpace(issuer) ||
            string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException("JwtSettings (SecretKey, Issuer, Audience) doit être configuré.");
        }

        if (secretKey.Length < 32)
        {
            throw new InvalidOperationException("JwtSettings:SecretKey doit contenir au moins 32 caractères.");
        }

        return new JwtConfiguration(secretKey, issuer, audience);
    }
}

public sealed record JwtConfiguration(string SecretKey, string Issuer, string Audience);
