using System.Net;
using System.Net.Mail;
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
            var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST");
            var smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
            var smtpUser = Environment.GetEnvironmentVariable("SMTP_USER");
            var smtpPass = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
            var destinataire = Environment.GetEnvironmentVariable("SMTP_RECIPIENT");

            if (string.IsNullOrWhiteSpace(smtpHost) ||
                string.IsNullOrWhiteSpace(smtpUser) ||
                string.IsNullOrWhiteSpace(smtpPass) ||
                string.IsNullOrWhiteSpace(destinataire))
            {
                _logger.LogError("❌ Configuration SMTP incomplète");
                throw new InvalidOperationException("Les variables d'environnement SMTP sont manquantes.");
            }

            var subject = $"Nouveau devis de {WebUtility.HtmlEncode(devis.Nom)}";

            var body = $@"
            <html>
            <body style=""font-family: Arial, sans-serif; line-height: 1.6; padding: 20px; color: #333;"">
                <h2 style=""color: #007BFF;"">📩 Nouvelle demande de devis</h2>
                <p><strong>Nom :</strong> {WebUtility.HtmlEncode(devis.Nom)}</p>
                <p><strong>Email :</strong> {WebUtility.HtmlEncode(devis.Email)}</p>
                <p><strong>Téléphone :</strong> {WebUtility.HtmlEncode(devis.Telephone)}</p>
                <p><strong>Départ :</strong> {WebUtility.HtmlEncode(devis.Depart)}</p>
                <p><strong>Arrivée :</strong> {WebUtility.HtmlEncode(devis.Arrivee)}</p>
                <p><strong>Date/Heure :</strong> {devis.DateHeure:dd/MM/yyyy HH:mm}</p>
                <p><strong>Message :</strong><br />{WebUtility.HtmlEncode(devis.Message ?? "Aucun message.")}</p>

                <hr style=""margin-top: 30px;"" />
                <p style=""font-size: 0.9em; color: #999;"">
                Cet email a été généré automatiquement depuis le site VTC.
                </p>
            </body>
            </html>";

            try
            {
                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true,
                    Timeout = 10000 // 10 secondes timeout
                };

                using var message = new MailMessage(smtpUser, destinataire, subject, body)
                {
                    IsBodyHtml = true
                };

                await client.SendMailAsync(message);
                _logger.LogInformation("✅ Email envoyé à {Destinataire} pour {Client}",
                    destinataire, devis.Nom);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "❌ Erreur SMTP lors de l'envoi du devis");
                throw new InvalidOperationException("Impossible d'envoyer l'email. Veuillez réessayer.", ex);
            }
        }
    }
}