using System.ComponentModel.DataAnnotations;

namespace Nexo.Models.DTOs
{
    public class UsuarioEditarDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Nome { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        public string? Senha { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        public bool Ativo { get; set; }
    }
}
