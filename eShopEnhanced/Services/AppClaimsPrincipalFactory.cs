using System.Security.Claims;
using eShopEnhanced.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace eShopEnhanced.Services
{
    // Adds first/last name claims to the login cookie, so the navbar can show the user's initials
    // without loading the user from the database on every request.
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
