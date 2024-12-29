using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for the role claim table.
/// </summary>
public class RoleClaimTableNames : TableNames
{
    /// <summary>
    /// The default table name in PascalCase (pre-conversion).
    /// </summary>
    public const string DefaultPascalCaseTable = "RoleClaims";

    private static readonly RoleClaimTableNames Default = new()
    {
        Table = DefaultPascalCaseTable,
        RoleId = nameof(IdentityRoleClaim<string>.RoleId),
        ClaimType = nameof(IdentityRoleClaim<string>.ClaimType),
        ClaimValue = nameof(IdentityRoleClaim<string>.ClaimValue)
    };

    /// <inheritdoc/>
    protected override TableNames GetDefault() => Default;


    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityRoleClaim{TKey}.RoleId"/> property.
    /// </summary>
    public string RoleId { get; set; } = Default.RoleId;

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityRoleClaim{TKey}.ClaimType"/> property.
    /// </summary>
    public string ClaimType { get; set; } = Default.ClaimType;

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityRoleClaim{TKey}.ClaimValue"/> property.
    /// </summary>
    public string ClaimValue { get; set; } = Default.ClaimValue;


    /// <inheritdoc/>
    internal override void ApplyNamingConversionToDefaults(Func<string, string> convertFunction)
    {
        RoleId = ConvertIfDefault(RoleId, Default.RoleId, convertFunction);
        ClaimType = ConvertIfDefault(ClaimType, Default.ClaimType, convertFunction);
        ClaimValue = ConvertIfDefault(ClaimValue, Default.ClaimValue, convertFunction);
    }
}
