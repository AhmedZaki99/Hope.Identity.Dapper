using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for the user role table.
/// </summary>
/// <remarks>
/// The default pre-conversion table name is "UserRoles", which is then converted using the provided <see cref="JsonNamingPolicy"/>.
/// </remarks>
public class UserRoleTableNames : TableNames
{
    /// <summary>
    /// The default table name in PascalCase (pre-conversion).
    /// </summary>
    public const string DefaultPascalCaseTable = "UserRoles";


    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserRole{TKey}.UserId"/> property.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserRole{TKey}.RoleId"/> property.
    /// </summary>
    public string RoleId { get; set; }


    internal UserRoleTableNames(JsonNamingPolicy? namingPolicy = null, string? table = null) 
        : base(table ?? namingPolicy.TryConvertName(DefaultPascalCaseTable), namingPolicy)
    {
        UserId = nameof(IdentityUserRole<string>.UserId).ToSqlColumn(namingPolicy);
        RoleId = nameof(IdentityUserRole<string>.RoleId).ToSqlColumn(namingPolicy);
    }
}
