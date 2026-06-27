using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using CattleFarm.Services.Interfaces;

namespace CattleFarm.Hubs
{
    [Authorize]
    public class FarmDashboardHub : Hub
    {
        private readonly IFarmAccessService _farmAccessService;

        public FarmDashboardHub(IFarmAccessService farmAccessService)
        {
            _farmAccessService = farmAccessService;
        }

        public async Task JoinUserGroup(string userId)
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var currentUserId = Context.UserIdentifier ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId == userId)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
                }
            }
        }

        public async Task JoinFarmGroup(int farmId)
        {
            if (farmId > 0)
            {
                var userIdString = Context.UserIdentifier ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdString, out int userId))
                {
                    var role = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                    var hasAccess = await _farmAccessService.CanOperateFarmAsync(farmId, userId, role);
                    if (hasAccess)
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, FarmGroup(farmId));
                    }
                }
            }
        }

        public static string UserGroup(int userId) => $"user:{userId}";

        public static string UserGroup(string userId) => $"user:{userId}";

        public static string FarmGroup(int farmId) => $"farm:{farmId}";
    }
}
