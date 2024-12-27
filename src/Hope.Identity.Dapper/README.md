# Hope.Identity.Dapper

**Hope.Identity.Dapper** is a .NET Core Identity provider that uses Dapper for data access, providing a lightweight and efficient way to manage identity data in SQL databases.


## Usage

### DapperUserStore

The `DapperUserStore` class provides an implementation for a Dapper-based Identity user store. Below are examples of how to use it.

#### Using Identity User

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddUserStore<DapperUserStore>()
    .AddRoleStore<DapperRoleStore>();

var app = builder.Build();
...
```

#### Using Custom User

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<CustomUser, IdentityRole<Guid>>()
    .AddRoleStore<DapperRoleStore<IdentityRole<Guid>, Guid>>();

builder.Services.AddScoped<DapperUserStore<CustomUser, IdentityRole<Guid>, Guid>>(sp =>
    new(sp.GetRequiredService<DbDataSource>(), sp.GetService<IdentityErrorDescriber>(), JsonNamingPolicy.SnakeCaseLower)
    {
        ExtraUserInsertProperties = [nameof(CustomUser.CreatedOn), nameof(CustomUser.FirstName), nameof(CustomUser.LastName)],
        ExtraUserUpdateProperties = [nameof(CustomUser.FirstName), nameof(CustomUser.LastName)]
    });

builder.Services.AddScoped<IUserStore<CustomUser>>(sp =>
    sp.GetRequiredService<DapperUserStore<CustomUser, IdentityRole<Guid>, Guid>>());

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

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<CustomUser, IdentityRole<Guid>>()
    .AddRoleStore<DapperRoleStore<IdentityRole<Guid>, Guid>>();

builder.Services.AddScoped<CustomUserStore>();
builder.Services.AddScoped<IUserStore<CustomUser>>(sp => sp.GetRequiredService<CustomUserStore>());

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