using System.Data.Common;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Provides an implementation for a Dapper-based Identity user store using <see cref="string"/> keys.
/// </summary>
/// <remarks>
/// This class exposes <see cref="ExtraUserInsertProperties"/> and <see cref="ExtraUserUpdateProperties"/> properties
/// to implement functionality required in <see cref="DapperUserStoreBase{TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim}"/>
/// </remarks>
/// <inheritdoc/>
public class DapperUserStore<TUser, TRole, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>
    : DapperUserStoreBase<TUser, TRole, string, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>
    where TUser : IdentityUser<string>
    where TRole : IdentityRole<string>
    where TUserClaim : IdentityUserClaim<string>, new()
    where TUserRole : IdentityUserRole<string>, new()
    where TUserLogin : IdentityUserLogin<string>, new()
    where TUserToken : IdentityUserToken<string>, new()
    where TRoleClaim : IdentityRoleClaim<string>, new()
{
    /// <summary>
    /// Gets an array of the extra property names within <typeparamref name="TUser"/> used for insert queries (excluding the base <see cref="IdentityUser{TKey}"/> properties).
    /// </summary>
    protected virtual string[] ExtraUserInsertProperties { get; }

    /// <summary>
    /// Gets an array of the extra property names within <typeparamref name="TUser"/> used for update queries (excluding the base <see cref="IdentityUser{TKey}"/> properties).
    /// </summary>
    protected virtual string[] ExtraUserUpdateProperties { get; }


    /// <inheritdoc/>
    protected override JsonNamingPolicy TableNamingPolicy { get; }


    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="dbDataSource">The <see cref="DbDataSource"/> used to create database connections.</param>
    /// <param name="describer">The <see cref="IdentityErrorDescriber"/> used to describe store errors.</param>
    /// <param name="tableNamingPolicy">
    /// The naming policy used to convert property names to SQL table names and/or column names.
    /// <br/><br/>
    /// For example, a naming policy like <see cref="JsonNamingPolicy.SnakeCaseLower"/> will build a query like this:
    /// <code>
    /// INSERT INTO users (id, user_name, normalized_user_name, email, normalized_email, ...) 
    /// VALUES (@Id, @UserName, @NormalizedUserName, @Email, @NormalizedEmail, ...)
    /// </code>
    /// </param>
    public DapperUserStore(DbDataSource dbDataSource, IdentityErrorDescriber? describer, JsonNamingPolicy tableNamingPolicy) 
        : base(dbDataSource, describer) 
    {
        TableNamingPolicy = tableNamingPolicy;

        ExtraUserInsertProperties = [];
        ExtraUserUpdateProperties = [];
    }


    /// <remarks></remarks>
    /// <inheritdoc/>
    protected override string GenerateNewKey() => Guid.NewGuid().ToString();

    /// <inheritdoc/>
    protected override string ConvertStringToKey(string key) => key;


    /// <inheritdoc/>
    protected override string[] GetUserInsertProperties(string[] identityUserInsertProperties) => [.. identityUserInsertProperties, .. ExtraUserInsertProperties];

    /// <inheritdoc/>
    protected override string[] GetUserUpdateProperties(string[] identityUserUpdateProperties) => [.. identityUserUpdateProperties, .. ExtraUserUpdateProperties];
}
