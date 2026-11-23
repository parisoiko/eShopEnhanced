using Microsoft.AspNetCore.Identity;

namespace eShopEnhanced.Models
{
    // Our user: everything Identity already has (Email, PasswordHash, PhoneNumber...)
    // PLUS the details the checkout form asks for, so registered customers don't type them twice.
    // Stored as extra columns in the AspNetUsers table. Same maximum lengths as Order.
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;     // max 50
        public string LastName { get; set; } = string.Empty;      // max 50
        public string AddressLine1 { get; set; } = string.Empty;  // max 100
        public string? AddressLine2 { get; set; }                 // max 100
        public string ZipCode { get; set; } = string.Empty;       // max 10
        public string City { get; set; } = string.Empty;          // max 50
        public string? State { get; set; }                        // max 50
        public string Country { get; set; } = string.Empty;       // max 50

        // PhoneNumber already exists on IdentityUser

        // "MP" for Maria Papadopoulou: shown in the round avatar in the navbar
        public string Initials =>
            $"{FirstName.Trim().FirstOrDefault()}{LastName.Trim().FirstOrDefault()}".ToUpperInvariant();
    }
}
