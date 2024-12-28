using System.Data.Common;
using System.Security.Claims;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Hope.Identity.Dapper;

/// <summary>
/// Provides an implementation for a Dapper-based Identity role store.
/// </summary>
/// <remarks>
/// This class implements most functionality required by <see cref="RoleStoreBase{TRole, TKey, TUserRole, TRoleClaim}"/>
/// </remarks>
/// <inheritdoc/>
public class DapperRoleStore<TRole, TKey, TUserRole, TRoleClaim>
    : RoleStoreBase<TRole, TKey, TUserRole, TRoleClaim>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
    where TUserRole : IdentityUserRole<TKey>, new()
    where TRoleClaim : IdentityRoleClaim<TKey>, new()
{

    #region Protected Properties

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
    /// Gets the options used to configure the store.
    /// </summary>
    protected virtual DapperStoreOptions Options { get; }


    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="dbDataSource">The <see cref="System.Data.Common.DbDataSource"/> used to create database connections.</param>
    /// <param name="options">The options used to configure the store.</param>
    /// <param name="describer">The <see cref="IdentityErrorDescriber"/> used to describe store errors.</param>
    public DapperRoleStore(DbDataSource dbDataSource, IOptions<DapperStoreOptions> options, IdentityErrorDescriber? describer) 
        : base(describer ?? new())
    {
        DbDataSource = dbDataSource;
        Options = options?.Value ?? new();

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

    /// <summary>
    /// Gets the full list of property names used to insert a new role.
    /// </summary>
    /// <remarks>
    /// The returned property names are expected to be the exact names before conversion, that are later converted using the <see cref="DapperStoreOptions.TableNamingPolicy"/>.
    /// <br/>
    /// Defaults to the sum of <see cref="IdentityRoleInsertProperties"/> and <see cref="DapperStoreOptions.ExtraRoleInsertProperties"/>
    /// </remarks>
    /// <returns>The full list of property names used to insert a new role.</returns>
    protected virtual string[] GetRoleInsertProperties()
    {
        return [.. IdentityRoleInsertProperties, .. Options.ExtraRoleInsertProperties];
    }

    /// <summary>
    /// Gets the full list of property names used to update a role.
    /// </summary>
    /// <remarks>
    /// The returned property names are expected to be the exact names before conversion, that are later converted using the <see cref="DapperStoreOptions.TableNamingPolicy"/>.
    /// <br/>
    /// Defaults to the sum of <see cref="IdentityRoleUpdateProperties"/> and <see cref="DapperStoreOptions.ExtraRoleUpdateProperties"/>
    /// </remarks>
    /// <returns>The full list of property names used to update a role.</returns>
    protected virtual string[] GetRoleUpdateProperties()
    {
        return [.. IdentityRoleUpdateProperties, .. Options.ExtraRoleUpdateProperties];
    }

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
            SELECT * FROM {Options.RoleNames.Table} WHERE {Options.RoleNames.Id} = @roleId LIMIT 1
            """, 
            new { roleId = convertedId });
    }

    /// <inheritdoc/>
    public override async Task<TRole?> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default)
    {
        using var connection = DbDataSource.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<TRole>(
            $"""
            SELECT * FROM {Options.RoleNames.Table} WHERE {Options.RoleNames.NormalizedName} = @normalizedName LIMIT 1
            """,
            new { normalizedName });
    }

    /// <inheritdoc/>
    public override async Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var result = await connection.QueryAsync<TRoleClaim>(
            $"""
            SELECT * FROM {Options.RoleClaimNames.Table} WHERE {Options.RoleClaimNames.RoleId} = @roleId
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
        var propertyNames = GetRoleInsertProperties();

        var insertCount = await connection.ExecuteAsync(
            $"""
            INSERT INTO {Options.RoleNames.Table} {propertyNames.BuildSqlColumnsBlock(Options.TableNamingPolicy, insertLines: true)}
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

        var propertyNames = GetRoleUpdateProperties();

        var updateCount = await connection.ExecuteAsync(
            $"""
            UPDATE {Options.RoleNames.Table} 
            SET {propertyNames.BuildSqlColumnsBlock(Options.TableNamingPolicy, insertLines: true)}
            = {propertyNames.BuildSqlParametersBlock(insertLines: true)}
            WHERE {Options.RoleNames.Id} = @{nameof(role.Id)};
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
            DELETE FROM {Options.RoleNames.Table} WHERE {Options.RoleNames.Id} = @roleId
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
            INSERT INTO {Options.RoleClaimNames.Table} 
            {propertyNames.BuildSqlColumnsBlock(Options.TableNamingPolicy)}
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
            DELETE FROM {Options.RoleClaimNames.Table}
            WHERE {propertyNames.BuildSqlColumnsBlock(Options.TableNamingPolicy)} = {propertyNames.BuildSqlParametersBlock()}
            """,
            propertyNames);
    }

    #endregion

}
