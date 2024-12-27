# Hope.Configuration.Extensions

**Hope.Configuration.Extensions** provides extension methods for [generic host](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host) and [web host](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host) [configurations](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration) following the [options pattern](https://learn.microsoft.com/en-us/dotnet/core/extensions/options).


## Usage

```c#

var builder = WebApplication.CreateBuilder(args);

builder
    .ConfigureOptions<EmailSenderOptions>(EmailSenderOptions.Key, out var emailSenderOptions);

emailSenderOptions.RequireProperty(o => o.SenderEmailAddress);

var jwtSigningKey = builder.Configuration.GetRequiredValue("JWT_SIGNING_KEY");

...
```
