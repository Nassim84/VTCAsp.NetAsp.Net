using SendGrid;
using SendGrid.Helpers.Mail;
using MonBackendVTC.Models;

namespace MonBackendVTC.Services
{
    public class EmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task EnvoyerDevisAsync(DevisRequest devis)
        {
            _logger.LogInformation("📨 Début envoi devis pour {Nom}", devis.Nom);

            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
            var fromEmail = Environment.GetEnvironmentVariable("SENDGRID_FROM_EMAIL");
            var toEmail = Environment.GetEnvironmentVariable("SMTP_RECIPIENT");

            if (string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(fromEmail) ||
                string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogError("❌ Variables d'environnement SendGrid manquantes !");
                throw new InvalidOperationException("Config SendGrid manquante");
            }

            var client = new SendGridClient(apiKey);

            var from = new EmailAddress(fromEmail, "VTC NDrive");
            var to = new EmailAddress(toEmail);

            var subject = $"🚗 Nouveau devis de {devis.Nom}";

            // HTML Mail responsive et sécurisé
            var htmlContent = $@"
<html>
<head>
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
<style>
body {{ font-family: Arial, sans-serif; line-height:1.5; padding:20px; color:#333; }}
h2 {{ color:#007BFF; }}
p {{ margin:5px 0; }}
hr {{ margin:20px 0; border:none; border-top:1px solid #eee; }}
.footer {{ font-size:0.8em; color:#999; }}
</style>
</head>
<body>
<h2>📩 Nouvelle demande de devis</h2>

<p><strong>Nom :</strong> {System.Net.WebUtility.HtmlEncode(devis.Nom)}</p>
<p><strong>Email :</strong> {System.Net.WebUtility.HtmlEncode(devis.Email)}</p>
<p><strong>Téléphone :</strong> {System.Net.WebUtility.HtmlEncode(devis.Telephone)}</p>
<p><strong>Départ :</strong> {System.Net.WebUtility.HtmlEncode(devis.Depart)}</p>
<p><strong>Arrivée :</strong> {System.Net.WebUtility.HtmlEncode(devis.Arrivee)}</p>
<p><strong>Date :</strong> {devis.DateHeure:dd/MM/yyyy HH:mm}</p>
<p><strong>Message :</strong><br/>{System.Net.WebUtility.HtmlEncode(devis.Message ?? "Aucun message")}</p>

<hr/>
<p class=""footer"">Cet email a été généré automatiquement depuis le site VTC NDrive.</p>
</body>
</html>";

            var msg = MailHelper.CreateSingleEmail(
                from,
                to,
                subject,
                "Nouveau devis reçu", // plain text fallback
                htmlContent
            );

            // Répondre au client si besoin
            msg.ReplyTo = new EmailAddress(devis.Email, devis.Nom);

            try
            {
                _logger.LogInformation("📡 Appel SendGrid API...");
                var response = await client.SendEmailAsync(msg);

                var body = await response.Body.ReadAsStringAsync();

                _logger.LogInformation("📬 SendGrid Status = {Status}", response.StatusCode);
                _logger.LogInformation("📬 SendGrid Body = {Body}", body);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"SendGrid failed: {response.StatusCode}");
                }

                _logger.LogInformation("✅ Email envoyé avec succès !");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ECHEC envoi email pour {Nom}", devis.Nom);
                throw;
            }
        }
    }
}
