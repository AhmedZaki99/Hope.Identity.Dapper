using System.Data.Common;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Hope.Identity.Dapper;

/// <summary>
/// Provides an implementation for a Dapper-based Identity role store.
/// </summary>
/// <inheritdoc/>
public class DapperRoleStore<TRole, TKey, TUserRole, TRoleClaim>
    : DapperRoleStoreBase<TRole, TKey, TUserRole, TRoleClaim>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
    where TUserRole : IdentityUserRole<TKey>, new()
    where TRoleClaim : IdentityRoleClaim<TKey>, new()
{
    /// <summary>
    /// Gets the options used to configure the store.
    /// </summary>
    protected virtual DapperStoreOptions Options { get; }


    /// <inheritdoc/>
    protected override JsonNamingPolicy? TableNamingPolicy => Options.TableNamingPolicy;

    /// <inheritdoc/>
    protected override RoleTableNames RoleNames => Options.RoleNames;

    /// <inheritdoc/>
    protected override RoleClaimTableNames RoleClaimNames => Options.RoleClaimNames;


    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="dbDataSource">The <see cref="DbDataSource"/> used to create database connections.</param>
    /// <param name="describer">The <see cref="IdentityErrorDescriber"/> used to describe store errors.</param>
    /// <param name="options">The options used to configure the store.</param>
    public DapperRoleStore(DbDataSource dbDataSource, IdentityErrorDescriber? describer, IOptions<DapperStoreOptions> options) 
        : base(dbDataSource, describer) 
    {
        Options = options.Value;
    }


    /// <inheritdoc/>
    protected override string[] GetRoleInsertProperties(string[] identityUserInsertProperties) => 
        [.. identityUserInsertProperties, .. Options.ExtraRoleInsertProperties];

    /// <inheritdoc/>
    protected override string[] GetRoleUpdateProperties(string[] identityUserUpdateProperties) => 
        [.. identityUserUpdateProperties, .. Options.ExtraRoleUpdateProperties];
}
