using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for the user login table.
/// </summary>
/// <remarks>
/// The default pre-conversion table name is "UserLogins", which is then converted using the provided <see cref="JsonNamingPolicy"/>.
/// </remarks>
public class UserLoginTableNames : TableNames
{
    /// <summary>
    /// The default table name in PascalCase (pre-conversion).
    /// </summary>
    public const string DefaultPascalCaseTable = "UserLogins";


    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserLogin{TKey}.UserId"/> property.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserLogin{TKey}.LoginProvider"/> property.
    /// </summary>
    public string LoginProvider { get; set; }

    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserLogin{TKey}.ProviderKey"/> property.
    /// </summary>
    public string ProviderKey { get; set; }

    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserLogin{TKey}.ProviderDisplayName"/> property.
    /// </summary>
    public string ProviderDisplayName { get; set; }


    internal UserLoginTableNames(JsonNamingPolicy? namingPolicy = null, string? table = null) 
        : base(table ?? namingPolicy.TryConvertName(DefaultPascalCaseTable), namingPolicy)
    {
        UserId = nameof(IdentityUserLogin<string>.UserId).ToSqlColumn(namingPolicy);
        LoginProvider = nameof(IdentityUserLogin<string>.LoginProvider).ToSqlColumn(namingPolicy);
        ProviderKey = nameof(IdentityUserLogin<string>.ProviderKey).ToSqlColumn(namingPolicy);
        ProviderDisplayName = nameof(IdentityUserLogin<string>.ProviderDisplayName).ToSqlColumn(namingPolicy);
    }
}
