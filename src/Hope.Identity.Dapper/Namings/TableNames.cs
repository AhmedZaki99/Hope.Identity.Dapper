using System.Text.Json;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for a specific table.
/// </summary>
public abstract class TableNames
{
    /// <summary>
    /// The name of the table.
    /// </summary>
    public string Table { get; set; }

    /// <summary>
    /// The naming policy to use for converting the underlying property names to default column names.
    /// </summary>
    protected internal JsonNamingPolicy? NamingPolicy { get; }


    internal TableNames(string table, JsonNamingPolicy? namingPolicy = null)
    {
        Table = table;
        NamingPolicy = namingPolicy;
    }
}
