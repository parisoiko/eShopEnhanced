using eShopEnhanced.Models;
using eShopEnhanced.Repositories;
using Moq;

namespace eShopEnhanced.Tests.Mocks
{
    // Fake repositories with a small, fixed set of data, so controllers can be tested
    // without a database. Moq creates a class that "pretends" to implement the interface;
    // .Setup(...) says what each method should return.
    //
    // The sample data (2 categories, 3 subcategories, 4 products):
    //
    //   Category  SubCategory     Id  Product             Price   Pick of the week
    //   --------  --------------  --  ------------------  ------  ----------------
    //   Pies      Fruit pies       1  Strawberry Pie      15.95   no
    //   Pies      Fruit pies       2  Rhubarb Pie         15.95   yes
    //   Pies      Seasonal pies    4  Pumpkin Pie         12.95   yes
    //   Cakes     Chocolate cakes  7  Black Forest Cake   24.95   yes
    public static class RepositoryMocks
    {
        public static readonly Category Pies = new() { CategoryId = 1, CategoryName = "Pies" };
        public static readonly Category Cakes = new() { CategoryId = 2, CategoryName = "Cakes" };

        private static readonly SubCategory FruitPies = new() { SubCategoryId = 1, SubCategoryName = "Fruit pies", Category = Pies, CategoryId = 1 };
        private static readonly SubCategory SeasonalPies = new() { SubCategoryId = 2, SubCategoryName = "Seasonal pies", Category = Pies, CategoryId = 1 };
        private static readonly SubCategory ChocolateCakes = new() { SubCategoryId = 3, SubCategoryName = "Chocolate cakes", Category = Cakes, CategoryId = 2 };

        public static List<Product> Products => new()
        {
            new Product { ProductId = 1, Name = "Strawberry Pie", Price = 15.95M, SubCategory = FruitPies, SubCategoryId = 1, InStock = true },
            new Product { ProductId = 2, Name = "Rhubarb Pie", Price = 15.95M, SubCategory = FruitPies, SubCategoryId = 1, InStock = true, IsPickOfTheWeek = true },
            new Product { ProductId = 4, Name = "Pumpkin Pie", Price = 12.95M, SubCategory = SeasonalPies, SubCategoryId = 2, InStock = true, IsPickOfTheWeek = true },
            new Product { ProductId = 7, Name = "Black Forest Cake", Price = 24.95M, SubCategory = ChocolateCakes, SubCategoryId = 3, InStock = true, IsPickOfTheWeek = true }
        };

        public static Mock<IProductRepository> GetProductRepository()
        {
            var products = Products;
            var mock = new Mock<IProductRepository>();

            // Our repository methods are async, so the fakes use ReturnsAsync (the course's
            // synchronous repository uses .Returns)
            mock.Setup(r => r.GetAllProductsAsync()).ReturnsAsync(products);

            mock.Setup(r => r.GetPicksOfTheWeekAsync())
                .ReturnsAsync(products.Where(p => p.IsPickOfTheWeek).ToList());

            // It.IsAny<string>() = "for any argument"; the lambda receives the actual argument
            mock.Setup(r => r.GetProductsByCategoryAsync(It.IsAny<string>()))
                .ReturnsAsync((string categoryName) => products
                    .Where(p => p.SubCategory.Category.CategoryName == categoryName).ToList());

            mock.Setup(r => r.GetProductByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => products.FirstOrDefault(p => p.ProductId == id));

            return mock;
        }

        public static Mock<ICategoryRepository> GetCategoryRepository()
        {
            var categories = new List<Category> { Cakes, Pies };
            var mock = new Mock<ICategoryRepository>();

            mock.Setup(r => r.GetAllCategoriesAsync()).ReturnsAsync(categories);

            // Case-insensitive, like SQL Server: "cakes" finds "Cakes"
            mock.Setup(r => r.GetCategoryByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((string name) => categories
                    .FirstOrDefault(c => string.Equals(c.CategoryName, name, StringComparison.OrdinalIgnoreCase)));

            return mock;
        }
    }
}
