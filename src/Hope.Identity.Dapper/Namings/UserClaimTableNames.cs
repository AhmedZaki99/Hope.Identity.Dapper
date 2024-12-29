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

    private static readonly UserClaimTableNames Default = new();


    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserClaim{TKey}.UserId"/> property.
    /// </summary>
    public string UserId { get; set; } = nameof(IdentityUserClaim<string>.UserId);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserClaim{TKey}.ClaimType"/> property.
    /// </summary>
    public string ClaimType { get; set; } = nameof(IdentityUserClaim<string>.ClaimType);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserClaim{TKey}.ClaimValue"/> property.
    /// </summary>
    public string ClaimValue { get; set; } = nameof(IdentityUserClaim<string>.ClaimValue);


    /// <summary>
    /// Initializes a new instance of the <see cref="UserClaimTableNames"/> class.
    /// </summary>
    public UserClaimTableNames() : base(DefaultPascalCaseTable) { }


    /// <inheritdoc/>
    internal override void ApplyNamingConversionToDefaults(Func<string, string> convertFunction)
    {
        Table = ConvertIfDefault(Table, Default.Table, convertFunction);
        UserId = ConvertIfDefault(UserId, Default.UserId, convertFunction);
        ClaimType = ConvertIfDefault(ClaimType, Default.ClaimType, convertFunction);
        ClaimValue = ConvertIfDefault(ClaimValue, Default.ClaimValue, convertFunction);
    }
}
