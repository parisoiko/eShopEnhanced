using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace eShopEnhanced.Models
{
    public class Order
    {
        // [BindNever]: model binding never fills these from the form, even if someone adds
        // fake fields like "OrderTotal=0.01" to the request. Only the server sets them.
        [BindNever]
        public int OrderId { get; set; }

        [BindNever]
        public List<OrderDetail>? OrderDetails { get; set; }

        // Validation rules. Each one is checked twice:
        //  - in the browser, before the form is sent (jQuery validation, from the data-val-* attributes
        //    that asp-for writes into the <input>), so the visitor gets instant feedback;
        //  - on the server, during model binding (ModelState.IsValid), because browser checks can be bypassed.
        // [Display(Name = ...)] is the label text; {0} in a message is replaced by it, {1} by the length.
        // [Required] is written out explicitly: the implicit "non-nullable string = required" rule is NOT
        // checked when a field is left out of the request completely (tested).
        // [StringLength] is also used by EF Core: the column becomes nvarchar(50) instead of nvarchar(max).

        [Required(ErrorMessage = "Please enter your first name")]
        [StringLength(50, ErrorMessage = "{0} can be at most {1} characters")]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your last name")]
        [StringLength(50, ErrorMessage = "{0} can be at most {1} characters")]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

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

        [Required(ErrorMessage = "Please enter your phone number")]
        [StringLength(25, ErrorMessage = "{0} can be at most {1} characters")]
        [Phone(ErrorMessage = "This doesn't look like a valid phone number")]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email address")]
        [StringLength(50, ErrorMessage = "{0} can be at most {1} characters")]
        [EmailAddress(ErrorMessage = "This doesn't look like a valid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        // Filled in by the server, never by the form
        [BindNever]
        public decimal OrderTotal { get; set; }
        [BindNever]
        public DateTime OrderPlaced { get; set; }
    }
}
