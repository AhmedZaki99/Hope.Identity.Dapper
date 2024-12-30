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
public class DapperRoleStoreTests : IAsyncLifetime
{

    #region Environment

    // System under test
    private DapperRoleStore? _sut;

    // Fixtures
    private readonly DbDataSourceFactory _dbDataSourceFactory;

    // Substitutes
    private readonly IOptions<DapperStoreOptions> _options;

    #endregion

    #region Setup

    public DapperRoleStoreTests(DbDataSourceFactory dbDataSourceFactory)
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
    public async Task FindByIdAsync_WithValidId_ReturnsRole()
    {
        // Arrange
        var roleId = Guid.NewGuid().ToString();
        var roleName = "role";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertRoleAsync(dbDataSource, roleId, roleName);

        _sut = new(dbDataSource, _options);

        // Act
        var role = await _sut.FindByIdAsync(roleId);

        // Assert
        role.Should().NotBeNull();
        role!.Id.Should().Be(roleId);
        role.Name.Should().Be(roleName);
    }

    [Fact]
    public async Task FindByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var roleId = Guid.NewGuid().ToString();
        var roleName = "role";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertRoleAsync(dbDataSource, roleId, roleName);

        _sut = new(dbDataSource, _options);

        // Act
        var role = await _sut.FindByIdAsync(Guid.NewGuid().ToString());

        // Assert
        role.Should().BeNull();
    }

    #endregion

    #region FindByNameAsync

    [Fact]
    public async Task FindByNameAsync_WithValidName_ReturnsRole()
    {
        // Arrange
        var roleId = Guid.NewGuid().ToString();
        var roleName = "role";
        var normalizedRoleName = roleName.ToUpper();

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertRoleAsync(dbDataSource, roleId, roleName, normalizedRoleName);

        _sut = new(dbDataSource, _options);

        // Act
        var role = await _sut.FindByNameAsync(normalizedRoleName);

        // Assert
        role.Should().NotBeNull();
        role!.Id.Should().Be(roleId);
        role.Name.Should().Be(roleName);
    }

    [Fact]
    public async Task FindByNameAsync_WithInvalidName_ReturnsNull()
    {
        // Arrange
        var roleId = Guid.NewGuid().ToString();
        var roleName = "role";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertRoleAsync(dbDataSource, roleId, roleName);

        _sut = new(dbDataSource, _options);

        // Act
        var role = await _sut.FindByNameAsync("INVALID_NAME");

        // Assert
        role.Should().BeNull();
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_WithValidRole_ReturnsSuccess()
    {
        // Arrange
        var roleId = Guid.NewGuid().ToString();
        var roleName = "role";

        await using var dbDataSource = _dbDataSourceFactory.Create();

        _sut = new(dbDataSource, _options);

        // Act
        var result = await _sut.CreateAsync(new IdentityRole { Id = roleId, Name = roleName });

        // Assert
        result.Should().Be(IdentityResult.Success);

        var role = await QueryRoleAsync(dbDataSource, roleId);
        role.Should().NotBeNull();
        role!.Id.Should().Be(roleId);
        role.Name.Should().Be(roleName);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_WithValidRole_ReturnsSuccess()
    {
        // Arrange
        var roleId = Guid.NewGuid().ToString();
        var roleName = "role";
        var updatedRoleName = "updatedRole";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertRoleAsync(dbDataSource, roleId, roleName);

        _sut = new(dbDataSource, _options);

        // Act
        var result = await _sut.UpdateAsync(new IdentityRole { Id = roleId, Name = updatedRoleName });

        // Assert
        result.Should().Be(IdentityResult.Success);

        var role = await QueryRoleAsync(dbDataSource, roleId);
        role.Should().NotBeNull();
        role!.Id.Should().Be(roleId);
        role.Name.Should().Be(updatedRoleName);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentRole_ReturnsFailed()
    {
        // Arrange
        var roleId = Guid.NewGuid().ToString();
        var updatedRoleName = "updatedRole";

        await using var dbDataSource = _dbDataSourceFactory.Create();

        _sut = new(dbDataSource, _options);

        // Act
        var result = await _sut.UpdateAsync(new IdentityRole { Id = roleId, Name = updatedRoleName });

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Description == $"Could not update role {updatedRoleName}.");
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_WithValidRole_ReturnsSuccess()
    {
        // Arrange
        var roleId = Guid.NewGuid().ToString();
        var roleName = "role";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertRoleAsync(dbDataSource, roleId, roleName);

        _sut = new(dbDataSource, _options);

        // Act
        var result = await _sut.DeleteAsync(new IdentityRole { Id = roleId, Name = roleName });

        // Assert
        result.Should().Be(IdentityResult.Success);

        var role = await QueryRoleAsync(dbDataSource, roleId);
        role.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentRole_ReturnsFailed()
    {
        // Arrange
        var roleId = Guid.NewGuid().ToString();
        var roleName = "role";

        await using var dbDataSource = _dbDataSourceFactory.Create();

        _sut = new(dbDataSource, _options);

        // Act
        var result = await _sut.DeleteAsync(new IdentityRole { Id = roleId, Name = roleName });

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Description == $"Role '{roleName}' not found.");
    }

    #endregion

    #region GetClaimsAsync

    [Fact]
    public async Task GetClaimsAsync_WithValidRole_ReturnsClaims()
    {
        // Arrange
        var roleId = Guid.NewGuid().ToString();
        var roleName = "role";
        var claim1Type = "type1";
        var claim1Value = "value1";
        var claim2Type = "type2";
        var claim2Value = "value2";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertRoleAsync(dbDataSource, roleId, roleName);
        await InsertRoleClaimsAsync(dbDataSource, roleId, new() { { claim1Type, claim1Value }, { claim2Type, claim2Value } });

        _sut = new(dbDataSource, _options);

        // Act
        var claims = await _sut.GetClaimsAsync(new IdentityRole { Id = roleId, Name = roleName });

        // Assert
        claims.Should().NotBeEmpty();
        claims.Should().Contain(c => c.Type == claim1Type && c.Value == claim1Value);
        claims.Should().Contain(c => c.Type == claim2Type && c.Value == claim2Value);
    }

    [Fact]
    public async Task GetClaimsAsync_WithInvalidRole_ReturnsEmptyCollection()
    {
        // Arrange
        var roleId = Guid.NewGuid().ToString();
        var roleName = "role";
        var claim1Type = "type1";
        var claim1Value = "value1";
        var claim2Type = "type2";
        var claim2Value = "value2";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertRoleAsync(dbDataSource, roleId, roleName);
        await InsertRoleClaimsAsync(dbDataSource, roleId, new() { { claim1Type, claim1Value }, { claim2Type, claim2Value } });

        _sut = new(dbDataSource, _options);

        // Act
        var claims = await _sut.GetClaimsAsync(new IdentityRole { Id = Guid.NewGuid().ToString() });

        // Assert
        claims.Should().BeEmpty();
    }

    #endregion

    #region AddClaimAsync

    [Fact]
    public async Task AddClaimAsync_WithValidRoleAndClaim_AddsClaim()
    {
        // Arrange
        var roleId = Guid.NewGuid().ToString();
        var roleName = "role";
        var claimType = "type";
        var claimValue = "value";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertRoleAsync(dbDataSource, roleId, roleName);

        _sut = new(dbDataSource, _options);

        // Act
        await _sut.AddClaimAsync(new IdentityRole { Id = roleId, Name = roleName }, new Claim(claimType, claimValue));

        // Assert
        var claim = await QueryClaimAsync(dbDataSource, roleId, claimType);
        claim.Should().NotBeNull();
        claim!.ClaimValue.Should().Be(claimValue);
    }

    #endregion

    #region RemoveClaimAsync

    [Fact]
    public async Task RemoveClaimAsync_WithValidRoleAndClaim_RemovesClaim()
    {
        // Arrange
        var roleId = Guid.NewGuid().ToString();
        var roleName = "role";
        var claimType = "type";
        var claimValue = "value";

        await using var dbDataSource = _dbDataSourceFactory.Create();
        await InsertRoleAsync(dbDataSource, roleId, roleName);
        await InsertRoleClaimAsync(dbDataSource, roleId, claimType, claimValue);

        _sut = new(dbDataSource, _options);

        // Act
        await _sut.RemoveClaimAsync(new IdentityRole { Id = roleId, Name = roleName }, new Claim(claimType, claimValue));

        // Assert
        var claim = await QueryClaimAsync(dbDataSource, roleId, claimType);
        claim.Should().BeNull();
    }

    #endregion

    #endregion

    #region Helper Methods

    private static async Task<IdentityRole?> QueryRoleAsync(DbDataSource dbDataSource, string roleId)
    {
        await using var connection = dbDataSource.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<IdentityRole>(
            """
                    SELECT * FROM identity.roles WHERE id = @roleId
                    """,
            new { roleId });
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


    private static async Task<IdentityRoleClaim<string>?> QueryClaimAsync(DbDataSource dbDataSource, string roleId, string claimType)
    {
        await using var connection = dbDataSource.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<IdentityRoleClaim<string>>(
            """
                    SELECT * FROM identity.role_claims WHERE role_id = @roleId AND claim_type = @claimType
                    """,
            new { roleId, claimType });
    }

    private static Task InsertRoleClaimAsync(DbDataSource dbDataSource, string roleId, string claimType, string claimValue)
    {
        return InsertRoleClaimsAsync(dbDataSource, roleId, new() { { claimType, claimValue } });
    }

    private static async Task InsertRoleClaimsAsync(DbDataSource dbDataSource, string roleId, Dictionary<string, string> claims)
    {
        await using var connection = dbDataSource.CreateConnection();

        var sqlBuilder = new StringBuilder("INSERT INTO identity.role_claims (role_id, claim_type, claim_value) VALUES ");
        var parameters = new DynamicParameters(new { roleId });

        int counter = 0;
        foreach (var (claimType, claimValue) in claims)
        {
            if (counter > 0)
            {
                sqlBuilder.AppendLine(",");
            }
            sqlBuilder.Append($"(@roleId, @claimType{counter}, @claimValue{counter})");

            parameters.Add($"claimType{counter}", claimType);
            parameters.Add($"claimValue{counter}", claimValue);

            counter++;
        }

        await connection.ExecuteAsync(sqlBuilder.ToString(), parameters);
    }

    #endregion

}
