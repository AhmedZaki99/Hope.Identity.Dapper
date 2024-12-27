using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for the user claim table.
/// </summary>
/// <param name="namingPolicy">The naming policy to use for converting the <see cref="IdentityUserClaim{TKey}"/> property names to default column names.</param>
/// <param name="table">The name of the table.</param>
/// <typeparam name="TKey">The type of the primary key for the user claim.</typeparam>
/// <remarks>
/// The default pre-conversion table name is "UserClaims", which the provided <paramref name="namingPolicy"/> use to set the default table name.
/// </remarks>
public class UserClaimTableNames<TKey>(JsonNamingPolicy namingPolicy, string? table = null)
    : TableNames(table ?? namingPolicy.ConvertName(DefaultPascalCaseTable), namingPolicy)
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// The default table name in PascalCase (pre-conversion).
    /// </summary>
    public const string DefaultPascalCaseTable = "UserClaims";


    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserClaim{TKey}.UserId"/> property.
    /// </summary>
    public string UserId { get; init; } = nameof(IdentityUserClaim<TKey>.UserId).ToSqlColumn(namingPolicy);

    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserClaim{TKey}.ClaimType"/> property.
    /// </summary>
    public string ClaimType { get; init; } = nameof(IdentityUserClaim<TKey>.ClaimType).ToSqlColumn(namingPolicy);

    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserClaim{TKey}.ClaimValue"/> property.
    /// </summary>
    public string ClaimValue { get; init; } = nameof(IdentityUserClaim<TKey>.ClaimValue).ToSqlColumn(namingPolicy);
}
