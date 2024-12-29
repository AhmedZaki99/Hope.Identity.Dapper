namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the table and column names for a specific table.
/// </summary>
public abstract class TableNames
{
    /// <summary>
    /// Gets or sets the name of the table.
    /// </summary>
    public string Table { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TableNames"/> class.
    /// </summary>
    /// <param name="table">The name of the table.</param>
    protected TableNames(string table)
    {
        Table = table;
    }


    /// <summary>
    /// Applies a naming conversion to the default table and column names.
    /// </summary>
    /// <param name="convertFunction">The function to apply to the names.</param>
    internal abstract void ApplyNamingConversionToDefaults(Func<string, string> convertFunction);


    /// <summary>
    /// Converts the given name using a convert function if it's set to the given default name.
    /// </summary>
    /// <param name="name">The name to convert.</param>
    /// <param name="defaultName">The default name to compare against.</param>
    /// <param name="convertFunction">The function to apply to the default name.</param>
    /// <returns>The converted name if it was the default name, otherwise the original name.</returns>
    protected static string ConvertIfDefault(string name, string defaultName, Func<string, string> convertFunction)
    {
        if (name == defaultName)
        {
            return convertFunction(name);
        }
        return name;
    }
}
