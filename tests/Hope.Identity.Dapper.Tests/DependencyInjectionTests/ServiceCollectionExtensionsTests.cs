using System.Data.Common;
using FluentAssertions;
using Hope.Identity.Dapper.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Hope.Identity.Dapper.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDapperStores_ShouldThrowArgumentException_WhenUserStoreTypeDoesNotInheritFromDapperUserStore()
    {
        // Arrange
        var services = Substitute.For<IServiceCollection>();

        // Act
        Action act = () => ServiceCollectionExtensions.AddDapperStores<NonDapperUserStore>(services);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("The user store type provided must inherit from DapperUserStore or one of its generic overloads*");
    }

    [Fact]
    public void AddDapperStores_ShouldThrowArgumentException_WhenRoleStoreTypeDoesNotInheritFromDapperRoleStore()
    {
        // Arrange
        var services = Substitute.For<IServiceCollection>();

        // Act
        Action act = () => ServiceCollectionExtensions.AddDapperStores<DapperUserStoreMock, NonDapperRoleStore>(services);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("The role store type provided must inherit from DapperRoleStore or one of its generic overloads*");
    }


    [Fact]
    public void AddDapperStores_ShouldAddUserStore_WhenUserStoreTypeInheritsFromDapperUserStore()
    {
        // Arrange
        var services = new ServiceCollection();
        var dbDataSource = Substitute.For<DbDataSource>();
        var options = Substitute.For<IOptions<DapperStoreOptions>>();

        services.AddSingleton(dbDataSource);
        services.AddSingleton(options);

        // Act
        ServiceCollectionExtensions.AddDapperStores<DapperUserStoreMock>(services);


        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var userStore = serviceProvider.GetService<IUserStore<IdentityUser>>();
        var concreteUserStore = serviceProvider.GetService<DapperUserStoreMock>();

        userStore.Should().NotBeNull();
        userStore.Should().BeOfType<DapperUserStoreMock>();
        concreteUserStore.Should().NotBeNull();
    }

    [Fact]
    public void AddDapperStores_ShouldAddUserAndRoleStore_WhenUserAndRoleStoreTypesInheritFromDapperStores()
    {
        // Arrange
        var services = new ServiceCollection();
        var dbDataSource = Substitute.For<DbDataSource>();
        var options = Substitute.For<IOptions<DapperStoreOptions>>();

        services.AddSingleton(dbDataSource);
        services.AddSingleton(options);

        // Act
        ServiceCollectionExtensions.AddDapperStores<DapperUserStoreMock, DapperRoleStoreMock>(services);


        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var userStore = serviceProvider.GetService<IUserStore<IdentityUser>>();
        var roleStore = serviceProvider.GetService<IRoleStore<IdentityRole>>();
        var concreteUserStore = serviceProvider.GetService<DapperUserStoreMock>();
        var concreteRoleStore = serviceProvider.GetService<DapperRoleStoreMock>();

        userStore.Should().NotBeNull();
        userStore.Should().BeOfType<DapperUserStoreMock>();
        roleStore.Should().NotBeNull();
        roleStore.Should().BeOfType<DapperRoleStoreMock>();
        concreteUserStore.Should().NotBeNull();
        concreteRoleStore.Should().NotBeNull();
    }

    [Fact]
    public void AddDapperStores_ShouldConfigureDapperStoreOptions_WhenSetupActionIsProvided()
    {
        // Arrange
        var services = new ServiceCollection();
        var setupAction = Substitute.For<Action<DapperStoreOptions>>();
        var dbDataSource = Substitute.For<DbDataSource>();
        var options = Substitute.For<IOptions<DapperStoreOptions>>();

        services.AddSingleton(dbDataSource);
        services.AddSingleton(options);


        // Act
        ServiceCollectionExtensions.AddDapperStores<DapperUserStoreMock>(services, setupAction);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var configureOptions = serviceProvider.GetService<IConfigureOptions<DapperStoreOptions>>();
        configureOptions.Should().NotBeNull();
    }


    [Fact]
    public void AddDapperStores_WithTypeParameters_ShouldAddUserStore_WhenItInheritFromDapperStore()
    {
        // Arrange
        var services = new ServiceCollection();
        var dbDataSource = Substitute.For<DbDataSource>();
        var options = Substitute.For<IOptions<DapperStoreOptions>>();

        services.AddSingleton(dbDataSource);
        services.AddSingleton(options);

        // Act
        ServiceCollectionExtensions.AddDapperStores(services, typeof(IdentityUser), userStoreType: typeof(DapperUserStoreMock));


        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var userStore = serviceProvider.GetService<IUserStore<IdentityUser>>();
        var concreteUserStore = serviceProvider.GetService<DapperUserStoreMock>();

        userStore.Should().NotBeNull();
        userStore.Should().BeOfType<DapperUserStoreMock>();
        concreteUserStore.Should().NotBeNull();
    }

    [Fact]
    public void AddDapperStores_WithTypeParameters_ShouldAddUserAndRoleStore_WhenTypesInheritFromDapperStores()
    {
        // Arrange
        var services = new ServiceCollection();
        var dbDataSource = Substitute.For<DbDataSource>();
        var options = Substitute.For<IOptions<DapperStoreOptions>>();

        services.AddSingleton(dbDataSource);
        services.AddSingleton(options);

        // Act
        ServiceCollectionExtensions.AddDapperStores(services, typeof(IdentityUser), typeof(IdentityRole), typeof(DapperUserStoreMock), typeof(DapperRoleStoreMock));


        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var userStore = serviceProvider.GetService<IUserStore<IdentityUser>>();
        var roleStore = serviceProvider.GetService<IRoleStore<IdentityRole>>();
        var concreteUserStore = serviceProvider.GetService<DapperUserStoreMock>();
        var concreteRoleStore = serviceProvider.GetService<DapperRoleStoreMock>();

        userStore.Should().NotBeNull();
        userStore.Should().BeOfType<DapperUserStoreMock>();
        roleStore.Should().NotBeNull();
        roleStore.Should().BeOfType<DapperRoleStoreMock>();
        concreteUserStore.Should().NotBeNull();
        concreteRoleStore.Should().NotBeNull();
    }


    [Fact]
    public void AddDapperStores_WithUserTypeOnly_ShouldThrowArgumentException_WhenUserTypeDoesNotInheritFromIdentityUser()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Action act = () => ServiceCollectionExtensions.AddDapperStores(services, typeof(NonIdentityUser));

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("The user type provided must inherit from IdentityUser<TKey>. (Parameter 'userType')");
    }

    [Fact]
    public void AddDapperStores_WithUserAndRoleTypesOnly_ShouldThrowArgumentException_WhenRoleTypeDoesNotInheritFromIdentityRole()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Action act = () => ServiceCollectionExtensions.AddDapperStores(services, typeof(IdentityUserMock), typeof(NonIdentityRole));

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("The role type provided must inherit from IdentityRole<TKey>. (Parameter 'roleType')");
    }

    [Fact]
    public void AddDapperStores_WithUserTypeOnly_ShouldAddDapperUserStore_WhenUserTypeInheritFromIdentityUser()
    {
        // Arrange
        var services = new ServiceCollection();
        var dbDataSource = Substitute.For<DbDataSource>();
        var options = Substitute.For<IOptions<DapperStoreOptions>>();

        services.AddSingleton(dbDataSource);
        services.AddSingleton(options);

        // Act
        ServiceCollectionExtensions.AddDapperStores(services, typeof(IdentityUserMock));


        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var userStore = serviceProvider.GetService<IUserStore<IdentityUserMock>>();

        userStore.Should().NotBeNull();
        userStore.Should().BeAssignableTo(typeof(DapperUserStore<,,,,,,,>));
    }

    [Fact]
    public void AddDapperStores_WithUserAndRoleTypesOnly_ShouldAddDapperStores_WhenUserAndRoleTypesInheritFromIdentityTypes()
    {
        // Arrange
        var services = new ServiceCollection();
        var dbDataSource = Substitute.For<DbDataSource>();
        var options = Substitute.For<IOptions<DapperStoreOptions>>();

        services.AddSingleton(dbDataSource);
        services.AddSingleton(options);

        // Act
        ServiceCollectionExtensions.AddDapperStores(services, typeof(IdentityUserMock), typeof(IdentityRoleMock));


        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var userStore = serviceProvider.GetService<IUserStore<IdentityUserMock>>();
        var roleStore = serviceProvider.GetService<IRoleStore<IdentityRoleMock>>();

        userStore.Should().NotBeNull();
        userStore.Should().BeAssignableTo(typeof(DapperUserStore<,,,,,,,>));
        roleStore.Should().NotBeNull();
        roleStore.Should().BeAssignableTo(typeof(DapperRoleStore<,,,>));
    }


    // Helper classes for testing
    private class NonIdentityUser { }
    private class IdentityUserMock : IdentityUser { }
    private class NonIdentityRole { }
    private class IdentityRoleMock : IdentityRole { }

    private class NonDapperUserStore { }
    private class NonDapperRoleStore { }

    private class DapperUserStoreMock : DapperUserStore
    {
        public DapperUserStoreMock(DbDataSource dbDataSource, IOptions<DapperStoreOptions> options, IdentityErrorDescriber? describer = null)
            : base(dbDataSource, options, describer) { }
    }

    private class DapperRoleStoreMock : DapperRoleStore
    {
        public DapperRoleStoreMock(DbDataSource dbDataSource, IOptions<DapperStoreOptions> options, IdentityErrorDescriber? describer = null)
            : base(dbDataSource, options, describer) { }
    }
}
