namespace CattleFarm.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string toName, string otp, string purpose);
        Task SendWelcomeEmailAsync(string toEmail, string toName);
        Task SendPaymentConfirmationAsync(string toEmail, string toName, string planName, string txId, decimal amount);
    }
}
