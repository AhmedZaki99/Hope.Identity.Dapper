using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for the user table.
/// </summary>
public class UserTableNames : TableNames
{
    /// <summary>
    /// The default table name in PascalCase (pre-conversion).
    /// </summary>
    public const string DefaultPascalCaseTable = "Users";

    private static readonly UserTableNames Default = new()
    {
        Table = DefaultPascalCaseTable,
        Id = nameof(IdentityUser.Id),
        NormalizedEmail = nameof(IdentityUser.NormalizedEmail),
        NormalizedUserName = nameof(IdentityUser.NormalizedUserName)
    };

    /// <inheritdoc/>
    protected override TableNames GetDefault() => Default;


    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUser{TKey}.Id"/> property.
    /// </summary>
    public string Id { get; set; } = Default.Id;

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUser{TKey}.NormalizedEmail"/> property.
    /// </summary>
    public string NormalizedEmail { get; set; } = Default.NormalizedEmail;

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUser{TKey}.NormalizedUserName"/> property.
    /// </summary>
    public string NormalizedUserName { get; set; } = Default.NormalizedUserName;


    /// <inheritdoc/>
    internal override void ApplyNamingConversionToDefaults(Func<string, string> convertFunction)
    {
        Id = ConvertIfDefault(Id, Default.Id, convertFunction);
        NormalizedEmail = ConvertIfDefault(NormalizedEmail, Default.NormalizedEmail, convertFunction);
        NormalizedUserName = ConvertIfDefault(NormalizedUserName, Default.NormalizedUserName, convertFunction);
    }
}
