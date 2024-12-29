using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for the role table.
/// </summary>
public class RoleTableNames : TableNames
{
    /// <summary>
    /// The default table name in PascalCase (pre-conversion).
    /// </summary>
    public const string DefaultPascalCaseTable = "Roles";

    private static readonly RoleTableNames Default = new()
    {
        Table = DefaultPascalCaseTable,
        Id = nameof(IdentityRole.Id),
        Name = nameof(IdentityRole.Name),
        NormalizedName = nameof(IdentityRole.NormalizedName)
    };

    /// <inheritdoc/>
    protected override TableNames GetDefault() => Default;


    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityRole{TKey}.Id"/> property.
    /// </summary>
    public string Id { get; set; } = Default.Id;

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityRole{TKey}.Name"/> property.
    /// </summary>
    public string Name { get; set; } = Default.Name;

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityRole{TKey}.NormalizedName"/> property.
    /// </summary>
    public string NormalizedName { get; set; } = Default.NormalizedName;


    /// <inheritdoc/>
    internal override void ApplyNamingConversionToDefaults(Func<string, string> convertFunction)
    {
        Id = ConvertIfDefault(Id, Default.Id, convertFunction);
        Name = ConvertIfDefault(Name, Default.Name, convertFunction);
        NormalizedName = ConvertIfDefault(NormalizedName, Default.NormalizedName, convertFunction);
    }
}
