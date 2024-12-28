using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
    public static IdentityBuilder AddDapperStores(this IdentityBuilder builder, Action<DapperStoreOptions> setupAction)
    {
        var identityUserType = FindGenericBaseType(builder.UserType, typeof(IdentityUser<>)) 
            ?? throw new InvalidOperationException("The user type provided must inherit from IdentityUser<TKey>.");

        var keyType = identityUserType.GenericTypeArguments[0];

        if (builder.RoleType is not null)
        {
            if (FindGenericBaseType(builder.RoleType, typeof(IdentityRole<>)) is null)
            {
                throw new InvalidOperationException("The role type provided must inherit from IdentityRole<TKey>.");
            }
            var roleStoreType = typeof(DapperRoleStore<,>).MakeGenericType(builder.RoleType, keyType);

            builder.Services.TryAddScoped(roleStoreType);
            builder.Services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(builder.RoleType), sp => sp.GetRequiredService(roleStoreType));
        }
        var userStoreType = builder.RoleType is null
            ? typeof(DapperUserStore<,>).MakeGenericType(builder.UserType, keyType)
            : typeof(DapperUserStore<,,>).MakeGenericType(builder.UserType, builder.RoleType, keyType);

        builder.Services.TryAddScoped(userStoreType);
        builder.Services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(builder.UserType), sp => sp.GetRequiredService(userStoreType));

        if (setupAction != null)
        {
            builder.Services.Configure(setupAction);
        }
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
        var userStoreType = typeof(TUserStore);

        if (FindGenericBaseType(userStoreType, typeof(DapperUserStore<,,,,,,,>)) is null)
        {
            throw new InvalidOperationException("The user store type provided must inherit from DapperUserStore or one of its generic overloads.");
        }

        builder.Services.TryAddScoped(userStoreType);
        builder.Services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(builder.UserType), sp => sp.GetRequiredService(userStoreType));

        if (setupAction != null)
        {
            builder.Services.Configure(setupAction);
        }
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
        if (FindGenericBaseType(builder.RoleType, typeof(IdentityRole<>)) is null)
        {
            throw new InvalidOperationException("The role type provided must inherit from IdentityRole<TKey>.");
        }

        var roleStoreType = typeof(TRoleStore);

        if (FindGenericBaseType(roleStoreType, typeof(DapperRoleStore<,,,>)) is null)
        {
            throw new InvalidOperationException("The role store type provided must inherit from DapperRoleStore or one of its generic overloads.");
        }

        builder.Services.TryAddScoped(roleStoreType);
        builder.Services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(builder.RoleType), sp => sp.GetRequiredService(roleStoreType));

        return AddDapperStores<TUserStore>(builder, setupAction);
    }


    private static Type? FindGenericBaseType(Type currentType, Type genericBaseType)
    {
        Type? type = currentType;
        while (type != null)
        {
            var genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;
            if (genericType != null && genericType == genericBaseType)
            {
                return type;
            }
            type = type.BaseType;
        }
        return null;
    }

}
