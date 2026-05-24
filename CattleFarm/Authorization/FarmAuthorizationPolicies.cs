using CattleFarm.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CattleFarm.Authorization
{
    public static class FarmPolicyNames
    {
        public const string RequireFarmOwnership = "RequireFarmOwnership";
        public const string RequireWorkerRole = "RequireWorkerRole";
        public const string RequireOwnerRole = "RequireOwnerRole";
    }

    public class FarmOwnershipRequirement : IAuthorizationRequirement { }

    public class FarmOwnershipHandler : AuthorizationHandler<FarmOwnershipRequirement, int>
    {
        private readonly CattleFarmDbContext _db;

        public FarmOwnershipHandler(CattleFarmDbContext db)
        {
            _db = db;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            FarmOwnershipRequirement requirement,
            int farmId)
        {
            if (context.User.IsInRole(AppRoles.Admin))
            {
                context.Succeed(requirement);
                return;
            }

            if (!int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return;
            }

            var ownsFarm = await _db.Farms.AnyAsync(f => f.Id == farmId && f.OwnerId == userId && !f.IsDeleted);
            if (ownsFarm)
            {
                context.Succeed(requirement);
            }
        }
    }
}
