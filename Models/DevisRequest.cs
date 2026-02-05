using System.ComponentModel.DataAnnotations;

namespace MonBackendVTC.Models
{
    public class DevisRequest
    {
        [Required]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Telephone { get; set; } = string.Empty;

        [Required]
        public string Depart { get; set; } = string.Empty;

        [Required]
        public string Arrivee { get; set; } = string.Empty;

        [Required]
        public DateTime DateHeure { get; set; }

        public string? Message { get; set; }
    }
}
