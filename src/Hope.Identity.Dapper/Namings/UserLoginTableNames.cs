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

    private static readonly UserLoginTableNames Default = new();


    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserLogin{TKey}.UserId"/> property.
    /// </summary>
    public string UserId { get; set; } = nameof(IdentityUserLogin<string>.UserId);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserLogin{TKey}.LoginProvider"/> property.
    /// </summary>
    public string LoginProvider { get; set; } = nameof(IdentityUserLogin<string>.LoginProvider);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserLogin{TKey}.ProviderKey"/> property.
    /// </summary>
    public string ProviderKey { get; set; } = nameof(IdentityUserLogin<string>.ProviderKey);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserLogin{TKey}.ProviderDisplayName"/> property.
    /// </summary>
    public string ProviderDisplayName { get; set; } = nameof(IdentityUserLogin<string>.ProviderDisplayName);


    /// <summary>
    /// Initializes a new instance of the <see cref="UserLoginTableNames"/> class.
    /// </summary>
    public UserLoginTableNames() : base(DefaultPascalCaseTable) { }


    /// <inheritdoc/>
    internal override void ApplyNamingConversionToDefaults(Func<string, string> convertFunction)
    {
        Table = ConvertIfDefault(Table, Default.Table, convertFunction);
        UserId = ConvertIfDefault(UserId, Default.UserId, convertFunction);
        LoginProvider = ConvertIfDefault(LoginProvider, Default.LoginProvider, convertFunction);
        ProviderKey = ConvertIfDefault(ProviderKey, Default.ProviderKey, convertFunction);
        ProviderDisplayName = ConvertIfDefault(ProviderDisplayName, Default.ProviderDisplayName, convertFunction);
    }
}
