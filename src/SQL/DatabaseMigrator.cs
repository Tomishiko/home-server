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
            Console.WriteLine($"Failed Script: {result.ErrorScript.Name}");
            throw result.Error;
        }
    }
}
