using eShopEnhanced.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace eShopEnhanced.Tests.Integration
{
    // Starts the whole web app in memory (no real web server, no browser) for the integration tests.
    // The only thing swapped out is the database: instead of SQL Server / LocalDB it uses a
    // SQLite database that lives in memory, so the tests never touch the real database and also
    // run on Linux (GitHub Actions). Program.cs then seeds it like a fresh install (DbInitializer).
    public class BakeryWebApplicationFactory : WebApplicationFactory<Program>
    {
        // An in-memory SQLite database exists only while a connection to it is open,
        // so one connection stays open for as long as the factory (= the test run) lives.
        private readonly SqliteConnection _connection = new("DataSource=:memory:");

        public BakeryWebApplicationFactory()
        {
            _connection.Open();

            // Every client talks https, like the real site (no https-redirect surprises)
            ClientOptions.BaseAddress = new Uri("https://localhost");
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureServices(services =>
            {
                // Remove what AddDbContext<eShopEnhancedDbContext>(... UseSqlServer ...) registered in Program.cs.
                // Since EF Core 8 the options are built from IDbContextOptionsConfiguration<T> entries,
                // so those must go too, or SQL Server would still be configured next to SQLite.
                services.RemoveAll<DbContextOptions<eShopEnhancedDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<eShopEnhancedDbContext>>();

                services.AddDbContext<eShopEnhancedDbContext>(options =>
                {
                    options.UseSqlite(_connection);

                    // SQL Server compares text case-insensitively by default; SQLite doesn't.
                    // See SqliteCaseInsensitiveModelCustomizer.
                    options.ReplaceService<IModelCustomizer, SqliteCaseInsensitiveModelCustomizer>();
                });
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _connection.Dispose();
            }
        }
    }

    // All integration test classes share ONE factory (one app, one database), see
    // [Collection(IntegrationTestCollection.Name)]. Besides being faster, this matters because
    // DbInitializer keeps its seed objects in static fields: seeding a second database in the same
    // test run would re-use objects that already carry ids from the first one.
    [CollectionDefinition(Name)]
    public class IntegrationTestCollection : ICollectionFixture<BakeryWebApplicationFactory>
    {
        public const string Name = "Integration tests";
    }
}
