using BCrypt.Net;
using CattleFarm.Hubs;
using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace CattleFarm.Services.Implementations
{
    public class FarmJoinService : IFarmJoinService
    {
        private readonly CattleFarmDbContext _db;
        private readonly INotificationService _notifications;
        private readonly IHubContext<FarmDashboardHub> _hub;

        public FarmJoinService(
            CattleFarmDbContext db,
            INotificationService notifications,
            IHubContext<FarmDashboardHub> hub)
        {
            _db = db;
            _notifications = notifications;
            _hub = hub;
        }

        // ── Worker: Browse farms ──────────────────────────────────────────────

        public async Task<FarmJoinBrowseViewModel> GetBrowseViewModelAsync(int workerUserId)
        {
            var farms = await _db.Farms
                .Where(f => !f.IsDeleted && f.IsActive && f.ApprovalStatus == ApprovalStatus.Approved)
                .Include(f => f.Workers)
                .ToListAsync();

            // Check if worker already belongs to a farm
            var activeMembership = await _db.FarmWorkers
                .Include(fw => fw.Farm)
                .FirstOrDefaultAsync(fw => fw.WorkerUserId == workerUserId && fw.IsActive);

            // Get all requests by this worker
            var myRequests = await _db.FarmJoinRequests
                .Where(r => r.WorkerUserId == workerUserId && r.ApplicantRole == JoinApplicantRole.Worker)
                .ToListAsync();

            var farmItems = farms.Select(f =>
            {
                var req = myRequests.FirstOrDefault(r => r.FarmId == f.Id);
                string appStatus = "None";
                DateTime? cooldown = null;

                if (activeMembership?.FarmId == f.Id)
                {
                    appStatus = "Accepted";
                }
                else if (req != null)
                {
                    appStatus = req.Status;
                    if (req.Status == "Rejected" && req.CanReApplyAt.HasValue && req.CanReApplyAt > DateTime.UtcNow)
                    {
                        appStatus = "Cooldown";
                        cooldown = req.CanReApplyAt;
                    }
                }

                return new FarmBrowseItem
                {
                    Id          = f.Id,
                    Name        = f.Name,
                    Location    = f.Location,
                    ImagePath   = f.ImagePath,
                    WorkerCount = f.Workers.Count(w => w.IsActive && !w.IsDeleted),
                    ApplicationStatus = appStatus,
                    CooldownEnds = cooldown,
                    AlreadyJoined = activeMembership?.FarmId == f.Id
                };
            }).ToList();

            return new FarmJoinBrowseViewModel
            {
                Farms = farmItems,
                MyActiveFarmId   = activeMembership?.FarmId,
                MyActiveFarmName = activeMembership?.Farm?.Name
            };
        }

        // ── Worker: Apply to farm ─────────────────────────────────────────────

        public async Task<(bool Success, string Message)> ApplyAsync(int farmId, int workerUserId, string? message)
        {
            var applicant = await _db.Users.FindAsync(workerUserId);
            if (applicant?.Role == AppRoles.Manager)
                return await ApplyAsManagerAsync(farmId, workerUserId, message);

            // Already a member?
            var alreadyMember = await _db.FarmWorkers
                .AnyAsync(fw => fw.FarmId == farmId && fw.WorkerUserId == workerUserId && fw.IsActive);
            if (alreadyMember)
                return (false, "You are already a member of this farm.");

            var farm = await _db.Farms
                .FirstOrDefaultAsync(f => f.Id == farmId && f.IsActive && !f.IsDeleted);
            if (farm == null)
                return (false, "Farm not found.");

            // Existing pending/accepted request?
            var existing = await _db.FarmJoinRequests
                .FirstOrDefaultAsync(r => r.FarmId == farmId && r.WorkerUserId == workerUserId);

            if (existing != null)
            {
                if (existing.Status == "Applied")
                    return (false, "Your request is already pending.");

                if (existing.Status == "Rejected" && existing.CanReApplyAt.HasValue && existing.CanReApplyAt > DateTime.UtcNow)
                    return (false, $"Cooldown active. You can apply again after {existing.CanReApplyAt.Value.ToLocalTime():MMM dd}.");

                // Re-apply after cooldown: update existing record
                existing.Status      = "Applied";
                existing.Message     = message;
                existing.AppliedAt   = DateTime.UtcNow;
                existing.ReviewedAt  = null;
                existing.OwnerNote   = null;
                existing.CanReApplyAt = null;
                await _db.SaveChangesAsync();
                await NotifyOwnerOfJoinRequestAsync(farm, existing.Id, JoinApplicantRole.Worker);
                return (true, "Application submitted.");
            }

            var joinRequest = new FarmJoinRequest
            {
                FarmId         = farmId,
                WorkerUserId   = workerUserId,
                ApplicantRole  = JoinApplicantRole.Worker,
                Message        = message,
                Status         = "Applied",
                AppliedAt      = DateTime.UtcNow
            };
            await _db.FarmJoinRequests.AddAsync(joinRequest);
            await _db.SaveChangesAsync();
            await NotifyOwnerOfJoinRequestAsync(farm, joinRequest.Id, JoinApplicantRole.Worker);
            return (true, "Application submitted successfully. You can join once the owner approves.");
        }

        // ── Worker: My requests ───────────────────────────────────────────────

        public async Task<FarmJoinBrowseViewModel> GetManagerBrowseViewModelAsync(int managerUserId)
        {
            var farms = await _db.Farms
                .Where(f => !f.IsDeleted && f.IsActive && f.ApprovalStatus == ApprovalStatus.Approved)
                .ToListAsync();

            var activeMembership = await _db.FarmManagers
                .Include(m => m.Farm)
                .FirstOrDefaultAsync(m => m.ManagerUserId == managerUserId && m.IsActive && !m.IsDeleted);

            var myRequests = await _db.FarmJoinRequests
                .Where(r => r.WorkerUserId == managerUserId && r.ApplicantRole == JoinApplicantRole.Manager)
                .ToListAsync();

            var farmItems = farms.Select(f =>
            {
                var req = myRequests.FirstOrDefault(r => r.FarmId == f.Id);
                string appStatus = "None";
                DateTime? cooldown = null;

                if (activeMembership?.FarmId == f.Id)
                    appStatus = "Accepted";
                else if (req != null)
                {
                    appStatus = req.Status;
                    if (req.Status == "Rejected" && req.CanReApplyAt.HasValue && req.CanReApplyAt > DateTime.UtcNow)
                    {
                        appStatus = "Cooldown";
                        cooldown = req.CanReApplyAt;
                    }
                }

                return new FarmBrowseItem
                {
                    Id = f.Id,
                    Name = f.Name,
                    Location = f.Location,
                    ImagePath = f.ImagePath,
                    WorkerCount = 0,
                    ApplicationStatus = appStatus,
                    CooldownEnds = cooldown,
                    AlreadyJoined = activeMembership?.FarmId == f.Id
                };
            }).ToList();

            return new FarmJoinBrowseViewModel
            {
                Farms = farmItems,
                MyActiveFarmId = activeMembership?.FarmId,
                MyActiveFarmName = activeMembership?.Farm?.Name
            };
        }

        public async Task<(bool Success, string Message)> ApplyAsManagerAsync(int farmId, int managerUserId, string? message)
        {
            var alreadyMember = await _db.FarmManagers
                .AnyAsync(m => m.FarmId == farmId && m.ManagerUserId == managerUserId && m.IsActive && !m.IsDeleted);
            if (alreadyMember)
                return (false, "You are already a manager on this farm.");

            var otherFarm = await _db.FarmManagers
                .AnyAsync(m => m.ManagerUserId == managerUserId && m.IsActive && !m.IsDeleted && m.FarmId != farmId);
            if (otherFarm)
                return (false, "You can only manage one farm at a time. Leave your current farm first.");

            var farm = await _db.Farms.FirstOrDefaultAsync(f => f.Id == farmId && f.IsActive && !f.IsDeleted);
            if (farm == null)
                return (false, "Farm not found.");

            var existing = await _db.FarmJoinRequests
                .FirstOrDefaultAsync(r => r.FarmId == farmId && r.WorkerUserId == managerUserId && r.ApplicantRole == JoinApplicantRole.Manager);

            if (existing != null)
            {
                if (existing.Status == "Applied")
                    return (false, "Your request is already pending.");
                if (existing.Status == "Rejected" && existing.CanReApplyAt.HasValue && existing.CanReApplyAt > DateTime.UtcNow)
                    return (false, $"Cooldown active. Re-apply after {existing.CanReApplyAt.Value.ToLocalTime():MMM dd}.");

                existing.Status = "Applied";
                existing.Message = message;
                existing.AppliedAt = DateTime.UtcNow;
                existing.ReviewedAt = null;
                existing.OwnerNote = null;
                existing.CanReApplyAt = null;
                await _db.SaveChangesAsync();
                await NotifyOwnerOfJoinRequestAsync(farm, existing.Id, JoinApplicantRole.Manager);
                return (true, "Application sent to the farm owner.");
            }

            var joinRequest = new FarmJoinRequest
            {
                FarmId = farmId,
                WorkerUserId = managerUserId,
                ApplicantRole = JoinApplicantRole.Manager,
                Message = message,
                Status = "Applied",
                AppliedAt = DateTime.UtcNow
            };
            await _db.FarmJoinRequests.AddAsync(joinRequest);
            await _db.SaveChangesAsync();
            await NotifyOwnerOfJoinRequestAsync(farm, joinRequest.Id, JoinApplicantRole.Manager);
            return (true, "Application sent successfully. The owner must approve before you can manage the farm.");
        }

        public async Task<IEnumerable<MyJoinRequestViewModel>> GetManagerRequestsAsync(int managerUserId)
            => await _db.FarmJoinRequests
                .Where(r => r.WorkerUserId == managerUserId && r.ApplicantRole == JoinApplicantRole.Manager)
                .Include(r => r.Farm)
                .OrderByDescending(r => r.AppliedAt)
                .Select(r => new MyJoinRequestViewModel
                {
                    Id = r.Id,
                    FarmName = r.Farm!.Name,
                    Status = r.Status,
                    AppliedAt = r.AppliedAt,
                    ReviewedAt = r.ReviewedAt,
                    CooldownEnds = r.CanReApplyAt,
                    ReviewNote = r.OwnerNote
                })
                .ToListAsync();

        public async Task<bool> LeaveManagerAsync(int farmId, int managerUserId)
        {
            var membership = await _db.FarmManagers
                .FirstOrDefaultAsync(m => m.FarmId == farmId && m.ManagerUserId == managerUserId && m.IsActive && !m.IsDeleted);
            if (membership == null) return false;
            membership.IsActive = false;
            membership.LeftAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<MyJoinRequestViewModel>> GetMyRequestsAsync(int workerUserId)
        {
            return await _db.FarmJoinRequests
                .Where(r => r.WorkerUserId == workerUserId && r.ApplicantRole == JoinApplicantRole.Worker)
                .Include(r => r.Farm)
                .OrderByDescending(r => r.AppliedAt)
                .Select(r => new MyJoinRequestViewModel
                {
                    Id           = r.Id,
                    FarmName     = r.Farm!.Name,
                    Status       = r.Status,
                    AppliedAt    = r.AppliedAt,
                    ReviewedAt   = r.ReviewedAt,
                    CooldownEnds = r.CanReApplyAt,
                    ReviewNote   = r.OwnerNote
                })
                .ToListAsync();
        }

        // ── Worker: Leave farm ────────────────────────────────────────────────

        public async Task<bool> LeaveAsync(int farmId, int workerUserId)
        {
            var membership = await _db.FarmWorkers
                .FirstOrDefaultAsync(fw => fw.FarmId == farmId && fw.WorkerUserId == workerUserId && fw.IsActive);
            if (membership == null) return false;

            membership.IsActive = false;
            membership.LeftAt   = DateTime.UtcNow;

            // Also deactivate Worker profile if linked
            var profile = await _db.Workers.FirstOrDefaultAsync(w => w.UserId == workerUserId && w.FarmId == farmId && !w.IsDeleted);
            if (profile != null) profile.IsActive = false;

            // Reset active tasks assigned to the worker on this farm
            var activeStatuses = new string[] { 
                CattleFarm.Models.TaskStatus.Pending, 
                CattleFarm.Models.TaskStatus.Accepted, 
                CattleFarm.Models.TaskStatus.InProgress, 
                CattleFarm.Models.TaskStatus.ProofSubmitted, 
                CattleFarm.Models.TaskStatus.Rejected 
            };
            var workerProfileId = profile?.Id;
            var activeTasks = await _db.TaskAssignments
                .Where(t => t.FarmId == farmId && 
                            (t.AssignedUserId == workerUserId || (workerProfileId.HasValue && t.AssignedWorkerId == workerProfileId.Value)) &&
                            activeStatuses.Contains(t.Status) && 
                            !t.IsDeleted)
                .ToListAsync();

            foreach (var task in activeTasks)
            {
                task.Status = CattleFarm.Models.TaskStatus.Pending;
                task.AssignedWorkerId = null;
                task.AssignedUserId = 0;
                task.UpdatedAt = DateTime.UtcNow;
            }

            var farm = await _db.Farms.FindAsync(farmId);
            if (farm != null)
            {
                // Send notification to farm owner
                var workerName = profile?.FullName ?? "A worker";
                await _notifications.SendAsync(
                    farm.OwnerId,
                    "Worker left farm",
                    $"{workerName} has left the farm \"{farm.Name}\". {activeTasks.Count} assigned active tasks have been reset to Pending status.",
                    NotificationType.WorkerAlert,
                    "Farm",
                    farm.Id
                );
            }

            await _db.SaveChangesAsync();
            return true;
        }

        // ── Owner: Incoming requests ──────────────────────────────────────────

        public async Task<IEnumerable<IncomingRequestViewModel>> GetIncomingAsync(int ownerUserId)
        {
            var caller = await _db.Users.FindAsync(ownerUserId);
            var query = _db.FarmJoinRequests
                .Include(r => r.Farm)
                .Include(r => r.WorkerUser)
                .Where(r => r.Status == "Applied");

            if (caller?.Role != AppRoles.Admin)
            {
                query = query.Where(r => r.Farm!.OwnerId == ownerUserId);
            }

            return await query
                .OrderByDescending(r => r.AppliedAt)
                .Select(r => new IncomingRequestViewModel
                {
                    Id           = r.Id,
                    FarmId       = r.FarmId,
                    FarmName     = r.Farm!.Name,
                    WorkerUserId = r.WorkerUserId,
                    WorkerName   = r.WorkerUser!.FullName,
                    WorkerEmail  = r.WorkerUser.Email,
                    ApplicantRole = r.ApplicantRole,
                    Message      = r.Message,
                    Status       = r.Status,
                    AppliedAt    = r.AppliedAt
                })
                .ToListAsync();
        }

        // ── Owner: Accept request ─────────────────────────────────────────────

        public async Task<(bool Success, string Message)> AcceptAsync(int requestId, int ownerUserId)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var request = await _db.FarmJoinRequests
                    .Include(r => r.Farm)
                    .Include(r => r.WorkerUser)
                    .FirstOrDefaultAsync(r => r.Id == requestId && r.Status == "Applied");

                if (request == null)
                    return (false, "Request not found.");
                if (request.Farm == null || request.WorkerUser == null)
                    return (false, "Request data is incomplete.");

                var farm = request.Farm;
                var workerUser = request.WorkerUser;
                var caller = await _db.Users.FindAsync(ownerUserId);
                var isAdmin = caller?.Role == AppRoles.Admin;

                if (!isAdmin && farm.OwnerId != ownerUserId)
                    return (false, "This farm does not belong to you.");

                request.Status     = "Accepted";
                request.ReviewedAt = DateTime.UtcNow;

                if (request.ApplicantRole == JoinApplicantRole.Manager)
                {
                    var activeManagers = await _db.FarmManagers
                        .CountAsync(m => m.FarmId == request.FarmId && m.IsActive && !m.IsDeleted);
                    if (activeManagers >= 1)
                        return (false, "This farm already has an active manager.");

                    var otherMemberships = await _db.FarmManagers
                        .Where(m => m.ManagerUserId == request.WorkerUserId && m.IsActive && !m.IsDeleted)
                        .ToListAsync();
                    foreach (var m in otherMemberships)
                    {
                        m.IsActive = false;
                        m.LeftAt = DateTime.UtcNow;
                    }

                    var existingMgr = await _db.FarmManagers
                        .FirstOrDefaultAsync(m => m.FarmId == request.FarmId && m.ManagerUserId == request.WorkerUserId);

                    if (existingMgr != null)
                    {
                        existingMgr.IsActive = true;
                        existingMgr.LeftAt = null;
                        existingMgr.JoinedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        await _db.FarmManagers.AddAsync(new FarmManager
                        {
                            FarmId = request.FarmId,
                            ManagerUserId = request.WorkerUserId,
                            Position = "Farm Manager",
                            JoinedAt = DateTime.UtcNow,
                            IsActive = true
                        });
                    }

                    await _db.SaveChangesAsync();
                    await tx.CommitAsync();

                    await _notifications.SendAsync(
                        request.WorkerUserId,
                        "Manager request accepted",
                        $"You can now manage {farm.Name}.",
                        NotificationType.JoinAccepted,
                        nameof(FarmJoinRequest),
                        request.Id);

                    return (true, $"{workerUser.FullName} accepted as farm manager.");
                }

                var activeWorkerCount = await _db.FarmWorkers
                    .CountAsync(fw => fw.FarmId == request.FarmId && fw.IsActive);
                if (activeWorkerCount >= farm.MaximumWorkers)
                    return (false, "This farm has reached its maximum worker limit.");

                // Check if FarmWorker already exists (e.g., re-joining)
                var existing = await _db.FarmWorkers
                    .FirstOrDefaultAsync(fw => fw.FarmId == request.FarmId && fw.WorkerUserId == request.WorkerUserId);

                if (existing != null)
                {
                    existing.IsActive = true;
                    existing.LeftAt   = null;
                    existing.JoinedAt = DateTime.UtcNow;
                    existing.WorkerStatus = WorkerStatusType.Available;

                    var profile = await FindWorkerProfileForRequestAsync(request);
                    if (profile != null)
                    {
                        profile.UserId = request.WorkerUserId;
                        profile.FarmId = request.FarmId;
                        profile.ImagePath ??= request.WorkerUser?.ProfileImagePath;
                        profile.IsActive = true;
                        profile.UpdatedAt = DateTime.UtcNow;
                        existing.Position = profile.Role;
                        existing.Salary = profile.Salary;
                    }
                }
                else
                {
                    // Link to Worker profile if exists
                    var workerProfile = await FindWorkerProfileForRequestAsync(request);
                    if (workerProfile != null)
                    {
                        workerProfile.UserId = request.WorkerUserId;
                        workerProfile.IsActive = true;
                        workerProfile.FarmId = request.FarmId;
                        workerProfile.ImagePath ??= request.WorkerUser?.ProfileImagePath;
                        workerProfile.UpdatedAt = DateTime.UtcNow;
                    }

                    await _db.FarmWorkers.AddAsync(new FarmWorker
                    {
                        FarmId          = request.FarmId,
                        WorkerUserId    = request.WorkerUserId,
                        Position        = workerProfile?.Role ?? WorkerPosition.Feeder,
                        Salary          = workerProfile?.Salary ?? 0,
                        JoinedAt        = DateTime.UtcNow,
                        IsActive        = true
                    });

                    // If no Worker profile yet, create a basic one
                    if (workerProfile == null)
                    {
                        var newProfile = new Worker
                        {
                            FullName  = workerUser.FullName,
                            Role      = "Worker",
                            Email     = workerUser.Email,
                            Phone     = workerUser.PhoneNumber,
                            ImagePath = workerUser.ProfileImagePath,
                            FarmId    = request.FarmId,
                            UserId    = request.WorkerUserId,
                            IsActive  = true,
                            HiredAt   = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _db.Workers.AddAsync(newProfile);
                    }
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                await _notifications.SendAsync(
                    request.WorkerUserId,
                    "Join request accepted",
                    $"Your request to join {farm.Name} was accepted.",
                    NotificationType.JoinAccepted,
                    nameof(FarmJoinRequest),
                    request.Id);

                await _hub.Clients.Group(FarmDashboardHub.FarmGroup(request.FarmId))
                    .SendAsync("WorkerJoined", new { request.FarmId, request.WorkerUserId });

                return (true, $"{workerUser.FullName} has been accepted.");
            }
            catch
            {
                await tx.RollbackAsync();
                return (false, "Could not accept the request. Please try again.");
            }
        }

        // ── Owner: Reject request ─────────────────────────────────────────────

        public async Task<(bool Success, string Message)> RejectAsync(int requestId, int ownerUserId, string? note)
        {
            var request = await _db.FarmJoinRequests
                .Include(r => r.Farm)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.Status == "Applied");

            if (request == null) return (false, "Request not found.");
            if (request.Farm == null) return (false, "Request data is incomplete.");
            var caller = await _db.Users.FindAsync(ownerUserId);
            var isAdmin = caller?.Role == AppRoles.Admin;
            if (!isAdmin && request.Farm.OwnerId != ownerUserId) return (false, "This farm does not belong to you.");

            request.Status       = "Rejected";
            request.ReviewedAt   = DateTime.UtcNow;
            request.OwnerNote    = note;
            request.CanReApplyAt = DateTime.UtcNow.AddDays(7);

            await _db.SaveChangesAsync();

            await _notifications.SendAsync(
                request.WorkerUserId,
                "Join request rejected",
                $"Your request to join {request.Farm.Name} was rejected. You can apply again after 7 days.",
                NotificationType.JoinRejected,
                nameof(FarmJoinRequest),
                request.Id);

            return (true, "Request rejected (7-day cooldown before re-apply).");
        }

        // ── Owner: Remove worker ──────────────────────────────────────────────

        public async Task<bool> RemoveWorkerAsync(int farmWorkerId, int ownerUserId)
        {
            var fw = await _db.FarmWorkers
                .Include(fw => fw.Farm)
                .FirstOrDefaultAsync(fw => fw.Id == farmWorkerId && fw.IsActive);

            var caller = await _db.Users.FindAsync(ownerUserId);
            var isAdmin = caller?.Role == AppRoles.Admin;
            if (fw?.Farm == null || (!isAdmin && fw.Farm.OwnerId != ownerUserId)) return false;

            fw.IsActive = false;
            fw.LeftAt   = DateTime.UtcNow;
            fw.RemovedByOwner = true;

            var profile = await _db.Workers.FirstOrDefaultAsync(w => w.UserId == fw.WorkerUserId && w.FarmId == fw.FarmId && !w.IsDeleted);
            if (profile != null) profile.IsActive = false;

            await _db.SaveChangesAsync();
            await _notifications.SendAsync(
                fw.WorkerUserId,
                "Removed from farm",
                $"You were removed from {fw.Farm.Name}.",
                NotificationType.Warning,
                nameof(FarmWorker),
                fw.Id);
            return true;
        }

        public async Task<bool> RemoveManagerAsync(int farmManagerId, int ownerUserId)
        {
            var fm = await _db.FarmManagers
                .Include(m => m.Farm)
                .FirstOrDefaultAsync(m => m.Id == farmManagerId && m.IsActive && !m.IsDeleted);

            var caller = await _db.Users.FindAsync(ownerUserId);
            var isAdmin = caller?.Role == AppRoles.Admin;
            if (fm?.Farm == null || (!isAdmin && fm.Farm.OwnerId != ownerUserId)) return false;

            fm.IsActive = false;
            fm.LeftAt = DateTime.UtcNow;
            fm.RemovedByOwner = true;
            await _db.SaveChangesAsync();

            await _notifications.SendAsync(
                fm.ManagerUserId,
                "Removed from farm",
                $"You were removed as manager from {fm.Farm.Name}.",
                NotificationType.Warning,
                nameof(FarmManager),
                fm.Id);
            return true;
        }

        private async Task NotifyOwnerOfJoinRequestAsync(Farm farm, int requestId, string applicantRole = JoinApplicantRole.Worker)
        {
            var who = applicantRole == JoinApplicantRole.Manager ? "A manager" : "A worker";
            await _notifications.SendAsync(
                farm.OwnerId,
                "Farm join request",
                $"{who} applied to join {farm.Name}.",
                NotificationType.FarmJoinRequest,
                nameof(FarmJoinRequest),
                requestId);

            await _hub.Clients.Group(FarmDashboardHub.FarmGroup(farm.Id))
                .SendAsync("JoinRequestReceived", new { farmId = farm.Id, requestId });
        }

        private async Task<Worker?> FindWorkerProfileForRequestAsync(FarmJoinRequest request)
        {
            var email = request.WorkerUser?.Email;

            return await _db.Workers
                .Where(w =>
                    !w.IsDeleted &&
                    (w.UserId == request.WorkerUserId ||
                     (!string.IsNullOrWhiteSpace(email) &&
                      w.Email == email &&
                      w.FarmId == request.FarmId)))
                .OrderByDescending(w => w.FarmId == request.FarmId && w.Email == email)
                .ThenByDescending(w => w.Salary)
                .ThenByDescending(w => w.ImagePath != null)
                .FirstOrDefaultAsync();
        }

        // ── Owner: Create login for manually-added worker ─────────────────────

        public async Task<(bool Success, string Message)> CreateWorkerLoginAsync(
            CreateWorkerLoginViewModel model, int ownerUserId)
        {
            // Find the worker profile — must belong to owner's farm
            var worker = await _db.Workers
                .Include(w => w.Farm)
                .FirstOrDefaultAsync(w => w.Id == model.WorkerId
                                       && w.Farm!.OwnerId == ownerUserId
                                       && !w.IsDeleted);

            if (worker == null)
                return (false, "Worker not found or not on your farm.");

            if (worker.UserId.HasValue)
                return (false, "This worker already has a login account.");

            // Validate email format
            if (string.IsNullOrWhiteSpace(model.Email))
                return (false, "Email is required.");
            try
            {
                var addr = new System.Net.Mail.MailAddress(model.Email);
                if (addr.Address != model.Email)
                    return (false, "Invalid email format.");
            }
            catch
            {
                return (false, "Invalid email format.");
            }

            // Check email/username uniqueness
            if (await _db.Users.AnyAsync(u => u.Email == model.Email))
                return (false, "This email is already registered.");

            if (await _db.Users.AnyAsync(u => u.Username == model.Username))
                return (false, "This username is already taken.");

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // Create User account
                var user = new User
                {
                    Username        = model.Username,
                    FullName        = worker.FullName,
                    Email           = model.Email,
                    PasswordHash    = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Role            = AppRoles.Worker,
                    IsEmailVerified = true,
                    IsActive        = true,
                    PhoneNumber     = worker.Phone,
                    CreatedAt       = DateTime.UtcNow
                };
                await _db.Users.AddAsync(user);
                await _db.SaveChangesAsync(); // get user.Id

                // Link User → Worker profile
                worker.UserId    = user.Id;
                worker.UpdatedAt = DateTime.UtcNow;

                // Add FarmWorker entry so they appear as a member
                if (!worker.FarmId.HasValue)
                    return (false, "Worker is not linked to a farm yet.");

                var existingFarmWorker = await _db.FarmWorkers
                    .FirstOrDefaultAsync(fw => fw.FarmId == worker.FarmId.Value && fw.WorkerUserId == user.Id);

                if (existingFarmWorker == null)
                {
                    await _db.FarmWorkers.AddAsync(new FarmWorker
                    {
                        FarmId          = worker.FarmId.Value,
                        WorkerUserId    = user.Id,
                        Position        = worker.Role,
                        Salary          = worker.Salary,
                        JoinedAt        = DateTime.UtcNow,
                        IsActive        = true
                    });
                }
                else
                {
                    existingFarmWorker.Position = worker.Role;
                    existingFarmWorker.Salary = worker.Salary;
                    existingFarmWorker.IsActive = true;
                    existingFarmWorker.LeftAt = null;
                    existingFarmWorker.UpdatedAt = DateTime.UtcNow;
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return (true, $"Login created. Email: {model.Email}, Password: {model.Password}");
            }
            catch
            {
                await tx.RollbackAsync();
                return (false, "Could not create login.");
            }
        }
    }
}
