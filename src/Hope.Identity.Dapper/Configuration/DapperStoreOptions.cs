using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Hope.Identity.Dapper;

/// <summary>
/// Represents the options for configuring the Dapper-based identity stores.
/// </summary>
public class DapperStoreOptions
{
    /// <summary>
    /// Gets or sets the database schema to use for the identity tables.
    /// </summary>
    public string? TableSchema { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="JsonNamingPolicy"/> used to convert property names to SQL table names and/or column names (<see langword="null"/> for no conversion).
    /// </summary>
    /// <remarks>
    /// For example, a naming policy like <see cref="JsonNamingPolicy.SnakeCaseLower"/> will build a query like this:
    /// <code>
    /// INSERT INTO users (id, user_name, normalized_user_name, email, normalized_email, ...) 
    /// VALUES (@Id, @UserName, @NormalizedUserName, @Email, @NormalizedEmail, ...)
    /// </code>
    /// </remarks>
    public JsonNamingPolicy? TableNamingPolicy 
    {
        get;
        set 
        {
            if (value == field)
            {
                return;
            }
            if (field is not null)
            {
                throw new InvalidOperationException("The naming policy cannot be changed after it has been set.");
            }
            field = value;

            foreach (var tableNames in EnumerateTables())
            {
                tableNames.ApplyNamingConversionToDefaults(value!.ConvertName);
            }
        }
    }


    /// <summary>
    /// Gets or sets the SQL table and column names used for the users table extending <see cref="IdentityUser{TKey}"/>.
    /// </summary>
    /// <remarks>
    /// The default names are generated using the <see cref="TableNamingPolicy"/> for <see cref="IdentityUser{TKey}"/> properties and a table name "Users".
    /// </remarks>
    public UserTableNames UserNames { get; set; }

    /// <summary>
    /// Gets or sets the SQL table and column names used for the user logins table extending <see cref="IdentityUserLogin{TKey}"/>.
    /// </summary>
    /// <remarks>
    /// The default names are generated using the <see cref="TableNamingPolicy"/> for <see cref="IdentityUserLogin{TKey}"/> properties and a table name "UserLogins".
    /// </remarks>
    public UserLoginTableNames UserLoginNames { get; set; }

    /// <summary>
    /// Gets or sets the SQL table and column names used for the user claims table extending <see cref="IdentityUserClaim{TKey}"/>.
    /// </summary>
    /// <remarks>
    /// The default names are generated using the <see cref="TableNamingPolicy"/> for <see cref="IdentityUserClaim{TKey}"/> properties and a table name "UserClaims".
    /// </remarks>
    public UserClaimTableNames UserClaimNames { get; set; }

    /// <summary>
    /// Gets or sets the SQL table and column names used for the user tokens table extending <see cref="IdentityUserToken{TKey}"/>.
    /// </summary>
    /// <remarks>
    /// The default names are generated using the <see cref="TableNamingPolicy"/> for <see cref="IdentityUserToken{TKey}"/> properties and a table name "UserTokens".
    /// </remarks>
    public UserTokenTableNames UserTokenNames { get; set; }

    /// <summary>
    /// Gets or sets the SQL table and column names used for the user roles table extending <see cref="IdentityUserRole{TKey}"/>.
    /// </summary>
    /// <remarks>
    /// The default names are generated using the <see cref="TableNamingPolicy"/> for <see cref="IdentityUserRole{TKey}"/> properties and a table name "UserRoles".
    /// </remarks>
    public UserRoleTableNames UserRoleNames { get; set; }

    /// <summary>
    /// Gets or sets the SQL table and column names used for the roles table extending <see cref="IdentityRole{TKey}"/>.
    /// </summary>
    /// <remarks>
    /// The default names are generated using the <see cref="TableNamingPolicy"/> for <see cref="IdentityRole{TKey}"/> properties and a table name "Roles".
    /// </remarks>
    public RoleTableNames RoleNames { get; set; }

    /// <summary>
    /// Gets or sets the SQL table and column names used for the role claims table extending <see cref="IdentityRoleClaim{TKey}"/>.
    /// </summary>
    /// <remarks>
    /// The default names are generated using the <see cref="TableNamingPolicy"/> for <see cref="IdentityRoleClaim{TKey}"/> properties and a table name "RoleClaims".
    /// </remarks>
    public RoleClaimTableNames RoleClaimNames { get; set; }


    /// <summary>
    /// Gets or sets a dictionary of the extra properties within the specified user type used for insert queries (excluding the base <see cref="IdentityUser{TKey}"/> properties).
    /// </summary>
    /// <remarks>
    /// <para>Keys represent the user type property names and values are the corresponding column names.</para>
    /// <para>Set values to null to use the provided <see cref="TableNamingPolicy"/> for property name conversion.</para>
    /// </remarks>
    public Dictionary<string, string?> ExtraUserInsertProperties { get; set; }

    /// <summary>
    /// Gets or sets a dictionary of the extra properties within the specified user type used for update queries (excluding the base <see cref="IdentityUser{TKey}"/> properties).
    /// </summary>
    /// <remarks>
    /// <para>Keys represent the user type property names and values are the corresponding column names.</para>
    /// <para>Set values to null to use the provided <see cref="TableNamingPolicy"/> for property name conversion.</para>
    /// </remarks>
    public Dictionary<string, string?> ExtraUserUpdateProperties { get; set; }

    /// <summary>
    /// Gets or sets a dictionary of the extra properties within the specified role type used for insert queries (excluding the base <see cref="IdentityRole{TKey}"/> properties).
    /// </summary>
    /// <remarks>
    /// <para>Keys represent the role type property names and values are the corresponding column names.</para>
    /// <para>Set values to null to use the provided <see cref="TableNamingPolicy"/> for property name conversion.</para>
    /// </remarks>
    public Dictionary<string, string?> ExtraRoleInsertProperties { get; set; }

    /// <summary>
    /// Gets or sets a dictionary of the extra properties within the specified role type used for update queries (excluding the base <see cref="IdentityRole{TKey}"/> properties).
    /// </summary>
    /// <remarks>
    /// <para>Keys represent the role type property names and values are the corresponding column names.</para>
    /// <para>Set values to null to use the provided <see cref="TableNamingPolicy"/> for property name conversion.</para>
    /// </remarks>
    public Dictionary<string, string?> ExtraRoleUpdateProperties { get; set; }


    /// <summary>
    /// Creates a new instance of <see cref="DapperStoreOptions"/>.
    /// </summary>
    public DapperStoreOptions()
    {
        UserNames = new();
        UserLoginNames = new();
        UserClaimNames = new();
        UserTokenNames = new();
        UserRoleNames = new();
        RoleNames = new();
        RoleClaimNames = new();

        ExtraUserInsertProperties = [];
        ExtraUserUpdateProperties = [];
        ExtraRoleInsertProperties = [];
        ExtraRoleUpdateProperties = [];
    }


    /// <summary>
    /// Enumerates all the table names used by the identity stores
    /// </summary>
    /// <returns>An enumerable of all the table names used by the identity stores</returns>
    public IEnumerable<TableNames> EnumerateTables()
    {
        yield return UserNames;
        yield return UserLoginNames;
        yield return UserClaimNames;
        yield return UserTokenNames;
        yield return UserRoleNames;
        yield return RoleNames;
        yield return RoleClaimNames;
    }
}
