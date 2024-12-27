using System.Data.Common;
using System.Security.Claims;
using System.Text.Json;
using Dapper;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Provides the base implementation for a Dapper-based Identity role store.
/// </summary>
/// <remarks>
/// This class implements most functionality required by <see cref="RoleStoreBase{TRole, TKey, TUserRole, TRoleClaim}"/>
/// </remarks>
/// <inheritdoc/>
public abstract class DapperRoleStoreBase<TRole, TKey, TUserRole, TRoleClaim>
    : RoleStoreBase<TRole, TKey, TUserRole, TRoleClaim>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
    where TUserRole : IdentityUserRole<TKey>, new()
    where TRoleClaim : IdentityRoleClaim<TKey>, new()
{

    #region Protected Properties

    /// <summary>
    /// Gets the <see cref="JsonNamingPolicy"/> used to convert property names to SQL table names and/or column names (<see langword="null"/> for no conversion).
    /// </summary>
    /// <remarks>
    /// For example, the following implementation:
    /// <code>
    /// protected override JsonNamingPolicy TableNamingPolicy { get; } = JsonNamingPolicy.SnakeCaseLower;
    /// </code>
    /// Will build a query like this:
    /// <code>
    /// INSERT INTO roles (id, name, normalized_name, ...) 
    /// VALUES (@Id, @Name, @NormalizedName, ...)
    /// </code>
    /// </remarks>
    protected abstract JsonNamingPolicy? TableNamingPolicy { get; }


    /// <summary>
    /// Gets the SQL table and column names used for the <typeparamref name="TRole"/> table.
    /// </summary>
    /// <remarks>
    /// The default names are generated using the <see cref="TableNamingPolicy"/> for <see cref="IdentityRole{TKey}"/> properties and a table name "Roles".
    /// </remarks>
    protected virtual RoleTableNames RoleNames { get; }

    /// <summary>
    /// Gets the SQL table and column names used for the <typeparamref name="TRoleClaim"/> table.
    /// </summary>
    /// <remarks>
    /// The default names are generated using the <see cref="TableNamingPolicy"/> for <see cref="IdentityRoleClaim{TKey}"/> properties and a table name "RoleClaims".
    /// </remarks>
    protected virtual RoleClaimTableNames RoleClaimNames { get; }


    /// <summary>
    /// Gets an array of the base <see cref="IdentityRole{TKey}"/> property names used for insert queries.
    /// </summary>
    protected string[] IdentityRoleInsertProperties { get; }

    /// <summary>
    /// Gets an array of the base <see cref="IdentityRole{TKey}"/> property names used for update queries.
    /// </summary>
    protected string[] IdentityRoleUpdateProperties { get; }

    #endregion

    #region Dependencies

    /// <summary>
    /// Gets the <see cref="System.Data.Common.DbDataSource"/> used to create database connections.
    /// </summary>
    protected DbDataSource DbDataSource { get; }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="dbDataSource">The <see cref="System.Data.Common.DbDataSource"/> used to create database connections.</param>
    /// <param name="describer">The <see cref="IdentityErrorDescriber"/> used to describe store errors.</param>
    public DapperRoleStoreBase(DbDataSource dbDataSource, IdentityErrorDescriber? describer) : base(describer ?? new())
    {
        DbDataSource = dbDataSource;

        RoleNames = new(TableNamingPolicy);
        RoleClaimNames = new(TableNamingPolicy);
        RoleClaimNames = new(TableNamingPolicy);

        IdentityRoleUpdateProperties = [
            nameof(IdentityRole<TKey>.Name),
            nameof(IdentityRole<TKey>.NormalizedName),
            nameof(IdentityRole<TKey>.ConcurrencyStamp)
        ];
        IdentityRoleInsertProperties = [nameof(IdentityRole<TKey>.Id), .. IdentityRoleUpdateProperties];
    }

    #endregion

    #region Virtual Methods

    /// <summary>
    /// Generates a new key for a created role (<see langword="default(TKey)"/> to use the database key generation).
    /// </summary>
    /// <remarks>
    /// If the key type is <see cref="string"/> or <see cref="Guid"/>, the default implementation will return a new <see cref="Guid"/> string.
    /// Otherwise, the default implementation returns <see langword="default(TKey)"/>.
    /// </remarks>
    /// <returns>The new key or <see langword="default(TKey)"/> to use the database key generation.</returns>
    protected virtual TKey? GenerateNewKey()
    {
        if (typeof(TKey) == typeof(string) || typeof(TKey) == typeof(Guid))
        {
            return ConvertIdFromString(Guid.NewGuid().ToString());
        }
        return default;
    }

    #endregion

    #region Abstract Methods

    /// <summary>
    /// Should be implemented to get the full list of property names used to insert a new role.
    /// </summary>
    /// <remarks>
    /// The returned property names are expected to be the exact names before conversion. 
    /// <br/>
    /// These names are later converted using the <see cref="TableNamingPolicy"/>.
    /// </remarks>
    /// <param name="identityRoleInsertProperties">The initial set of property names existing in the base <see cref="IdentityRole{TKey}"/>.</param>
    /// <returns>The full list of property names used to insert a new role.</returns>
    protected abstract string[] GetRoleInsertProperties(string[] identityRoleInsertProperties);

    /// <summary>
    /// Should be implemented to get the full list of property names used to update a role.
    /// </summary>
    /// <remarks>
    /// The returned property names are expected to be the exact names before conversion. 
    /// <br/>
    /// These names are later converted using the <see cref="TableNamingPolicy"/>.
    /// </remarks>
    /// <param name="identityRoleUpdateProperties">The initial set of property names existing in the base <see cref="IdentityRole{TKey}"/>.</param>
    /// <returns>The full list of property names used to update a role.</returns>
    protected abstract string[] GetRoleUpdateProperties(string[] identityRoleUpdateProperties);

    #endregion

    #region Queryable Store Implementation

    /// <inheritdoc/>
    public override IQueryable<TRole> Roles => throw new NotImplementedException();

    #endregion

    #region Role Store Implementation

    /// <inheritdoc/>
    public override async Task<TRole?> FindByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var convertedId = ConvertIdFromString(id);
        if (convertedId is null)
        {
            return null;
        }
        using var connection = DbDataSource.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<TRole>(
            $"""
            SELECT * FROM {RoleNames.Table} WHERE {RoleNames.Id} = @roleId LIMIT 1
            """, 
            new { roleId = convertedId });
    }

    /// <inheritdoc/>
    public override async Task<TRole?> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default)
    {
        using var connection = DbDataSource.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<TRole>(
            $"""
            SELECT * FROM {RoleNames.Table} WHERE {RoleNames.NormalizedName} = @normalizedName LIMIT 1
            """,
            new { normalizedName });
    }

    /// <inheritdoc/>
    public override async Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var result = await connection.QueryAsync<TRoleClaim>(
            $"""
            SELECT * FROM {RoleClaimNames.Table} WHERE {RoleClaimNames.RoleId} = @roleId
            """,
            new { roleId = role.Id });

        return result.Select(roleClaim => roleClaim.ToClaim()).ToList();
    }

    /// <inheritdoc/>
    public override async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        if (role.Id.Equals(default))
        {
            role.Id = GenerateNewKey() ?? role.Id;
        }
        var propertyNames = GetRoleInsertProperties(IdentityRoleInsertProperties);

        var insertCount = await connection.ExecuteAsync(
            $"""
            INSERT INTO {RoleNames.Table} {propertyNames.BuildSqlColumnsBlock(TableNamingPolicy, insertLines: true)}
            VALUES {propertyNames.BuildSqlParametersBlock(insertLines: true)};
            """,
            role);

        return insertCount > 0
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError { Description = $"Could not insert role {role.Name}." });
    }

    /// <inheritdoc/>
    public override async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var propertyNames = GetRoleUpdateProperties(IdentityRoleUpdateProperties);

        var updateCount = await connection.ExecuteAsync(
            $"""
            UPDATE {RoleNames.Table} 
            SET {propertyNames.BuildSqlColumnsBlock(TableNamingPolicy, insertLines: true)}
            = {propertyNames.BuildSqlParametersBlock(insertLines: true)}
            WHERE {RoleNames.Id} = @{nameof(role.Id)};
            """,
            role);

        return updateCount > 0
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError { Description = $"Could not update role {role.Name}." });
    }

    /// <inheritdoc/>
    public override async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            $"""
            DELETE FROM {RoleNames.Table} WHERE {RoleNames.Id} = @roleId
            """,
            new { roleId = role.Id });

        return IdentityResult.Success;
    }

    #endregion

    #region Claim Store Implementation

    /// <inheritdoc/>
    public override async Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var roleClaim = CreateRoleClaim(role, claim);

        string[] propertyNames = [
            nameof(roleClaim.RoleId),
            nameof(roleClaim.ClaimType),
            nameof(roleClaim.ClaimValue)
        ];

        await connection.ExecuteAsync(
            $"""
            INSERT INTO {RoleClaimNames.Table} 
            {propertyNames.BuildSqlColumnsBlock(TableNamingPolicy)}
            VALUES {propertyNames.BuildSqlParametersBlock()}
            """,
            propertyNames);
    }

    /// <inheritdoc/>
    public override async Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var roleClaim = CreateRoleClaim(role, claim);

        string[] propertyNames = [
            nameof(roleClaim.RoleId),
            nameof(roleClaim.ClaimType),
            nameof(roleClaim.ClaimValue)
        ];

        await connection.ExecuteAsync(
            $"""
            DELETE FROM {RoleClaimNames.Table}
            WHERE {propertyNames.BuildSqlColumnsBlock(TableNamingPolicy)} = {propertyNames.BuildSqlParametersBlock()}
            """,
            propertyNames);
    }

    #endregion

}
