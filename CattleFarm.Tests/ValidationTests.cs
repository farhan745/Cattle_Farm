using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CattleFarm.ViewModels;
using Xunit;

namespace CattleFarm.Tests
{
    public class ValidationTests
    {
        [Fact]
        public void RegisterViewModel_ValidModel_PassesValidation()
        {
            var model = new RegisterViewModel
            {
                FullName = "Rahman Hossain",
                Username = "rahman123",
                Email = "rahman@example.com",
                Password = "Password123",
                ConfirmPassword = "Password123",
                Role = "User"
            };

            var context = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, context, results, true);

            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Fact]
        public void RegisterViewModel_MissingRequiredFields_FailsValidation()
        {
            var model = new RegisterViewModel
            {
                FullName = "", // Required
                Username = "", // Required
                Email = "invalid-email", // Invalid
                Password = "123", // Too short (min 6)
                ConfirmPassword = "1234", // Mismatch
                Role = "User"
            };

            var context = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, context, results, true);

            Assert.False(isValid);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("FullName"));
            Assert.Contains(results, r => r.MemberNames.Contains("Username"));
            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
            Assert.Contains(results, r => r.MemberNames.Contains("Password"));
        }

        [Fact]
        public void LoginViewModel_MissingEmail_FailsValidation()
        {
            var model = new LoginViewModel
            {
                Email = "",
                Password = "somepassword"
            };

            var context = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, context, results, true);

            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
        }
    }
}
