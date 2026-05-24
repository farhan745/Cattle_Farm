using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;

namespace CattleFarm.Services.Implementations
{
    public class CurrencyService : ICurrencyService
    {
        private readonly CurrencySettings _defaultSettings;
        private readonly IWebHostEnvironment _env;
        private readonly string _settingsFilePath;
        private CurrencySettings? _cachedSettings;
        private readonly object _lock = new object();

        public CurrencyService(IOptions<CurrencySettings> defaultSettings, IWebHostEnvironment env)
        {
            _defaultSettings = defaultSettings.Value ?? new CurrencySettings();
            _env = env;
            _settingsFilePath = Path.Combine(_env.WebRootPath, "uploads", "system_settings.json");
            LoadSettings();
        }

        public string CurrencyCode => GetSettings().DefaultCurrency;
        public string CurrencySymbol => GetSettings().Symbol;

        public CurrencySettings GetSettings()
        {
            lock (_lock)
            {
                if (_cachedSettings == null)
                {
                    LoadSettings();
                }
                return _cachedSettings ?? _defaultSettings;
            }
        }

        public string Format(decimal amount)
        {
            var settings = GetSettings();
            CultureInfo culture;
            try
            {
                culture = new CultureInfo(settings.CultureCode);
            }
            catch
            {
                // Fallback to en-BD if not found, then en-US if en-BD is not supported by the OS
                try
                {
                    culture = new CultureInfo("en-BD");
                }
                catch
                {
                    culture = new CultureInfo("en-US");
                }
            }

            // Format numeric value with specified decimal places and culture
            string numeric = amount.ToString("N" + settings.DecimalPlaces, culture);

            // Return custom symbol with exact spacing
            return $"{settings.Symbol} {numeric}";
        }

        public string Format(decimal? amount)
        {
            if (!amount.HasValue)
                return $"{CurrencySymbol} 0.00";
            return Format(amount.Value);
        }

        public async Task UpdateSettingsAsync(CurrencySettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            lock (_lock)
            {
                _cachedSettings = settings;
            }

            // Ensure the upload directory exists
            var directory = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Serialize settings to dynamic JSON file
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(settings, options);
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }

        private void LoadSettings()
        {
            lock (_lock)
            {
                if (File.Exists(_settingsFilePath))
                {
                    try
                    {
                        string json = File.ReadAllText(_settingsFilePath);
                        _cachedSettings = JsonSerializer.Deserialize<CurrencySettings>(json) ?? _defaultSettings;
                        return;
                    }
                    catch
                    {
                        // Fallback on serialization error
                    }
                }
                _cachedSettings = _defaultSettings;
            }
        }
    }
}
