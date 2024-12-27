using System.Data.Common;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Provides an implementation for a Dapper-based Identity role store using the default Identity models, mainly <see cref="IdentityRole"/>, and <see cref="string"/> keys.
/// </summary>
public abstract class DapperRoleStore
    : DapperRoleStore<IdentityRole<string>>
{
    /// <remarks>
    /// Typically returns an empty array.
    /// </remarks>
    /// <inheritdoc/>
    protected sealed override string[] ExtraRoleInsertProperties { get; }

    /// <remarks>
    /// Typically returns an empty array.
    /// </remarks>
    /// <inheritdoc/>
    protected sealed override string[] ExtraRoleUpdateProperties { get; }


    /// <inheritdoc/>
    public DapperRoleStore(DbDataSource dbDataSource, IdentityErrorDescriber? describer, JsonNamingPolicy tableNamingPolicy) 
        : base(dbDataSource, describer, tableNamingPolicy) 
    {
        ExtraRoleInsertProperties = [];
        ExtraRoleUpdateProperties = [];
    }
}

/// <summary>
/// Provides an implementation for a Dapper-based Identity role store using default Identity models and <see cref="string"/> keys.
/// </summary>
/// <inheritdoc/>
public class DapperRoleStore<TRole>
    : DapperRoleStore<TRole, IdentityUserRole<string>, IdentityRoleClaim<string>>
    where TRole : IdentityRole<string>
{
    /// <inheritdoc/>
    public DapperRoleStore(DbDataSource dbDataSource, IdentityErrorDescriber? describer, JsonNamingPolicy tableNamingPolicy) 
        : base(dbDataSource, describer, tableNamingPolicy) { }
}
