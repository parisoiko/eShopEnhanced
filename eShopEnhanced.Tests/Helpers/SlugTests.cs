using eShopEnhanced.Helpers;

namespace eShopEnhanced.Tests.Helpers
{
    // Unit tests for Slug.From: the readable part of product URLs like /products/7/black-forest-cake
    public class SlugTests
    {
        [Theory]
        [InlineData("Black Forest Cake", "black-forest-cake")]
        [InlineData("Ham & Cheese Croissant", "ham-and-cheese-croissant")]
        [InlineData("Cupcakes", "cupcakes")]
        [InlineData("Cheese cake", "cheese-cake")]
        public void From_ProductName_ReturnsLowercaseDashedSlug(string name, string expected)
        {
            // Act
            string slug = Slug.From(name);

            // Assert
            Assert.Equal(expected, slug);
        }

        [Theory]
        [InlineData("  Apple   Pie  ", "apple-pie")]          // extra spaces collapse, ends trimmed
        [InlineData("Grandma's Apple Pie!", "grandma-s-apple-pie")]
        [InlineData("Pain au chocolat (2 pcs.)", "pain-au-chocolat-2-pcs")]
        [InlineData("--Sourdough--", "sourdough")]
        public void From_TextWithPunctuationOrExtraSpaces_TrimsAndCollapsesDashes(string name, string expected)
        {
            string slug = Slug.From(name);

            Assert.Equal(expected, slug);
        }
    }
}
