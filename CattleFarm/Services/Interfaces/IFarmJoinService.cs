using CattleFarm.ViewModels;

namespace CattleFarm.Services.Interfaces
{
    public interface IFarmJoinService
    {
        // Worker side
        Task<FarmJoinBrowseViewModel> GetBrowseViewModelAsync(int workerUserId);
        Task<(bool Success, string Message)> ApplyAsync(int farmId, int workerUserId, string? message);
        Task<IEnumerable<MyJoinRequestViewModel>> GetMyRequestsAsync(int workerUserId);
        Task<bool> LeaveAsync(int farmId, int workerUserId);

        // Owner side
        Task<IEnumerable<IncomingRequestViewModel>> GetIncomingAsync(int ownerUserId);
        Task<(bool Success, string Message)> AcceptAsync(int requestId, int ownerUserId);
        Task<(bool Success, string Message)> RejectAsync(int requestId, int ownerUserId, string? note);
        Task<bool> RemoveWorkerAsync(int farmWorkerId, int ownerUserId);

        // Owner creates login for manually-added worker
        Task<(bool Success, string Message)> CreateWorkerLoginAsync(CreateWorkerLoginViewModel model, int ownerUserId);
    }
}
