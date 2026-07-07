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
            var resendApiKey = Environment.GetEnvironmentVariable("RESEND_API_KEY") ?? _config["Email:ResendApiKey"];
            
            if (string.IsNullOrEmpty(resendApiKey))
            {
                throw new Exception("Resend API Key is not configured.");
            }

            var fromName = _config["Email:FromName"] ?? "Wishtra";
            var fromEmail = _config["Email:FromEmail"] ?? "onboarding@resend.dev";

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", resendApiKey);

            var payload = new
            {
                from = $"{fromName} <{fromEmail}>",
                to = new[] { toEmail },
                subject = "Reset your Wishtra password",
                html = BuildEmailBody(resetLink)
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.resend.com/emails", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to send email via Resend: {response.StatusCode} - {errorText}");
            }
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
                  "<div class=\"header\"><h1>🔐 Wishtra</h1><p>Password Reset Request</p></div>" +
                  "<div class=\"body\">" +
                    "<p>Hi there,</p>" +
                    "<p>We received a request to reset your Wishtra password. Click the button below to set a new password. This link will expire in <strong>1 hour</strong>.</p>" +
                    $"<a href=\"{resetLink}\" class=\"btn\">Reset My Password</a>" +
                    "<p class=\"note\">If you didn't request this, you can safely ignore this email. Your password will not change.</p>" +
                  "</div>" +
                  $"<div class=\"footer\">© {year} Wishtra — Made with ♥ for dreamers everywhere.</div>" +
                "</div>" +
                "</body></html>";
        }

    }
}
