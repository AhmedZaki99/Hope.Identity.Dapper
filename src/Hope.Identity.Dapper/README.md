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
        options.UserNames.Table = "app_users";

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

### DapperUserStoreBase

The `DapperUserStoreBase` class provides a base implementation for a Dapper-based Identity user store. Below is an example of how to use it.

```c#
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<CustomUser, IdentityRole<Guid>>()
    .AddUserStore<CustomUserStore>();
    .AddRoleStore<DapperRoleStore<IdentityRole<Guid>, Guid>>();

var app = builder.Build();

...

public class CustomUser : IdentityUser<Guid>
{
    public DateTime CreatedOn { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class CustomUserStore : DapperUserStoreBase<CustomUser, IdentityRole<Guid>, Guid>
{
    protected override JsonNamingPolicy? TableNamingPolicy { get; } = JsonNamingPolicy.SnakeCaseLower;

    public CustomUserStore(DbDataSource dbDataSource, IdentityErrorDescriber? describer)
        : base(dbDataSource, describer) { }


    protected override string[] GetUserInsertProperties(string[] identityUserInsertProperties)
    {
        return [.. identityUserInsertProperties, nameof(CustomUser.CreatedOn), nameof(CustomUser.FirstName), nameof(CustomUser.LastName)];
    }

    protected override string[] GetUserUpdateProperties(string[] identityUserUpdateProperties)
    {
        return [.. identityUserUpdateProperties, nameof(CustomUser.FirstName), nameof(CustomUser.LastName)];
    }
}
```


### DapperRoleStore and DapperRoleStoreBase

The package also provides `DapperRoleStore` and `DapperRoleStoreBase` classes for managing roles following the same concept for user stores.


## Documentation

For more information, please refer to the **[ASP.NET Core Identity documentation](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)** and **[Dapper documentation](https://github.com/DapperLib/Dapper)**.