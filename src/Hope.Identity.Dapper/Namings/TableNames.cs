using System.Text.Json;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for a specific table.
/// </summary>
/// <param name="table">The name of the table.</param>
/// <param name="namingPolicy">The naming policy to use for converting the underlying property names to default column names (<see langword="null"/> for no conversion).</param>
public abstract class TableNames(string table, JsonNamingPolicy? namingPolicy = null)
{
    /// <summary>
    /// The name of the table.
    /// </summary>
    public string Table { get; } = table;

    /// <summary>
    /// The naming policy to use for converting the underlying property names to default column names.
    /// </summary>
    protected JsonNamingPolicy? NamingPolicy { get; } = namingPolicy;
}
