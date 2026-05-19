using CattleFarm.Services.Interfaces;
using System.Text.Json;

namespace CattleFarm.Services.Implementations
{
    /// <summary>
    /// Integrates with SSLCommerz payment gateway via REST API.
    /// Uses sandbox mode when IsSandbox=true in appsettings.
    /// </summary>
    public class SslCommerzService : IPaymentGatewayService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _http;
        private readonly ILogger<SslCommerzService> _logger;

        public SslCommerzService(IConfiguration config, IHttpClientFactory httpFactory, ILogger<SslCommerzService> logger)
        {
            _config = config;
            _http = httpFactory.CreateClient("SSLCommerz");
            _logger = logger;
        }

        public async Task<PaymentInitResponse> InitiatePaymentAsync(PaymentInitRequest request)
        {
            var cfg = _config.GetSection("SSLCommerz");
            var storeId  = cfg["StoreId"]       ?? "";
            var storePass = cfg["StorePassword"] ?? "";
            var baseUrl  = cfg["BaseUrl"]        ?? "https://sandbox.sslcommerz.com/gwprocess/v4/api.php";

            var form = new Dictionary<string, string>
            {
                ["store_id"]       = storeId,
                ["store_passwd"]   = storePass,
                ["total_amount"]   = request.Amount.ToString("F2"),
                ["currency"]       = request.Currency,
                ["tran_id"]        = request.TransactionId,
                ["success_url"]    = request.SuccessUrl,
                ["fail_url"]       = request.FailUrl,
                ["cancel_url"]     = request.CancelUrl,
                ["ipn_url"]        = request.IpnUrl,
                ["cus_name"]       = request.UserName,
                ["cus_email"]      = request.UserEmail,
                ["cus_add1"]       = request.UserAddress,
                ["cus_city"]       = "Dhaka",
                ["cus_country"]    = "Bangladesh",
                ["cus_phone"]      = request.UserPhone,
                ["product_name"]   = $"CattleFarm {request.PlanName} Subscription",
                ["product_category"] = "Software Subscription",
                ["product_profile"]  = "general",
                ["shipping_method"]  = "NO",
                ["num_of_item"]      = "1",
                ["product_amount"]   = request.Amount.ToString("F2"),
                ["vat"]              = "0",
                ["discount_amount"]  = "0",
                ["convenience_fee"]  = "0",
            };

            try
            {
                var response = await _http.PostAsync(baseUrl, new FormUrlEncodedContent(form));
                var content  = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("SSLCommerz response: {Content}", content);

                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                var status = root.TryGetProperty("status", out var s) ? s.GetString() : null;
                if (status == "SUCCESS")
                {
                    return new PaymentInitResponse
                    {
                        Success    = true,
                        GatewayUrl = root.TryGetProperty("GatewayPageURL", out var g) ? g.GetString() : null,
                        SessionKey = root.TryGetProperty("sessionkey",     out var k) ? k.GetString() : null
                    };
                }

                var errMsg = root.TryGetProperty("failedreason", out var f) ? f.GetString() : "Unknown error";
                return new PaymentInitResponse { Success = false, Error = errMsg };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SSLCommerz initiation failed");
                return new PaymentInitResponse { Success = false, Error = ex.Message };
            }
        }

        public async Task<bool> ValidatePaymentAsync(string sessionKey, string transactionId, decimal amount)
        {
            var cfg = _config.GetSection("SSLCommerz");
            var validationUrl = cfg["ValidationUrl"]
                ?? "https://sandbox.sslcommerz.com/validator/api/validationserverAPI.php";

            var url = $"{validationUrl}?val_id={sessionKey}&store_id={cfg["StoreId"]}&store_passwd={cfg["StorePassword"]}&v=1&format=json";
            try
            {
                var response = await _http.GetAsync(url);
                var content  = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                var status = root.TryGetProperty("status", out var s) ? s.GetString() : "";
                return status == "VALID" || status == "VALIDATED";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SSLCommerz validation failed");
                return false;
            }
        }
    }
}
