using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MonBackendVTC.Models;
using MonBackendVTC.Services;

namespace MonBackendVTC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("devis")]
    public class DevisController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly ILogger<DevisController> _logger;

        public DevisController(EmailService emailService, ILogger<DevisController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Envoyer([FromBody] DevisRequest devis)
        {
            _logger.LogInformation("📩 Nouvelle demande reçue de {Nom}", devis.Nom);

            // Validation du modèle
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("❌ Modèle invalide pour {Nom}", devis.Nom);
                return BadRequest(ModelState);
            }

            // Capture de DateTime.Now UNE SEULE FOIS
            var maintenant = DateTime.Now;

            // Validations métier
            if (devis.Depart?.Trim().Equals(devis.Arrivee?.Trim(), StringComparison.OrdinalIgnoreCase) == true)
            {
                _logger.LogWarning("⚠️ Départ et arrivée identiques pour {Nom}", devis.Nom);
                return BadRequest(new { message = "Le départ et l'arrivée ne peuvent pas être identiques." });
            }

            if (devis.DateHeure <= maintenant)
            {
                _logger.LogWarning("⚠️ Date passée pour {Nom}", devis.Nom);
                return BadRequest(new { message = "La date de départ doit être dans le futur." });
            }

            if (devis.DateHeure > maintenant.AddYears(1))
            {
                _logger.LogWarning("⚠️ Date trop éloignée pour {Nom}", devis.Nom);
                return BadRequest(new { message = "La date ne peut pas dépasser 1 an." });
            }

            try
            {
                await _emailService.EnvoyerDevisAsync(devis);
                _logger.LogInformation("✅ Devis traité avec succès pour {Nom}", devis.Nom);

                return Ok(new
                {
                    message = "Devis envoyé avec succès. Nous vous recontacterons rapidement.",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de l'envoi du devis pour {Nom}", devis.Nom);

                return StatusCode(500, new
                {
                    message = "Erreur serveur. Veuillez réessayer ou nous contacter directement."
                });
            }
        }
    }
}