using System.Data.Common;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Provides the base implementation for a Dapper-based Identity user store using the default Identity models, mainly <see cref="IdentityUser"/>, and <see cref="string"/> keys.
/// </summary>
public abstract class DapperUserStoreBase
    : DapperUserStoreBase<IdentityUser<string>>
{
    /// <inheritdoc/>
    public DapperUserStoreBase(DbDataSource dbDataSource, IdentityErrorDescriber? describer) : base(dbDataSource, describer) { }


    /// <inheritdoc/>
    protected override string[] GetUserInsertProperties(string[] identityUserInsertProperties) => identityUserInsertProperties;

    /// <inheritdoc/>
    protected override string[] GetUserUpdateProperties(string[] identityUserUpdateProperties) => identityUserUpdateProperties;
}

/// <inheritdoc/>
public abstract class DapperUserStoreBase<TUser>
    : DapperUserStoreBase<TUser, IdentityRole<string>>
    where TUser : IdentityUser<string>
{
    /// <inheritdoc/>
    public DapperUserStoreBase(DbDataSource dbDataSource, IdentityErrorDescriber? describer) : base(dbDataSource, describer) { }
}

/// <summary>
/// Provides the base implementation for a Dapper-based Identity user store using the default Identity models and <see cref="string"/> keys.
/// </summary>
/// <inheritdoc/>
public abstract class DapperUserStoreBase<TUser, TRole>
    : DapperUserStoreBase<TUser, TRole, string>
    where TUser : IdentityUser<string>
    where TRole : IdentityRole<string>
{
    /// <inheritdoc/>
    public DapperUserStoreBase(DbDataSource dbDataSource, IdentityErrorDescriber? describer) : base(dbDataSource, describer) { }


    /// <inheritdoc/>
    protected override string GenerateNewKey() => Guid.NewGuid().ToString();

    /// <inheritdoc/>
    protected override string ConvertStringToKey(string key) => key;
}

/// <summary>
/// Provides the base implementation for a Dapper-based Identity user store using the default Identity claim, role, login, and token models.
/// </summary>
/// <inheritdoc/>
public abstract class DapperUserStoreBase<TUser, TRole, TKey> 
    : DapperUserStoreBase<TUser, TRole, TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>, IdentityRoleClaim<TKey>>
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
{
    /// <inheritdoc/>
    public DapperUserStoreBase(DbDataSource dbDataSource, IdentityErrorDescriber? describer) : base(dbDataSource, describer) { }
}
