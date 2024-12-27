using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for the user token table.
/// </summary>
/// <remarks>
/// The default pre-conversion table name is "UserTokens", which is then converted using the provided <see cref="JsonNamingPolicy"/>.
/// </remarks>
public class UserTokenTableNames : TableNames
{
    /// <summary>
    /// The default table name in PascalCase (pre-conversion).
    /// </summary>
    public const string DefaultPascalCaseTable = "UserTokens";


    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserToken{TKey}.UserId"/> property.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserToken{TKey}.LoginProvider"/> property.
    /// </summary>
    public string LoginProvider { get; set; }

    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserToken{TKey}.Name"/> property.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserToken{TKey}.Value"/> property.
    /// </summary>
    public string Value { get; set; }


    internal UserTokenTableNames(JsonNamingPolicy? namingPolicy = null, string? table = null) 
        : base(table ?? namingPolicy.TryConvertName(DefaultPascalCaseTable), namingPolicy)
    {
        UserId = nameof(IdentityUserToken<string>.UserId).ToSqlColumn(namingPolicy);
        LoginProvider = nameof(IdentityUserToken<string>.LoginProvider).ToSqlColumn(namingPolicy);
        Name = nameof(IdentityUserToken<string>.Name).ToSqlColumn(namingPolicy);
        Value = nameof(IdentityUserToken<string>.Value).ToSqlColumn(namingPolicy);
    }
}
