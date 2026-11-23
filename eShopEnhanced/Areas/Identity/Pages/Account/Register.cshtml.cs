using System.ComponentModel.DataAnnotations;
using eShopEnhanced.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eShopEnhanced.Areas.Identity.Pages.Account
{
    // Overrides the Identity UI Register page (same path) to also collect the checkout details.
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public RegisterModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        // Where to go after registering, e.g. /Order/Checkout when the visitor came from checkout
        public string ReturnUrl { get; set; } = "/";

        // The form fields: account (email + password) and the same details as the checkout form,
        // with the same rules and messages as Order.
        public class InputModel
        {
            [Required(ErrorMessage = "Please enter your email address")]
            [StringLength(50, ErrorMessage = "{0} can be at most {1} characters")]
            [EmailAddress(ErrorMessage = "This doesn't look like a valid email address")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please choose a password")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "{0} must be at least {2} characters")]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please type your password again")]
            [DataType(DataType.Password)]
            [Compare(nameof(Password), ErrorMessage = "The two passwords don't match")]
            [Display(Name = "Confirm password")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please enter your first name")]
            [StringLength(50, ErrorMessage = "{0} can be at most {1} characters")]
            [Display(Name = "First name")]
            public string FirstName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please enter your last name")]
            [StringLength(50, ErrorMessage = "{0} can be at most {1} characters")]
            [Display(Name = "Last name")]
            public string LastName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please enter your phone number")]
            [StringLength(25, ErrorMessage = "{0} can be at most {1} characters")]
            [Phone(ErrorMessage = "This doesn't look like a valid phone number")]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please enter your address")]
            [StringLength(100, ErrorMessage = "{0} can be at most {1} characters")]
            [Display(Name = "Address")]
            public string AddressLine1 { get; set; } = string.Empty;

            [StringLength(100, ErrorMessage = "{0} can be at most {1} characters")]
            [Display(Name = "Address line 2")]
            public string? AddressLine2 { get; set; }

            [Required(ErrorMessage = "Please enter your postal code")]
            [StringLength(10, MinimumLength = 4, ErrorMessage = "{0} must be between {2} and {1} characters")]
            [Display(Name = "Postal code")]
            public string ZipCode { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please enter your city")]
            [StringLength(50, ErrorMessage = "{0} can be at most {1} characters")]
            [Display(Name = "City")]
            public string City { get; set; } = string.Empty;

            [StringLength(50, ErrorMessage = "{0} can be at most {1} characters")]
            [Display(Name = "Region")]
            public string? State { get; set; }

            [Required(ErrorMessage = "Please enter your country")]
            [StringLength(50, ErrorMessage = "{0} can be at most {1} characters")]
            [Display(Name = "Country")]
            public string Country { get; set; } = string.Empty;
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = SafeReturnUrl(returnUrl);
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = SafeReturnUrl(returnUrl);

            if (!ModelState.IsValid)
            {
                return Page();   // show the form again, with what was typed and the error messages
            }

            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                PhoneNumber = Input.PhoneNumber,
                FirstName = Input.FirstName.Trim(),
                LastName = Input.LastName.Trim(),
                AddressLine1 = Input.AddressLine1,
                AddressLine2 = Input.AddressLine2,
                ZipCode = Input.ZipCode,
                City = Input.City,
                State = Input.State,
                Country = Input.Country
            };

            // Also enforces Identity's own rules (password strength, duplicate email)
            IdentityResult result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);   // log the new user in straight away
                return LocalRedirect(ReturnUrl);
            }

            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        // Local URLs only (open-redirect protection)
        private string SafeReturnUrl(string? returnUrl) =>
            !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
    }
}
