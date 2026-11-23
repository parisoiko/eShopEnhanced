using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace eShopEnhanced.Tests.Integration
{
    // The shop relies on SQL Server's default case-insensitive text comparison:
    // /products/cakes finds the category "Cakes" with a plain CategoryName == "cakes" in
    // CategoryRepository. SQLite compares text case-sensitively, so in the tests every text column
    // gets SQLite's built-in NOCASE collation, which makes "=" behave like on SQL Server.
    // Only used by the integration tests; the web project's model is not changed.
    public class SqliteCaseInsensitiveModelCustomizer : RelationalModelCustomizer
    {
        public SqliteCaseInsensitiveModelCustomizer(ModelCustomizerDependencies dependencies)
            : base(dependencies)
        {
        }

        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            // First the app's own model (OnModelCreating), then the collation on top
            base.Customize(modelBuilder, context);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties().Where(p => p.ClrType == typeof(string)))
                {
                    property.SetCollation("NOCASE");
                }
            }
        }
    }
}
