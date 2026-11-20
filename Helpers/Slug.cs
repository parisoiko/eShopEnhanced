using System.Text.RegularExpressions;

namespace eShopEnhanced.Helpers
{
    public static class Slug
    {
        // Turns a name into a URL-friendly piece of text:
        //   "Black Forest Cake"       -> "black-forest-cake"
        //   "Ham & Cheese Croissant"  -> "ham-and-cheese-croissant"
        //   "Cupcakes"                -> "cupcakes"
        public static string From(string text)
        {
            string slug = text.ToLowerInvariant().Replace("&", " and ");

            // Anything that isn't a letter or digit becomes a dash, and repeated dashes collapse into one
            slug = Regex.Replace(slug, "[^a-z0-9]+", "-");

            return slug.Trim('-');
        }
    }
}
