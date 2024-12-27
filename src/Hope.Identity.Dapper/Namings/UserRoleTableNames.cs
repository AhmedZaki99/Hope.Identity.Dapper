using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for the user role table.
/// </summary>
/// <param name="namingPolicy">The naming policy to use for converting the <see cref="IdentityUserRole{TKey}"/> property names to default column names.</param>
/// <param name="table">The name of the table.</param>
/// <typeparam name="TKey">The type of the primary key for the user role.</typeparam>
/// <remarks>
/// The default pre-conversion table name is "UserRoles", which the provided <paramref name="namingPolicy"/> use to set the default table name.
/// </remarks>
public class UserRoleTableNames<TKey>(JsonNamingPolicy namingPolicy, string? table = null)
    : TableNames(table ?? namingPolicy.ConvertName(DefaultPascalCaseTable), namingPolicy)
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// The default table name in PascalCase (pre-conversion).
    /// </summary>
    public const string DefaultPascalCaseTable = "UserRoles";


    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserRole{TKey}.UserId"/> property.
    /// </summary>
    public string UserId { get; init; } = nameof(IdentityUserRole<TKey>.UserId).ToSqlColumn(namingPolicy);

    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserRole{TKey}.RoleId"/> property.
    /// </summary>
    public string RoleId { get; init; } = nameof(IdentityUserRole<TKey>.RoleId).ToSqlColumn(namingPolicy);
}
