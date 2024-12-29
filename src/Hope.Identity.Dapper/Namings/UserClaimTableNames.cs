using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for the user claim table.
/// </summary>
public class UserClaimTableNames : TableNames
{
    /// <summary>
    /// The default table name in PascalCase (pre-conversion).
    /// </summary>
    public const string DefaultPascalCaseTable = "UserClaims";

    private static readonly UserClaimTableNames Default = new()
    {
        Table = DefaultPascalCaseTable,
        UserId = nameof(IdentityUserClaim<string>.UserId),
        ClaimType = nameof(IdentityUserClaim<string>.ClaimType),
        ClaimValue = nameof(IdentityUserClaim<string>.ClaimValue)
    };

    /// <inheritdoc/>
    protected override TableNames GetDefault() => Default;


    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserClaim{TKey}.UserId"/> property.
    /// </summary>
    public string UserId { get; set; } = Default.UserId;

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserClaim{TKey}.ClaimType"/> property.
    /// </summary>
    public string ClaimType { get; set; } = Default.ClaimType;

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserClaim{TKey}.ClaimValue"/> property.
    /// </summary>
    public string ClaimValue { get; set; } = Default.ClaimValue;


    /// <inheritdoc/>
    internal override void ApplyNamingConversionToDefaults(Func<string, string> convertFunction)
    {
        UserId = ConvertIfDefault(UserId, Default.UserId, convertFunction);
        ClaimType = ConvertIfDefault(ClaimType, Default.ClaimType, convertFunction);
        ClaimValue = ConvertIfDefault(ClaimValue, Default.ClaimValue, convertFunction);
    }
}
