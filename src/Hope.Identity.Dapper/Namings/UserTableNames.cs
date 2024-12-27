using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for the user table.
/// </summary>
/// <param name="namingPolicy">The naming policy to use for converting the <see cref="IdentityUser{TKey}"/> property names to default column names.</param>
/// <param name="table">The name of the table.</param>
/// <typeparam name="TKey">The type of the primary key for the user.</typeparam>
/// <remarks>
/// The default pre-conversion table name is "Users", which the provided <paramref name="namingPolicy"/> use to set the default table name.
/// </remarks>
public class UserTableNames<TKey>(JsonNamingPolicy namingPolicy, string? table = null)
    : TableNames(table ?? namingPolicy.ConvertName(DefaultPascalCaseTable), namingPolicy)
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// The default table name in PascalCase (pre-conversion).
    /// </summary>
    public const string DefaultPascalCaseTable = "Users";


    /// <summary>
    /// Gets the column name for the <see cref="IdentityUser{TKey}.Id"/> property.
    /// </summary>
    public string Id { get; init; } = nameof(IdentityUser<TKey>.Id).ToSqlColumn(namingPolicy);

    /// <summary>
    /// Gets the column name for the <see cref="IdentityUser{TKey}.NormalizedEmail"/> property.
    /// </summary>
    public string NormalizedEmail { get; init; } = nameof(IdentityUser<TKey>.NormalizedEmail).ToSqlColumn(namingPolicy);

    /// <summary>
    /// Gets the column name for the <see cref="IdentityUser{TKey}.NormalizedUserName"/> property.
    /// </summary>
    public string NormalizedUserName { get; init; } = nameof(IdentityUser<TKey>.NormalizedUserName).ToSqlColumn(namingPolicy);
}
