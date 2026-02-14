using System.ComponentModel.DataAnnotations;

namespace Nexo.Models.DTOs
{
    public class UsuarioCriarDto
    {
        [Required]
        [StringLength(100)]
        public required string Nome { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MinLength(6)]
        public required string Senha { get; set; }

        [Required]
        public int RoleId { get; set; }
    }
}
