using System.Security.Claims;
using CattleFarm.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace CattleFarm.Authorization
{
    public static class UserClaimsHelper
    {
        public static string GetDisplayName(User user)
        {
            if (!string.IsNullOrWhiteSpace(user.FullName))
                return user.FullName.Trim();
            return user.Username;
        }

        public static IEnumerable<Claim> BuildClaims(User user)
        {
            yield return new Claim(ClaimTypes.NameIdentifier, user.Id.ToString());
            yield return new Claim(ClaimTypes.Name, GetDisplayName(user));
            yield return new Claim(ClaimTypes.Email, user.Email);
            yield return new Claim(ClaimTypes.Role, user.Role);
            yield return new Claim("FullName", user.FullName ?? string.Empty);
            yield return new Claim("ProfileImage", user.ProfileImagePath ?? string.Empty);
        }

        public static string GetInitials(string? displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName)) return "U";
            var parts = displayName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
            return displayName.Length >= 2
                ? displayName[..2].ToUpperInvariant()
                : displayName.ToUpperInvariant();
        }

        public static Task SignInAsync(HttpContext httpContext, User user, bool isPersistent = false)
        {
            var identity = new ClaimsIdentity(BuildClaims(user), CookieAuthenticationDefaults.AuthenticationScheme);
            var props = new AuthenticationProperties { IsPersistent = isPersistent };
            if (isPersistent)
                props.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7);
            return httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                props);
        }
    }
}
