using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for the user token table.
/// </summary>
/// <param name="namingPolicy">The naming policy to use for converting the <see cref="IdentityUserToken{TKey}"/> property names to default column names.</param>
/// <param name="table">The name of the table.</param>
/// <typeparam name="TKey">The type of the primary key for the user token.</typeparam>
/// <remarks>
/// The default pre-conversion table name is "UserTokens", which the provided <paramref name="namingPolicy"/> use to set the default table name.
/// </remarks>
public class UserTokenTableNames<TKey>(JsonNamingPolicy namingPolicy, string? table = null)
    : TableNames(table ?? namingPolicy.ConvertName(DefaultPascalCaseTable), namingPolicy)
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// The default table name in PascalCase (pre-conversion).
    /// </summary>
    public const string DefaultPascalCaseTable = "UserTokens";


    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserToken{TKey}.UserId"/> property.
    /// </summary>
    public string UserId { get; init; } = nameof(IdentityUserToken<TKey>.UserId).ToSqlColumn(namingPolicy);

    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserToken{TKey}.LoginProvider"/> property.
    /// </summary>
    public string LoginProvider { get; init; } = nameof(IdentityUserToken<TKey>.LoginProvider).ToSqlColumn(namingPolicy);

    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserToken{TKey}.Name"/> property.
    /// </summary>
    public string Name { get; init; } = nameof(IdentityUserToken<TKey>.Name).ToSqlColumn(namingPolicy);

    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserToken{TKey}.Value"/> property.
    /// </summary>
    public string Value { get; init; } = nameof(IdentityUserToken<TKey>.Value).ToSqlColumn(namingPolicy);
}
