using Microsoft.AspNetCore.Mvc;
using MonBackendVTC.Models;
using MonBackendVTC.Services;

namespace MonBackendVTC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevisController : ControllerBase
    {
        private readonly EmailService _emailService;

        public DevisController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost]
        public IActionResult Envoyer([FromBody] DevisRequest devis)
        {
            Console.WriteLine($"[DevisController] 📩 Nouvelle demande reçue de {devis.Nom} ({devis.Email})");

            // Vérifie que le modèle est valide
            if (!ModelState.IsValid)
            {
                Console.WriteLine("[DevisController] ❌ Modèle invalide");
                return BadRequest(ModelState);
            }

            // Validation métier personnalisée
            if (devis.Depart == devis.Arrivee)
            {
                Console.WriteLine("[DevisController] ⚠️ Lieu de départ et d'arrivée identiques");
                return BadRequest(new { message = "Le départ et l'arrivée ne peuvent pas être identiques." });
            }

            if (devis.DateHeure <= DateTime.Now)
            {
                Console.WriteLine("[DevisController] ⚠️ Date de départ passée");
                return BadRequest(new { message = "La date de départ doit être dans le futur." });
            }

            try
            {
                _emailService.EnvoyerDevis(devis);
                Console.WriteLine("[DevisController] ✅ Email envoyé avec succès !");
                return Ok(new { message = "Devis envoyé avec succès." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DevisController] ❌ Erreur lors de l'envoi du mail : {ex.Message}");
                return StatusCode(500, new { message = "Erreur serveur : impossible d'envoyer le devis." });
            }
        }
    }
}
