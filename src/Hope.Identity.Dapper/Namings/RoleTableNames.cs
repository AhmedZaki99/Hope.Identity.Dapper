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

    private static readonly RoleTableNames Default = new();


    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityRole{TKey}.Id"/> property.
    /// </summary>
    public string Id { get; set; } = nameof(IdentityRole.Id);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityRole{TKey}.Name"/> property.
    /// </summary>
    public string Name { get; set; } = nameof(IdentityRole.Name);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityRole{TKey}.NormalizedName"/> property.
    /// </summary>
    public string NormalizedName { get; set; } = nameof(IdentityRole.NormalizedName);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityRole{TKey}.ConcurrencyStamp"/> property.
    /// </summary>
    public string ConcurrencyStamp { get; set; } = nameof(IdentityRole.ConcurrencyStamp);


    /// <summary>
    /// Initializes a new instance of the <see cref="RoleTableNames"/> class.
    /// </summary>
    public RoleTableNames() : base(DefaultPascalCaseTable) { }


    /// <inheritdoc/>
    internal override void ApplyNamingConversionToDefaults(Func<string, string> convertFunction)
    {
        Table = ConvertIfDefault(Table, Default.Table, convertFunction);
        Id = ConvertIfDefault(Id, Default.Id, convertFunction);
        Name = ConvertIfDefault(Name, Default.Name, convertFunction);
        NormalizedName = ConvertIfDefault(NormalizedName, Default.NormalizedName, convertFunction);
        ConcurrencyStamp = ConvertIfDefault(ConcurrencyStamp, Default.ConcurrencyStamp, convertFunction);
    }
}
