using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MonBackendVTC.Models;

namespace MonBackendVTC.Services
{
    public class EmailService
    {
        private readonly HttpClient _http;

        public EmailService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient();
        }

        public async Task EnvoyerDevis(DevisRequest devis)
        {
            var apiKey = Environment.GetEnvironmentVariable("BREVO_API_KEY");
            var senderEmail = Environment.GetEnvironmentVariable("BREVO_SENDER_EMAIL");
            var senderName = Environment.GetEnvironmentVariable("BREVO_SENDER_NAME");
            var toEmail = Environment.GetEnvironmentVariable("BREVO_RECIPIENT");

            Console.WriteLine("[BREVO] Envoi devis mail...");
            Console.WriteLine($"[BREVO] To: {toEmail}");

            var html = $@"
<h2>📩 Nouvelle demande de devis</h2>
<p><b>Nom :</b> {devis.Nom}</p>
<p><b>Email :</b> {devis.Email}</p>
<p><b>Téléphone :</b> {devis.Telephone}</p>
<p><b>Départ :</b> {devis.Depart}</p>
<p><b>Arrivée :</b> {devis.Arrivee}</p>
<p><b>Date :</b> {devis.DateHeure}</p>
<p><b>Message :</b> {devis.Message}</p>
";

            var payload = new
            {
                sender = new { name = senderName, email = senderEmail },
                to = new[] { new { email = toEmail, name = "Admin" } },
                subject = $"Nouveau devis de {devis.Nom}",
                htmlContent = html
            };

            var json = JsonSerializer.Serialize(payload);

            var req = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.brevo.com/v3/smtp/email"
            );

            req.Headers.Add("api-key", apiKey);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _http.SendAsync(req);
            var body = await res.Content.ReadAsStringAsync();

            Console.WriteLine("[BREVO] Status: " + res.StatusCode);
            Console.WriteLine("[BREVO] Response: " + body);

            if (!res.IsSuccessStatusCode)
                throw new Exception("Brevo send failed");
        }
    }
}
