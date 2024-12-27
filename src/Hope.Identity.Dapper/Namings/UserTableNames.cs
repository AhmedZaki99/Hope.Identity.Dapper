using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for the user table.
/// </summary>
/// <remarks>
/// The default pre-conversion table name is "Users", which is then converted using the provided <see cref="JsonNamingPolicy"/>.
/// </remarks>
public class UserTableNames : TableNames
{
    /// <summary>
    /// The default table name in PascalCase (pre-conversion).
    /// </summary>
    public const string DefaultPascalCaseTable = "Users";


    /// <summary>
    /// Gets the column name for the <see cref="IdentityUser{TKey}.Id"/> property.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets the column name for the <see cref="IdentityUser{TKey}.NormalizedEmail"/> property.
    /// </summary>
    public string NormalizedEmail { get; set; }

    /// <summary>
    /// Gets the column name for the <see cref="IdentityUser{TKey}.NormalizedUserName"/> property.
    /// </summary>
    public string NormalizedUserName { get; set; }


    internal UserTableNames(JsonNamingPolicy? namingPolicy = null, string? table = null) 
        : base(table ?? namingPolicy.TryConvertName(DefaultPascalCaseTable), namingPolicy)
    {
        Id = nameof(IdentityUser.Id).ToSqlColumn(NamingPolicy);
        NormalizedEmail = nameof(IdentityUser.NormalizedEmail).ToSqlColumn(NamingPolicy);
        NormalizedUserName = nameof(IdentityUser.NormalizedUserName).ToSqlColumn(NamingPolicy);
    }
}
