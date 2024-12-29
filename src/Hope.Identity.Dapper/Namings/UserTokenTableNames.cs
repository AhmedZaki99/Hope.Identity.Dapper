using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for the user token table.
/// </summary>
public class UserTokenTableNames : TableNames
{
    /// <summary>
    /// The default table name in PascalCase (pre-conversion).
    /// </summary>
    public const string DefaultPascalCaseTable = "UserTokens";

    private static readonly UserTokenTableNames Default = new()
    {
        Table = DefaultPascalCaseTable,
        UserId = nameof(IdentityUserToken<string>.UserId),
        LoginProvider = nameof(IdentityUserToken<string>.LoginProvider),
        Name = nameof(IdentityUserToken<string>.Name),
        Value = nameof(IdentityUserToken<string>.Value)
    };

    /// <inheritdoc/>
    protected override TableNames GetDefault() => Default;


    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserToken{TKey}.UserId"/> property.
    /// </summary>
    public string UserId { get; set; } = Default.UserId;

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserToken{TKey}.LoginProvider"/> property.
    /// </summary>
    public string LoginProvider { get; set; } = Default.LoginProvider;

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserToken{TKey}.Name"/> property.
    /// </summary>
    public string Name { get; set; } = Default.Name;

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserToken{TKey}.Value"/> property.
    /// </summary>
    public string Value { get; set; } = Default.Value;


    /// <inheritdoc/>
    internal override void ApplyNamingConversionToDefaults(Func<string, string> convertFunction)
    {
        UserId = ConvertIfDefault(UserId, Default.UserId, convertFunction);
        LoginProvider = ConvertIfDefault(LoginProvider, Default.LoginProvider, convertFunction);
        Name = ConvertIfDefault(Name, Default.Name, convertFunction);
        Value = ConvertIfDefault(Value, Default.Value, convertFunction);
    }
}
