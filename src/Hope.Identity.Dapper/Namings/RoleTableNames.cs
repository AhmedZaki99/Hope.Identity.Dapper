using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for the role table.
/// </summary>
/// <param name="namingPolicy">The naming policy to use for converting the <see cref="IdentityRole{TKey}"/> property names to default column names.</param>
/// <param name="table">The name of the table.</param>
/// <typeparam name="TKey">The type of the primary key for the role.</typeparam>
/// <remarks>
/// The default pre-conversion table name is "Roles", which the provided <paramref name="namingPolicy"/> use to set the default table name.
/// </remarks>
public class RoleTableNames<TKey>(JsonNamingPolicy namingPolicy, string? table = null)
    : TableNames(table ?? namingPolicy.ConvertName(DefaultPascalCaseTable), namingPolicy)
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// The default table name in PascalCase (pre-conversion).
    /// </summary>
    public const string DefaultPascalCaseTable = "Roles";


    /// <summary>
    /// Gets the column name for the <see cref="IdentityRole{TKey}.Id"/> property.
    /// </summary>
    public string Id { get; init; } = nameof(IdentityRole<TKey>.Id).ToSqlColumn(namingPolicy);

    /// <summary>
    /// Gets the column name for the <see cref="IdentityRole{TKey}.Name"/> property.
    /// </summary>
    public string Name { get; init; } = nameof(IdentityRole<TKey>.Name).ToSqlColumn(namingPolicy);

    /// <summary>
    /// Gets the column name for the <see cref="IdentityRole{TKey}.NormalizedName"/> property.
    /// </summary>
    public string NormalizedName { get; init; } = nameof(IdentityRole<TKey>.NormalizedName).ToSqlColumn(namingPolicy);
}
