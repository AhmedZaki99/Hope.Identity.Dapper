using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for the role claim table.
/// </summary>
/// <param name="namingPolicy">
/// The naming policy to use for converting the <see cref="IdentityRoleClaim{TKey}"/> property names to default column names (<see langword="null"/> for no conversion).
/// </param>
/// <param name="table">The name of the table.</param>
/// <typeparam name="TKey">The type of the primary key for the role claim.</typeparam>
/// <remarks>
/// The default pre-conversion table name is "RoleClaims", which the provided <paramref name="namingPolicy"/> use to set the default table name.
/// </remarks>
public class RoleClaimTableNames<TKey>(JsonNamingPolicy? namingPolicy = null, string? table = null)
    : TableNames(table ?? namingPolicy.TryConvertName(DefaultPascalCaseTable), namingPolicy)
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// The default table name in PascalCase (pre-conversion).
    /// </summary>
    public const string DefaultPascalCaseTable = "RoleClaims";


    /// <summary>
    /// Gets the column name for the <see cref="IdentityRoleClaim{TKey}.RoleId"/> property.
    /// </summary>
    public string RoleId { get; init; } = nameof(IdentityRoleClaim<TKey>.RoleId).ToSqlColumn(namingPolicy);

    /// <summary>
    /// Gets the column name for the <see cref="IdentityRoleClaim{TKey}.ClaimType"/> property.
    /// </summary>
    public string ClaimType { get; init; } = nameof(IdentityRoleClaim<TKey>.ClaimType).ToSqlColumn(namingPolicy);

    /// <summary>
    /// Gets the column name for the <see cref="IdentityRoleClaim{TKey}.ClaimValue"/> property.
    /// </summary>
    public string ClaimValue { get; init; } = nameof(IdentityRoleClaim<TKey>.ClaimValue).ToSqlColumn(namingPolicy);
}
