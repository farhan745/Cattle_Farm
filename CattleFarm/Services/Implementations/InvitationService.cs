using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using CattleFarm.ViewModels;
using Microsoft.Extensions.Logging;

namespace CattleFarm.Services.Implementations
{
    public class InvitationService : IInvitationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IEmailService _emailService;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<InvitationService> _logger;

        public InvitationService(
            IUnitOfWork uow, 
            IEmailService emailService, 
            IAuditService auditService,
            INotificationService notificationService,
            ILogger<InvitationService> logger)
        {
            _uow = uow;
            _emailService = emailService;
            _auditService = auditService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<DoctorInvitation> CreateInvitationAsync(CreateDoctorInvitationVM vm, int createdByUserId, string baseUrl)
        {
            // Verify if email has already been invited
            bool alreadyInvited = await _uow.DoctorInvitations.IsEmailAlreadyInvitedAsync(vm.Email);
            if (alreadyInvited)
            {
                throw new InvalidOperationException($"An active invitation already exists for {vm.Email}.");
            }

            var email = vm.Email.Trim();
            var existingUser = await _uow.Users.GetByEmailAsync(email);
            if (existingUser != null && existingUser.Role == AppRoles.Doctor)
            {
                throw new InvalidOperationException($"A veterinarian account already exists for {email}.");
            }

            var invitation = new DoctorInvitation
            {
                Token = Guid.NewGuid().ToString("N"),
                DoctorName = vm.DoctorName,
                Email = email,
                PhoneNumber = vm.PhoneNumber,
                FarmId = null,
                CreatedByUserId = createdByUserId,
                Notes = vm.Notes,
                ExpectedJoiningDate = vm.ExpectedJoiningDate,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsUsed = false,
                InvitationStatus = InvitationStatus.Pending
            };

            await _uow.DoctorInvitations.AddAsync(invitation);
            await _uow.SaveChangesAsync();

            const string farmName = "Smart Cattle Farm";

            // Generate link
            string inviteLink = $"{baseUrl.TrimEnd('/')}/Doctor/CompleteProfile?token={invitation.Token}";

            // Send Email
            try
            {
                await _emailService.SendDoctorInvitationAsync(invitation.Email, invitation.DoctorName, farmName, inviteLink, invitation.ExpiresAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send doctor invitation email to {Email}", invitation.Email);
                // We still save the invitation in DB, but alert via logs
            }

            // Log activity and send notification
            await _auditService.LogActivityAsync(createdByUserId, $"Sent doctor invitation to Dr. {invitation.DoctorName} ({invitation.Email})", "DoctorInvitation", invitation.Id);
            await _notificationService.SendAsync(createdByUserId, "Invitation Sent", $"Invitation sent to Dr. {invitation.DoctorName}.", NotificationType.DoctorInvitationSent, "DoctorInvitation", invitation.Id);

            return invitation;
        }

        public async Task<DoctorInvitation?> GetByTokenAsync(string token)
        {
            return await _uow.DoctorInvitations.GetByTokenAsync(token);
        }

        public async Task<DoctorInvitation?> GetByIdAsync(int id)
        {
            return await _uow.DoctorInvitations.GetByIdAsync(id);
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            var invitation = await _uow.DoctorInvitations.GetByTokenAsync(token);
            if (invitation == null || invitation.IsUsed || invitation.InvitationStatus != InvitationStatus.Pending)
            {
                return false;
            }

            if (invitation.ExpiresAt < DateTime.UtcNow)
            {
                // Soft transition status to Expired
                invitation.InvitationStatus = InvitationStatus.Expired;
                _uow.DoctorInvitations.Update(invitation);
                await _uow.SaveChangesAsync();
                return false;
            }

            return true;
        }

        public async Task<(IEnumerable<DoctorInvitation> Items, int Total)> GetPagedAsync(int page, int size, string? search)
        {
            // Automate check of expired pending invitations on load
            var pendingExpired = await _uow.DoctorInvitations.GetPagedAsync(1, 100);
            bool updatedAny = false;
            foreach (var invite in pendingExpired.Items)
            {
                if (invite.InvitationStatus == InvitationStatus.Pending && invite.ExpiresAt < DateTime.UtcNow)
                {
                    invite.InvitationStatus = InvitationStatus.Expired;
                    _uow.DoctorInvitations.Update(invite);
                    updatedAny = true;
                }
            }
            if (updatedAny)
            {
                await _uow.SaveChangesAsync();
            }

            return await _uow.DoctorInvitations.GetPagedAsync(page, size, search);
        }

        public async Task<bool> ResendInvitationAsync(int id, string baseUrl)
        {
            var invitation = await _uow.DoctorInvitations.GetByIdAsync(id);
            if (invitation == null || invitation.IsUsed || invitation.InvitationStatus == InvitationStatus.Accepted)
            {
                return false;
            }

            // Refresh token and expiry
            invitation.Token = Guid.NewGuid().ToString("N");
            invitation.ExpiresAt = DateTime.UtcNow.AddDays(7);
            invitation.InvitationStatus = InvitationStatus.Pending;
            invitation.CreatedAt = DateTime.UtcNow;

            _uow.DoctorInvitations.Update(invitation);
            await _uow.SaveChangesAsync();

            // Retrieve farm name
            string farmName = "Smart Cattle Farm";
            if (invitation.FarmId.HasValue)
            {
                var farm = await _uow.Farms.GetByIdAsync(invitation.FarmId.Value);
                if (farm != null)
                {
                    farmName = farm.Name;
                }
            }

            string inviteLink = $"{baseUrl.TrimEnd('/')}/Doctor/CompleteProfile?token={invitation.Token}";

            try
            {
                await _emailService.SendDoctorInvitationAsync(invitation.Email, invitation.DoctorName, farmName, inviteLink, invitation.ExpiresAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resend doctor invitation email to {Email}", invitation.Email);
            }

            await _auditService.LogActivityAsync(invitation.CreatedByUserId, $"Resent doctor invitation to Dr. {invitation.DoctorName} ({invitation.Email})", "DoctorInvitation", invitation.Id);
            await _notificationService.SendAsync(invitation.CreatedByUserId, "Invitation Resent", $"Resent invitation to Dr. {invitation.DoctorName}.", NotificationType.DoctorInvitationSent, "DoctorInvitation", invitation.Id);

            return true;
        }

        public async Task<bool> RevokeInvitationAsync(int id, int revokedByUserId)
        {
            var invitation = await _uow.DoctorInvitations.GetByIdAsync(id);
            if (invitation == null || invitation.IsUsed || invitation.InvitationStatus == InvitationStatus.Accepted)
            {
                return false;
            }

            invitation.InvitationStatus = InvitationStatus.Revoked;
            invitation.RevokedAt = DateTime.UtcNow;
            invitation.RevokedByUserId = revokedByUserId;

            _uow.DoctorInvitations.Update(invitation);
            await _uow.SaveChangesAsync();

            await _auditService.LogActivityAsync(revokedByUserId, $"Revoked doctor invitation for Dr. {invitation.DoctorName} ({invitation.Email})", "DoctorInvitation", invitation.Id);

            return true;
        }
    }
}
