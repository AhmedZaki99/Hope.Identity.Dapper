using System.Text.Json;
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
            AddRoleStore(builder.Services, keyType, builder.RoleType);
        }
        AddUserStore(builder.Services, keyType, builder.UserType, builder.RoleType);

        if (setupAction != null)
        {
            builder.Services.Configure(setupAction);
        }
        return builder;
    }


    private static void AddUserStore(IServiceCollection services, Type keyType, Type userType, Type? roleType = null)
    {
        var userStoreType = roleType is null
            ? typeof(DapperUserStore<,>).MakeGenericType(userType, keyType)
            : typeof(DapperUserStore<,,>).MakeGenericType(userType, roleType, keyType);

        services.TryAddScoped(userStoreType);
        services.TryAddScoped(typeof(IUserStore<>).MakeGenericType(userType), sp => sp.GetRequiredService(userStoreType));
    }

    private static void AddRoleStore(IServiceCollection services, Type keyType, Type roleType)
    {
        var roleStoreType = typeof(DapperRoleStore<,>).MakeGenericType(roleType, keyType);

        services.TryAddScoped(roleStoreType);
        services.TryAddScoped(typeof(IRoleStore<>).MakeGenericType(roleType), sp => sp.GetRequiredService(roleStoreType));
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
