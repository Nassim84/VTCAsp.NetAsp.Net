using System.ComponentModel.DataAnnotations;

namespace MonBackendVTC.Models
{
    public class DevisRequest
    {
        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Le nom doit contenir entre 2 et 100 caractères")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Format de téléphone invalide")]
        [StringLength(20)]
        public string Telephone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le lieu de départ est obligatoire")]
        [StringLength(200, MinimumLength = 2)]
        public string Depart { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le lieu d'arrivée est obligatoire")]
        [StringLength(200, MinimumLength = 2)]
        public string Arrivee { get; set; } = string.Empty;

        [Required(ErrorMessage = "La date et l'heure sont obligatoires")]
        public DateTime DateHeure { get; set; }

        [StringLength(1000, ErrorMessage = "Le message ne peut pas dépasser 1000 caractères")]
        public string? Message { get; set; }
    }
}