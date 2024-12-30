using System.Text.Json;
using FluentAssertions;
using NSubstitute;

namespace Hope.Identity.Dapper.Tests;

public class SqlQueryExtensionsTests
{
    [Fact]
    public void BuildSqlColumnsBlock_ShouldReturnColumnsInParentheses()
    {
        // Arrange
        var propertyNames = new[] { "Id", "UserName", "NormalizedUserName" };
        var expectedNames = new[] { "id", "user_name", "normalized_user_name" };
        var expected = $"({string.Join(", ", expectedNames)})";

        var namingPolicy = Substitute.For<JsonNamingPolicy>();
        for (int i = 0; i < propertyNames.Length; i++)
        {
            namingPolicy.ConvertName(propertyNames[i]).Returns(expectedNames[i]);
        }


        // Act
        var result = SqlQueryExtensions.BuildSqlColumnsBlock(propertyNames, namingPolicy);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void BuildSqlColumnsBlock_ShouldReturnColumnsInParenthesesWithNewLines()
    {
        // Arrange
        var propertyNames = new[] { "Id", "UserName", "NormalizedUserName" };
        var expectedNames = new[] { "id", "user_name", "normalized_user_name" };
        var expected = 
            $"""
            (
                {expectedNames[0]},
                {expectedNames[1]},
                {expectedNames[2]})
            """;
        expected = expected.Replace("\n", Environment.NewLine);

        var namingPolicy = Substitute.For<JsonNamingPolicy>();
        for (int i = 0; i < propertyNames.Length; i++)
        {
            namingPolicy.ConvertName(propertyNames[i]).Returns(expectedNames[i]);
        }


        // Act
        var result = SqlQueryExtensions.BuildSqlColumnsBlock(propertyNames, namingPolicy, true);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void BuildSqlParametersBlock_ShouldReturnParametersInParentheses()
    {
        // Arrange
        var propertyNames = new[] { "Id", "UserName", "NormalizedUserName" };
        var expected = "(@Id, @UserName, @NormalizedUserName)";

        // Act
        var result = SqlQueryExtensions.BuildSqlParametersBlock(propertyNames);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void BuildSqlParametersBlock_ShouldReturnParametersInParenthesesWithNewLines()
    {
        // Arrange
        var propertyNames = new[] { "Id", "UserName", "NormalizedUserName" };
        var expected = 
            $"""
            (
                @Id,
                @UserName,
                @NormalizedUserName)
            """;
        expected = expected.Replace("\n", Environment.NewLine);


        // Act
        var result = SqlQueryExtensions.BuildSqlParametersBlock(propertyNames, true);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToSqlColumn_ShouldReturnConvertedColumnName()
    {
        // Arrange
        var propertyName = "UserName";
        var expected = "user_name";
        var namingPolicy = Substitute.For<JsonNamingPolicy>();
        namingPolicy.ConvertName(propertyName).Returns(expected);

        // Act
        var result = SqlQueryExtensions.ToSqlColumn(propertyName, namingPolicy);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToSqlAssignment_ShouldReturnSqlAssignment()
    {
        // Arrange
        var propertyName = "UserName";
        var expectedColumnName = "user_name";
        var expected = $"{expectedColumnName} = @UserName";
        var namingPolicy = Substitute.For<JsonNamingPolicy>();
        namingPolicy.ConvertName(propertyName).Returns(expectedColumnName);

        // Act
        var result = SqlQueryExtensions.ToSqlAssignment(propertyName, namingPolicy);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void TryConvertName_ShouldReturnConvertedName()
    {
        // Arrange
        var name = "UserName";
        var expected = "user_name";
        var namingPolicy = Substitute.For<JsonNamingPolicy>();
        namingPolicy.ConvertName(name).Returns(expected);

        // Act
        var result = SqlQueryExtensions.TryConvertName(namingPolicy, name);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void TryConvertName_ShouldReturnOriginalNameIfNamingPolicyIsNull()
    {
        // Arrange
        var name = "UserName";

        // Act
        var result = SqlQueryExtensions.TryConvertName(null, name);

        // Assert
        result.Should().Be(name);
    }
}
