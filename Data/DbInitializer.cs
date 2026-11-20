using eShopEnhanced.Models;
namespace eShopEnhanced.Data
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

            if (!context.Products.Any())
            {
                context.Products.AddRange(Products);
            }

            context.SaveChanges();
        }

        // Course images (hosted by Pluralsight) and our own images in wwwroot/images/products
        private const string CourseImages = "https://gillcleerenpluralsight.blob.core.windows.net/files/bethanyspieshop";
        private const string OurImages = "/images/products";

        // Each dictionary is built once, then reused, so every product points at the SAME
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
                        new Category { CategoryName = "Pies", Description = "Sweet pies, fresh from the oven", ImageUrl = $"{CourseImages}/fruitpies/strawberrypie.jpg" },
                        new Category { CategoryName = "Cakes", Description = "Cakes for every occasion", ImageUrl = $"{OurImages}/black-forest-cake.jpg" },
                        new Category { CategoryName = "Cupcakes", Description = "Small cakes, big taste", ImageUrl = $"{OurImages}/chocolate-cupcake.jpg" },
                        new Category { CategoryName = "Cookies", Description = "Crunchy, chewy and everything in between", ImageUrl = $"{OurImages}/chocolate-chip-cookies.jpg" },
                        new Category { CategoryName = "Croissants", Description = "Flaky, buttery and baked every morning", ImageUrl = $"{OurImages}/butter-croissant.jpg" },
                        new Category { CategoryName = "Breads", Description = "Slow-risen loaves with a proper crust", ImageUrl = $"{OurImages}/sourdough.jpg" }
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

                        new SubCategory { SubCategoryName = "Cheese cakes", Description = "Cheesy all the way", Category = Categories["Cakes"] },
                        new SubCategory { SubCategoryName = "Chocolate cakes", Description = "For chocolate lovers", Category = Categories["Cakes"] },
                        new SubCategory { SubCategoryName = "Classic cakes", Description = "The ones everybody loves", Category = Categories["Cakes"] },

                        new SubCategory { SubCategoryName = "Classic cupcakes", Description = "Simple and perfect", Category = Categories["Cupcakes"] },
                        new SubCategory { SubCategoryName = "Specialty cupcakes", Description = "Something a little different", Category = Categories["Cupcakes"] },

                        new SubCategory { SubCategoryName = "Classic cookies", Description = "Cookie jar favourites", Category = Categories["Cookies"] },
                        new SubCategory { SubCategoryName = "Butter cookies", Description = "Rich and crumbly", Category = Categories["Cookies"] },
                        new SubCategory { SubCategoryName = "Seasonal cookies", Description = "Spiced for the holidays", Category = Categories["Cookies"] },

                        new SubCategory { SubCategoryName = "Classic croissants", Description = "Pure butter, nothing else", Category = Categories["Croissants"] },
                        new SubCategory { SubCategoryName = "Sweet croissants", Description = "Filled with something sweet", Category = Categories["Croissants"] },
                        new SubCategory { SubCategoryName = "Savoury croissants", Description = "Perfect for lunch", Category = Categories["Croissants"] },

                        new SubCategory { SubCategoryName = "Sourdough breads", Description = "Naturally leavened", Category = Categories["Breads"] },
                        new SubCategory { SubCategoryName = "Italian breads", Description = "Open crumb, olive oil", Category = Categories["Breads"] }
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

        // Small helper so each product fits on a few lines
        private static Product CreateProduct(string name, decimal price, string subCategory, string imageUrl,
            string shortDescription, string longDescription, string? allergyInformation = null, bool isPickOfTheWeek = false)
        {
            return new Product
            {
                Name = name,
                Price = price,
                SubCategory = SubCategories[subCategory],
                ImageUrl = imageUrl,
                ImageThumbnailUrl = imageUrl,
                ShortDescription = shortDescription,
                LongDescription = longDescription,
                AllergyInformation = allergyInformation,
                InStock = true,
                IsPickOfTheWeek = isPickOfTheWeek
            };
        }

        private static Product[] Products => new Product[]
        {
            // ----- Pies -----
            CreateProduct("Strawberry Pie", 15.95M, "Fruit pies", $"{CourseImages}/fruitpies/strawberrypie.jpg",
                "Fresh strawberries on a crunchy base",
                "Sweet, ripe strawberries piled high on a buttery shortcrust base with a light vanilla cream.",
                "Contains gluten and dairy"),
            CreateProduct("Rhubarb Pie", 15.95M, "Fruit pies", $"{CourseImages}/fruitpies/rhubarbpie.jpg",
                "Tangy rhubarb, just sweet enough",
                "Tart rhubarb baked until soft in a golden pastry case, balanced with a touch of sugar.",
                "Contains gluten", isPickOfTheWeek: true),
            CreateProduct("Apple Pie", 14.95M, "Fruit pies", $"{OurImages}/apple-pie.jpg",
                "Grandma's recipe, with cinnamon",
                "Chunky apples with cinnamon and a hint of lemon under a flaky double crust.",
                "Contains gluten and dairy"),
            CreateProduct("Pumpkin Pie", 12.95M, "Seasonal pies", $"{CourseImages}/seasonal/pumpkinpie.jpg",
                "Our autumn favourite",
                "Smooth spiced pumpkin filling with cinnamon and nutmeg, topped with whipped cream.",
                "Contains gluten, dairy and eggs", isPickOfTheWeek: true),

            // ----- Cakes -----
            CreateProduct("Cheese cake", 18.95M, "Cheese cakes", $"{CourseImages}/cheesecakes/cheesecake.jpg",
                "Creamy, rich and smooth",
                "A classic baked cheesecake with a biscuit crust and a velvety cream cheese filling.",
                "Contains gluten, dairy and eggs"),
            CreateProduct("Chocolate Cake", 21.50M, "Chocolate cakes", $"{OurImages}/chocolate-cake.jpg",
                "Dark, moist and very chocolatey",
                "Layers of dark chocolate sponge with a rich chocolate ganache.",
                "Contains gluten, dairy and eggs"),
            CreateProduct("Black Forest Cake", 24.95M, "Chocolate cakes", $"{OurImages}/black-forest-cake.jpg",
                "Chocolate, cherries and cream",
                "The German classic: chocolate sponge, sour cherries and whipped cream, finished with chocolate shavings.",
                "Contains gluten, dairy and eggs", isPickOfTheWeek: true),
            CreateProduct("Carrot Cake", 19.95M, "Classic cakes", $"{OurImages}/carrot-cake.jpg",
                "Spiced sponge, cream cheese frosting",
                "Moist carrot and walnut sponge with cinnamon, topped with a thick cream cheese frosting.",
                "Contains gluten, dairy, eggs and nuts"),

            // ----- Cupcakes -----
            CreateProduct("Vanilla Cupcake", 3.50M, "Classic cupcakes", $"{OurImages}/vanilla-cupcake.jpg",
                "Light vanilla sponge, cherry frosting",
                "A fluffy vanilla cupcake with a swirl of cherry buttercream and a fresh cherry on top.",
                "Contains gluten, dairy and eggs"),
            CreateProduct("Chocolate Cupcake", 3.50M, "Classic cupcakes", $"{OurImages}/chocolate-cupcake.jpg",
                "Chocolate on chocolate",
                "Rich chocolate sponge topped with a tall swirl of chocolate buttercream.",
                "Contains gluten, dairy and eggs"),
            CreateProduct("Red Velvet Cupcake", 3.95M, "Specialty cupcakes", $"{OurImages}/red-velvet-cupcake.jpg",
                "Soft, red and a little cocoa",
                "A tender red velvet sponge with a hint of cocoa and buttermilk.",
                "Contains gluten, dairy and eggs"),
            CreateProduct("Lemon Cupcake", 3.95M, "Specialty cupcakes", $"{OurImages}/lemon-cupcake.jpg",
                "Zesty lemon with a berry centre",
                "Lemon sponge filled with strawberry jam and topped with lemon cream.",
                "Contains gluten, dairy and eggs"),

            // ----- Cookies -----
            CreateProduct("Chocolate Chip Cookies", 6.50M, "Classic cookies", $"{OurImages}/chocolate-chip-cookies.jpg",
                "Box of 6, crispy edges, soft middle",
                "Brown-butter cookies loaded with dark chocolate chips. Sold in a box of 6.",
                "Contains gluten, dairy and eggs"),
            CreateProduct("Oatmeal Raisin Cookies", 5.95M, "Classic cookies", $"{OurImages}/oatmeal-raisin-cookies.jpg",
                "Box of 6, chewy and wholesome",
                "Rolled oats, golden raisins and a little orange zest. Sold in a box of 6.",
                "Contains gluten, dairy and eggs"),
            CreateProduct("Shortbread Fingers", 5.50M, "Butter cookies", $"{OurImages}/shortbread.jpg",
                "Box of 8, Scottish style",
                "Just butter, sugar and flour, baked low and slow until perfectly crumbly. Sold in a box of 8.",
                "Contains gluten and dairy"),
            CreateProduct("Gingerbread Cookies", 6.95M, "Seasonal cookies", $"{OurImages}/gingerbread-cookies.jpg",
                "Box of 10, spiced and decorated",
                "Ginger, cinnamon and clove cookies in festive shapes. Sold in a box of 10.",
                "Contains gluten and eggs", isPickOfTheWeek: true),

            // ----- Croissants -----
            CreateProduct("Butter Croissant", 1.80M, "Classic croissants", $"{OurImages}/butter-croissant.jpg",
                "Flaky, golden, all butter",
                "Made with French butter and laminated over three days for perfect layers.",
                "Contains gluten and dairy"),
            CreateProduct("Pain au Chocolat", 2.20M, "Sweet croissants", $"{OurImages}/pain-au-chocolat.jpg",
                "Croissant dough with two bars of chocolate",
                "Our croissant dough rolled around two sticks of dark chocolate.",
                "Contains gluten and dairy"),
            CreateProduct("Almond Croissant", 2.60M, "Sweet croissants", $"{OurImages}/almond-croissant.jpg",
                "Filled with almond cream",
                "Twice-baked croissant filled with frangipane and topped with toasted almonds.",
                "Contains gluten, dairy, eggs and nuts"),
            CreateProduct("Ham & Cheese Croissant", 3.20M, "Savoury croissants", $"{OurImages}/ham-cheese-croissant.jpg",
                "A savoury classic",
                "A butter croissant filled with ham and melted cheese.",
                "Contains gluten and dairy"),

            // ----- Breads -----
            CreateProduct("Sourdough Loaf", 4.50M, "Sourdough breads", $"{OurImages}/sourdough.jpg",
                "48-hour sourdough",
                "Naturally leavened with our own starter, with a crackling crust and an open crumb.",
                "Contains gluten"),
            CreateProduct("Rye Bread", 4.20M, "Sourdough breads", $"{OurImages}/rye-bread.jpg",
                "Dense, dark and hearty",
                "A German-style rye sourdough, great with butter or smoked fish.",
                "Contains gluten"),
            CreateProduct("Ciabatta", 3.20M, "Italian breads", $"{OurImages}/ciabatta.jpg",
                "Airy Italian slipper bread",
                "A very wet dough baked into a light, airy loaf. Perfect for sandwiches.",
                "Contains gluten"),
            CreateProduct("Rosemary Focaccia", 3.80M, "Italian breads", $"{OurImages}/focaccia.jpg",
                "Olive oil and rosemary",
                "Soft, dimpled focaccia baked with extra virgin olive oil, sea salt and fresh rosemary.",
                "Contains gluten")
        };
    }
}
