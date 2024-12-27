using System.Data.Common;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Provides an implementation for a Dapper-based Identity user store using the default Identity models, mainly <see cref="IdentityUser"/>, and <see cref="string"/> keys.
/// </summary>
public abstract class DapperUserStore
    : DapperUserStore<IdentityUser<string>>
{
    /// <remarks>
    /// Typically returns an empty array.
    /// </remarks>
    /// <inheritdoc/>
    protected sealed override string[] ExtraUserInsertProperties { get; }

    /// <remarks>
    /// Typically returns an empty array.
    /// </remarks>
    /// <inheritdoc/>
    protected sealed override string[] ExtraUserUpdateProperties { get; }


    /// <inheritdoc/>
    public DapperUserStore(DbDataSource dbDataSource, IdentityErrorDescriber? describer, JsonNamingPolicy tableNamingPolicy) 
        : base(dbDataSource, describer, tableNamingPolicy) 
    {
        ExtraUserInsertProperties = [];
        ExtraUserUpdateProperties = [];
    }
}

/// <inheritdoc/>
public abstract class DapperUserStore<TUser>
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
    : DapperUserStore<TUser, TRole, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>>
    where TUser : IdentityUser<string>
    where TRole : IdentityRole<string>
{
    /// <inheritdoc/>
    public DapperUserStore(DbDataSource dbDataSource, IdentityErrorDescriber? describer, JsonNamingPolicy tableNamingPolicy) 
        : base(dbDataSource, describer, tableNamingPolicy) { }
}
