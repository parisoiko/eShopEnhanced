namespace eShopEnhanced.Models
{
    public static class DbInitializer
    {
        public static void Seed(IApplicationBuilder applicationBuilder)
        {
            // The DbContext is a scoped service, so we create a scope to get one outside of a web request
            using var scope = applicationBuilder.ApplicationServices.CreateScope();
            eShopEnhancedDbContext context = scope.ServiceProvider.GetRequiredService<eShopEnhancedDbContext>();

            // Only seed an empty database, so restarting the app doesn't add duplicates
            if (!context.Categories.Any())
            {
                context.Categories.AddRange(Categories.Values);
            }

            if (!context.SubCategories.Any())
            {
                context.SubCategories.AddRange(SubCategories.Values);
            }

            if (!context.Pies.Any())
            {
                context.Pies.AddRange(
                    new Pie
                    {
                        Name = "Strawberry Pie",
                        Price = 15.95M,
                        ShortDescription = "Fresh strawberries on a crunchy base",
                        LongDescription = "Sweet, ripe strawberries piled high on a buttery shortcrust base with a light vanilla cream.",
                        SubCategory = SubCategories["Fruit pies"],
                        ImageUrl = "https://gillcleerenpluralsight.blob.core.windows.net/files/bethanyspieshop/fruitpies/strawberrypie.jpg",
                        ImageThumbnailUrl = "https://gillcleerenpluralsight.blob.core.windows.net/files/bethanyspieshop/fruitpies/strawberrypiesmall.jpg",
                        InStock = true,
                        IsPieOfTheWeek = false
                    },
                    new Pie
                    {
                        Name = "Cheese cake",
                        Price = 18.95M,
                        ShortDescription = "Creamy, rich and smooth",
                        LongDescription = "A classic baked cheesecake with a biscuit crust and a velvety cream cheese filling.",
                        AllergyInformation = "Contains dairy and gluten",
                        SubCategory = SubCategories["Cheese cakes"],
                        ImageUrl = "https://gillcleerenpluralsight.blob.core.windows.net/files/bethanyspieshop/cheesecakes/cheesecake.jpg",
                        ImageThumbnailUrl = "https://gillcleerenpluralsight.blob.core.windows.net/files/bethanyspieshop/cheesecakes/cheesecakesmall.jpg",
                        InStock = true,
                        IsPieOfTheWeek = false
                    },
                    new Pie
                    {
                        Name = "Rhubarb Pie",
                        Price = 15.95M,
                        ShortDescription = "Tangy rhubarb, just sweet enough",
                        LongDescription = "Tart rhubarb baked until soft in a golden pastry case, balanced with a touch of sugar.",
                        SubCategory = SubCategories["Fruit pies"],
                        ImageUrl = "https://gillcleerenpluralsight.blob.core.windows.net/files/bethanyspieshop/fruitpies/rhubarbpie.jpg",
                        ImageThumbnailUrl = "https://gillcleerenpluralsight.blob.core.windows.net/files/bethanyspieshop/fruitpies/rhubarbpiesmall.jpg",
                        InStock = true,
                        IsPieOfTheWeek = true
                    },
                    new Pie
                    {
                        Name = "Pumpkin Pie",
                        Price = 12.95M,
                        ShortDescription = "Our autumn favourite",
                        LongDescription = "Smooth spiced pumpkin filling with cinnamon and nutmeg, topped with whipped cream.",
                        SubCategory = SubCategories["Seasonal pies"],
                        ImageUrl = "https://gillcleerenpluralsight.blob.core.windows.net/files/bethanyspieshop/seasonal/pumpkinpie.jpg",
                        ImageThumbnailUrl = "https://gillcleerenpluralsight.blob.core.windows.net/files/bethanyspieshop/seasonal/pumpkinpiesmall.jpg",
                        InStock = true,
                        IsPieOfTheWeek = true
                    }
                );
            }

            context.SaveChanges();
        }

        // Each dictionary is built once, then reused, so every pie points at the SAME
        // SubCategory object and every subcategory at the SAME Category object.
        // That way EF inserts each one only once.
        private static Dictionary<string, Category>? categories;

        public static Dictionary<string, Category> Categories
        {
            get
            {
                if (categories == null)
                {
                    var categoryList = new Category[]
                    {
                        new Category { CategoryName = "Pies", Description = "Sweet pies, fresh from the oven" },
                        new Category { CategoryName = "Cakes", Description = "Cakes for every occasion" },
                        new Category { CategoryName = "Cupcakes", Description = "Small cakes, big taste" }
                    };

                    categories = new Dictionary<string, Category>();

                    foreach (Category category in categoryList)
                    {
                        categories.Add(category.CategoryName, category);
                    }
                }

                return categories;
            }
        }

        private static Dictionary<string, SubCategory>? subCategories;

        public static Dictionary<string, SubCategory> SubCategories
        {
            get
            {
                if (subCategories == null)
                {
                    var subCategoryList = new SubCategory[]
                    {
                        new SubCategory { SubCategoryName = "Fruit pies", Description = "All-fruity pies", Category = Categories["Pies"] },
                        new SubCategory { SubCategoryName = "Seasonal pies", Description = "Get in the mood for a seasonal pie", Category = Categories["Pies"] },
                        new SubCategory { SubCategoryName = "Cheese cakes", Description = "Cheesy all the way", Category = Categories["Cakes"] }
                    };

                    subCategories = new Dictionary<string, SubCategory>();

                    foreach (SubCategory subCategory in subCategoryList)
                    {
                        subCategories.Add(subCategory.SubCategoryName, subCategory);
                    }
                }

                return subCategories;
            }
        }
    }
}
