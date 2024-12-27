using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for the user claim table.
/// </summary>
/// <remarks>
/// The default pre-conversion table name is "UserClaims", which is then converted using the provided <see cref="JsonNamingPolicy"/>.
/// </remarks>
public class UserClaimTableNames : TableNames
{
    /// <summary>
    /// The default table name in PascalCase (pre-conversion).
    /// </summary>
    public const string DefaultPascalCaseTable = "UserClaims";


    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserClaim{TKey}.UserId"/> property.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserClaim{TKey}.ClaimType"/> property.
    /// </summary>
    public string ClaimType { get; set; }

    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserClaim{TKey}.ClaimValue"/> property.
    /// </summary>
    public string ClaimValue { get; set; }


    internal UserClaimTableNames(JsonNamingPolicy? namingPolicy = null, string? table = null) 
        : base(table ?? namingPolicy.TryConvertName(DefaultPascalCaseTable), namingPolicy)
    {
        UserId = nameof(IdentityUserClaim<string>.UserId).ToSqlColumn(namingPolicy);
        ClaimType = nameof(IdentityUserClaim<string>.ClaimType).ToSqlColumn(namingPolicy);
        ClaimValue = nameof(IdentityUserClaim<string>.ClaimValue).ToSqlColumn(namingPolicy);
    }
}
