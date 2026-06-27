namespace CattleFarm.Services.Interfaces
{
    public interface ISmsService
    {
        Task<bool> SendSmsAsync(string toPhoneNumber, string message);
    }
}
