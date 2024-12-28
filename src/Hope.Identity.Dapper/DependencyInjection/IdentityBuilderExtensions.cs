using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper.DependencyInjection;

/// <summary>
/// Contains extension methods to configure Dapper-based identity stores.
/// </summary>
public static class IdentityBuilderExtensions
{

    /// <summary>
    /// Adds Dapper implementation of identity information stores.
    /// </summary>
    /// <param name="builder">The <see cref="IdentityBuilder"/> instance this method extends.</param>
    /// <param name="setupAction">An action to configure the <see cref="DapperStoreOptions"/>.</param>
    /// <returns>The <see cref="IdentityBuilder"/> instance to allow chaining up identity configuration.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IdentityBuilder AddDapperStores(this IdentityBuilder builder, Action<DapperStoreOptions>? setupAction = null)
    {
        builder.Services.AddDapperStores(builder.UserType, builder.RoleType, setupAction: setupAction);
        return builder;
    }

    /// <summary>
    /// Adds Dapper implementation of identity information stores with the specified user store type.
    /// </summary>
    /// <param name="builder">The <see cref="IdentityBuilder"/> instance this method extends.</param>
    /// <param name="setupAction">An action to configure the <see cref="DapperStoreOptions"/>.</param>
    /// <typeparam name="TUserStore">The type of the user store to use.</typeparam>
    /// <returns>The <see cref="IdentityBuilder"/> instance to allow chaining up identity configuration.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IdentityBuilder AddDapperStores<TUserStore>(this IdentityBuilder builder, Action<DapperStoreOptions> setupAction)
        where TUserStore : class
    {
        builder.Services.AddDapperStores<TUserStore>(builder.UserType, setupAction: setupAction);
        return builder;
    }

    /// <summary>
    /// Adds Dapper implementation of identity information stores with the specified user store and role store types.
    /// </summary>
    /// <param name="builder">The <see cref="IdentityBuilder"/> instance this method extends.</param>
    /// <param name="setupAction">An action to configure the <see cref="DapperStoreOptions"/>.</param>
    /// <typeparam name="TUserStore">The type of the user store to use.</typeparam>
    /// <typeparam name="TRoleStore">The type of the role store to use.</typeparam>
    /// <returns>The <see cref="IdentityBuilder"/> instance to allow chaining up identity configuration.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static IdentityBuilder AddDapperStores<TUserStore, TRoleStore>(this IdentityBuilder builder, Action<DapperStoreOptions> setupAction)
        where TUserStore : class
        where TRoleStore : class
    {
        if (builder.RoleType is null)
        {
            throw new InvalidOperationException("The role type must be provided when using this overload.");
        }
        builder.Services.AddDapperStores<TUserStore, TRoleStore>(builder.UserType, builder.RoleType, setupAction: setupAction);
        return builder;
    }
}
