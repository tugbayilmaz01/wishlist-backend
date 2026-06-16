using System.Net;
using System.Net.Mail;

namespace WishlistApi.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? _config["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? _config["Email:SmtpPort"] ?? "587");
            var smtpUser = Environment.GetEnvironmentVariable("SMTP_USER") ?? _config["Email:SmtpUser"] ?? "";
            var smtpPass = (Environment.GetEnvironmentVariable("SMTP_PASS") ?? _config["Email:SmtpPass"] ?? "").Replace(" ", "");
            var fromName = _config["Email:FromName"] ?? "WishIt";

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUser, smtpPass)
            };

            var mail = new MailMessage
            {
                From = new MailAddress(smtpUser, fromName),
                Subject = "Reset your WishIt password",
                IsBodyHtml = true,
                Body = BuildEmailBody(resetLink)
            };
            mail.To.Add(toEmail);

            await client.SendMailAsync(mail);
        }

        private static string BuildEmailBody(string resetLink)
        {
            var year = DateTime.UtcNow.Year;
            return
                "<!DOCTYPE html>" +
                "<html><head><meta charset=\"UTF-8\" /><style>" +
                "body { font-family: 'Segoe UI', Arial, sans-serif; background: #faf6f4; margin: 0; padding: 0; }" +
                ".wrapper { max-width: 520px; margin: 40px auto; background: #fff; border-radius: 24px; overflow: hidden; box-shadow: 0 8px 30px rgba(56,22,31,0.08); }" +
                ".header { background: linear-gradient(135deg, #ff425d, #ff6b7a); padding: 36px 40px; text-align: center; }" +
                ".header h1 { color: #fff; margin: 0; font-size: 28px; letter-spacing: -0.5px; }" +
                ".header p { color: rgba(255,255,255,0.85); margin: 6px 0 0; font-size: 14px; }" +
                ".body { padding: 40px; }" +
                ".body p { color: #5a4a52; line-height: 1.7; font-size: 15px; margin: 0 0 20px; }" +
                ".btn { display: block; width: fit-content; margin: 0 auto 24px; padding: 14px 36px; background: linear-gradient(135deg, #ff425d, #ff6b7a); color: #fff !important; text-decoration: none; border-radius: 14px; font-weight: 700; font-size: 15px; }" +
                ".note { font-size: 13px !important; color: #b0929a !important; text-align: center; }" +
                ".footer { background: #fdfaf7; padding: 20px 40px; text-align: center; font-size: 12px; color: #b0929a; border-top: 1px solid #f0e6e9; }" +
                "</style></head><body>" +
                "<div class=\"wrapper\">" +
                  "<div class=\"header\"><h1>🔐 WishIt</h1><p>Password Reset Request</p></div>" +
                  "<div class=\"body\">" +
                    "<p>Hi there,</p>" +
                    "<p>We received a request to reset your WishIt password. Click the button below to set a new password. This link will expire in <strong>1 hour</strong>.</p>" +
                    $"<a href=\"{resetLink}\" class=\"btn\">Reset My Password</a>" +
                    "<p class=\"note\">If you didn't request this, you can safely ignore this email. Your password will not change.</p>" +
                  "</div>" +
                  $"<div class=\"footer\">© {year} WishIt — Made with ♥ for dreamers everywhere.</div>" +
                "</div>" +
                "</body></html>";
        }

    }
}
