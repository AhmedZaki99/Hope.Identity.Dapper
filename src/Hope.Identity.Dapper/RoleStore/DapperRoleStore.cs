using System.Data.Common;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Provides an implementation for a Dapper-based Identity role store using <see cref="string"/> keys.
/// </summary>
/// <remarks>
/// This class exposes <see cref="ExtraRoleInsertProperties"/> and <see cref="ExtraRoleUpdateProperties"/> properties
/// to implement functionality required in <see cref="DapperRoleStoreBase{TRole, TKey, TUserRole, TRoleClaim}"/>
/// </remarks>
/// <inheritdoc/>
public class DapperRoleStore<TRole, TUserRole, TRoleClaim>
    : DapperRoleStoreBase<TRole, string, TUserRole, TRoleClaim>
    where TRole : IdentityRole<string>
    where TUserRole : IdentityUserRole<string>, new()
    where TRoleClaim : IdentityRoleClaim<string>, new()
{
    /// <summary>
    /// Gets an array of the extra property names within <typeparamref name="TRole"/> used for insert queries (excluding the base <see cref="IdentityRole{TKey}"/> properties).
    /// </summary>
    protected virtual string[] ExtraRoleInsertProperties { get; }

    /// <summary>
    /// Gets an array of the extra property names within <typeparamref name="TRole"/> used for update queries (excluding the base <see cref="IdentityRole{TKey}"/> properties).
    /// </summary>
    protected virtual string[] ExtraRoleUpdateProperties { get; }


    /// <inheritdoc/>
    protected override JsonNamingPolicy? TableNamingPolicy { get; }


    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="dbDataSource">The <see cref="DbDataSource"/> used to create database connections.</param>
    /// <param name="describer">The <see cref="IdentityErrorDescriber"/> used to describe store errors.</param>
    /// <param name="tableNamingPolicy">
    /// The naming policy used to convert property names to SQL table names and/or column names (<see langword="null"/> for no conversion).
    /// <br/><br/>
    /// For example, a naming policy like <see cref="JsonNamingPolicy.SnakeCaseLower"/> will build a query like this:
    /// <code>
    /// INSERT INTO roles (id, name, normalized_name, ...) 
    /// VALUES (@Id, @Name, @NormalizedName, ...)
    /// </code>
    /// </param>
    public DapperRoleStore(DbDataSource dbDataSource, IdentityErrorDescriber? describer, JsonNamingPolicy? tableNamingPolicy = null) 
        : base(dbDataSource, describer) 
    {
        TableNamingPolicy = tableNamingPolicy;

        ExtraRoleInsertProperties = [];
        ExtraRoleUpdateProperties = [];
    }


    /// <remarks></remarks>
    /// <inheritdoc/>
    protected override string GenerateNewKey() => Guid.NewGuid().ToString();

    /// <inheritdoc/>
    protected override string ConvertStringToKey(string key) => key;


    /// <inheritdoc/>
    protected override string[] GetRoleInsertProperties(string[] identityUserInsertProperties) => [.. identityUserInsertProperties, .. ExtraRoleInsertProperties];

    /// <inheritdoc/>
    protected override string[] GetRoleUpdateProperties(string[] identityUserUpdateProperties) => [.. identityUserUpdateProperties, .. ExtraRoleUpdateProperties];
}
