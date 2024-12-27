using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for the role claim table.
/// </summary>
/// <remarks>
/// The default pre-conversion table name is "RoleClaims", which is then converted using the provided <see cref="JsonNamingPolicy"/>.
/// </remarks>
public class RoleClaimTableNames : TableNames
{
    /// <summary>
    /// The default table name in PascalCase (pre-conversion).
    /// </summary>
    public const string DefaultPascalCaseTable = "RoleClaims";


    /// <summary>
    /// Gets the column name for the <see cref="IdentityRoleClaim{TKey}.RoleId"/> property.
    /// </summary>
    public string RoleId { get; set; }

    /// <summary>
    /// Gets the column name for the <see cref="IdentityRoleClaim{TKey}.ClaimType"/> property.
    /// </summary>
    public string ClaimType { get; set; }

    /// <summary>
    /// Gets the column name for the <see cref="IdentityRoleClaim{TKey}.ClaimValue"/> property.
    /// </summary>
    public string ClaimValue { get; set; }


    internal RoleClaimTableNames(JsonNamingPolicy? namingPolicy = null, string? table = null) 
        : base(table ?? namingPolicy.TryConvertName(DefaultPascalCaseTable), namingPolicy)
    {
        RoleId = nameof(IdentityRoleClaim<string>.RoleId).ToSqlColumn(namingPolicy);
        ClaimType = nameof(IdentityRoleClaim<string>.ClaimType).ToSqlColumn(namingPolicy);
        ClaimValue = nameof(IdentityRoleClaim<string>.ClaimValue).ToSqlColumn(namingPolicy);
    }
}
