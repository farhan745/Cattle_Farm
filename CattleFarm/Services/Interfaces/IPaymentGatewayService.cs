namespace CattleFarm.Services.Interfaces
{
    public class PaymentInitRequest
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserPhone { get; set; } = "01700000000";
        public string UserAddress { get; set; } = "Dhaka, Bangladesh";
        public string PlanName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "BDT";
        public string TransactionId { get; set; } = string.Empty;
        public string SuccessUrl { get; set; } = string.Empty;
        public string FailUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
        public string IpnUrl { get; set; } = string.Empty;
    }

    public class PaymentInitResponse
    {
        public bool Success { get; set; }
        public string? GatewayUrl { get; set; }
        public string? SessionKey { get; set; }
        public string? Error { get; set; }
    }

    public interface IPaymentGatewayService
    {
        Task<PaymentInitResponse> InitiatePaymentAsync(PaymentInitRequest request);
        Task<bool> ValidatePaymentAsync(string sessionKey, string transactionId, decimal amount);
    }
}
