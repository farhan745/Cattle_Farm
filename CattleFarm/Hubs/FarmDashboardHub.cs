using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CattleFarm.Hubs
{
    [Authorize]
    public class FarmDashboardHub : Hub
    {
        public async Task JoinUserGroup(string userId)
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
            }
        }

        public async Task JoinFarmGroup(int farmId)
        {
            if (farmId > 0)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, FarmGroup(farmId));
            }
        }

        public static string UserGroup(int userId) => $"user:{userId}";

        public static string UserGroup(string userId) => $"user:{userId}";

        public static string FarmGroup(int farmId) => $"farm:{farmId}";
    }
}
