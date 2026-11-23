using System.ComponentModel.DataAnnotations;
using System.Reflection;
using eShopEnhanced.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace eShopEnhanced.Tests.Models
{
    // Checks the validation attributes on Order ([Required], [StringLength], [EmailAddress]...)
    // directly with the DataAnnotations Validator: the same rules model binding applies on the
    // server before OrderController sees ModelState.IsValid.
    public class OrderValidationTests
    {
        private static Order CreateValidOrder() => new()
        {
            FirstName = "Maria",
            LastName = "Papadopoulou",
            AddressLine1 = "Ermou 10",
            ZipCode = "10563",
            City = "Athens",
            Country = "Greece",
            PhoneNumber = "+30 210 1234567",
            Email = "maria@example.com"
        };

        // Runs every attribute on every property and returns the failures
        private static List<ValidationResult> Validate(Order order)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(order, new ValidationContext(order), results, validateAllProperties: true);
            return results;
        }

        private static bool HasErrorFor(List<ValidationResult> results, string propertyName) =>
            results.Any(r => r.MemberNames.Contains(propertyName));

        [Fact]
        public void Validate_CompleteOrder_HasNoErrors()
        {
            var results = Validate(CreateValidOrder());

            Assert.Empty(results);
        }

        [Fact]
        public void Validate_EmptyOrder_ReportsEveryRequiredField()
        {
            // Arrange: a brand-new Order, as if the form was posted empty
            var order = new Order();

            // Act
            var results = Validate(order);

            // Assert: every required field has an error, the optional ones don't
            string[] required =
            {
                nameof(Order.FirstName), nameof(Order.LastName), nameof(Order.AddressLine1), nameof(Order.ZipCode),
                nameof(Order.City), nameof(Order.Country), nameof(Order.PhoneNumber), nameof(Order.Email)
            };
            foreach (string property in required)
            {
                Assert.True(HasErrorFor(results, property), $"Expected a validation error for {property}");
            }
            Assert.False(HasErrorFor(results, nameof(Order.AddressLine2)));
            Assert.False(HasErrorFor(results, nameof(Order.State)));
        }

        [Fact]
        public void Validate_MissingFirstName_UsesFriendlyMessage()
        {
            var order = CreateValidOrder();
            order.FirstName = "";

            var results = Validate(order);

            var error = Assert.Single(results);
            Assert.Equal("Please enter your first name", error.ErrorMessage);
        }

        [Theory]
        [InlineData("maria")]
        [InlineData("maria@")]
        [InlineData("@example.com")]
        public void Validate_InvalidEmail_ReportsEmailError(string email)
        {
            var order = CreateValidOrder();
            order.Email = email;

            var results = Validate(order);

            var error = Assert.Single(results);
            Assert.Contains(nameof(Order.Email), error.MemberNames);
            Assert.Equal("This doesn't look like a valid email address", error.ErrorMessage);
        }

        [Fact]
        public void Validate_TooLongFirstName_ReportsMaximumLength()
        {
            var order = CreateValidOrder();
            order.FirstName = new string('a', 51);   // the limit is 50

            var results = Validate(order);

            var error = Assert.Single(results);
            Assert.Contains(nameof(Order.FirstName), error.MemberNames);
            // {0} and {1} in the message are filled with the display name and the limit
            Assert.Equal("First name can be at most 50 characters", error.ErrorMessage);
        }

        [Fact]
        public void Validate_FirstNameOfExactlyMaximumLength_IsValid()
        {
            var order = CreateValidOrder();
            order.FirstName = new string('a', 50);

            Assert.Empty(Validate(order));
        }

        [Theory]
        [InlineData("123")]           // too short (minimum 4)
        [InlineData("12345678901")]   // too long (maximum 10)
        public void Validate_PostalCodeWithWrongLength_ReportsLengthRange(string zipCode)
        {
            var order = CreateValidOrder();
            order.ZipCode = zipCode;

            var results = Validate(order);

            var error = Assert.Single(results);
            Assert.Contains(nameof(Order.ZipCode), error.MemberNames);
            Assert.Equal("Postal code must be between 4 and 10 characters", error.ErrorMessage);
        }

        // [BindNever] keeps model binding from filling these from the posted form, so a visitor
        // can't send "OrderTotal=0.01". Checked with reflection: the attribute must stay on them.
        [Theory]
        [InlineData(nameof(Order.OrderId))]
        [InlineData(nameof(Order.OrderTotal))]
        [InlineData(nameof(Order.OrderPlaced))]
        [InlineData(nameof(Order.OrderDetails))]
        public void ServerOnlyProperty_HasBindNeverAttribute(string propertyName)
        {
            PropertyInfo? property = typeof(Order).GetProperty(propertyName);

            Assert.NotNull(property);
            Assert.NotNull(property.GetCustomAttribute<BindNeverAttribute>());
        }

        [Theory]
        [InlineData(nameof(Order.FirstName))]
        [InlineData(nameof(Order.Email))]
        public void FormField_DoesNotHaveBindNeverAttribute(string propertyName)
        {
            PropertyInfo property = typeof(Order).GetProperty(propertyName)!;

            Assert.Null(property.GetCustomAttribute<BindNeverAttribute>());
        }
    }
}
