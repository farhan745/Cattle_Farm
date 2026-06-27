using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CattleFarm.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CattleFarm.Services.Implementations
{
    public class SmsService : ISmsService
    {
        private readonly ILogger<SmsService> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public SmsService(ILogger<SmsService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task<bool> SendSmsAsync(string toPhoneNumber, string message)
        {
            var sid = _configuration["Twilio:AccountSid"];
            var token = _configuration["Twilio:AuthToken"];
            var from = _configuration["Twilio:FromPhoneNumber"];

            if (string.IsNullOrWhiteSpace(sid) || sid.Contains("YOUR_TWILIO_ACCOUNT_SID") ||
                string.IsNullOrWhiteSpace(token) || token.Contains("YOUR_TWILIO_AUTH_TOKEN") ||
                string.IsNullOrWhiteSpace(from) || from.Contains("YOUR_TWILIO_FROM_PHONE_NUMBER"))
            {
                _logger.LogWarning("Twilio SMS is not configured in settings. SMS STUB [To: {Phone}]: {Msg}", toPhoneNumber, message);
                return true;
            }

            try
            {
                var requestUrl = $"https://api.twilio.com/2010-04-01/Accounts/{sid}/Messages.json";
                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                
                var byteArray = Encoding.ASCII.GetBytes($"{sid}:{token}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                var postData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("To", toPhoneNumber),
                    new KeyValuePair<string, string>("From", from),
                    new KeyValuePair<string, string>("Body", message)
                };

                request.Content = new FormUrlEncodedContent(postData);

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Twilio SMS sent successfully to {Phone}", toPhoneNumber);
                    return true;
                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to send Twilio SMS to {Phone}. Status Code: {Status}. Error: {Error}", 
                        toPhoneNumber, response.StatusCode, responseBody);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending Twilio SMS to {Phone}", toPhoneNumber);
                return false;
            }
        }
    }
}

