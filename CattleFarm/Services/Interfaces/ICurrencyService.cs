using CattleFarm.Models;

namespace CattleFarm.Services.Interfaces
{
    public interface ICurrencyService
    {
        string CurrencyCode { get; }
        string CurrencySymbol { get; }
        string Format(decimal amount);
        string Format(decimal? amount);
        CurrencySettings GetSettings();
        Task UpdateSettingsAsync(CurrencySettings settings);
    }
}
