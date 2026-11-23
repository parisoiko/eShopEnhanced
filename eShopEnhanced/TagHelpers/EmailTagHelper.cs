using Microsoft.AspNetCore.Razor.TagHelpers;

namespace eShopEnhanced.TagHelpers
{
    // Renders <email address="..." content="..."></email> as a mailto: link.
    public class EmailTagHelper : TagHelper
    {
        public string? Address { get; set; }
        public string? Content { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "a";

            output.Attributes.SetAttribute("href", $"mailto:{Address}");

            // Fall back to the address as link text
            output.Content.SetContent(Content ?? Address);
        }
    }
}
