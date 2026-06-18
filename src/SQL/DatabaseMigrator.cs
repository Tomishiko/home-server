using DbUp;

namespace SQL;

public static class DatabaseMigrator
{
    public static void Upgrade(string connectionString)
    {
        EnsureDatabase.For.PostgresqlDatabase(connectionString);

        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(typeof(DatabaseMigrator).Assembly) // Finds scripts in this project
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            throw new Exception($"Failed Script: {result.ErrorScript.Name}", result.Error);
        }
    }
}
