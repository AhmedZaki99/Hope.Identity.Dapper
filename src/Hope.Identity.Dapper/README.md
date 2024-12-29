# Hope.Identity.Dapper

**Hope.Identity.Dapper** is a .NET Core Identity provider that uses Dapper for data access, providing a lightweight and efficient way to manage identity data in SQL databases.


## Usage

### DapperUserStore

The `DapperUserStore` class provides an implementation for a Dapper-based Identity user store. Below are examples of how to use it.

#### Using Identity User

```c#
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddIdentityCore<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddDapperStores();

// OR

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddDapperStores();


var app = builder.Build();
...
```

#### Using Custom User

```c#
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityCore<CustomUser>()
    .AddRoles<IdentityRole<Guid>>()
    .AddDapperStores(options =>
    {
        options.TableSchema = "identity";
        options.TableNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.UserNames.Table = "custom_users";

        options.ExtraUserInsertProperties = [
            nameof(CustomUser.CreatedOn), nameof(CustomUser.FirstName), nameof(CustomUser.LastName)];

        options.ExtraUserUpdateProperties = [
            nameof(CustomUser.FirstName), nameof(CustomUser.LastName)];
    });

var app = builder.Build();

...

public class CustomUser : IdentityUser<Guid>
{
    public DateTime CreatedOn { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
```

#### Using Custom Implementation

```c#
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<CustomUser, IdentityRole<Guid>>()
    .AddDapperStores<CustomUserStore, DapperRoleStore<IdentityRole<Guid>, Guid>>(options =>
    {
        options.TableSchema = "identity";
        options.TableNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.UserNames.Table = "custom_users";

        options.ExtraUserInsertProperties = [
            nameof(CustomUser.CreatedOn), nameof(CustomUser.FirstName), nameof(CustomUser.LastName)];

        options.ExtraUserUpdateProperties = [
            nameof(CustomUser.FirstName), nameof(CustomUser.LastName)];
    });

var app = builder.Build();

...

public class CustomUser : IdentityUser<Guid>
{
    public DateTime CreatedOn { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsDeleted { get; set; }
}

public class CustomUserStore : DapperUserStore<CustomUser, IdentityRole<Guid>, Guid>
{
    private readonly string _tabelPrefix;
    private readonly string _isDeletedColumn;

    public CustomUserStore(DbDataSource dbDataSource, IOptions<DapperStoreOptions> options, IdentityErrorDescriber? describer)
        : base(dbDataSource, options, describer) 
    { 
        _tabelPrefix = Options.TableSchema is null ? string.Empty : $"{Options.TableSchema}.";
        _isDeletedColumn = nameof(CustomUser.IsDeleted).ToSqlColumn(Options.TableNamingPolicy);
    }


    public override string GetBaseUserSqlCondition(DynamicParameters sqlParameters, string tableAlias = "")
    {
        var columnPrefix = string.IsNullOrEmpty(tableAlias) ? string.Empty : $"{tableAlias}.";

        return $"{columnPrefix}{_isDeletedColumn} = FALSE";
    }

    public override async Task<IdentityResult> DeleteAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        await using var connection = await DbDataSource.OpenConnectionAsync(cancellationToken);

        var dynamicParams = new DynamicParameters(new { userId = user.Id });

        await connection.ExecuteAsync(
            $"""
            UPDATE {_tabelPrefix}{Options.UserNames.Table} SET {_isDeletedColumn} = TRUE 
            WHERE {GetBaseUserSqlCondition(dynamicParams)} 
            AND {Options.UserNames.Id} = @userId
            """,
            dynamicParams);

        return IdentityResult.Success;
    }
}
```


### DapperRoleStore

The package also provides `DapperRoleStore` class for managing roles following the same concept as the user store.


## Documentation

For more information, please refer to the **[ASP.NET Core Identity documentation](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)** and **[Dapper documentation](https://github.com/DapperLib/Dapper)**.