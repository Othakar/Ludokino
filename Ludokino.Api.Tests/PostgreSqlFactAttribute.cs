using Xunit;

namespace Ludokino.Api.Tests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class PostgreSqlFactAttribute : FactAttribute
{
    public PostgreSqlFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("LUDOKINO_TEST_CONNECTION_STRING")))
        {
            Skip = "Définir LUDOKINO_TEST_CONNECTION_STRING pour exécuter les tests PostgreSQL.";
        }
    }
}