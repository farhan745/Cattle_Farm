using System.Security.Claims;
using CattleFarm.Authorization;
using CattleFarm.Models;

namespace CattleFarm.Tests;

public class UserClaimsHelperTests
{
    [Theory]
    [InlineData(null, "U")]
    [InlineData("", "U")]
    [InlineData("Rahman Hossain", "RH")]
    [InlineData("Owner", "OW")]
    [InlineData("A", "A")]
    public void GetInitials_ReturnsExpectedInitials(string? displayName, string expected)
    {
        var initials = UserClaimsHelper.GetInitials(displayName);

        Assert.Equal(expected, initials);
    }

    [Fact]
    public void BuildClaims_IncludesCoreIdentityClaims()
    {
        var user = new User
        {
            Id = 42,
            Username = "owner",
            FullName = "Farm Owner",
            Email = "owner@example.com",
            Role = AppRoles.Owner,
            ProfileImagePath = "/uploads/avatars/owner.jpg"
        };

        var claims = UserClaimsHelper.BuildClaims(user).ToList();

        Assert.Contains(claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == "42");
        Assert.Contains(claims, c => c.Type == ClaimTypes.Name && c.Value == "Farm Owner");
        Assert.Contains(claims, c => c.Type == ClaimTypes.Email && c.Value == "owner@example.com");
        Assert.Contains(claims, c => c.Type == ClaimTypes.Role && c.Value == AppRoles.Owner);
        Assert.Contains(claims, c => c.Type == "ProfileImage" && c.Value == "/uploads/avatars/owner.jpg");
    }
}
