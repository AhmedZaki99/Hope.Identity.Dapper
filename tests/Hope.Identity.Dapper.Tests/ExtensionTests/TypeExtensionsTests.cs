using FluentAssertions;

namespace Hope.Identity.Dapper.Tests;

public class TypeExtensionsTests
{
    [Fact]
    public void HasGenericBaseType_ShouldReturnTrue_WhenTypeHasGenericBaseType()
    {
        // Arrange
        var currentType = typeof(DerivedClass);
        var genericBaseType = typeof(BaseClass<>);

        // Act
        var result = TypeExtensions.HasGenericBaseType(currentType, genericBaseType);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasGenericBaseType_ShouldReturnFalse_WhenTypeDoesNotHaveGenericBaseType()
    {
        // Arrange
        var currentType = typeof(NonDerivedClass);
        var genericBaseType = typeof(BaseClass<>);

        // Act
        var result = TypeExtensions.HasGenericBaseType(currentType, genericBaseType);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void FindGenericBaseType_ShouldReturnGenericBaseType_WhenTypeHasGenericBaseType()
    {
        // Arrange
        var currentType = typeof(DerivedClass);
        var genericBaseType = typeof(BaseClass<>);

        // Act
        var result = TypeExtensions.FindGenericBaseType(currentType, genericBaseType);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be<BaseClass<int>>();
    }

    [Fact]
    public void FindGenericBaseType_ShouldReturnNull_WhenTypeDoesNotHaveGenericBaseType()
    {
        // Arrange
        var currentType = typeof(NonDerivedClass);
        var genericBaseType = typeof(BaseClass<>);

        // Act
        var result = TypeExtensions.FindGenericBaseType(currentType, genericBaseType);

        // Assert
        result.Should().BeNull();
    }

    // Helper classes for testing
    private class BaseClass<T> { }
    private class DerivedClass : BaseClass<int> { }
    private class NonDerivedClass { }
}
