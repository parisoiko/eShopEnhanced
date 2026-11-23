using Microsoft.AspNetCore.Razor.TagHelpers;

namespace eShopEnhanced.TagHelpers
{
    // A custom tag helper: write <email address="..." content="..."></email> in any view
    // and it is turned into <a href="mailto:...">...</a> on the server.
    //
    // How Razor finds it:
    //  - the class name "EmailTagHelper" minus "TagHelper" = the tag name <email>
    //  - each public property becomes an attribute, in kebab-case: Address -> address, Content -> content
    //  - "@addTagHelper *, eShopEnhanced" in Views/_ViewImports.cshtml makes all tag helpers in this project available
    public class EmailTagHelper : TagHelper
    {
        public string? Address { get; set; }
        public string? Content { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // <email> -> <a>
            output.TagName = "a";

            // href="mailto:info@..." (Razor HTML-encodes the value for us)
            output.Attributes.SetAttribute("href", $"mailto:{Address}");

            // The link text: the content attribute, or the address itself if no content was given
            output.Content.SetContent(Content ?? Address);
        }
    }
}
