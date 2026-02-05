using MonBackendVTC.Models;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;

namespace MonBackendVTC.Services
{
    public class EmailService
    {
        public void EnvoyerDevis(DevisRequest devis)
        {
            var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST");
            var smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587");
            var smtpUser = Environment.GetEnvironmentVariable("SMTP_USER");
            var smtpPass = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
            var destinataire = Environment.GetEnvironmentVariable("SMTP_RECIPIENT");

            Console.WriteLine("[SMTP TEST] Test connectivité...");

            try
            {
                using var tcp = new TcpClient();
                var task = tcp.ConnectAsync(smtpHost, smtpPort);
                task.Wait(5000);

                if (tcp.Connected)
                    Console.WriteLine("[SMTP TEST] ✅ SMTP reachable");
                else
                    Console.WriteLine("[SMTP TEST] ❌ SMTP NOT reachable");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[SMTP TEST] ❌ Exception: " + ex.Message);
            }

            if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPass) || string.IsNullOrWhiteSpace(destinataire))
            {
                throw new InvalidOperationException("Les variables d'environnement SMTP sont manquantes ou incomplètes.");
            }

            var subject = $"Nouveau devis de {devis.Nom}";

            var body = $@"
            <html>
            <body style=""font-family: Arial, sans-serif; line-height: 1.6; padding: 20px; color: #333;"">
                <h2 style=""color: #007BFF;"">📩 Nouvelle demande de devis</h2>
                <p><strong>Nom :</strong> {devis.Nom}</p>
                <p><strong>Email :</strong> {devis.Email}</p>
                <p><strong>Téléphone :</strong> {devis.Telephone}</p>
                <p><strong>Départ :</strong> {devis.Depart}</p>
                <p><strong>Arrivée :</strong> {devis.Arrivee}</p>
                <p><strong>Date/Heure :</strong> {devis.DateHeure:dd/MM/yyyy HH:mm}</p>
                <p><strong>Message :</strong><br />{(string.IsNullOrWhiteSpace(devis.Message) ? "Aucun message." : devis.Message)}</p>

                <hr style=""margin-top: 30px;"" />
                <p style=""font-size: 0.9em; color: #999;"">
                Cet email a été généré automatiquement depuis le site VTC.
                </p>
            </body>
            </html>";

            var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var message = new MailMessage(smtpUser, destinataire, subject, body)
            {
                IsBodyHtml = true
            };

            client.Send(message);
        }
    }
}