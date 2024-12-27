using System.Data.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Hope.Identity.Dapper;

/// <summary>
/// Provides an implementation for a Dapper-based Identity role store using the default Identity models, mainly <see cref="IdentityRole"/>, and <see cref="string"/> keys.
/// </summary>
public abstract class DapperRoleStore
    : DapperRoleStore<IdentityRole<string>>
{
    /// <inheritdoc/>
    public DapperRoleStore(DbDataSource dbDataSource, IdentityErrorDescriber? describer, IOptions<DapperStoreOptions> options) 
        : base(dbDataSource, describer, options) { }


    /// <inheritdoc/>
    protected override string[] GetRoleInsertProperties(string[] identityUserInsertProperties) => identityUserInsertProperties;

    /// <inheritdoc/>
    protected override string[] GetRoleUpdateProperties(string[] identityUserUpdateProperties) => identityUserUpdateProperties;
}

/// <summary>
/// Provides an implementation for a Dapper-based Identity role store using default Identity models and <see cref="string"/> keys.
/// </summary>
/// <inheritdoc/>
public class DapperRoleStore<TRole>
    : DapperRoleStore<TRole, string>
    where TRole : IdentityRole<string>
{
    /// <inheritdoc/>
    public DapperRoleStore(DbDataSource dbDataSource, IdentityErrorDescriber? describer, IOptions<DapperStoreOptions> options) 
        : base(dbDataSource, describer, options) { }
}

/// <summary>
/// Provides an implementation for a Dapper-based Identity role store using default Identity models.
/// </summary>
/// <inheritdoc/>
public class DapperRoleStore<TRole, TKey>
    : DapperRoleStore<TRole, TKey, IdentityUserRole<TKey>, IdentityRoleClaim<TKey>>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
{
    /// <inheritdoc/>
    public DapperRoleStore(DbDataSource dbDataSource, IdentityErrorDescriber? describer, IOptions<DapperStoreOptions> options) 
        : base(dbDataSource, describer, options) { }
}
