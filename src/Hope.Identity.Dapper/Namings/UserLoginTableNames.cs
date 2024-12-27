using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for the user login table.
/// </summary>
/// <param name="namingPolicy">
/// The naming policy to use for converting the <see cref="IdentityUserLogin{TKey}"/> property names to default column names (<see langword="null"/> for no conversion).
/// </param>
/// <param name="table">The name of the table.</param>
/// <typeparam name="TKey">The type of the primary key for the user login.</typeparam>
/// <remarks>
/// The default pre-conversion table name is "UserLogins", which the provided <paramref name="namingPolicy"/> use to set the default table name.
/// </remarks>
public class UserLoginTableNames<TKey>(JsonNamingPolicy? namingPolicy = null, string? table = null)
    : TableNames(table ?? namingPolicy.TryConvertName(DefaultPascalCaseTable), namingPolicy)
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// The default table name in PascalCase (pre-conversion).
    /// </summary>
    public const string DefaultPascalCaseTable = "UserLogins";


    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserLogin{TKey}.UserId"/> property.
    /// </summary>
    public string UserId { get; init; } = nameof(IdentityUserLogin<TKey>.UserId).ToSqlColumn(namingPolicy);

    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserLogin{TKey}.LoginProvider"/> property.
    /// </summary>
    public string LoginProvider { get; init; } = nameof(IdentityUserLogin<TKey>.LoginProvider).ToSqlColumn(namingPolicy);

    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserLogin{TKey}.ProviderKey"/> property.
    /// </summary>
    public string ProviderKey { get; init; } = nameof(IdentityUserLogin<TKey>.ProviderKey).ToSqlColumn(namingPolicy);

    /// <summary>
    /// Gets the column name for the <see cref="IdentityUserLogin{TKey}.ProviderDisplayName"/> property.
    /// </summary>
    public string ProviderDisplayName { get; init; } = nameof(IdentityUserLogin<TKey>.ProviderDisplayName).ToSqlColumn(namingPolicy);
}
