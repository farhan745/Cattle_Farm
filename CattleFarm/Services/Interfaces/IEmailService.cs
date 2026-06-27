namespace CattleFarm.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string toEmail, string toName);
        Task SendPaymentConfirmationAsync(string toEmail, string toName, string planName, string txId, decimal amount);
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
        Task SendDoctorInvitationAsync(string toEmail, string toName, string farmName, string inviteLink, DateTime expiresAt);
        Task SendDoctorWelcomeAsync(string toEmail, string toName, string loginUrl);
    }
}
