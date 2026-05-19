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

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
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

        public async Task SendOtpEmailAsync(string toEmail, string toName, string otp, string purpose)
        {
            var title = purpose == "EmailVerify" ? "Email Verification" : "Password Reset";
            var action = purpose == "EmailVerify"
                ? "verify your email address and activate your account"
                : "reset your password";

            var html = $@"
<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""/></head>
<body style=""margin:0;padding:0;background:#FAF8F4;font-family:'Segoe UI',Arial,sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
    <tr><td align=""center"" style=""padding:40px 20px;"">
      <table width=""560"" cellpadding=""0"" cellspacing=""0"" style=""background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);"">
        <!-- Header -->
        <tr><td style=""background:linear-gradient(135deg,#1C3A2A 0%,#2D5A3D 100%);padding:40px 48px;text-align:center;"">
          <div style=""font-size:40px;margin-bottom:12px;"">🐄</div>
          <div style=""color:#ffffff;font-size:22px;font-weight:700;letter-spacing:2px;"">SMART CATTLE FARM</div>
          <div style=""color:rgba(255,255,255,0.7);font-size:13px;margin-top:4px;"">{title}</div>
        </td></tr>
        <!-- Body -->
        <tr><td style=""padding:48px;"">
          <p style=""color:#3D3D3A;font-size:16px;margin:0 0 16px;"">Hello <strong>{toName}</strong>,</p>
          <p style=""color:#6B6B66;font-size:15px;line-height:1.6;margin:0 0 32px;"">
            Use the code below to {action}. This code expires in <strong>10 minutes</strong>.
          </p>
          <!-- OTP Box -->
          <div style=""background:#F2EDE4;border:2px solid #DDD5C8;border-radius:12px;padding:32px;text-align:center;margin:0 0 32px;"">
            <div style=""font-size:11px;color:#6B6B66;letter-spacing:3px;text-transform:uppercase;margin-bottom:12px;"">Your Verification Code</div>
            <div style=""font-size:48px;font-weight:800;letter-spacing:16px;color:#1C3A2A;font-family:'Courier New',monospace;"">{otp}</div>
          </div>
          <div style=""background:#fff8e1;border-left:4px solid #C8761A;padding:16px 20px;border-radius:0 8px 8px 0;margin-bottom:24px;"">
            <p style=""margin:0;color:#6B6B66;font-size:13px;"">⚠️ <strong>Never share this code with anyone.</strong> Smart Cattle Farm staff will never ask for your OTP.</p>
          </div>
          <p style=""color:#6B6B66;font-size:13px;margin:0;"">If you didn't request this, please ignore this email or contact support.</p>
        </td></tr>
        <!-- Footer -->
        <tr><td style=""background:#F2EDE4;padding:24px 48px;text-align:center;"">
          <p style=""color:#6B6B66;font-size:12px;margin:0;"">© {DateTime.Now.Year} Smart Cattle Farm · All rights reserved</p>
        </td></tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";
            await SendAsync(toEmail, toName, $"[Smart Cattle Farm] Your {title} Code: {otp}", html);
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
          <tr><td style=""padding:8px 0;""><span style=""color:#6B6B66;"">Amount</span></td><td style=""text-align:right;font-weight:700;color:#C8761A;"">৳{amount:N0}</td></tr>
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
    }
}
