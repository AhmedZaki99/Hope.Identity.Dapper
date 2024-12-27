using System.Data.Common;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Hope.Identity.Dapper;

/// <summary>
/// Provides an implementation for a Dapper-based Identity user store.
/// </summary>
/// <inheritdoc/>
public class DapperUserStore<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>
    : DapperUserStoreBase<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>, new()
    where TUserRole : IdentityUserRole<TKey>, new()
    where TUserLogin : IdentityUserLogin<TKey>, new()
    where TUserToken : IdentityUserToken<TKey>, new()
    where TRoleClaim : IdentityRoleClaim<TKey>, new()
{
    /// <summary>
    /// Gets the options used to configure the store.
    /// </summary>
    protected virtual DapperStoreOptions Options { get; }


    /// <inheritdoc/>
    protected override JsonNamingPolicy? TableNamingPolicy => Options.TableNamingPolicy;

    /// <inheritdoc/>
    protected override UserTableNames UserNames => Options.UserNames;

    /// <inheritdoc/>
    protected override UserLoginTableNames UserLoginNames => Options.UserLoginNames;

    /// <inheritdoc/>
    protected override UserClaimTableNames UserClaimNames => Options.UserClaimNames;

    /// <inheritdoc/>
    protected override UserTokenTableNames UserTokenNames => Options.UserTokenNames;

    /// <inheritdoc/>
    protected override RoleTableNames RoleNames => Options.RoleNames;

    /// <inheritdoc/>
    protected override UserRoleTableNames UserRoleNames => Options.UserRoleNames;


    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="dbDataSource">The <see cref="DbDataSource"/> used to create database connections.</param>
    /// <param name="describer">The <see cref="IdentityErrorDescriber"/> used to describe store errors.</param>
    /// <param name="options">The options used to configure the store.</param>
    public DapperUserStore(DbDataSource dbDataSource, IdentityErrorDescriber? describer, IOptions<DapperStoreOptions> options) 
        : base(dbDataSource, describer) 
    {
        Options = options.Value;
    }


    /// <inheritdoc/>
    protected override string[] GetUserInsertProperties(string[] identityUserInsertProperties) => 
        [.. identityUserInsertProperties, .. Options.ExtraUserInsertProperties];

    /// <inheritdoc/>
    protected override string[] GetUserUpdateProperties(string[] identityUserUpdateProperties) => 
        [.. identityUserUpdateProperties, .. Options.ExtraUserUpdateProperties];


    /// <inheritdoc/>
    protected override string GetBaseUserSqlCondition(string tableAlias = "") => Options.BaseUserSqlConditionGetter(tableAlias);
}
