using System.Text.Json;
using FluentAssertions;

namespace Hope.Identity.Dapper.Tests;

public class DapperStoreOptionsTests
{
    [Fact]
    public void DapperStoreOptions_ShouldInitializeWithDefaultValues()
    {
        // Act
        var options = new DapperStoreOptions();

        // Assert
        options.TableSchema.Should().BeNull();
        options.TableNamingPolicy.Should().BeNull();
        options.UserNames.Table.Should().Be("Users");
        options.UserLoginNames.Table.Should().Be("UserLogins");
        options.UserClaimNames.Table.Should().Be("UserClaims");
        options.UserTokenNames.Table.Should().Be("UserTokens");
        options.UserRoleNames.Table.Should().Be("UserRoles");
        options.RoleNames.Table.Should().Be("Roles");
        options.RoleClaimNames.Table.Should().Be("RoleClaims");
        options.ExtraUserInsertProperties.Should().BeEmpty();
        options.ExtraUserUpdateProperties.Should().BeEmpty();
        options.ExtraRoleInsertProperties.Should().BeEmpty();
        options.ExtraRoleUpdateProperties.Should().BeEmpty();
    }

    [Fact]
    public void DapperStoreOptions_ShouldApplyTableNamingPolicy()
    {
        // Arrange
        var options = new DapperStoreOptions();
        var namingPolicy = JsonNamingPolicy.CamelCase;

        // Act
        options.TableNamingPolicy = namingPolicy;

        // Assert
        options.UserNames.Table.Should().Be("users");
        options.UserLoginNames.Table.Should().Be("userLogins");
        options.UserClaimNames.Table.Should().Be("userClaims");
        options.UserTokenNames.Table.Should().Be("userTokens");
        options.UserRoleNames.Table.Should().Be("userRoles");
        options.RoleNames.Table.Should().Be("roles");
        options.RoleClaimNames.Table.Should().Be("roleClaims");
        options.UserNames.Id.Should().Be("id");
        options.UserNames.NormalizedEmail.Should().Be("normalizedEmail");
        options.UserNames.NormalizedUserName.Should().Be("normalizedUserName");
        options.UserLoginNames.UserId.Should().Be("userId");
        options.UserLoginNames.LoginProvider.Should().Be("loginProvider");
        options.UserLoginNames.ProviderKey.Should().Be("providerKey");
        options.UserLoginNames.ProviderDisplayName.Should().Be("providerDisplayName");
        options.UserClaimNames.UserId.Should().Be("userId");
        options.UserClaimNames.ClaimType.Should().Be("claimType");
        options.UserClaimNames.ClaimValue.Should().Be("claimValue");
        options.UserTokenNames.UserId.Should().Be("userId");
        options.UserTokenNames.LoginProvider.Should().Be("loginProvider");
        options.UserTokenNames.Name.Should().Be("name");
        options.UserTokenNames.Value.Should().Be("value");
        options.UserRoleNames.UserId.Should().Be("userId");
        options.UserRoleNames.RoleId.Should().Be("roleId");
        options.RoleNames.Id.Should().Be("id");
        options.RoleNames.Name.Should().Be("name");
        options.RoleNames.NormalizedName.Should().Be("normalizedName");
        options.RoleClaimNames.RoleId.Should().Be("roleId");
        options.RoleClaimNames.ClaimType.Should().Be("claimType");
        options.RoleClaimNames.ClaimValue.Should().Be("claimValue");
    }

    [Fact]
    public void DapperStoreOptions_WithCustomNames_ShouldApplyTableNamingPolicyForDefaultsOnly()
    {
        // Arrange
        var options = new DapperStoreOptions();
        var namingPolicy = JsonNamingPolicy.CamelCase;

        // Act
        options.UserNames.Table = "Custom_Users";
        options.UserNames.NormalizedUserName = "custom-normalized-user-name";
        options.RoleClaimNames.ClaimType = "CUSTOM_CLAIM_TYPE";
        options.TableNamingPolicy = namingPolicy;

        // Assert
        options.UserNames.Table.Should().Be("Custom_Users");
        options.UserNames.NormalizedUserName.Should().Be("custom-normalized-user-name");
        options.RoleClaimNames.ClaimType.Should().Be("CUSTOM_CLAIM_TYPE");

        options.UserLoginNames.Table.Should().Be("userLogins");
        options.UserClaimNames.Table.Should().Be("userClaims");
        options.UserTokenNames.Table.Should().Be("userTokens");
        options.UserRoleNames.Table.Should().Be("userRoles");
        options.RoleNames.Table.Should().Be("roles");
        options.RoleClaimNames.Table.Should().Be("roleClaims");
        options.UserNames.Id.Should().Be("id");
        options.UserNames.NormalizedEmail.Should().Be("normalizedEmail");
        options.UserLoginNames.UserId.Should().Be("userId");
        options.UserLoginNames.LoginProvider.Should().Be("loginProvider");
        options.UserLoginNames.ProviderKey.Should().Be("providerKey");
        options.UserLoginNames.ProviderDisplayName.Should().Be("providerDisplayName");
        options.UserClaimNames.UserId.Should().Be("userId");
        options.UserClaimNames.ClaimType.Should().Be("claimType");
        options.UserClaimNames.ClaimValue.Should().Be("claimValue");
        options.UserTokenNames.UserId.Should().Be("userId");
        options.UserTokenNames.LoginProvider.Should().Be("loginProvider");
        options.UserTokenNames.Name.Should().Be("name");
        options.UserTokenNames.Value.Should().Be("value");
        options.UserRoleNames.UserId.Should().Be("userId");
        options.UserRoleNames.RoleId.Should().Be("roleId");
        options.RoleNames.Id.Should().Be("id");
        options.RoleNames.Name.Should().Be("name");
        options.RoleNames.NormalizedName.Should().Be("normalizedName");
        options.RoleClaimNames.RoleId.Should().Be("roleId");
        options.RoleClaimNames.ClaimValue.Should().Be("claimValue");
    }

    [Fact]
    public void DapperStoreOptions_ShouldThrowInvalidOperationException_WhenTableNamingPolicyIsChanged()
    {
        // Arrange
        var options = new DapperStoreOptions();
        var namingPolicy = JsonNamingPolicy.CamelCase;

        // Act
        options.TableNamingPolicy = namingPolicy;
        Action act = () => options.TableNamingPolicy = JsonNamingPolicy.SnakeCaseLower;

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("The naming policy cannot be changed after it has been set.");
    }
}
