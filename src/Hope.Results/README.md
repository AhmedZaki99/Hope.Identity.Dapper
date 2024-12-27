# Hope.Results

**Hope.Results** is an extension for [Altmann's](https://github.com/altmann) [FluentResults](https://github.com/altmann/FluentResults) that provides common error and result types.


## Usage Examples

Error models:

```c#
public Result DoSomething()
{
    // Do something
    if (validationFailed)
    {
        return new ValidationError("Value must be ...");
    }
    if (notFound)
    {
        return new NotFoundError("Id not found");
    }
    return Result.Ok();
}

public void CallDoSomething()
{
    var result = DoSomething();
    if (result.IsSuccess)
    {
        Console.WriteLine("Success");
        return;
    }

    if (result.HasError<ValidationError>())
    {
        var error = result.Errors.First();
        Console.WriteLine($"Validation error: {error.Message}");
    }
    if (result.HasError<NotFoundError>())
    {
        var error = result.Errors.First();
        Console.WriteLine($"Not found error: {error.Message}");
    }
}
```

Pagination:

```c#
public async Task<PagedList<Book>> ListAsync(PaginationParams? paginationParams = null, CancellationToken cancellationToken = default)
{
    var tenantId = _multiTenancyContext.CurrentTenantId;
    await using var connection = await _dbDataSource.OpenConnectionAsync(cancellationToken);

    paginationParams ??= new();

    var condition = "tenant_id = @tenantId";
    var sql =
        $"""
        SELECT COUNT(*) FROM books
        WHERE {condition};

        SELECT * FROM books 
        WHERE {condition}
        ORDER BY created_on_utc DESC
        LIMIT @pageSize OFFSET @offset;
        """;

    var param = new
    {
        tenantId,
        pageSize = paginationParams.PageSize,
        offset = paginationParams.Offset
    };

    using var multi = await connection.QueryMultipleAsync(sql, param);

    var count = await multi.ReadFirstAsync<int>();
    var items = await multi.ReadAsync<Book>();

    return new PagedList<Book>
    {
        PageNumber = paginationParams.PageNumber,
        PageSize = paginationParams.PageSize,
        TotalCount = count,
        Items = items.ToList()
    };
}
```

## Documentation

For more information, please refer to the **[FluentResults documentation](https://github.com/altmann/FluentResults/blob/master/README.md)**.