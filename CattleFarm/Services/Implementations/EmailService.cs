using CattleFarm.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CattleFarm.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;
        private readonly ICurrencyService _currency;

        public EmailService(IConfiguration config, ILogger<EmailService> logger, ICurrencyService currency)
        {
            _config = config;
            _logger = logger;
            _currency = currency;
        }

        private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            var cfg = _config.GetSection("Email");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(cfg["FromName"], cfg["FromAddress"]));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(cfg["Host"], int.Parse(cfg["Port"] ?? "587"), SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(cfg["Username"], cfg["Password"]);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                _logger.LogInformation("Email sent to {Email} — {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string toName)
        {
            var html = $@"
<!DOCTYPE html><html><body style=""background:#FAF8F4;font-family:'Segoe UI',Arial,sans-serif;"">
  <table width=""100%""><tr><td align=""center"" style=""padding:40px 20px;"">
    <table width=""560"" style=""background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);"">
      <tr><td style=""background:linear-gradient(135deg,#1C3A2A,#2D5A3D);padding:40px;text-align:center;"">
        <div style=""font-size:40px;"">🐄</div>
        <div style=""color:#fff;font-size:22px;font-weight:700;"">Welcome to Smart Cattle Farm!</div>
      </td></tr>
      <tr><td style=""padding:48px;"">
        <p style=""color:#1A1A18;font-size:17px;"">Hi <strong>{toName}</strong>, welcome aboard! 🎉</p>
        <p style=""color:#6B6B66;line-height:1.7;"">Your account is ready. Start managing your farm, tracking cattle health, monitoring milk production, and much more.</p>
        <div style=""text-align:center;margin:32px 0;"">
          <a href=""https://localhost:7170"" style=""background:#C8761A;color:#fff;padding:14px 36px;border-radius:8px;text-decoration:none;font-weight:700;"">Go to Dashboard →</a>
        </div>
      </td></tr>
      <tr><td style=""background:#F2EDE4;padding:24px;text-align:center;"">
        <p style=""color:#6B6B66;font-size:12px;margin:0;"">© {DateTime.Now.Year} Smart Cattle Farm</p>
      </td></tr>
    </table>
  </td></tr></table>
</body></html>";
            await SendAsync(toEmail, toName, "Welcome to Smart Cattle Farm! 🐄", html);
        }

        public async Task SendPaymentConfirmationAsync(string toEmail, string toName, string planName, string txId, decimal amount)
        {
            var html = $@"
<!DOCTYPE html><html><body style=""background:#FAF8F4;font-family:'Segoe UI',Arial,sans-serif;"">
  <table width=""100%""><tr><td align=""center"" style=""padding:40px 20px;"">
    <table width=""560"" style=""background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);"">
      <tr><td style=""background:linear-gradient(135deg,#1C3A2A,#2D5A3D);padding:40px;text-align:center;"">
        <div style=""font-size:40px;"">✅</div>
        <div style=""color:#fff;font-size:22px;font-weight:700;"">Payment Confirmed!</div>
      </td></tr>
      <tr><td style=""padding:48px;"">
        <p style=""color:#1A1A18;font-size:17px;"">Hi <strong>{toName}</strong>,</p>
        <p style=""color:#6B6B66;"">Your subscription payment has been confirmed. Here are your details:</p>
        <table width=""100%"" style=""background:#F2EDE4;border-radius:12px;padding:24px;margin:24px 0;"">
          <tr><td style=""padding:8px 0;""><span style=""color:#6B6B66;"">Plan</span></td><td style=""text-align:right;font-weight:700;color:#1C3A2A;"">{planName}</td></tr>
          <tr><td style=""padding:8px 0;""><span style=""color:#6B6B66;"">Amount</span></td><td style=""text-align:right;font-weight:700;color:#C8761A;"">{_currency.Format(amount)}</td></tr>
          <tr><td style=""padding:8px 0;""><span style=""color:#6B6B66;"">Transaction ID</span></td><td style=""text-align:right;font-size:12px;color:#3D3D3A;"">{txId}</td></tr>
          <tr><td style=""padding:8px 0;""><span style=""color:#6B6B66;"">Date</span></td><td style=""text-align:right;color:#3D3D3A;"">{DateTime.Now:MMMM dd, yyyy}</td></tr>
        </table>
        <div style=""text-align:center;"">
          <a href=""https://localhost:7170/Subscription"" style=""background:#C8761A;color:#fff;padding:14px 36px;border-radius:8px;text-decoration:none;font-weight:700;"">View My Subscription →</a>
        </div>
      </td></tr>
      <tr><td style=""background:#F2EDE4;padding:24px;text-align:center;"">
        <p style=""color:#6B6B66;font-size:12px;margin:0;"">© {DateTime.Now.Year} Smart Cattle Farm</p>
      </td></tr>
    </table>
  </td></tr></table>
</body></html>";
            await SendAsync(toEmail, toName, $"Payment Confirmed — {planName} Plan Active ✓", html);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var html = $@"
<!DOCTYPE html><html><body style=""background:#FAF8F4;font-family:'Segoe UI',Arial,sans-serif;"">
  <table width=""100%""><tr><td align=""center"" style=""padding:40px 20px;"">
    <table width=""560"" style=""background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);"">
      <tr><td style=""background:linear-gradient(135deg,#1C3A2A,#2D5A3D);padding:40px;text-align:center;"">
        <div style=""font-size:40px;"">🔑</div>
        <div style=""color:#fff;font-size:22px;font-weight:700;"">Reset Your Password</div>
      </td></tr>
      <tr><td style=""padding:48px;"">
        <p style=""color:#1A1A18;font-size:17px;"">Hello,</p>
        <p style=""color:#6B6B66;line-height:1.7;"">We received a request to reset your password for your Smart Cattle Farm account. If you did not make this request, you can safely ignore this email.</p>
        <p style=""color:#6B6B66;line-height:1.7;"">To reset your password, please click the button below. This link is valid for 1 hour.</p>
        <div style=""text-align:center;margin:32px 0;"">
          <a href=""{resetLink}"" style=""background:#C8761A;color:#fff;padding:14px 36px;border-radius:8px;text-decoration:none;font-weight:700;display:inline-block;"">Reset Password</a>
        </div>
        <p style=""color:#8C8C85;font-size:13px;line-height:1.5;margin-top:24px;"">If the button above doesn't work, copy and paste this URL into your browser:<br/><span style=""color:#C8761A;word-break:break-all;"">{resetLink}</span></p>
      </td></tr>
      <tr><td style=""background:#F2EDE4;padding:24px;text-align:center;"">
        <p style=""color:#6B6B66;font-size:12px;margin:0;"">© {DateTime.Now.Year} Smart Cattle Farm</p>
      </td></tr>
    </table>
  </td></tr></table>
</body></html>";
            await SendAsync(toEmail, "Valued User", "Reset Your Password — Smart Cattle Farm", html);
        }
    }
}
