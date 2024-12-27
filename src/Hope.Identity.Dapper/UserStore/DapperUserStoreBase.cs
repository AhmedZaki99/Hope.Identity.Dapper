using System.Data.Common;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Dapper;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Provides the base implementation for a Dapper-based Identity user store.
/// </summary>
/// <remarks>
/// This class implements most functionality required by <see cref="UserStoreBase{TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim}"/>
/// </remarks>
/// <inheritdoc/>
public abstract class DapperUserStoreBase<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim> 
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
    /// Gets the <see cref="JsonNamingPolicy"/> used to convert property names to SQL table names and/or column names.
    /// </summary>
    /// <remarks>
    /// For example, the following implementation:
    /// <code>
    /// protected override JsonNamingPolicy TableNamingPolicy { get; } = JsonNamingPolicy.SnakeCaseLower;
    /// </code>
    /// Will build a query like this:
    /// <code>
    /// INSERT INTO users (id, user_name, normalized_user_name, email, normalized_email, ...) 
    /// VALUES (@Id, @UserName, @NormalizedUserName, @Email, @NormalizedEmail, ...)
    /// </code>
    /// </remarks>
    protected abstract JsonNamingPolicy TableNamingPolicy { get; }


    /// <summary>
    /// Gets the SQL table and column names used for the <typeparamref name="TUser"/> table.
    /// </summary>
    /// <remarks>
    /// The default names are generated using the <see cref="TableNamingPolicy"/> for <see cref="IdentityUser{TKey}"/> properties and a table name "Users".
    /// </remarks>
    protected virtual UserTableNames<TKey> UserNames { get; }

    /// <summary>
    /// Gets the SQL table and column names used for the <typeparamref name="TUserLogin"/> table.
    /// </summary>
    /// <remarks>
    /// The default names are generated using the <see cref="TableNamingPolicy"/> for <see cref="IdentityUserLogin{TKey}"/> properties and a table name "UserLogins".
    /// </remarks>
    protected virtual UserLoginTableNames<TKey> UserLoginNames { get; }

    /// <summary>
    /// Gets the SQL table and column names used for the <typeparamref name="TUserClaim"/> table.
    /// </summary>
    /// <remarks>
    /// The default names are generated using the <see cref="TableNamingPolicy"/> for <see cref="IdentityUserClaim{TKey}"/> properties and a table name "UserClaims".
    /// </remarks>
    protected virtual UserClaimTableNames<TKey> UserClaimNames { get; }

    /// <summary>
    /// Gets the SQL table and column names used for the <typeparamref name="TUserToken"/> table.
    /// </summary>
    /// <remarks>
    /// The default names are generated using the <see cref="TableNamingPolicy"/> for <see cref="IdentityUserToken{TKey}"/> properties and a table name "UserTokens".
    /// </remarks>
    protected virtual UserTokenTableNames<TKey> UserTokenNames { get; }

    /// <summary>
    /// Gets the SQL table and column names used for the <typeparamref name="TRole"/> table.
    /// </summary>
    /// <remarks>
    /// The default names are generated using the <see cref="TableNamingPolicy"/> for <see cref="IdentityRole{TKey}"/> properties and a table name "Roles".
    /// </remarks>
    protected virtual RoleTableNames<TKey> RoleNames { get; }

    /// <summary>
    /// Gets the SQL table and column names used for the <typeparamref name="TUserRole"/> table.
    /// </summary>
    /// <remarks>
    /// The default names are generated using the <see cref="TableNamingPolicy"/> for <see cref="IdentityUserRole{TKey}"/> properties and a table name "UserRoles".
    /// </remarks>
    protected virtual UserRoleTableNames<TKey> UserRoleNames { get; }


    /// <summary>
    /// Gets an array of the base <see cref="IdentityUser{TKey}"/> property names used for insert queries.
    /// </summary>
    protected string[] IdentityUserInsertProperties { get; }

    /// <summary>
    /// Gets an array of the base <see cref="IdentityUser{TKey}"/> property names used for update queries.
    /// </summary>
    protected string[] IdentityUserUpdateProperties { get; }

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
    public DapperUserStoreBase(DbDataSource dbDataSource, IdentityErrorDescriber? describer) : base(describer ?? new())
    {
        DbDataSource = dbDataSource;

        UserNames = new(TableNamingPolicy);
        UserLoginNames = new(TableNamingPolicy);
        UserClaimNames = new(TableNamingPolicy);
        UserTokenNames = new(TableNamingPolicy);
        RoleNames = new(TableNamingPolicy);
        UserRoleNames = new(TableNamingPolicy);

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
    /// Gets the base SQL condition used for all user queries. Defaults to "TRUE" (no base condition).
    /// </summary>
    /// <remarks>
    /// This base condition is helpful to limit the set of users affected by the current implementation of the store, for example:
    /// <code>
    /// protected override string GetBaseUserSqlCondition(string tableAlias = "")
    /// {
    ///     var tablePrefix = string.IsNullOrEmpty(tableAlias) ? string.Empty : $"{tableAlias}.";
    ///     
    ///     return $"{tablePrefix}{nameof(User.TenantId).ToSqlColumn(TableNamingPolicy)} = @{nameof(User.TenantId)}";
    /// }
    /// </code>
    /// </remarks>
    /// <param name="tableAlias">The alias to use for the user table.</param>
    /// <returns>The base SQL condition used for all user queries.</returns>
    protected virtual string GetBaseUserSqlCondition(string tableAlias = "") => "TRUE";

    #endregion

    #region Abstract Methods

    /// <summary>
    /// Generates a new key for a created user.
    /// </summary>
    /// <remarks>
    /// When implemented for reference-type <typeparamref name="TKey"/>, return <see langword="null"/> if you want to use the database key generation.
    /// <code>
    /// // For example:
    /// protected override string GenerateNewKey() => null!;
    /// </code>
    /// </remarks>
    /// <returns>The new key.</returns>
    protected abstract TKey GenerateNewKey();

    /// <summary>
    /// Converts a string key to the key type used by the database (<typeparamref name="TKey"/>).
    /// </summary>
    /// <param name="key">The string key to convert.</param>
    /// <returns>The converted key.</returns>
    protected abstract TKey ConvertStringToKey(string key);


    /// <summary>
    /// Should be implemented to get the full list of property names used to insert a new user.
    /// </summary>
    /// <remarks>
    /// The returned property names are expected to be the exact names before conversion. 
    /// <br/>
    /// These names are later converted using the <see cref="TableNamingPolicy"/>.
    /// </remarks>
    /// <param name="identityUserInsertProperties">The initial set of property names existing in the base <see cref="IdentityUser{TKey}"/>.</param>
    /// <returns>The full list of property names used to insert a new user.</returns>
    protected abstract string[] GetUserInsertProperties(string[] identityUserInsertProperties);

    /// <summary>
    /// Should be implemented to get the full list of property names used to update a user.
    /// </summary>
    /// <remarks>
    /// The returned property names are expected to be the exact names before conversion. 
    /// <br/>
    /// These names are later converted using the <see cref="TableNamingPolicy"/>.
    /// </remarks>
    /// <param name="identityUserUpdateProperties">The initial set of property names existing in the base <see cref="IdentityUser{TKey}"/>.</param>
    /// <returns>The full list of property names used to update a user.</returns>
    protected abstract string[] GetUserUpdateProperties(string[] identityUserUpdateProperties);

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
            SELECT * FROM {UserNames.Table} 
            WHERE {GetBaseUserSqlCondition()} 
            AND {UserNames.Id} = @userId 
            LIMIT 1
            """,
            new { userId });
    }

    /// <inheritdoc/>
    public override Task<TUser?> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return FindUserAsync(ConvertStringToKey(userId), cancellationToken);
    }

    /// <inheritdoc/>
    public override async Task<TUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<TUser>(
            $"""
            SELECT * FROM {UserNames.Table} 
            WHERE {GetBaseUserSqlCondition()} 
            AND {UserNames.NormalizedUserName} = @normalizedUserName 
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
            SELECT * FROM {UserNames.Table} 
            WHERE {GetBaseUserSqlCondition()} 
            AND {UserNames.NormalizedEmail} = @normalizedEmail 
            LIMIT 1
            """,
            new { normalizedEmail });
    }

    /// <inheritdoc/>
    public override async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        user.Id = GenerateNewKey();
        var propertyNames = GetUserInsertProperties(IdentityUserInsertProperties);

        var insertCount = await connection.ExecuteAsync(
            $"""
            INSERT INTO {UserNames.Table} {propertyNames.BuildSqlColumnsBlock(TableNamingPolicy, insertLines: true)}
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

        var propertyNames = GetUserUpdateProperties(IdentityUserUpdateProperties);

        var updateCount = await connection.ExecuteAsync(
            $"""
            UPDATE {UserNames.Table} 
            SET {propertyNames.BuildSqlColumnsBlock(TableNamingPolicy, insertLines: true)}
            = {propertyNames.BuildSqlParametersBlock(insertLines: true)}
            WHERE {GetBaseUserSqlCondition()} 
            AND {UserNames.Id} = @{nameof(user.Id)};
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
            DELETE FROM {UserNames.Table}
            WHERE {GetBaseUserSqlCondition()} 
            AND {UserNames.Id} = @userId
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
            SELECT * FROM {UserClaimNames.Table} WHERE {UserClaimNames.UserId} = @userId
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
            SELECT u.* FROM {UserNames.Table} u
            JOIN {UserClaimNames.Table} uc ON u.{UserNames.Id} = uc.{UserClaimNames.UserId}
            WHERE {GetBaseUserSqlCondition("u")}
            AND uc.{UserClaimNames.ClaimType} = @claimType 
            AND uc.{UserClaimNames.ClaimValue} = @claimValue
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
            $"INSERT INTO {UserClaimNames.Table} ({UserClaimNames.UserId}, {UserClaimNames.ClaimType}, {UserClaimNames.ClaimValue}) VALUES");

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
            UPDATE {UserClaimNames.Table} 
            SET ({UserClaimNames.ClaimType}, {UserClaimNames.ClaimValue}) = (@newClaimType, @newClaimValue)
            WHERE ({UserClaimNames.UserId}, {UserClaimNames.ClaimType}, {UserClaimNames.ClaimValue}) = (@userId, @claimType, @claimValue)
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

        sqlBuilder.AppendLine($"DELETE FROM {UserClaimNames.Table} WHERE {UserClaimNames.UserId} = @userId");
        sqlBuilder.Append($"AND ({UserClaimNames.ClaimType}, {UserClaimNames.ClaimValue}) IN (");

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
            SELECT * FROM {UserLoginNames.Table} 
            WHERE ({UserLoginNames.UserId}, {UserLoginNames.LoginProvider}, {UserLoginNames.ProviderKey}) = (@userId, @loginProvider, @providerKey)
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
            SELECT * FROM {UserLoginNames.Table} 
            WHERE {UserLoginNames.LoginProvider} = @loginProvider AND {UserLoginNames.ProviderKey} = @providerKey 
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
            SELECT * FROM {UserLoginNames.Table} WHERE {UserLoginNames.UserId} = @userId
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

        await connection.ExecuteAsync(
            $"""
            INSERT INTO {UserLoginNames.Table} 
            ({UserLoginNames.UserId}, {UserLoginNames.LoginProvider}, {UserLoginNames.ProviderKey}, {UserLoginNames.ProviderDisplayName}) 
            VALUES (@userId, @loginProvider, @providerKey, @providerDisplayName)
            """,
            new
            {
                userId = user.Id,
                loginProvider = login.LoginProvider,
                providerKey = login.ProviderKey,
                providerDisplayName = login.ProviderDisplayName
            });
    }

    /// <inheritdoc/>
    public override async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            $"""
            DELETE FROM {UserLoginNames.Table} 
            WHERE ({UserLoginNames.UserId}, {UserLoginNames.LoginProvider}, {UserLoginNames.ProviderKey}) = (@userId, @loginProvider, @providerKey)
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
            SELECT * FROM {UserTokenNames.Table} 
            WHERE ({UserLoginNames.UserId}, {UserLoginNames.LoginProvider}, {UserLoginNames.ProviderKey}) = (@userId, @loginProvider, @name)
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
            INSERT INTO {UserTokenNames.Table} 
            {propertyNames.BuildSqlColumnsBlock(TableNamingPolicy)}
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
            DELETE FROM {UserTokenNames.Table} 
            WHERE {propertyNames.BuildSqlColumnsBlock(TableNamingPolicy)} 
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
            SELECT * FROM {RoleNames.Table} 
            WHERE {RoleNames.NormalizedName} = @normalizedRoleName 
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
            SELECT * FROM {UserRoleNames.Table} 
            WHERE ({UserRoleNames.UserId}, {UserRoleNames.RoleId}) = (@userId, @roleId)
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
            SELECT r.{RoleNames.Name} FROM {RoleNames.Table} r
            JOIN {UserRoleNames.Table} ur ON r.{RoleNames.Id} = ur.{UserRoleNames.RoleId}
            WHERE ur.{UserRoleNames.UserId} = @userId
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
            SELECT u.* FROM {UserNames.Table} u
            JOIN {UserRoleNames.Table} ur ON u.{UserNames.Id} = ur.{UserRoleNames.UserId}
            JOIN {RoleNames.Table} r ON ur.{UserRoleNames.RoleId} = r.{RoleNames.Id}
            WHERE r.{RoleNames.NormalizedName} = @normalizedRoleName
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
                SELECT 1 FROM {UserRoleNames.Table} ur
                JOIN {RoleNames.Table} r ON ur.{UserRoleNames.RoleId} = r.{RoleNames.Id}
                WHERE ur.{UserRoleNames.UserId} = @userId AND r.{RoleNames.NormalizedName} = @normalizedRoleName
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
            INSERT INTO {UserRoleNames.Table} 
            ({UserRoleNames.UserId}, {UserRoleNames.RoleId}) 
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
            DELETE FROM {UserRoleNames.Table} 
            WHERE ({UserRoleNames.UserId}, {UserRoleNames.RoleId}) = (@userId, @roleId)
            """,
            new { userId = user.Id, roleId = role.Id });
    }

    #endregion

}
