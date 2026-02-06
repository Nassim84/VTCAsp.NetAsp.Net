using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MonBackendVTC.Models;
using Microsoft.Extensions.Logging;

namespace MonBackendVTC.Services
{
    public class EmailService
    {
        private readonly HttpClient _http;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IHttpClientFactory factory, ILogger<EmailService> logger)
        {
            _http = factory.CreateClient();
            _logger = logger;
        }

        public async Task EnvoyerDevisAsync(DevisRequest devis)
        {
            var apiKey = Environment.GetEnvironmentVariable("BREVO_API_KEY");
            var senderEmail = Environment.GetEnvironmentVariable("BREVO_SENDER_EMAIL");
            var senderName = Environment.GetEnvironmentVariable("BREVO_SENDER_NAME");
            var toEmail = Environment.GetEnvironmentVariable("BREVO_RECIPIENT");

            _logger.LogInformation("📨 Début envoi devis pour {Nom}", devis.Nom);
            _logger.LogInformation("[BREVO] Envoi à : {To}", toEmail);

            if (string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(senderEmail) ||
                string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogError("❌ Variables d'environnement Brevo manquantes !");
                throw new InvalidOperationException("Config Brevo manquante");
            }

            // HTML mail responsive
            var html = $@"
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

            var payload = new
            {
                sender = new { name = senderName, email = senderEmail },
                to = new[] { new { email = toEmail, name = "Admin" } },
                subject = $"🚗 Nouveau devis de {devis.Nom}",
                htmlContent = html,
                replyTo = new { email = devis.Email, name = devis.Nom }
            };

            var json = JsonSerializer.Serialize(payload);
            var req = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
            req.Headers.Add("api-key", apiKey);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                _logger.LogInformation("📡 Appel API Brevo...");
                var res = await _http.SendAsync(req);
                var body = await res.Content.ReadAsStringAsync();

                _logger.LogInformation("[BREVO] Status: {Status}", res.StatusCode);
                _logger.LogInformation("[BREVO] Body: {Body}", body);

                if (!res.IsSuccessStatusCode)
                    throw new Exception($"Brevo send failed: {res.StatusCode}");

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
