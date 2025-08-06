using System.Data;
using Npgsql;

namespace TennisScores.Tests.Integration.Services;
public class DatabaseTests
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=tennisdb;Username=dan;Password=uginale";

    [Fact]
    public async Task Tables_Should_Exist_In_Database()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        // Récupérer la liste des tables du schéma public
        DataTable tables = connection.GetSchema("Tables", [null, "public"]);

        // Liste des tables attendues
        string[] expectedTables = { "Players", "Matches", "Sets", "Games", "Points" };

        foreach (var tableName in expectedTables)
        {
            bool tableExists = false;
            foreach (DataRow row in tables.Rows)
            {
                if (string.Equals(row["table_name"].ToString(), tableName, StringComparison.OrdinalIgnoreCase))
                {
                    tableExists = true;
                    break;
                }
            }
            Assert.True(tableExists, $"Table '{tableName}' should exist in the database.");
        }
    }
}
