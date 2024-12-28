namespace Hope.Identity.Dapper;

/// <summary>
/// Provides extension methods for <see cref="Type"/>.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Determines whether the specified type has a generic base type.
    /// </summary>
    /// <param name="currentType">The type to inspect.</param>
    /// <param name="genericBaseType">The generic base type to find.</param>
    /// <returns><c>true</c> if the type has a generic base type; otherwise, <c>false</c>.</returns>
    public static bool HasGenericBaseType(this Type currentType, Type genericBaseType)
    {
        return FindGenericBaseType(currentType, genericBaseType) is not null;
    }

    /// <summary>
    /// Finds the generic base type of the specified type.
    /// </summary>
    /// <param name="currentType">The type to inspect.</param>
    /// <param name="genericBaseType">The generic base type to find.</param>
    /// <returns>The generic base type if found; otherwise, <c>null</c>.</returns>
    public static Type? FindGenericBaseType(this Type currentType, Type genericBaseType)
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
