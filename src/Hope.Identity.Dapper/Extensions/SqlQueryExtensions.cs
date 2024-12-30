using System.Text.Json;

namespace Hope.Identity.Dapper;

/// <summary>
/// Extensions of string values and string arrays for building SQL queries.
/// </summary>
public static class SqlQueryExtensions
{
    private static readonly string _newLine = Environment.NewLine;
    private static readonly string _lineIndentation = string.Concat(Enumerable.Repeat(" ", 4));

    /// <summary>
    /// Converts a given set of property names to the corresponding SQL column names wrapped in parentheses.
    /// </summary>
    /// <remarks>
    /// Example usage:
    /// <code>
    /// ["Id", "UserName", "NormalizedUserName"].BuildSqlColumnsBlock(JsonNamingPolicy.SnakeCaseLower);
    /// 
    /// // Output:
    /// // (id, user_name, normalized_user_name)
    /// </code>
    /// </remarks>
    /// <param name="propertyNames">The property names to convert to SQL column names.</param>
    /// <param name="namingPolicy">The naming policy to use for converting the property names to column names (<see langword="null"/> for no conversion).</param>
    /// <param name="insertLines">Whether to insert new lines between each column name.</param>
    /// <returns>The SQL columns block.</returns>
    public static string BuildSqlColumnsBlock(this IEnumerable<string> propertyNames, JsonNamingPolicy? namingPolicy = null, bool insertLines = false)
    {
        var queryProperties = propertyNames.Select(namingPolicy.TryConvertName);
        if (insertLines)
        {
            queryProperties = queryProperties.Select(name => $"{_lineIndentation}{name}");
            return
                $"""
                (
                {string.Join("," + _newLine, queryProperties)})
                """;
        }
        return $"({string.Join(", ", queryProperties)})";
    }

    /// <summary>
    /// Converts a given set of property names to the corresponding SQL parameter names wrapped in parentheses.
    /// </summary>
    /// <remarks>
    /// Example usage:
    /// <code>
    /// ["Id", "UserName", "NormalizedUserName"].BuildSqlParametersBlock();
    /// 
    /// // Output:
    /// // (@Id, @UserName, @NormalizedUserName)
    /// </code>
    /// </remarks>
    /// <param name="propertyNames">The property names to convert to SQL parameter names.</param>
    /// <param name="insertLines">Whether to insert new lines between each parameter name.</param>
    /// <param name="prefix">The prefix to add to each parameter name.</param>
    /// <param name="suffix">The suffix to add to each parameter name.</param>
    /// <returns>The SQL parameters block.</returns>
    public static string BuildSqlParametersBlock(this IEnumerable<string> propertyNames, bool insertLines = false, string prefix = "@", string suffix = "")
    {
        var queryParameters = propertyNames.Select(name => $"{prefix}{name}{suffix}");
        if (insertLines)
        {
            queryParameters = queryParameters.Select(name => $"{_lineIndentation}{name}");
            return
                $"""
                (
                {string.Join("," + _newLine, queryParameters)})
                """;
        }
        return $"({string.Join(", ", queryParameters)})";
    }


    /// <summary>
    /// Converts a property name to the corresponding SQL column using the provided <paramref name="namingPolicy"/>.
    /// </summary>
    /// <param name="propertyName">The property name to convert to a SQL column name.</param>
    /// <param name="namingPolicy">The naming policy to use for converting the property name to a column name (<see langword="null"/> for no conversion).</param>
    /// <returns>The SQL column name.</returns>
    public static string ToSqlColumn(this string propertyName, JsonNamingPolicy? namingPolicy = null)
    {
        return namingPolicy.TryConvertName(propertyName);
    }

    /// <summary>
    /// Converts a property name to the corresponding SQL parameter assignment using the provided <paramref name="namingPolicy"/>.
    /// </summary>
    /// <remarks>
    /// Example usage:
    /// <code>
    /// "UserName".ToSqlAssignment(JsonNamingPolicy.SnakeCaseLower);
    /// 
    /// // Output:
    /// // user_name = @UserName
    /// </code>
    /// </remarks>
    /// <param name="propertyName">The property name to convert to a SQL column name.</param>
    /// <param name="namingPolicy">The naming policy to use for converting the property name to a column name (<see langword="null"/> for no conversion).</param>
    /// <param name="parameterPrefix">The prefix to add to the parameter name.</param>
    /// <returns>The SQL assignment.</returns>
    public static string ToSqlAssignment(this string propertyName, JsonNamingPolicy? namingPolicy = null, string parameterPrefix = "@")
    {
        return $"{propertyName.ToSqlColumn(namingPolicy)} = {parameterPrefix}{propertyName}";
    }


    /// <summary>
    /// Try to convert the specified name using the provided <paramref name="namingPolicy"/>, or if <see langword="null"/> return the name without conversion.
    /// </summary>
    /// <param name="namingPolicy">The naming policy to use for converting the name (<see langword="null"/> for no conversion).</param>
    /// <param name="name">The name to convert.</param>
    /// <returns>The converted name or the original name if the naming policy is <see langword="null"/>.</returns>
    public static string TryConvertName(this JsonNamingPolicy? namingPolicy, string name)
    {
        return namingPolicy?.ConvertName(name) ?? name;
    }
}
