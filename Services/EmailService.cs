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

            // 🔍 DEBUG CONFIG
            _logger.LogInformation("🔍 SENDGRID_API_KEY présent ? {HasKey}", !string.IsNullOrWhiteSpace(apiKey));
            _logger.LogInformation("🔍 FROM_EMAIL = {From}", fromEmail);
            _logger.LogInformation("🔍 TO_EMAIL = {To}", toEmail);

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

            var htmlContent = $@"
<html>
<body style=""font-family: Arial; padding:20px;"">
<h2>Nouvelle demande de devis</h2>

<p><b>Nom:</b> {System.Net.WebUtility.HtmlEncode(devis.Nom)}</p>
<p><b>Email:</b> {System.Net.WebUtility.HtmlEncode(devis.Email)}</p>
<p><b>Téléphone:</b> {System.Net.WebUtility.HtmlEncode(devis.Telephone)}</p>
<p><b>Départ:</b> {System.Net.WebUtility.HtmlEncode(devis.Depart)}</p>
<p><b>Arrivée:</b> {System.Net.WebUtility.HtmlEncode(devis.Arrivee)}</p>
<p><b>Date:</b> {devis.DateHeure:dd/MM/yyyy HH:mm}</p>
<p><b>Message:</b><br/>
{System.Net.WebUtility.HtmlEncode(devis.Message ?? "Aucun")}</p>

</body>
</html>";

            var msg = MailHelper.CreateSingleEmail(
                from,
                to,
                subject,
                "Nouveau devis reçu",
                htmlContent
            );

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
                _logger.LogError(ex, "❌ ECHEC envoi email");
                throw;
            }
        }
    }
}
