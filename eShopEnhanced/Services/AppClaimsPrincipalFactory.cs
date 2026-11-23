using System.Security.Claims;
using eShopEnhanced.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace eShopEnhanced.Services
{
    // When someone logs in, Identity builds a "ClaimsPrincipal": the bundle of facts about the user
    // (id, email...) that's stored, encrypted, in the login cookie and available on every request as
    // User. This factory adds the first and last name to that bundle, so the navbar can show
    // "MP" without loading the user from the database on every page.
    public class AppClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser>
    {
        public const string FirstNameClaim = "FirstName";
        public const string LastNameClaim = "LastName";

        public AppClaimsPrincipalFactory(UserManager<ApplicationUser> userManager, IOptions<IdentityOptions> options)
            : base(userManager, options)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim(FirstNameClaim, user.FirstName));
            identity.AddClaim(new Claim(LastNameClaim, user.LastName));
            return identity;
        }
    }
}
