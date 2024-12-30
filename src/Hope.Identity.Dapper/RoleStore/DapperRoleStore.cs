using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
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

    #region Private Properties

    [field: MaybeNull]
    private string TablePrefix => field ??= Options.TableSchema is null ? string.Empty : $"{Options.TableSchema}.";

    #endregion

    #region Protected Properties

    /// <summary>
    /// Gets a dictionary of base properties used for insert queries, 
    /// where keys are the base <see cref="IdentityRole{TKey}"/> property names and values are the corresponding column names.
    /// </summary>
    protected virtual Dictionary<string, string> IdentityRoleInsertProperties { get; }

    /// <summary>
    /// Gets a dictionary of base properties used for update queries, 
    /// where keys are the base <see cref="IdentityRole{TKey}"/> property names and values are the corresponding column names.
    /// </summary>
    protected virtual Dictionary<string, string> IdentityRoleUpdateProperties { get; }

    #endregion

    #region Dependencies

    /// <summary>
    /// Gets the <see cref="System.Data.Common.DbDataSource"/> used to create database connections.
    /// </summary>
    protected virtual DbDataSource DbDataSource { get; }

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
    public DapperRoleStore(DbDataSource dbDataSource, IOptions<DapperStoreOptions> options, IdentityErrorDescriber? describer = null)
        : base(describer ?? new())
    {
        DbDataSource = dbDataSource;
        Options = options?.Value ?? new();

        IdentityRoleUpdateProperties = new() {
            { nameof(IdentityRole<TKey>.Name) , Options.RoleNames.Name },
            { nameof(IdentityRole<TKey>.NormalizedName), Options.RoleNames.NormalizedName },
            { nameof(IdentityRole<TKey>.ConcurrencyStamp), Options.RoleNames.ConcurrencyStamp }
        };
        IdentityRoleInsertProperties = new(IdentityRoleUpdateProperties) {
            { nameof(IdentityRole<TKey>.Id), Options.RoleNames.Id }
        };
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
    /// Gets a dictionary of the full set of properties used to insert a new role, 
    /// where keys represent the role type property names and values are the corresponding column names.
    /// </summary>
    /// <remarks>
    /// <para>Defaults to the sum of <see cref="IdentityRoleInsertProperties"/> and <see cref="DapperStoreOptions.ExtraRoleInsertProperties"/>.</para>
    /// <para>Properties with <c>null</c> values in <see cref="DapperStoreOptions.ExtraRoleInsertProperties"/> should be converted using <see cref="DapperStoreOptions.TableNamingPolicy"/>.</para>
    /// </remarks>
    /// <returns>A dictionary of the full set of properties used to insert a new role.</returns>
    protected virtual Dictionary<string, string> GetRoleInsertProperties()
    {
        return Options.ExtraRoleInsertProperties
            .Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value ?? Options.TableNamingPolicy.TryConvertName(kvp.Key)))
            .Concat(IdentityRoleInsertProperties)
            .DistinctBy(kvp => kvp.Key)
            .ToDictionary();
    }

    /// <summary>
    /// Gets a dictionary of the full set of properties used to update a role, 
    /// where keys represent the role type property names and values are the corresponding column names.
    /// </summary>
    /// <remarks>
    /// <para>Defaults to the sum of <see cref="IdentityRoleUpdateProperties"/> and <see cref="DapperStoreOptions.ExtraRoleUpdateProperties"/>.</para>
    /// <para>Properties with <c>null</c> values in <see cref="DapperStoreOptions.ExtraRoleUpdateProperties"/> should be converted using <see cref="DapperStoreOptions.TableNamingPolicy"/>.</para>
    /// </remarks>
    /// <returns>A dictionary of the full set of properties used to update a role.</returns>
    protected virtual Dictionary<string, string> GetRoleUpdateProperties()
    {
        return Options.ExtraRoleUpdateProperties
            .Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value ?? Options.TableNamingPolicy.TryConvertName(kvp.Key)))
            .Concat(IdentityRoleUpdateProperties)
            .DistinctBy(kvp => kvp.Key)
            .ToDictionary();
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
        await using var connection = DbDataSource.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<TRole>(
            $"""
            SELECT * FROM {TablePrefix}{Options.RoleNames.Table} WHERE {Options.RoleNames.Id} = @roleId LIMIT 1
            """, 
            new { roleId = convertedId });
    }

    /// <inheritdoc/>
    public override async Task<TRole?> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default)
    {
        await using var connection = DbDataSource.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<TRole>(
            $"""
            SELECT * FROM {TablePrefix}{Options.RoleNames.Table} WHERE {Options.RoleNames.NormalizedName} = @normalizedName LIMIT 1
            """,
            new { normalizedName });
    }

    /// <inheritdoc/>
    public override async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default)
    {
        if (role.Id.Equals(default))
        {
            role.Id = GenerateNewKey() ?? role.Id;
        }
        if (role.Name is null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "The role Name is required." });
        }
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var propertyDictionary = GetRoleInsertProperties();

        var insertCount = await connection.ExecuteAsync(
            $"""
            INSERT INTO {TablePrefix}{Options.RoleNames.Table} {propertyDictionary.Values.BuildSqlColumnsBlock(insertLines: true)}
            VALUES {propertyDictionary.Keys.BuildSqlParametersBlock(insertLines: true)};
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

        var propertyDictionary = GetRoleUpdateProperties();

        var updateCount = await connection.ExecuteAsync(
            $"""
            UPDATE {TablePrefix}{Options.RoleNames.Table} 
            SET {propertyDictionary.Values.BuildSqlColumnsBlock(insertLines: true)}
            = {propertyDictionary.Keys.BuildSqlParametersBlock(insertLines: true)}
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

        var deleteCount = await connection.ExecuteAsync(
            $"""
            DELETE FROM {TablePrefix}{Options.RoleNames.Table} WHERE {Options.RoleNames.Id} = @roleId
            """,
            new { roleId = role.Id });

        return deleteCount > 0
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError { Description = $"Role '{role.Name}' not found." });
    }

    #endregion

    #region Claim Store Implementation

    /// <inheritdoc/>
    public override async Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var result = await connection.QueryAsync<TRoleClaim>(
            $"""
            SELECT * FROM {TablePrefix}{Options.RoleClaimNames.Table} WHERE {Options.RoleClaimNames.RoleId} = @roleId
            """,
            new { roleId = role.Id });

        return result.Select(roleClaim => roleClaim.ToClaim()).ToList();
    }

    /// <inheritdoc/>
    public override async Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var roleClaim = CreateRoleClaim(role, claim);

        var propertyDictionary = new Dictionary<string, string> {
            { nameof(roleClaim.RoleId), Options.RoleClaimNames.RoleId },
            { nameof(roleClaim.ClaimType), Options.RoleClaimNames.ClaimType },
            { nameof(roleClaim.ClaimValue), Options.RoleClaimNames.ClaimValue }
        };

        await connection.ExecuteAsync(
            $"""
            INSERT INTO {TablePrefix}{Options.RoleClaimNames.Table} {propertyDictionary.Values.BuildSqlColumnsBlock()}
            VALUES {propertyDictionary.Keys.BuildSqlParametersBlock()}
            """,
            roleClaim);
    }

    /// <inheritdoc/>
    public override async Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var roleClaim = CreateRoleClaim(role, claim);

        var propertyDictionary = new Dictionary<string, string> {
            { nameof(roleClaim.RoleId), Options.RoleClaimNames.RoleId },
            { nameof(roleClaim.ClaimType), Options.RoleClaimNames.ClaimType },
            { nameof(roleClaim.ClaimValue), Options.RoleClaimNames.ClaimValue }
        };

        await connection.ExecuteAsync(
            $"""
            DELETE FROM {TablePrefix}{Options.RoleClaimNames.Table}
            WHERE {propertyDictionary.Values.BuildSqlColumnsBlock()} = {propertyDictionary.Keys.BuildSqlParametersBlock()}
            """,
            roleClaim);
    }

    #endregion

}
