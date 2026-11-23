using eShopEnhanced.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace eShopEnhanced.Tests.TagHelpers
{
    // Unit tests for <email address="..." content="..."></email>.
    // There's no page or Razor here: we build the two objects Razor would normally pass to
    // Process(...) ourselves, call it, and look at what it wrote into the output.
    public class EmailTagHelperTests
    {
        // What Razor passes in: information about the tag as written in the view
        private static TagHelperContext CreateContext() =>
            new TagHelperContext(
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object>(),
                uniqueId: "test");

        // What Razor passes in to be filled: starts as <email>, optionally with extra
        // attributes that aren't properties of the tag helper (like class="nav-link")
        private static TagHelperOutput CreateOutput(TagHelperAttributeList? attributes = null) =>
            new TagHelperOutput(
                tagName: "email",
                attributes: attributes ?? new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                    Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        [Fact]
        public void Process_WithAddressAndContent_CreatesMailtoLink()
        {
            // Arrange
            var tagHelper = new EmailTagHelper { Address = "info@bakery-paris.example", Content = "Contact us" };
            var output = CreateOutput();

            // Act
            tagHelper.Process(CreateContext(), output);

            // Assert: <a href="mailto:info@bakery-paris.example">Contact us</a>
            Assert.Equal("a", output.TagName);
            Assert.Equal("mailto:info@bakery-paris.example", output.Attributes["href"].Value);
            Assert.Equal("Contact us", output.Content.GetContent());
        }

        [Fact]
        public void Process_WithoutContent_UsesTheAddressAsLinkText()
        {
            var tagHelper = new EmailTagHelper { Address = "info@bakery-paris.example" };
            var output = CreateOutput();

            tagHelper.Process(CreateContext(), output);

            Assert.Equal("info@bakery-paris.example", output.Content.GetContent());
        }

        [Fact]
        public void Process_KeepsOtherAttributes()
        {
            // The navbar uses <email ... class="nav-link">: the class must end up on the <a>
            var tagHelper = new EmailTagHelper { Address = "info@bakery-paris.example", Content = "Contact" };
            var output = CreateOutput(new TagHelperAttributeList { { "class", "nav-link" } });

            tagHelper.Process(CreateContext(), output);

            Assert.Equal("nav-link", output.Attributes["class"].Value);
            Assert.Equal("mailto:info@bakery-paris.example", output.Attributes["href"].Value);
        }

        [Fact]
        public void Process_EncodesTheLinkText()
        {
            // Text is HTML-encoded, so content can never inject HTML or scripts into the page
            var tagHelper = new EmailTagHelper { Address = "info@bakery-paris.example", Content = "<script>alert('hi')</script>" };
            var output = CreateOutput();

            tagHelper.Process(CreateContext(), output);

            Assert.DoesNotContain("<script>", output.Content.GetContent());
            Assert.Contains("&lt;script&gt;", output.Content.GetContent());
        }
    }
}
