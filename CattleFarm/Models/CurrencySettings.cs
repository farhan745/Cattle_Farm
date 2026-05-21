namespace CattleFarm.Models
{
    public class CurrencySettings
    {
        public string DefaultCurrency { get; set; } = "BDT";
        public string Symbol { get; set; } = "৳";
        public string CultureCode { get; set; } = "en-BD";
        public int DecimalPlaces { get; set; } = 2;
    }
}
