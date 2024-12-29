using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for the user login table.
/// </summary>
public class UserLoginTableNames : TableNames
{
    /// <summary>
    /// The default table name in PascalCase (pre-conversion).
    /// </summary>
    public const string DefaultPascalCaseTable = "UserLogins";

    private static readonly UserLoginTableNames Default = new()
    {
        Table = DefaultPascalCaseTable,
        UserId = nameof(IdentityUserLogin<string>.UserId),
        LoginProvider = nameof(IdentityUserLogin<string>.LoginProvider),
        ProviderKey = nameof(IdentityUserLogin<string>.ProviderKey),
        ProviderDisplayName = nameof(IdentityUserLogin<string>.ProviderDisplayName)
    };

    /// <inheritdoc/>
    protected override TableNames GetDefault() => Default;


    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserLogin{TKey}.UserId"/> property.
    /// </summary>
    public string UserId { get; set; } = Default.UserId;

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserLogin{TKey}.LoginProvider"/> property.
    /// </summary>
    public string LoginProvider { get; set; } = Default.LoginProvider;

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserLogin{TKey}.ProviderKey"/> property.
    /// </summary>
    public string ProviderKey { get; set; } = Default.ProviderKey;

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserLogin{TKey}.ProviderDisplayName"/> property.
    /// </summary>
    public string ProviderDisplayName { get; set; } = Default.ProviderDisplayName;


    /// <inheritdoc/>
    internal override void ApplyNamingConversionToDefaults(Func<string, string> convertFunction)
    {
        UserId = ConvertIfDefault(UserId, Default.UserId, convertFunction);
        LoginProvider = ConvertIfDefault(LoginProvider, Default.LoginProvider, convertFunction);
        ProviderKey = ConvertIfDefault(ProviderKey, Default.ProviderKey, convertFunction);
        ProviderDisplayName = ConvertIfDefault(ProviderDisplayName, Default.ProviderDisplayName, convertFunction);
    }
}
