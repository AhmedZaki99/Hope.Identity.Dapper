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

    private static readonly UserTableNames Default = new();


    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUser{TKey}.Id"/> property.
    /// </summary>
    public string Id { get; set; } = nameof(IdentityUser.Id);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUser{TKey}.NormalizedEmail"/> property.
    /// </summary>
    public string NormalizedEmail { get; set; } = nameof(IdentityUser.NormalizedEmail);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUser{TKey}.NormalizedUserName"/> property.
    /// </summary>
    public string NormalizedUserName { get; set; } = nameof(IdentityUser.NormalizedUserName);


    /// <summary>
    /// Initializes a new instance of the <see cref="UserTableNames"/> class.
    /// </summary>
    public UserTableNames() : base(DefaultPascalCaseTable) { }


    /// <inheritdoc/>
    internal override void ApplyNamingConversionToDefaults(Func<string, string> convertFunction)
    {
        Table = ConvertIfDefault(Table, Default.Table, convertFunction);
        Id = ConvertIfDefault(Id, Default.Id, convertFunction);
        NormalizedEmail = ConvertIfDefault(NormalizedEmail, Default.NormalizedEmail, convertFunction);
        NormalizedUserName = ConvertIfDefault(NormalizedUserName, Default.NormalizedUserName, convertFunction);
    }
}
