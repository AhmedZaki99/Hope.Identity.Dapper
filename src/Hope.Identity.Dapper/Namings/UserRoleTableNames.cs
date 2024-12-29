using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for the user role table.
/// </summary>
public class UserRoleTableNames : TableNames
{
    /// <summary>
    /// The default table name in PascalCase (pre-conversion).
    /// </summary>
    public const string DefaultPascalCaseTable = "UserRoles";

    private static readonly UserRoleTableNames Default = new();


    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserRole{TKey}.UserId"/> property.
    /// </summary>
    public string UserId { get; set; } = nameof(IdentityUserRole<string>.UserId);

    /// <summary>
    /// Gets or sets the column name for the <see cref="IdentityUserRole{TKey}.RoleId"/> property.
    /// </summary>
    public string RoleId { get; set; } = nameof(IdentityUserRole<string>.RoleId);


    /// <summary>
    /// Initializes a new instance of the <see cref="UserRoleTableNames"/> class.
    /// </summary>
    public UserRoleTableNames() : base(DefaultPascalCaseTable) { }


    /// <inheritdoc/>
    internal override void ApplyNamingConversionToDefaults(Func<string, string> convertFunction)
    {
        Table = ConvertIfDefault(Table, Default.Table, convertFunction);
        UserId = ConvertIfDefault(UserId, Default.UserId, convertFunction);
        RoleId = ConvertIfDefault(RoleId, Default.RoleId, convertFunction);
    }
}
