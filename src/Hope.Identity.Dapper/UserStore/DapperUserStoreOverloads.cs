using System.Data.Common;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Provides an implementation for a Dapper-based Identity user store using the default Identity models, mainly <see cref="IdentityUser"/>, and <see cref="string"/> keys.
/// </summary>
public class DapperUserStore
    : DapperUserStore<IdentityUser<string>>
{
    /// <remarks>
    /// Typically returns an empty array.
    /// </remarks>
    /// <inheritdoc/>
    protected sealed override string[] ExtraUserInsertProperties { get; set; }

    /// <remarks>
    /// Typically returns an empty array.
    /// </remarks>
    /// <inheritdoc/>
    protected sealed override string[] ExtraUserUpdateProperties { get; set; }


    /// <inheritdoc/>
    public DapperUserStore(DbDataSource dbDataSource, IdentityErrorDescriber? describer, JsonNamingPolicy tableNamingPolicy) 
        : base(dbDataSource, describer, tableNamingPolicy) 
    {
        ExtraUserInsertProperties = [];
        ExtraUserUpdateProperties = [];
    }
}

/// <inheritdoc/>
public class DapperUserStore<TUser>
    : DapperUserStore<TUser, IdentityRole<string>>
    where TUser : IdentityUser<string>
{
    /// <inheritdoc/>
    public DapperUserStore(DbDataSource dbDataSource, IdentityErrorDescriber? describer, JsonNamingPolicy tableNamingPolicy) 
        : base(dbDataSource, describer, tableNamingPolicy) { }
}

/// <summary>
/// Provides an implementation for a Dapper-based Identity user store using default Identity models and <see cref="string"/> keys.
/// </summary>
/// <inheritdoc/>
public class DapperUserStore<TUser, TRole>
    : DapperUserStore<TUser, TRole, string>
    where TUser : IdentityUser<string>
    where TRole : IdentityRole<string>
{
    /// <inheritdoc/>
    public DapperUserStore(DbDataSource dbDataSource, IdentityErrorDescriber? describer, JsonNamingPolicy tableNamingPolicy) 
        : base(dbDataSource, describer, tableNamingPolicy) { }
}

/// <summary>
/// Provides an implementation for a Dapper-based Identity user store using default Identity models and <see cref="string"/> keys.
/// </summary>
/// <inheritdoc/>
public class DapperUserStore<TUser, TRole, TKey>
    : DapperUserStore<TUser, TRole, TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>, IdentityRoleClaim<TKey>>
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
{
    /// <inheritdoc/>
    public DapperUserStore(DbDataSource dbDataSource, IdentityErrorDescriber? describer, JsonNamingPolicy tableNamingPolicy) 
        : base(dbDataSource, describer, tableNamingPolicy) { }
}
