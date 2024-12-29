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

    private static readonly UserTokenTableNames Default = new();


    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserToken{TKey}.UserId"/> property.
    /// </summary>
    public string UserId { get; set; } = nameof(IdentityUserToken<string>.UserId);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserToken{TKey}.LoginProvider"/> property.
    /// </summary>
    public string LoginProvider { get; set; } = nameof(IdentityUserToken<string>.LoginProvider);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserToken{TKey}.Name"/> property.
    /// </summary>
    public string Name { get; set; } = nameof(IdentityUserToken<string>.Name);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserToken{TKey}.Value"/> property.
    /// </summary>
    public string Value { get; set; } = nameof(IdentityUserToken<string>.Value);


    /// <summary>
    /// Initializes a new instance of the <see cref="UserTokenTableNames"/> class.
    /// </summary>
    public UserTokenTableNames() : base(DefaultPascalCaseTable) { }


    /// <inheritdoc/>
    internal override void ApplyNamingConversionToDefaults(Func<string, string> convertFunction)
    {
        Table = ConvertIfDefault(Table, Default.Table, convertFunction);
        UserId = ConvertIfDefault(UserId, Default.UserId, convertFunction);
        LoginProvider = ConvertIfDefault(LoginProvider, Default.LoginProvider, convertFunction);
        Name = ConvertIfDefault(Name, Default.Name, convertFunction);
        Value = ConvertIfDefault(Value, Default.Value, convertFunction);
    }
}
