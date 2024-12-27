namespace Hope.Identity.Dapper;

/// <summary>
/// A delegate that returns a SQL statement based on the specified table alias.
/// </summary>
/// <param name="tableAlias">The alias to use for the table.</param>
/// <returns>The SQL statement.</returns>
public delegate string SqlStatementDelegate(string? tableAlias = null);
