using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for the role table.
/// </summary>
/// <remarks>
/// The default pre-conversion table name is "Roles", which is then converted using the provided <see cref="JsonNamingPolicy"/>.
/// </remarks>
public class RoleTableNames : TableNames
{
    /// <summary>
    /// The default table name in PascalCase (pre-conversion).
    /// </summary>
    public const string DefaultPascalCaseTable = "Roles";


    /// <summary>
    /// Gets the column name for the <see cref="IdentityRole{TKey}.Id"/> property.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets the column name for the <see cref="IdentityRole{TKey}.Name"/> property.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets the column name for the <see cref="IdentityRole{TKey}.NormalizedName"/> property.
    /// </summary>
    public string NormalizedName { get; set; }


    internal RoleTableNames(JsonNamingPolicy? namingPolicy = null, string? table = null) 
        : base(table ?? namingPolicy.TryConvertName(DefaultPascalCaseTable), namingPolicy)
    {
        Id = nameof(IdentityRole.Id).ToSqlColumn(namingPolicy);
        Name = nameof(IdentityRole.Name).ToSqlColumn(namingPolicy);
        NormalizedName = nameof(IdentityRole.NormalizedName).ToSqlColumn(namingPolicy);
    }
}
