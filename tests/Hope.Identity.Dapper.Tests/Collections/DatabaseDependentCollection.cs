namespace Hope.Identity.Dapper.Tests;


[CollectionDefinition(Name)]
public class DatabaseDependentCollection : ICollectionFixture<DbDataSourceFactory>
{
    public const string Name = "DatabaseDependent";
}
