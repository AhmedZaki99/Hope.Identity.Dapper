using System.Data.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Hope.Identity.Dapper;

/// <summary>
/// Provides an implementation for a Dapper-based Identity role store using the default Identity models, mainly <see cref="IdentityRole"/>, and <see cref="string"/> keys.
/// </summary>
public class DapperRoleStore
    : DapperRoleStore<IdentityRole>
{
    /// <inheritdoc/>
    public DapperRoleStore(DbDataSource dbDataSource, IOptions<DapperStoreOptions> options, IdentityErrorDescriber? describer = null)
        : base(dbDataSource, options, describer) { }


    /// <inheritdoc/>
    protected override Dictionary<string, string> GetRoleInsertProperties() => IdentityRoleInsertProperties;

    /// <inheritdoc/>
    protected override Dictionary<string, string> GetRoleUpdateProperties() => IdentityRoleUpdateProperties;
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
    public DapperRoleStore(DbDataSource dbDataSource, IOptions<DapperStoreOptions> options, IdentityErrorDescriber? describer = null)
        : base(dbDataSource, options, describer) { }
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
    public DapperRoleStore(DbDataSource dbDataSource, IOptions<DapperStoreOptions> options, IdentityErrorDescriber? describer = null) 
        : base(dbDataSource, options, describer) { }
}
