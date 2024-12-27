using System.Data.Common;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Provides the base implementation for a Dapper-based Identity role store using the default Identity models, mainly <see cref="IdentityRole"/>, and <see cref="string"/> keys.
/// </summary>
public abstract class DapperRoleStoreBase
    : DapperRoleStoreBase<IdentityRole<string>>
{
    /// <inheritdoc/>
    public DapperRoleStoreBase(DbDataSource dbDataSource, IdentityErrorDescriber? describer) : base(dbDataSource, describer) { }


    /// <inheritdoc/>
    protected override string[] GetRoleInsertProperties(string[] identityUserInsertProperties) => identityUserInsertProperties;

    /// <inheritdoc/>
    protected override string[] GetRoleUpdateProperties(string[] identityUserUpdateProperties) => identityUserUpdateProperties;
}

/// <summary>
/// Provides the base implementation for a Dapper-based Identity role store using the default Identity models and <see cref="string"/> keys.
/// </summary>
/// <inheritdoc/>
public abstract class DapperRoleStoreBase<TRole>
    : DapperRoleStoreBase<TRole, string>
    where TRole : IdentityRole<string>
{
    /// <inheritdoc/>
    public DapperRoleStoreBase(DbDataSource dbDataSource, IdentityErrorDescriber? describer) : base(dbDataSource, describer) { }


    /// <inheritdoc/>
    protected override string GenerateNewKey() => Guid.NewGuid().ToString();

    /// <inheritdoc/>
    protected override string ConvertStringToKey(string key) => key;
}

/// <summary>
/// Provides the base implementation for a Dapper-based Identity role store using the default Identity models.
/// </summary>
/// <inheritdoc/>
public abstract class DapperRoleStoreBase<TRole, TKey> 
    : DapperRoleStoreBase<TRole, TKey, IdentityUserRole<TKey>, IdentityRoleClaim<TKey>>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
{
    /// <inheritdoc/>
    public DapperRoleStoreBase(DbDataSource dbDataSource, IdentityErrorDescriber? describer) : base(dbDataSource, describer) { }
}
