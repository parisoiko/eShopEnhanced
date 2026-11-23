using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace eShopEnhanced.Tests.Integration
{
    // Integration tests: real HTTP requests through the whole app (routing, controllers, EF Core,
    // Razor views, antiforgery, authentication), with an in-memory SQLite database seeded by
    // DbInitializer (6 categories, 15 subcategories, 24 products). See BakeryWebApplicationFactory.
    [Collection(IntegrationTestCollection.Name)]
    public class ShopPagesTests
    {
        private readonly BakeryWebApplicationFactory _factory;

        public ShopPagesTests(BakeryWebApplicationFactory factory)
        {
            _factory = factory;
        }

        // A fresh "browser" for each test: its own cookies (session = its own cart), and redirects
        // are NOT followed, so a test can check the redirect itself
        private HttpClient CreateClient() =>
            _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true
            });

        // ---------- Pages ----------

        [Fact]
        public async Task HomePage_ReturnsOkAndShowsAllSixCategories()
        {
            // Arrange
            var client = CreateClient();

            // Act
            var response = await client.GetAsync("/");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string html = await response.Content.ReadAsStringAsync();
            foreach (string category in new[] { "Pies", "Cakes", "Cupcakes", "Cookies", "Croissants", "Breads" })
            {
                Assert.Contains(category, html);
            }
        }

        [Fact]
        public async Task CategoryPage_WithKnownCategory_ShowsItsProducts()
        {
            var client = CreateClient();

            var response = await client.GetAsync("/products/cakes");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string html = await response.Content.ReadAsStringAsync();
            Assert.Contains("Black Forest Cake", html);
            Assert.Contains("Carrot Cake", html);
            Assert.Contains("Chocolate Cake", html);
            Assert.Contains("24,95", html);              // el-GR price format: 24,95 €
            Assert.DoesNotContain("Strawberry Pie", html); // a pie doesn't belong here
        }

        [Fact]
        public async Task CategoryPage_WithUnknownCategory_ReturnsNotFound()
        {
            var client = CreateClient();

            var response = await client.GetAsync("/products/pizza");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ProductPage_WithUnknownId_ReturnsNotFound()
        {
            var client = CreateClient();

            var response = await client.GetAsync("/products/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ProductPage_WithoutSlug_RedirectsPermanentlyToSlugUrl()
        {
            var client = CreateClient();

            var response = await client.GetAsync("/products/7");

            Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
            Assert.Equal("/products/7/black-forest-cake", response.Headers.Location?.OriginalString);
        }

        [Fact]
        public async Task ProductPage_WithCorrectSlug_ShowsTheProduct()
        {
            var client = CreateClient();

            var response = await client.GetAsync("/products/7/black-forest-cake");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Black Forest Cake", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task SearchSuggestions_ForCroiss_ReturnsCroissants()
        {
            var client = CreateClient();

            var response = await client.GetAsync("/products/search/suggestions?q=croiss");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
            string html = await response.Content.ReadAsStringAsync();
            Assert.Contains("Butter Croissant", html);
            Assert.Contains("Almond Croissant", html);
            Assert.DoesNotContain("<html", html);   // only the dropdown fragment, no layout
        }

        // ---------- Checkout needs an account ----------

        [Fact]
        public async Task Checkout_WhenAnonymous_RedirectsToLogin()
        {
            var client = CreateClient();

            var response = await client.GetAsync("/Order/Checkout");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            string location = response.Headers.Location!.ToString();
            Assert.Contains("/Identity/Account/Login", location);
            Assert.Contains("ReturnUrl=%2FOrder%2FCheckout", location);
        }

        // ---------- Add to cart (as site.js does it) ----------

        [Fact]
        public async Task AddToCart_WithAntiforgeryToken_ReturnsCartJson()
        {
            // Arrange: open a product page like a visitor would; this also sets the antiforgery
            // and session cookies, and the page's form contains the matching token
            var client = CreateClient();
            var page = await client.GetAsync("/products/7/black-forest-cake");
            string token = ExtractAntiforgeryToken(await page.Content.ReadAsStringAsync());

            var request = new HttpRequestMessage(HttpMethod.Post, "/ShoppingCart/AddToShoppingCart")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["productId"] = "7",
                    ["returnUrl"] = "/products/7/black-forest-cake",
                    ["__RequestVerificationToken"] = token
                })
            };
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");   // what site.js sends

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

            using JsonDocument json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            JsonElement root = json.RootElement;
            Assert.Equal(7, root.GetProperty("productId").GetInt32());
            Assert.Equal(1, root.GetProperty("amount").GetInt32());
            Assert.Equal(1, root.GetProperty("cartCount").GetInt32());
            Assert.Equal(24.95m, root.GetProperty("cartTotal").GetDecimal());
            string cartTotalText = root.GetProperty("cartTotalText").GetString()!;
            Assert.Contains("24,95", cartTotalText);
            Assert.Contains("€", cartTotalText);
            Assert.Contains("Black Forest Cake", root.GetProperty("message").GetString());
        }

        [Fact]
        public async Task AddToCart_WithoutAntiforgeryToken_ReturnsBadRequest()
        {
            var client = CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "/ShoppingCart/AddToShoppingCart")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["productId"] = "7" })
            };
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // The form tag helper writes <input name="__RequestVerificationToken" type="hidden" value="..." />
        // into every <form method="post">
        private static string ExtractAntiforgeryToken(string html)
        {
            Match match = Regex.Match(html,
                "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
            Assert.True(match.Success, "No antiforgery token found on the page");
            return match.Groups[1].Value;
        }
    }
}
