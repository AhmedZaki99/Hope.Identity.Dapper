using System.Data.Common;
using FluentAssertions;
using Hope.Identity.Dapper.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Hope.Identity.Dapper.Tests;

public class IdentityBuilderExtensionsTests
{
    [Fact]
    public void AddDapperStores_ShouldAddDapperStores_WhenCalledWithSetupAction()
    {
        // Arrange
        var services = new ServiceCollection();
        var dbDataSource = Substitute.For<DbDataSource>();
        var options = Substitute.For<IOptions<DapperStoreOptions>>();
        var setupAction = Substitute.For<Action<DapperStoreOptions>>();

        services.AddSingleton(dbDataSource);
        services.AddSingleton(options);

        var builder = new IdentityBuilder(typeof(IdentityUser), typeof(IdentityRole), services);


        // Act
        IdentityBuilderExtensions.AddDapperStores(builder, setupAction);


        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var userStore = serviceProvider.GetService<IUserStore<IdentityUser>>();
        var roleStore = serviceProvider.GetService<IRoleStore<IdentityRole>>();

        userStore.Should().NotBeNull();
        userStore.Should().BeAssignableTo(typeof(DapperUserStore<,,,,,,,>));
        roleStore.Should().NotBeNull();
        roleStore.Should().BeAssignableTo(typeof(DapperRoleStore<,,,>));
    }

    [Fact]
    public void AddDapperStores_ShouldAddDapperStores_WhenCalledWithUserStoreTypeAndSetupAction()
    {
        // Arrange
        var services = new ServiceCollection();
        var dbDataSource = Substitute.For<DbDataSource>();
        var options = Substitute.For<IOptions<DapperStoreOptions>>();
        var setupAction = Substitute.For<Action<DapperStoreOptions>>();

        services.AddSingleton(dbDataSource);
        services.AddSingleton(options);

        var builder = new IdentityBuilder(typeof(IdentityUser), typeof(IdentityRole), services);


        // Act
        IdentityBuilderExtensions.AddDapperStores<DapperUserStoreMock>(builder, setupAction);


        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var userStore = serviceProvider.GetService<IUserStore<IdentityUser>>();
        var concreteUserStore = serviceProvider.GetService<DapperUserStoreMock>();

        userStore.Should().NotBeNull();
        userStore.Should().BeOfType<DapperUserStoreMock>();
        concreteUserStore.Should().NotBeNull();
    }

    [Fact]
    public void AddDapperStores_ShouldAddDapperStores_WhenCalledWithUserStoreTypeRoleStoreTypeAndSetupAction()
    {
        // Arrange
        var services = new ServiceCollection();
        var dbDataSource = Substitute.For<DbDataSource>();
        var options = Substitute.For<IOptions<DapperStoreOptions>>();
        var setupAction = Substitute.For<Action<DapperStoreOptions>>();

        services.AddSingleton(dbDataSource);
        services.AddSingleton(options);

        var builder = new IdentityBuilder(typeof(IdentityUser), typeof(IdentityRole), services);


        // Act
        IdentityBuilderExtensions.AddDapperStores<DapperUserStoreMock, DapperRoleStoreMock>(builder, setupAction);


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
    public void AddDapperStores_ShouldThrowInvalidOperationException_WhenRoleTypeIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        var setupAction = Substitute.For<Action<DapperStoreOptions>>();
        var builder = new IdentityBuilder(typeof(IdentityUser), services);

        // Act
        Action act = () => IdentityBuilderExtensions.AddDapperStores<DapperUserStoreMock, DapperRoleStoreMock>(builder, setupAction);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("The role type must be provided when using this overload.");
    }


    // Helper classes for testing
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
