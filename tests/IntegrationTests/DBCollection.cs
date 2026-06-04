using Tests.Integration.Infra;

namespace Tests.Integration;

[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<WebAppFactory>
{
}
