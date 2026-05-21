namespace CattleFarm.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string toEmail, string toName);
        Task SendPaymentConfirmationAsync(string toEmail, string toName, string planName, string txId, decimal amount);
    }
}
