using System.Data.Common;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Dapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Hope.Identity.Dapper.Tests;

[Collection(DatabaseDependentCollection.Name)]
public class DapperUserStoreTests : IAsyncLifetime
{

    #region Environment

    // System under test
    private DapperUserStore? _sut;

    // Fixtures
    private readonly DbDataSourceFactory _dbDataSourceFactory;

    // Substitutes
    private readonly IOptions<DapperStoreOptions> _options;

    #endregion

    #region Setup

    public DapperUserStoreTests(DbDataSourceFactory dbDataSourceFactory)
    {
        _dbDataSourceFactory = dbDataSourceFactory;

        _options = Substitute.For<IOptions<DapperStoreOptions>>();
        _options.Value.Returns(new DapperStoreOptions()
        {
            TableSchema = "identity",
            TableNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync()
    {
        await _dbDataSourceFactory.ResetDatabaseAsync();
    }

    #endregion

    #region Tests

    #region FindByIdAsync

    [Fact]
    public async Task FindByIdAsync_WithValidId_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userName = "user";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName);

        _sut = new(dbDataSource, _options);

        // Act
        var user = await _sut.FindByIdAsync(userId);

        // Assert
        user.Should().NotBeNull();
        user!.Id.Should().Be(userId);
        user.UserName.Should().Be(userName);
    }

    [Fact]
    public async Task FindByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userName = "user";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName);

        _sut = new(dbDataSource, _options);

        // Act
        var user = await _sut.FindByIdAsync(Guid.NewGuid().ToString());

        // Assert
        user.Should().BeNull();
    }

    #endregion

    #region FindByNameAsync

    [Fact]
    public async Task FindByNameAsync_WithValidName_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userName = "user";
        var normalizedUserName = userName.ToUpper();

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName, normalizedUserName);

        _sut = new(dbDataSource, _options);

        // Act
        var user = await _sut.FindByNameAsync(normalizedUserName);

        // Assert
        user.Should().NotBeNull();
        user!.Id.Should().Be(userId);
        user.UserName.Should().Be(userName);
    }

    [Fact]
    public async Task FindByNameAsync_WithInvalidName_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userName = "user";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName);

        _sut = new(dbDataSource, _options);

        // Act
        var user = await _sut.FindByNameAsync("INVALID_NAME");

        // Assert
        user.Should().BeNull();
    }

    #endregion

    #region FindByEmailAsync

    [Fact]
    public async Task FindByEmailAsync_WithValidEmail_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var email = "user@test.com";
        var normalizedEmail = email.ToUpper();

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserByEmailAsync(dbDataSource, userId, email, normalizedEmail);

        _sut = new(dbDataSource, _options);

        // Act
        var user = await _sut.FindByEmailAsync(normalizedEmail);

        // Assert
        user.Should().NotBeNull();
        user!.Id.Should().Be(userId);
        user.Email.Should().Be(email);
    }

    [Fact]
    public async Task FindByEmailAsync_WithInvalidEmail_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var email = "user@test.com";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserByEmailAsync(dbDataSource, userId, email);

        _sut = new(dbDataSource, _options);

        // Act
        var user = await _sut.FindByEmailAsync("INVALID_NAME");

        // Assert
        user.Should().BeNull();
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_WithValidUser_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userName = "user";

        await using var dbDataSource = _dbDataSourceFactory.Create();

        _sut = new(dbDataSource, _options);

        // Act
        var result = await _sut.CreateAsync(new IdentityUser { Id = userId, UserName = userName });

        // Assert
        result.Should().Be(IdentityResult.Success);

        var user = await QueryUserAsync(dbDataSource, userId);
        user.Should().NotBeNull();
        user!.Id.Should().Be(userId);
        user.UserName.Should().Be(userName);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_WithValidUser_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userName = "user";
        var updatedUserName = "updatedUser";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName);

        _sut = new(dbDataSource, _options);

        // Act
        var result = await _sut.UpdateAsync(new IdentityUser { Id = userId, UserName = updatedUserName });

        // Assert
        result.Should().Be(IdentityResult.Success);

        var user = await QueryUserAsync(dbDataSource, userId);
        user.Should().NotBeNull();
        user!.Id.Should().Be(userId);
        user.UserName.Should().Be(updatedUserName);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentUser_ReturnsFailed()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var updatedUserName = "updatedUser";

        await using var dbDataSource = _dbDataSourceFactory.Create();

        _sut = new(dbDataSource, _options);

        // Act
        var result = await _sut.UpdateAsync(new IdentityUser { Id = userId, UserName = updatedUserName });

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Description == $"Could not update user {updatedUserName}.");
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_WithValidUser_ReturnsSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userName = "user";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName);

        _sut = new(dbDataSource, _options);

        // Act
        var result = await _sut.DeleteAsync(new IdentityUser { Id = userId, UserName = userName });

        // Assert
        result.Should().Be(IdentityResult.Success);

        var user = await QueryUserAsync(dbDataSource, userId);
        user.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentUser_ReturnsFailed()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userName = "user";

        await using var dbDataSource = _dbDataSourceFactory.Create();

        _sut = new(dbDataSource, _options);

        // Act
        var result = await _sut.DeleteAsync(new IdentityUser { Id = userId, UserName = userName });

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Description == $"User '{userName}' not found.");
    }

    #endregion

    #region GetClaimsAsync

    [Fact]
    public async Task GetClaimsAsync_WithValidUser_ReturnsClaims()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userName = "user";
        var claim1Type = "type1";
        var claim1Value = "value1";
        var claim2Type = "type2";
        var claim2Value = "value2";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName);
        await InsertUserClaimsAsync(dbDataSource, userId, new() { { claim1Type, claim1Value }, { claim2Type, claim2Value } });

        _sut = new(dbDataSource, _options);

        // Act
        var claims = await _sut.GetClaimsAsync(new IdentityUser { Id = userId, UserName = userName });

        // Assert
        claims.Should().NotBeEmpty();
        claims.Should().Contain(c => c.Type == claim1Type && c.Value == claim1Value);
        claims.Should().Contain(c => c.Type == claim2Type && c.Value == claim2Value);
    }

    [Fact]
    public async Task GetClaimsAsync_WithInvalidUser_ReturnsEmptyCollection()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userName = "user";
        var claim1Type = "type1";
        var claim1Value = "value1";
        var claim2Type = "type2";
        var claim2Value = "value2";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName);
        await InsertUserClaimsAsync(dbDataSource, userId, new() { { claim1Type, claim1Value }, { claim2Type, claim2Value } });

        _sut = new(dbDataSource, _options);

        // Act
        var claims = await _sut.GetClaimsAsync(new IdentityUser { Id = Guid.NewGuid().ToString() });

        // Assert
        claims.Should().BeEmpty();
    }

    #endregion

    #region GetUsersForClaimAsync

    [Fact]
    public async Task GetUsersForClaimAsync_WithValidClaim_ReturnsUsers()
    {
        // Arrange
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();
        var userName1 = "user1";
        var userName2 = "user2";
        var claimType = "type";
        var claimValue = "value";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId1, userName1);
        await InsertUserAsync(dbDataSource, userId2, userName2);
        await InsertUserClaimAsync(dbDataSource, userId1, claimType, claimValue);
        await InsertUserClaimAsync(dbDataSource, userId2, claimType, claimValue);

        _sut = new(dbDataSource, _options);

        // Act
        var users = await _sut.GetUsersForClaimAsync(new Claim(claimType, claimValue));

        // Assert
        users.Should().NotBeEmpty();
        users.Should().Contain(u => u.Id == userId1 && u.UserName == userName1);
        users.Should().Contain(u => u.Id == userId2 && u.UserName == userName2);
    }

    [Fact]
    public async Task GetUsersForClaimAsync_WithInvalidClaim_ReturnsEmptyCollection()
    {
        // Arrange
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();
        var userName1 = "user1";
        var userName2 = "user2";
        var claimType = "type";
        var claimValue = "value";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId1, userName1);
        await InsertUserAsync(dbDataSource, userId2, userName2);
        await InsertUserClaimAsync(dbDataSource, userId1, claimType, claimValue);
        await InsertUserClaimAsync(dbDataSource, userId2, claimType, claimValue);

        _sut = new(dbDataSource, _options);

        // Act
        var users = await _sut.GetUsersForClaimAsync(new Claim("INVALID_TYPE", "INVALID_VALUE"));

        // Assert
        users.Should().BeEmpty();
    }

    #endregion

    #region AddClaimAsync

    [Fact]
    public async Task AddClaimAsync_WithValidUserAndClaim_AddsClaim()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userName = "user";
        var claimType = "type";
        var claimValue = "value";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName);

        _sut = new(dbDataSource, _options);

        // Act
        await _sut.AddClaimsAsync(new IdentityUser { Id = userId, UserName = userName }, [new Claim(claimType, claimValue)]);

        // Assert
        var claim = await QueryClaimAsync(dbDataSource, userId, claimType);
        claim.Should().NotBeNull();
        claim!.ClaimValue.Should().Be(claimValue);
    }

    #endregion

    #region ReplaceClaimAsync

    [Fact]
    public async Task ReplaceClaimAsync_WithValidUserAndClaim_ReplacesClaim()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userName = "user";
        var claimType = "type";
        var claimValue = "value";
        var newClaimValue = "newValue";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName);
        await InsertUserClaimAsync(dbDataSource, userId, claimType, claimValue);

        _sut = new(dbDataSource, _options);

        // Act
        await _sut.ReplaceClaimAsync(new IdentityUser { Id = userId, UserName = userName }, new Claim(claimType, claimValue), new Claim(claimType, newClaimValue));

        // Assert
        var claim = await QueryClaimAsync(dbDataSource, userId, claimType);
        claim.Should().NotBeNull();
        claim!.ClaimValue.Should().Be(newClaimValue);
    }

    #endregion

    #region RemoveClaimAsync

    [Fact]
    public async Task RemoveClaimAsync_WithValidUserAndClaim_RemovesClaim()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userName = "user";
        var claimType = "type";
        var claimValue = "value";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName);
        await InsertUserClaimAsync(dbDataSource, userId, claimType, claimValue);

        _sut = new(dbDataSource, _options);

        // Act
        await _sut.RemoveClaimsAsync(new IdentityUser { Id = userId, UserName = userName }, [new Claim(claimType, claimValue)]);

        // Assert
        var claim = await QueryClaimAsync(dbDataSource, userId, claimType);
        claim.Should().BeNull();
    }

    #endregion

    #region GetLoginsAsync

    [Fact]
    public async Task GetLoginsAsync_WithValidUser_ReturnsLogins()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userName = "user";
        var loginProvider = "provider";
        var providerKey = "key";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName);
        await InsertUserLoginAsync(dbDataSource, userId, loginProvider, providerKey);

        _sut = new(dbDataSource, _options);

        // Act
        var logins = await _sut.GetLoginsAsync(new IdentityUser { Id = userId, UserName = userName });

        // Assert
        logins.Should().NotBeEmpty();
        logins.Should().Contain(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);
    }

    [Fact]
    public async Task GetLoginsAsync_WithInvalidUser_ReturnsEmptyCollection()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userName = "user";
        var loginProvider = "provider";
        var providerKey = "key";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName);
        await InsertUserLoginAsync(dbDataSource, userId, loginProvider, providerKey);

        _sut = new(dbDataSource, _options);

        // Act
        var logins = await _sut.GetLoginsAsync(new IdentityUser { Id = Guid.NewGuid().ToString() });

        // Assert
        logins.Should().BeEmpty();
    }

    #endregion

    #region AddLoginAsync

    [Fact]
    public async Task AddLoginAsync_WithValidUserAndLogin_AddsLogin()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userName = "user";
        var loginProvider = "provider";
        var providerKey = "key";
        var displayName = "name";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName);

        _sut = new(dbDataSource, _options);

        // Act
        await _sut.AddLoginAsync(new IdentityUser { Id = userId, UserName = userName }, new UserLoginInfo(loginProvider, providerKey, displayName));

        // Assert
        var login = await QueryLoginAsync(dbDataSource, loginProvider, providerKey);
        login.Should().NotBeNull();
        login!.UserId.Should().Be(userId);
    }

    #endregion

    #region RemoveLoginAsync

    [Fact]
    public async Task RemoveLoginAsync_WithValidUserAndLogin_RemovesLogin()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userName = "user";
        var loginProvider = "provider";
        var providerKey = "key";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName);
        await InsertUserLoginAsync(dbDataSource, userId, loginProvider, providerKey);

        _sut = new(dbDataSource, _options);

        // Act
        await _sut.RemoveLoginAsync(new IdentityUser { Id = userId, UserName = userName }, loginProvider, providerKey);

        // Assert
        var login = await QueryLoginAsync(dbDataSource, loginProvider, providerKey);
        login.Should().BeNull();
    }

    #endregion

    #region GetRolesAsync

    [Fact]
    public async Task GetRolesAsync_WithValidUser_ReturnsRoles()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var roleId = Guid.NewGuid().ToString();
        var userName = "user";
        var roleName = "role";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName);
        await InsertRoleAsync(dbDataSource, roleId, roleName);
        await InsertUserRoleAsync(dbDataSource, userId, roleId);

        _sut = new(dbDataSource, _options);

        // Act
        var roles = await _sut.GetRolesAsync(new IdentityUser { Id = userId, UserName = userName });

        // Assert
        roles.Should().NotBeEmpty();
        roles.Should().Contain(role => role == roleName);
    }

    [Fact]
    public async Task GetRolesAsync_WithInvalidUser_ReturnsEmptyCollection()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var roleId = Guid.NewGuid().ToString();
        var userName = "user";
        var roleName = "role";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName);
        await InsertRoleAsync(dbDataSource, roleId, roleName);
        await InsertUserRoleAsync(dbDataSource, userId, roleId);

        _sut = new(dbDataSource, _options);

        // Act
        var roles = await _sut.GetRolesAsync(new IdentityUser { Id = Guid.NewGuid().ToString() });

        // Assert
        roles.Should().BeEmpty();
    }

    #endregion

    #region GetUsersInRoleAsync

    [Fact]
    public async Task GetUsersInRoleAsync_WithValidRole_ReturnsUsers()
    {
        // Arrange
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();
        var roleId = Guid.NewGuid().ToString();
        var userName1 = "user1";
        var userName2 = "user2";
        var roleName = "role";
        var normalizedRoleName = roleName.ToUpper();

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId1, userName1);
        await InsertUserAsync(dbDataSource, userId2, userName2);
        await InsertRoleAsync(dbDataSource, roleId, roleName, normalizedRoleName);
        await InsertUserRoleAsync(dbDataSource, userId1, roleId);
        await InsertUserRoleAsync(dbDataSource, userId2, roleId);

        _sut = new(dbDataSource, _options);

        // Act
        var users = await _sut.GetUsersInRoleAsync(normalizedRoleName);

        // Assert
        users.Should().NotBeEmpty();
        users.Should().Contain(u => u.Id == userId1 && u.UserName == userName1);
        users.Should().Contain(u => u.Id == userId2 && u.UserName == userName2);
    }

    [Fact]
    public async Task GetUsersInRoleAsync_WithInvalidRole_ReturnsEmptyCollection()
    {
        // Arrange
        var userId1 = Guid.NewGuid().ToString();
        var userId2 = Guid.NewGuid().ToString();
        var roleId = Guid.NewGuid().ToString();
        var userName1 = "user1";
        var userName2 = "user2";
        var roleName = "role";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId1, userName1);
        await InsertUserAsync(dbDataSource, userId2, userName2);
        await InsertRoleAsync(dbDataSource, roleId, roleName);
        await InsertUserRoleAsync(dbDataSource, userId1, roleId);
        await InsertUserRoleAsync(dbDataSource, userId2, roleId);

        _sut = new(dbDataSource, _options);

        // Act
        var users = await _sut.GetUsersInRoleAsync("INVALID_ROLE");

        // Assert
        users.Should().BeEmpty();
    }

    #endregion

    #region IsInRoleAsync

    [Fact]
    public async Task IsInRoleAsync_WithValidUserAndRole_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var roleId = Guid.NewGuid().ToString();
        var userName = "user";
        var roleName = "role";
        var normalizedRoleName = roleName.ToUpper();

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName);
        await InsertRoleAsync(dbDataSource, roleId, roleName, normalizedRoleName);
        await InsertUserRoleAsync(dbDataSource, userId, roleId);

        _sut = new(dbDataSource, _options);

        // Act
        var isInRole = await _sut.IsInRoleAsync(new IdentityUser { Id = userId, UserName = userName }, normalizedRoleName);

        // Assert
        isInRole.Should().BeTrue();
    }

    [Fact]
    public async Task IsInRoleAsync_WithInvalidUser_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var roleId = Guid.NewGuid().ToString();
        var userName = "user";
        var roleName = "role";
        var normalizedRoleName = roleName.ToUpper();

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName);
        await InsertRoleAsync(dbDataSource, roleId, roleName, normalizedRoleName);
        await InsertUserRoleAsync(dbDataSource, userId, roleId);

        _sut = new(dbDataSource, _options);

        // Act
        var isInRole = await _sut.IsInRoleAsync(new IdentityUser { Id = Guid.NewGuid().ToString() }, normalizedRoleName);

        // Assert
        isInRole.Should().BeFalse();
    }

    [Fact]
    public async Task IsInRoleAsync_WithInvalidRole_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var roleId = Guid.NewGuid().ToString();
        var userName = "user";
        var roleName = "role";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName);
        await InsertRoleAsync(dbDataSource, roleId, roleName);
        await InsertUserRoleAsync(dbDataSource, userId, roleId);

        _sut = new(dbDataSource, _options);

        // Act
        var isInRole = await _sut.IsInRoleAsync(new IdentityUser { Id = userId, UserName = userName }, "INVALID_ROLE");

        // Assert
        isInRole.Should().BeFalse();
    }

    #endregion

    #region AddToRoleAsync

    [Fact]
    public async Task AddToRoleAsync_WithValidUserAndRole_AddsRole()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var roleId = Guid.NewGuid().ToString();
        var userName = "user";
        var roleName = "role";
        var normalizedRoleName = roleName.ToUpper();

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName);
        await InsertRoleAsync(dbDataSource, roleId, roleName, normalizedRoleName);

        _sut = new(dbDataSource, _options);

        // Act
        await _sut.AddToRoleAsync(new IdentityUser { Id = userId, UserName = userName }, normalizedRoleName);

        // Assert
        var user = await QueryUserAsync(dbDataSource, userId);
        user.Should().NotBeNull();
        user!.Id.Should().Be(userId);
        user.UserName.Should().Be(userName);
    }

    [Fact]
    public async Task AddToRoleAsync_WithNonExistingRole_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userName = "user";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName);

        _sut = new(dbDataSource, _options);

        // Act
        Func<Task> act = async () => await _sut.AddToRoleAsync(new IdentityUser { Id = userId, UserName = userName }, "INVALID_ROLE");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region RemoveFromRoleAsync

    [Fact]
    public async Task RemoveFromRoleAsync_WithValidUserAndRole_RemovesRole()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var roleId = Guid.NewGuid().ToString();
        var userName = "user";
        var roleName = "role";
        var normalizedRoleName = roleName.ToUpper();

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName);
        await InsertRoleAsync(dbDataSource, roleId, roleName, normalizedRoleName);
        await InsertUserRoleAsync(dbDataSource, userId, roleId);

        _sut = new(dbDataSource, _options);

        // Act
        await _sut.RemoveFromRoleAsync(new IdentityUser { Id = userId, UserName = userName }, normalizedRoleName);

        // Assert
        var user = await QueryUserAsync(dbDataSource, userId);
        user.Should().NotBeNull();
        user!.Id.Should().Be(userId);
        user.UserName.Should().Be(userName);
    }

    [Fact]
    public async Task RemoveFromRoleAsync_WithNonExistingRole_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userName = "user";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertUserAsync(dbDataSource, userId, userName);

        _sut = new(dbDataSource, _options);

        // Act
        Func<Task> act = async () => await _sut.RemoveFromRoleAsync(new IdentityUser { Id = userId, UserName = userName }, "INVALID_ROLE");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #endregion


    #region Helper Methods

    private static async Task<IdentityUser?> QueryUserAsync(DbDataSource dbDataSource, string userId)
    {
        await using var connection = dbDataSource.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<IdentityUser>(
            """
            SELECT * FROM identity.users WHERE id = @userId
            """, 
            new { userId });
    }
    
    private static async Task InsertUserAsync(DbDataSource dbDataSource, string userId, string userName, string? normalizedUserName = null)
    {
        await using var connection = dbDataSource.CreateConnection();
        await connection.ExecuteAsync(
            """
            INSERT INTO identity.users (id, user_name, normalized_user_name) VALUES (@userId, @userName, @normalizedUserName)
            """,
            new { userId, userName, normalizedUserName });
    }
    
    private static async Task InsertUserByEmailAsync(DbDataSource dbDataSource, string userId, string email, string? normalizedEmail = null)
    {
        await using var connection = dbDataSource.CreateConnection();
        await connection.ExecuteAsync(
            """
            INSERT INTO identity.users (id, email, normalized_email) VALUES (@userId, @email, @normalizedEmail)
            """,
            new { userId, email, normalizedEmail });
    }


    private static async Task<IdentityUserClaim<string>?> QueryClaimAsync(DbDataSource dbDataSource, string userId, string claimType)
    {
        await using var connection = dbDataSource.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<IdentityUserClaim<string>>(
            """
            SELECT * FROM identity.user_claims WHERE user_id = @userId AND claim_type = @claimType
            """,
            new { userId, claimType });
    }

    private static Task InsertUserClaimAsync(DbDataSource dbDataSource, string userId, string claimType, string claimValue)
    {
        return InsertUserClaimsAsync(dbDataSource, userId, new() { { claimType, claimValue } });
    }

    private static async Task InsertUserClaimsAsync(DbDataSource dbDataSource, string userId, Dictionary<string, string> claims)
    {
        await using var connection = dbDataSource.CreateConnection();

        var sqlBuilder = new StringBuilder("INSERT INTO identity.user_claims (user_id, claim_type, claim_value) VALUES ");
        var parameters = new DynamicParameters(new { userId });

        int counter = 0;
        foreach (var (claimType, claimValue) in claims)
        {
            if (counter > 0)
            {
                sqlBuilder.AppendLine(",");
            }
            sqlBuilder.Append($"(@userId, @claimType{counter}, @claimValue{counter})");

            parameters.Add($"claimType{counter}", claimType);
            parameters.Add($"claimValue{counter}", claimValue);

            counter++;
        }

        await connection.ExecuteAsync(sqlBuilder.ToString(), parameters);
    }


    private static async Task<IdentityUserLogin<string>?> QueryLoginAsync(DbDataSource dbDataSource, string loginProvider, string providerKey)
    {
        await using var connection = dbDataSource.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<IdentityUserLogin<string>>(
            """
            SELECT * FROM identity.user_logins WHERE login_provider = @loginProvider AND provider_key = @providerKey
            """,
            new { loginProvider, providerKey });
    }

    private static async Task InsertUserLoginAsync(DbDataSource dbDataSource, string userId, string loginProvider, string providerKey)
    {
        await using var connection = dbDataSource.CreateConnection();
        await connection.ExecuteAsync(
            """
            INSERT INTO identity.user_logins (login_provider, provider_key, user_id) VALUES (@loginProvider, @providerKey, @userId)
            """,
            new { loginProvider, providerKey, userId });
    }
    

    private static async Task InsertRoleAsync(DbDataSource dbDataSource, string roleId, string name, string? normalizedName = null)
    {
        await using var connection = dbDataSource.CreateConnection();
        await connection.ExecuteAsync(
            """
            INSERT INTO identity.roles (id, name, normalized_name) VALUES (@roleId, @name, @normalizedName)
            """,
            new { roleId, name, normalizedName });
    }
    
    private static async Task InsertUserRoleAsync(DbDataSource dbDataSource, string userId, string roleId)
    {
        await using var connection = dbDataSource.CreateConnection();
        await connection.ExecuteAsync(
            """
            INSERT INTO identity.user_roles (user_id, role_id) VALUES (@userId, @roleId)
            """,
            new { userId, roleId });
    }

    #endregion

}
