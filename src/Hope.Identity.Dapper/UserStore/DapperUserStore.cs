using System.Data.Common;
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

    #region Protected Properties

    /// <summary>
    /// Gets an array of the base <see cref="IdentityUser{TKey}"/> property names used for insert queries.
    /// </summary>
    protected virtual string[] IdentityUserInsertProperties { get; }

    /// <summary>
    /// Gets an array of the base <see cref="IdentityUser{TKey}"/> property names used for update queries.
    /// </summary>
    protected virtual string[] IdentityUserUpdateProperties { get; }

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
    public DapperUserStore(DbDataSource dbDataSource, IOptions<DapperStoreOptions> options, IdentityErrorDescriber? describer) 
        : base(describer ?? new())
    {
        DbDataSource = dbDataSource;
        Options = options?.Value ?? new();

        IdentityUserUpdateProperties = [
            nameof(IdentityUser<TKey>.UserName),
            nameof(IdentityUser<TKey>.NormalizedUserName),
            nameof(IdentityUser<TKey>.Email),
            nameof(IdentityUser<TKey>.NormalizedEmail),
            nameof(IdentityUser<TKey>.EmailConfirmed),
            nameof(IdentityUser<TKey>.PasswordHash),
            nameof(IdentityUser<TKey>.SecurityStamp),
            nameof(IdentityUser<TKey>.ConcurrencyStamp),
            nameof(IdentityUser<TKey>.PhoneNumber),
            nameof(IdentityUser<TKey>.PhoneNumberConfirmed),
            nameof(IdentityUser<TKey>.TwoFactorEnabled),
            nameof(IdentityUser<TKey>.LockoutEnd),
            nameof(IdentityUser<TKey>.LockoutEnabled),
            nameof(IdentityUser<TKey>.AccessFailedCount)
        ];
        IdentityUserInsertProperties = [nameof(IdentityUser<TKey>.Id), .. IdentityUserUpdateProperties];
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
    /// Gets the full list of property names used to insert a new user.
    /// </summary>
    /// <remarks>
    /// The returned property names are expected to be the exact names before conversion, that are later converted using the <see cref="DapperStoreOptions.TableNamingPolicy"/>.
    /// <br/>
    /// Defaults to the sum of <see cref="IdentityUserInsertProperties"/> and <see cref="DapperStoreOptions.ExtraUserInsertProperties"/>
    /// </remarks>
    /// <returns>The full list of property names used to insert a new user.</returns>
    protected virtual string[] GetUserInsertProperties()
    {
        return [.. IdentityUserInsertProperties, .. Options.ExtraUserInsertProperties];
    }

    /// <summary>
    /// Gets the full list of property names used to update a user.
    /// </summary>
    /// <remarks>
    /// The returned property names are expected to be the exact names before conversion, that are later converted using the <see cref="DapperStoreOptions.TableNamingPolicy"/>.
    /// <br/>
    /// Defaults to the sum of <see cref="IdentityUserUpdateProperties"/> and <see cref="DapperStoreOptions.ExtraUserUpdateProperties"/>
    /// </remarks>
    /// <returns>The full list of property names used to update a user.</returns>
    protected virtual string[] GetUserUpdateProperties()
    {
        return [.. IdentityUserUpdateProperties, .. Options.ExtraUserUpdateProperties];
    }

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

        return await connection.QuerySingleOrDefaultAsync<TUser>(
            $"""
            SELECT * FROM {Options.UserNames.Table} 
            WHERE {Options.BaseUserSqlConditionGetter()} 
            AND {Options.UserNames.Id} = @userId 
            LIMIT 1
            """,
            new { userId });
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

        return await connection.QuerySingleOrDefaultAsync<TUser>(
            $"""
            SELECT * FROM {Options.UserNames.Table} 
            WHERE {Options.BaseUserSqlConditionGetter()} 
            AND {Options.UserNames.NormalizedUserName} = @normalizedUserName 
            LIMIT 1
            """,
            new { normalizedUserName });
    }

    /// <inheritdoc/>
    public override async Task<TUser?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<TUser>(
            $"""
            SELECT * FROM {Options.UserNames.Table} 
            WHERE {Options.BaseUserSqlConditionGetter()} 
            AND {Options.UserNames.NormalizedEmail} = @normalizedEmail 
            LIMIT 1
            """,
            new { normalizedEmail });
    }

    /// <inheritdoc/>
    public override async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        if (user.Id.Equals(default))
        {
            user.Id = GenerateNewKey() ?? user.Id; 
        }
        var propertyNames = GetUserInsertProperties();

        var insertCount = await connection.ExecuteAsync(
            $"""
            INSERT INTO {Options.UserNames.Table} {propertyNames.BuildSqlColumnsBlock(Options.TableNamingPolicy, insertLines: true)}
            VALUES {propertyNames.BuildSqlParametersBlock(insertLines: true)};
            """,
            user);

        return insertCount > 0
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError { Description = $"Could not insert user {user.Email}." });
    }

    /// <inheritdoc/>
    public override async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var propertyNames = GetUserUpdateProperties();

        var updateCount = await connection.ExecuteAsync(
            $"""
            UPDATE {Options.UserNames.Table} 
            SET {propertyNames.BuildSqlColumnsBlock(Options.TableNamingPolicy, insertLines: true)}
            = {propertyNames.BuildSqlParametersBlock(insertLines: true)}
            WHERE {Options.BaseUserSqlConditionGetter()} 
            AND {Options.UserNames.Id} = @{nameof(user.Id)};
            """,
            user);

        return updateCount > 0
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError { Description = $"Could not update user {user.Email}." });
    }

    /// <inheritdoc/>
    public override async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            $"""
            DELETE FROM {Options.UserNames.Table}
            WHERE {Options.BaseUserSqlConditionGetter()} 
            AND {Options.UserNames.Id} = @userId
            """,
            new { userId = user.Id });

        return IdentityResult.Success;
    }

    #endregion

    #region Claim Stores Implementation

    /// <inheritdoc/>
    public override async Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var result = await connection.QueryAsync<TUserClaim>(
            $"""
            SELECT * FROM {Options.UserClaimNames.Table} WHERE {Options.UserClaimNames.UserId} = @userId
            """,
            new { userId = user.Id });

        return result.Select(userClaim => userClaim.ToClaim()).ToList();
    }

    /// <inheritdoc/>
    public override async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var result = await connection.QueryAsync<TUser>(
            $"""
            SELECT u.* FROM {Options.UserNames.Table} u
            JOIN {Options.UserClaimNames.Table} uc ON u.{Options.UserNames.Id} = uc.{Options.UserClaimNames.UserId}
            WHERE {Options.BaseUserSqlConditionGetter("u")}
            AND uc.{Options.UserClaimNames.ClaimType} = @claimType 
            AND uc.{Options.UserClaimNames.ClaimValue} = @claimValue
            """,
            new { claimType = claim.Type, claimValue = claim.Value });

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
        sqlBuilder.AppendLine(
            $"INSERT INTO {Options.UserClaimNames.Table} ({Options.UserClaimNames.UserId}, {Options.UserClaimNames.ClaimType}, {Options.UserClaimNames.ClaimValue}) VALUES");

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
            UPDATE {Options.UserClaimNames.Table} 
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

        sqlBuilder.AppendLine($"DELETE FROM {Options.UserClaimNames.Table} WHERE {Options.UserClaimNames.UserId} = @userId");
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
            SELECT * FROM {Options.UserLoginNames.Table} 
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
            SELECT * FROM {Options.UserLoginNames.Table} 
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
            SELECT * FROM {Options.UserLoginNames.Table} WHERE {Options.UserLoginNames.UserId} = @userId
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

        string[] propertyNames = [
            nameof(userLogin.UserId),
            nameof(userLogin.LoginProvider),
            nameof(userLogin.ProviderKey),
            nameof(userLogin.ProviderDisplayName)
        ];

        await connection.ExecuteAsync(
            $"""
            INSERT INTO {Options.UserLoginNames.Table} 
            {propertyNames.BuildSqlColumnsBlock(Options.TableNamingPolicy)} 
            VALUES {propertyNames.BuildSqlParametersBlock()}
            """,
            userLogin);
    }

    /// <inheritdoc/>
    public override async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            $"""
            DELETE FROM {Options.UserLoginNames.Table} 
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
            SELECT * FROM {Options.UserTokenNames.Table} 
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

        string[] propertyNames = [
            nameof(token.UserId),
            nameof(token.LoginProvider),
            nameof(token.Name),
            nameof(token.Value)
        ];

        await connection.ExecuteAsync(
            $"""
            INSERT INTO {Options.UserTokenNames.Table} 
            {propertyNames.BuildSqlColumnsBlock(Options.TableNamingPolicy)}
            VALUES {propertyNames.BuildSqlParametersBlock()}
            """,
            token);
    }

    /// <inheritdoc/>
    protected override async Task RemoveUserTokenAsync(TUserToken token)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync();

        string[] propertyNames = [
            nameof(token.UserId),
            nameof(token.LoginProvider),
            nameof(token.Name)
        ];

        await connection.ExecuteAsync(
            $"""
            DELETE FROM {Options.UserTokenNames.Table} 
            WHERE {propertyNames.BuildSqlColumnsBlock(Options.TableNamingPolicy)} 
            = {propertyNames.BuildSqlParametersBlock()}
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
            SELECT * FROM {Options.RoleNames.Table} 
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
            SELECT * FROM {Options.UserRoleNames.Table} 
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
            SELECT r.{Options.RoleNames.Name} FROM {Options.RoleNames.Table} r
            JOIN {Options.UserRoleNames.Table} ur ON r.{Options.RoleNames.Id} = ur.{Options.UserRoleNames.RoleId}
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
            SELECT u.* FROM {Options.UserNames.Table} u
            JOIN {Options.UserRoleNames.Table} ur ON u.{Options.UserNames.Id} = ur.{Options.UserRoleNames.UserId}
            JOIN {Options.RoleNames.Table} r ON ur.{Options.UserRoleNames.RoleId} = r.{Options.RoleNames.Id}
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
                SELECT 1 FROM {Options.UserRoleNames.Table} ur
                JOIN {Options.RoleNames.Table} r ON ur.{Options.UserRoleNames.RoleId} = r.{Options.RoleNames.Id}
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
            INSERT INTO {Options.UserRoleNames.Table} 
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
            DELETE FROM {Options.UserRoleNames.Table} 
            WHERE ({Options.UserRoleNames.UserId}, {Options.UserRoleNames.RoleId}) = (@userId, @roleId)
            """,
            new { userId = user.Id, roleId = role.Id });
    }

    #endregion

}
