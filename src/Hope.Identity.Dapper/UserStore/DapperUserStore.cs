using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Hope.Identity.Dapper;

/// <summary>
/// Provides an implementation for a Dapper-based Identity user store.
/// </summary>
/// <remarks>
/// This class implements most functionality required by <see cref="UserStoreBase{TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim}"/>
/// </remarks>
/// <inheritdoc/>
public class DapperUserStore<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim> 
    : UserStoreBase<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>, new()
    where TUserRole : IdentityUserRole<TKey>, new()
    where TUserLogin : IdentityUserLogin<TKey>, new()
    where TUserToken : IdentityUserToken<TKey>, new()
    where TRoleClaim : IdentityRoleClaim<TKey>, new()
{

    #region Private Properties

    [field: MaybeNull]
    private string TablePrefix => field ??= Options.TableSchema is null ? string.Empty : $"{Options.TableSchema}.";

    #endregion

    #region Protected Properties

    /// <summary>
    /// Gets a dictionary of base properties used for insert queries, 
    /// where keys are the base <see cref="IdentityUser{TKey}"/> property names and values are the corresponding column names.
    /// </summary>
    protected virtual Dictionary<string, string> IdentityUserInsertProperties { get; }

    /// <summary>
    /// Gets a dictionary of base properties used for update queries, 
    /// where keys are the base <see cref="IdentityUser{TKey}"/> property names and values are the corresponding column names.
    /// </summary>
    protected virtual Dictionary<string, string> IdentityUserUpdateProperties { get; }

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
    public DapperUserStore(DbDataSource dbDataSource, IOptions<DapperStoreOptions> options, IdentityErrorDescriber? describer = null) 
        : base(describer ?? new())
    {
        DbDataSource = dbDataSource;
        Options = options?.Value ?? new();

        IdentityUserUpdateProperties = new() {
            { nameof(IdentityUser<TKey>.UserName), Options.UserNames.UserName },
            { nameof(IdentityUser<TKey>.NormalizedUserName), Options.UserNames.NormalizedUserName },
            { nameof(IdentityUser<TKey>.Email), Options.UserNames.Email },
            { nameof(IdentityUser<TKey>.NormalizedEmail), Options.UserNames.NormalizedEmail },
            { nameof(IdentityUser<TKey>.EmailConfirmed), Options.UserNames.EmailConfirmed },
            { nameof(IdentityUser<TKey>.PasswordHash), Options.UserNames.PasswordHash },
            { nameof(IdentityUser<TKey>.SecurityStamp), Options.UserNames.SecurityStamp },
            { nameof(IdentityUser<TKey>.ConcurrencyStamp), Options.UserNames.ConcurrencyStamp },
            { nameof(IdentityUser<TKey>.PhoneNumber), Options.UserNames.PhoneNumber },
            { nameof(IdentityUser<TKey>.PhoneNumberConfirmed), Options.UserNames.PhoneNumberConfirmed },
            { nameof(IdentityUser<TKey>.TwoFactorEnabled), Options.UserNames.TwoFactorEnabled },
            { nameof(IdentityUser<TKey>.LockoutEnd), Options.UserNames.LockoutEnd },
            { nameof(IdentityUser<TKey>.LockoutEnabled), Options.UserNames.LockoutEnabled },
            { nameof(IdentityUser<TKey>.AccessFailedCount), Options.UserNames.AccessFailedCount }
        };
        IdentityUserInsertProperties = new(IdentityUserUpdateProperties) {
            { nameof(IdentityUser<TKey>.Id), Options.UserNames.Id }
        };
    }

    #endregion

    #region Virtual Methods

    /// <summary>
    /// Generates a new key for a created user (<see langword="default(TKey)"/> to use the database key generation).
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
    /// Gets a dictionary of the full set of properties used to insert a new user, 
    /// where keys represent the user type property names and values are the corresponding column names.
    /// </summary>
    /// <remarks>
    /// <para>Defaults to the sum of <see cref="IdentityUserInsertProperties"/> and <see cref="DapperStoreOptions.ExtraUserInsertProperties"/>.</para>
    /// <para>Properties with <c>null</c> values in <see cref="DapperStoreOptions.ExtraUserInsertProperties"/> should be converted using <see cref="DapperStoreOptions.TableNamingPolicy"/>.</para>
    /// </remarks>
    /// <returns>A dictionary of the full set of properties used to insert a new user.</returns>
    protected virtual Dictionary<string, string> GetUserInsertProperties()
    {
        return Options.ExtraUserInsertProperties
            .Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value ?? Options.TableNamingPolicy.TryConvertName(kvp.Key)))
            .Concat(IdentityUserInsertProperties)
            .DistinctBy(kvp => kvp.Key)
            .ToDictionary();
    }

    /// <summary>
    /// Gets a dictionary of the full set of properties used to update a user, 
    /// where keys represent the user type property names and values are the corresponding column names.
    /// </summary>
    /// <remarks>
    /// <para>Defaults to the sum of <see cref="IdentityUserUpdateProperties"/> and <see cref="DapperStoreOptions.ExtraUserUpdateProperties"/>.</para>
    /// <para>Properties with <c>null</c> values in <see cref="DapperStoreOptions.ExtraRoleUpdateProperties"/> should be converted using <see cref="DapperStoreOptions.TableNamingPolicy"/>.</para>
    /// </remarks>
    /// <returns>A dictionary of the full set of properties used to update a user.</returns>
    protected virtual Dictionary<string, string> GetUserUpdateProperties()
    {
        return Options.ExtraUserUpdateProperties
            .Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value ?? Options.TableNamingPolicy.TryConvertName(kvp.Key)))
            .Concat(IdentityUserUpdateProperties)
            .DistinctBy(kvp => kvp.Key)
            .ToDictionary();
    }

    /// <summary>
    /// Gets the base SQL condition used for all user queries. When not overridden returns "TRUE" (no base condition).
    /// </summary>
    /// <remarks>
    /// This base condition is helpful to limit the set of users affected by the current implementation of the store, for example:
    /// <code>
    /// protected override string GetBaseUserSqlCondition(DynamicParameters sqlParameters, string tableAlias = "")
    /// {
    ///     sqlParameters.Add("tenantId", _currentTenantId);
    /// 
    ///     var columnPrefix = string.IsNullOrEmpty(tableAlias) ? string.Empty : $"{tableAlias}.";
    ///     return $"{columnPrefix}{nameof(User.TenantId).ToSqlColumn(Options.TableNamingPolicy)} = @tenantId";
    /// }
    /// </code>
    /// Notice that the <paramref name="sqlParameters"/> argument should be used to add any parameters included in the returned condition.
    /// </remarks>
    /// <param name="sqlParameters">The <see cref="DynamicParameters"/> used to add SQL parameters.</param>
    /// <param name="tableAlias">The alias to use for the user table.</param>
    /// <returns>The base SQL condition used for all user queries.</returns>
    protected virtual string GetBaseUserSqlCondition(DynamicParameters sqlParameters, string tableAlias = "") => "TRUE";

    #endregion

    #region Queryable Store Implementation

    /// <inheritdoc/>
    public override IQueryable<TUser> Users => throw new NotImplementedException();

    #endregion

    #region User Stores Implementation

    /// <inheritdoc/>
    protected override async Task<TUser?> FindUserAsync(TKey userId, CancellationToken cancellationToken)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var dynamicParams = new DynamicParameters(new { userId });

        return await connection.QuerySingleOrDefaultAsync<TUser>(
            $"""
            SELECT * FROM {TablePrefix}{Options.UserNames.Table} 
            WHERE {GetBaseUserSqlCondition(dynamicParams)} 
            AND {Options.UserNames.Id} = @userId 
            LIMIT 1
            """,
            dynamicParams);
    }

    /// <inheritdoc/>
    public override Task<TUser?> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var convertedId = ConvertIdFromString(userId);
        if (convertedId is null)
        {
            return Task.FromResult<TUser?>(null);
        }
        return FindUserAsync(convertedId, cancellationToken);
    }

    /// <inheritdoc/>
    public override async Task<TUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var dynamicParams = new DynamicParameters(new { normalizedUserName });

        return await connection.QuerySingleOrDefaultAsync<TUser>(
            $"""
            SELECT * FROM {TablePrefix}{Options.UserNames.Table} 
            WHERE {GetBaseUserSqlCondition(dynamicParams)} 
            AND {Options.UserNames.NormalizedUserName} = @normalizedUserName 
            LIMIT 1
            """,
            dynamicParams);
    }

    /// <inheritdoc/>
    public override async Task<TUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var dynamicParams = new DynamicParameters(new { normalizedEmail });

        return await connection.QuerySingleOrDefaultAsync<TUser>(
            $"""
            SELECT * FROM {TablePrefix}{Options.UserNames.Table} 
            WHERE {GetBaseUserSqlCondition(dynamicParams)} 
            AND {Options.UserNames.NormalizedEmail} = @normalizedEmail 
            LIMIT 1
            """,
            dynamicParams);
    }

    /// <inheritdoc/>
    public override async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        if (user.Id.Equals(default))
        {
            user.Id = GenerateNewKey() ?? user.Id; 
        }
        var propertyDictionary = GetUserInsertProperties();

        var insertCount = await connection.ExecuteAsync(
            $"""
            INSERT INTO {TablePrefix}{Options.UserNames.Table} {propertyDictionary.Values.BuildSqlColumnsBlock(insertLines: true)}
            VALUES {propertyDictionary.Keys.BuildSqlParametersBlock(insertLines: true)};
            """,
            user);

        return insertCount > 0
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError { Description = $"Could not insert user {user.UserName}." });
    }

    /// <inheritdoc/>
    public override async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var propertyDictionary = GetUserUpdateProperties();
        var dynamicParams = new DynamicParameters(user);

        var updateCount = await connection.ExecuteAsync(
            $"""
            UPDATE {TablePrefix}{Options.UserNames.Table} 
            SET {propertyDictionary.Values.BuildSqlColumnsBlock(insertLines: true)}
            = {propertyDictionary.Keys.BuildSqlParametersBlock(insertLines: true)}
            WHERE {GetBaseUserSqlCondition(dynamicParams)} 
            AND {Options.UserNames.Id} = @{nameof(user.Id)};
            """,
            dynamicParams);

        return updateCount > 0
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError { Description = $"Could not update user {user.UserName}." });
    }

    /// <inheritdoc/>
    public override async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var dynamicParams = new DynamicParameters(new { userId = user.Id });

        var deleteCount = await connection.ExecuteAsync(
            $"""
            DELETE FROM {TablePrefix}{Options.UserNames.Table}
            WHERE {GetBaseUserSqlCondition(dynamicParams)} 
            AND {Options.UserNames.Id} = @userId
            """,
            dynamicParams);

        return deleteCount > 0
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError { Description = $"User '{user.UserName}' not found." });
    }

    #endregion

    #region Claim Stores Implementation

    /// <inheritdoc/>
    public override async Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var result = await connection.QueryAsync<TUserClaim>(
            $"""
            SELECT * FROM {TablePrefix}{Options.UserClaimNames.Table} WHERE {Options.UserClaimNames.UserId} = @userId
            """,
            new { userId = user.Id });

        return result.Select(userClaim => userClaim.ToClaim()).ToList();
    }

    /// <inheritdoc/>
    public override async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var dynamicParams = new DynamicParameters(new { claimType = claim.Type, claimValue = claim.Value });

        var result = await connection.QueryAsync<TUser>(
            $"""
            SELECT u.* FROM {TablePrefix}{Options.UserNames.Table} u
            JOIN {TablePrefix}{Options.UserClaimNames.Table} uc 
            ON u.{Options.UserNames.Id} = uc.{Options.UserClaimNames.UserId}
            WHERE {GetBaseUserSqlCondition(dynamicParams, tableAlias: "u")}
            AND uc.{Options.UserClaimNames.ClaimType} = @claimType 
            AND uc.{Options.UserClaimNames.ClaimValue} = @claimValue
            """,
            dynamicParams);

        return result.ToList();
    }

    /// <inheritdoc/>
    public override async Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
    {
        var claimsArray = claims.ToArray();
        if (claimsArray.Length == 0)
        {
            return;
        }
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var sqlBuilder = new StringBuilder();

        sqlBuilder.Append($"INSERT INTO {TablePrefix}{Options.UserClaimNames.Table} ");
        sqlBuilder.AppendLine($"({Options.UserClaimNames.UserId}, {Options.UserClaimNames.ClaimType}, {Options.UserClaimNames.ClaimValue}) VALUES");

        var dynamicParams = new DynamicParameters();
        dynamicParams.Add("userId", user.Id);

        for (int i = 0; i < claimsArray.Length; i++)
        {
            if (i != 0)
            {
                sqlBuilder.AppendLine(",");
            }
            var claimTypeParam = $"claim{i}_type";
            var claimValueParam = $"claim{i}_value";

            sqlBuilder.Append($"(@userId, @{claimTypeParam}, @{claimValueParam})");

            dynamicParams.Add(claimTypeParam, claimsArray[i].Type);
            dynamicParams.Add(claimValueParam, claimsArray[i].Value);
        }

        await connection.ExecuteAsync(sqlBuilder.ToString(), dynamicParams);
    }

    /// <inheritdoc/>
    public override async Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            $"""
            UPDATE {TablePrefix}{Options.UserClaimNames.Table} 
            SET ({Options.UserClaimNames.ClaimType}, {Options.UserClaimNames.ClaimValue}) = (@newClaimType, @newClaimValue)
            WHERE ({Options.UserClaimNames.UserId}, {Options.UserClaimNames.ClaimType}, {Options.UserClaimNames.ClaimValue}) 
            = (@userId, @claimType, @claimValue)
            """,
            new
            {
                userId = user.Id,
                claimType = claim.Type,
                claimValue = claim.Value,
                newClaimType = newClaim.Type,
                newClaimValue = newClaim.Value
            });
    }

    /// <inheritdoc/>
    public override async Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
    {
        var claimsArray = claims.ToArray();
        if (claimsArray.Length == 0)
        {
            return;
        }
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var sqlBuilder = new StringBuilder();

        sqlBuilder.AppendLine($"DELETE FROM {TablePrefix}{Options.UserClaimNames.Table} WHERE {Options.UserClaimNames.UserId} = @userId");
        sqlBuilder.Append($"AND ({Options.UserClaimNames.ClaimType}, {Options.UserClaimNames.ClaimValue}) IN (");

        var dynamicParams = new DynamicParameters();
        dynamicParams.Add("userId", user.Id);

        for (int i = 0; i < claimsArray.Length; i++)
        {
            if (i != 0)
            {
                sqlBuilder.Append(", ");
            }
            var claimTypeParam = $"claimType{i}";
            var claimValueParam = $"claimValue{i}";

            sqlBuilder.Append($"(@{claimTypeParam}, @{claimValueParam})");

            dynamicParams.Add(claimTypeParam, claimsArray[i].Type);
            dynamicParams.Add(claimValueParam, claimsArray[i].Value);
        }
        sqlBuilder.Append(')');

        await connection.ExecuteAsync(sqlBuilder.ToString(), dynamicParams);
    }

    #endregion

    #region Login Stores Implementation

    /// <inheritdoc/>
    protected override async Task<TUserLogin?> FindUserLoginAsync(TKey userId, string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<TUserLogin>(
            $"""
            SELECT * FROM {TablePrefix}{Options.UserLoginNames.Table} 
            WHERE ({Options.UserLoginNames.UserId}, {Options.UserLoginNames.LoginProvider}, {Options.UserLoginNames.ProviderKey}) 
            = (@userId, @loginProvider, @providerKey)
            LIMIT 1
            """,
            new { userId, loginProvider, providerKey });
    }

    /// <inheritdoc/>
    protected override async Task<TUserLogin?> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<TUserLogin>(
            $"""
            SELECT * FROM {TablePrefix}{Options.UserLoginNames.Table} 
            WHERE {Options.UserLoginNames.LoginProvider} = @loginProvider AND {Options.UserLoginNames.ProviderKey} = @providerKey 
            LIMIT 1
            """,
            new { loginProvider, providerKey });
    }

    /// <inheritdoc/>
    public override async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var result = await connection.QueryAsync<TUserLogin>(
            $"""
            SELECT * FROM {TablePrefix}{Options.UserLoginNames.Table} WHERE {Options.UserLoginNames.UserId} = @userId
            """,
            new { userId = user.Id });

        return result
            .Select(login => new UserLoginInfo(login.LoginProvider, login.ProviderKey, login.ProviderDisplayName))
            .ToList();
    }

    /// <inheritdoc/>
    public override async Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var userLogin = CreateUserLogin(user, login);

        var propertyDictionary = new Dictionary<string, string> {
            { nameof(userLogin.UserId), Options.UserLoginNames.UserId },
            { nameof(userLogin.LoginProvider), Options.UserLoginNames.LoginProvider },
            { nameof(userLogin.ProviderKey), Options.UserLoginNames.ProviderKey },
            { nameof(userLogin.ProviderDisplayName), Options.UserLoginNames.ProviderDisplayName }
        };

        await connection.ExecuteAsync(
            $"""
            INSERT INTO {TablePrefix}{Options.UserLoginNames.Table} 
            {propertyDictionary.Values.BuildSqlColumnsBlock()} 
            VALUES {propertyDictionary.Keys.BuildSqlParametersBlock()}
            """,
            userLogin);
    }

    /// <inheritdoc/>
    public override async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            $"""
            DELETE FROM {TablePrefix}{Options.UserLoginNames.Table} 
            WHERE ({Options.UserLoginNames.UserId}, {Options.UserLoginNames.LoginProvider}, {Options.UserLoginNames.ProviderKey}) 
            = (@userId, @loginProvider, @providerKey)
            """,
            new
            {
                userId = user.Id,
                loginProvider,
                providerKey
            });
    }

    #endregion

    #region Token Stores Implementation

    /// <inheritdoc/>
    protected override async Task<TUserToken?> FindTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<TUserToken>(
            $"""
            SELECT * FROM {TablePrefix}{Options.UserTokenNames.Table} 
            WHERE ({Options.UserLoginNames.UserId}, {Options.UserLoginNames.LoginProvider}, {Options.UserLoginNames.ProviderKey}) 
            = (@userId, @loginProvider, @name)
            LIMIT 1
            """,
            new { userId = user.Id, loginProvider, name });
    }

    /// <inheritdoc/>
    protected override async Task AddUserTokenAsync(TUserToken token)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync();

        var propertyDictionary = new Dictionary<string, string> {
            { nameof(token.UserId), Options.UserTokenNames.UserId },
            { nameof(token.LoginProvider), Options.UserTokenNames.LoginProvider },
            { nameof(token.Name), Options.UserTokenNames.Name },
            { nameof(token.Value), Options.UserTokenNames.Value }
        };

        await connection.ExecuteAsync(
            $"""
            INSERT INTO {TablePrefix}{Options.UserTokenNames.Table} 
            {propertyDictionary.Values.BuildSqlColumnsBlock()}
            VALUES {propertyDictionary.Keys.BuildSqlParametersBlock()}
            """,
            token);
    }

    /// <inheritdoc/>
    protected override async Task RemoveUserTokenAsync(TUserToken token)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync();
        
        var propertyDictionary = new Dictionary<string, string> {
            { nameof(token.UserId), Options.UserTokenNames.UserId },
            { nameof(token.LoginProvider), Options.UserTokenNames.LoginProvider },
            { nameof(token.Name), Options.UserTokenNames.Name }
        };

        await connection.ExecuteAsync(
            $"""
            DELETE FROM {TablePrefix}{Options.UserTokenNames.Table} 
            WHERE {propertyDictionary.Values.BuildSqlColumnsBlock()} 
            = {propertyDictionary.Keys.BuildSqlParametersBlock()}
            """,
            token);
    }

    #endregion

    #region Role Stores Implementation

    /// <inheritdoc/>
    protected override async Task<TRole?> FindRoleAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<TRole>(
            $"""
            SELECT * FROM {TablePrefix}{Options.RoleNames.Table} 
            WHERE {Options.RoleNames.NormalizedName} = @normalizedRoleName 
            LIMIT 1
            """,
            new { normalizedRoleName });
    }

    /// <inheritdoc/>
    protected override async Task<TUserRole?> FindUserRoleAsync(TKey userId, TKey roleId, CancellationToken cancellationToken)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<TUserRole>(
            $"""
            SELECT * FROM {TablePrefix}{Options.UserRoleNames.Table} 
            WHERE ({Options.UserRoleNames.UserId}, {Options.UserRoleNames.RoleId}) = (@userId, @roleId)
            LIMIT 1
            """,
            new { userId, roleId });
    }

    /// <inheritdoc/>
    public override async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var result = await connection.QueryAsync<string>(
            $"""
            SELECT r.{Options.RoleNames.Name} FROM {TablePrefix}{Options.RoleNames.Table} r
            JOIN {TablePrefix}{Options.UserRoleNames.Table} ur 
            ON r.{Options.RoleNames.Id} = ur.{Options.UserRoleNames.RoleId}
            WHERE ur.{Options.UserRoleNames.UserId} = @userId
            """,
            new { userId = user.Id });

        return result.ToList();
    }

    /// <inheritdoc/>
    public override async Task<IList<TUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var result = await connection.QueryAsync<TUser>(
            $"""
            SELECT u.* FROM {TablePrefix}{Options.UserNames.Table} u
            JOIN {TablePrefix}{Options.UserRoleNames.Table} ur ON u.{Options.UserNames.Id} = ur.{Options.UserRoleNames.UserId}
            JOIN {TablePrefix}{Options.RoleNames.Table} r ON ur.{Options.UserRoleNames.RoleId} = r.{Options.RoleNames.Id}
            WHERE r.{Options.RoleNames.NormalizedName} = @normalizedRoleName
            """,
            new { normalizedRoleName });

        return result.ToList();
    }

    /// <inheritdoc/>
    public override async Task<bool> IsInRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(
            $"""
            SELECT EXISTS (
                SELECT 1 FROM {TablePrefix}{Options.UserRoleNames.Table} ur
                JOIN {TablePrefix}{Options.RoleNames.Table} r 
                ON ur.{Options.UserRoleNames.RoleId} = r.{Options.RoleNames.Id}
                WHERE ur.{Options.UserRoleNames.UserId} = @userId AND r.{Options.RoleNames.NormalizedName} = @normalizedRoleName
            )
            """,
            new { userId = user.Id, normalizedRoleName });
    }

    /// <inheritdoc/>
    public override async Task AddToRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var role = await FindRoleAsync(normalizedRoleName, cancellationToken) 
            ?? throw new InvalidOperationException($"Role {normalizedRoleName} not found.");

        await connection.ExecuteAsync(
            $"""
            INSERT INTO {TablePrefix}{Options.UserRoleNames.Table} 
            ({Options.UserRoleNames.UserId}, {Options.UserRoleNames.RoleId}) 
            VALUES (@userId, @roleId)
            """,
            new { userId = user.Id, roleId = role.Id });
    }

    /// <inheritdoc/>
    public override async Task RemoveFromRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var role = await FindRoleAsync(normalizedRoleName, cancellationToken)
            ?? throw new InvalidOperationException($"Role {normalizedRoleName} not found.");

        await connection.ExecuteAsync(
            $"""
            DELETE FROM {TablePrefix}{Options.UserRoleNames.Table} 
            WHERE ({Options.UserRoleNames.UserId}, {Options.UserRoleNames.RoleId}) = (@userId, @roleId)
            """,
            new { userId = user.Id, roleId = role.Id });
    }

    #endregion

}
