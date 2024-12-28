using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hope.Identity.Dapper.DependencyInjection;

/// <summary>
/// Provides extension methods for adding Dapper-based identity stores to the service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <exception cref="ArgumentException">Thrown when the user store type does not inherit from DapperUserStore.</exception>
    /// <inheritdoc cref="AddDapperStores{TUserStore}(IServiceCollection, Type, Action{DapperStoreOptions}?)"/>
    public static IServiceCollection AddDapperStores<TUserStore>(this IServiceCollection services, Action<DapperStoreOptions>? setupAction = null)
        where TUserStore : class
    {
        var dapperUserStoreType = typeof(TUserStore).FindGenericBaseType(typeof(DapperUserStore<,,,,,,,>))
            ?? throw new ArgumentException("The user store type provided must inherit from DapperUserStore or one of its generic overloads", nameof(TUserStore));

        var userType = dapperUserStoreType.GenericTypeArguments[0];

        return AddDapperStores<TUserStore>(services, userType, setupAction);
    }

    /// <exception cref="ArgumentException">Thrown when the user store type does not inherit from DapperUserStore or the role store type does not inherit from DapperRoleStore.</exception>
    /// <inheritdoc cref="AddDapperStores{TUserStore, TRoleStore}(IServiceCollection, Type, Type, Action{DapperStoreOptions}?)"/>
    public static IServiceCollection AddDapperStores<TUserStore, TRoleStore>(this IServiceCollection services, Action<DapperStoreOptions>? setupAction = null)
        where TUserStore : class
        where TRoleStore : class
    {
        var dapperUserStoreType = typeof(TUserStore).FindGenericBaseType(typeof(DapperUserStore<,,,,,,,>))
            ?? throw new ArgumentException("The user store type provided must inherit from DapperUserStore or one of its generic overloads", nameof(TUserStore));

        var dapperRoleStoreType = typeof(TUserStore).FindGenericBaseType(typeof(DapperRoleStore<,,,>))
            ?? throw new ArgumentException("The role store type provided must inherit from DapperRoleStore or one of its generic overloads", nameof(TRoleStore));

        var userType = dapperUserStoreType.GenericTypeArguments[0];
        var roleType = dapperRoleStoreType.GenericTypeArguments[0];

        return AddDapperStores<TUserStore, TRoleStore>(services, userType, roleType, setupAction);
    }

    /// <inheritdoc cref="AddDapperStores{TUserStore, TRoleStore}(IServiceCollection, Type, Type, Action{DapperStoreOptions}?)"/>
    public static IServiceCollection AddDapperStores<TUserStore>(this IServiceCollection services, Type userType, Action<DapperStoreOptions>? setupAction = null)
        where TUserStore : class
    {
        return AddDapperStores(services, userType, userStoreType: typeof(TUserStore), setupAction: setupAction);
    }

    /// <typeparam name="TUserStore">The type of the user store.</typeparam>
    /// <typeparam name="TRoleStore">The type of the role store.</typeparam>
    /// <inheritdoc cref="AddDapperStores(IServiceCollection, Type, Type?, Type?, Type?, Action{DapperStoreOptions}?)"/>
    public static IServiceCollection AddDapperStores<TUserStore, TRoleStore>(this IServiceCollection services, Type userType, Type roleType, Action<DapperStoreOptions>? setupAction = null)
        where TUserStore : class
        where TRoleStore : class
    {
        return AddDapperStores(services, userType, roleType, userStoreType: typeof(TUserStore), roleStoreType: typeof(TRoleStore), setupAction: setupAction);
    }

    /// <summary>
    /// Adds Dapper-based identity stores to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="userType">The type of the user.</param>
    /// <param name="roleType">The type of the role.</param>
    /// <param name="userStoreType">The type of the user store.</param>
    /// <param name="roleStoreType">The type of the role store.</param>
    /// <param name="setupAction">A delegate to configure <see cref="DapperStoreOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> to allow chaining up service configuration.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the user type does not inherit from IdentityUser, 
    /// the role type does not inherit from IdentityRole, 
    /// the user store type does not inherit from DapperUserStore, 
    /// or the role store type does not inherit from DapperRoleStore.
    /// </exception>
    public static IServiceCollection AddDapperStores(
        this IServiceCollection services,
        Type userType,
        Type? roleType = null,
        Type? userStoreType = null,
        Type? roleStoreType = null,
        Action<DapperStoreOptions>? setupAction = null)
    {
        var identityUserType = userType.FindGenericBaseType(typeof(IdentityUser<>))
            ?? throw new ArgumentException("The user type provided must inherit from IdentityUser<TKey>.", nameof(userType));

        var keyType = identityUserType.GenericTypeArguments[0];

        if (roleType is not null && !roleType.HasGenericBaseType(typeof(IdentityRole<>)))
        {
            throw new ArgumentException("The role type provided must inherit from IdentityRole<TKey>.", nameof(roleType));
        }
        if (userStoreType is not null && !userStoreType.HasGenericBaseType(typeof(DapperUserStore<,,,,,,,>)))
        {
            throw new ArgumentException("The user store type provided must inherit from DapperUserStore or one of its generic overloads.", nameof(userStoreType));
        }
        if (roleStoreType is not null && !roleStoreType.HasGenericBaseType(typeof(DapperRoleStore<,,,>)))
        {
            throw new ArgumentException("The role store type provided must inherit from DapperRoleStore or one of its generic overloads.", nameof(roleStoreType));
        }

        userStoreType ??= roleType is null
            ? typeof(DapperUserStore<,>).MakeGenericType(userType, keyType)
            : typeof(DapperUserStore<,,>).MakeGenericType(userType, roleType, keyType);

        services.AddScoped(userStoreType);
        services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), sp => sp.GetRequiredService(userStoreType));

        if (roleType is not null)
        {
            roleStoreType ??= typeof(DapperRoleStore<,>).MakeGenericType(roleType, keyType);

            services.AddScoped(roleStoreType);
            services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType), sp => sp.GetRequiredService(roleStoreType));
        }

        if (setupAction != null)
        {
            services.Configure(setupAction);
        }
        return services;
    }
}
