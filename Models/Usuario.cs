using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexo.Models;

[Table("usuarios")]
public class Usuario
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public required string Nome { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public required string Email { get; set; }

    [Required]
    public required string Senha { get; set; }

    [Required]
    public bool Ativo { get; set; } = true;

    [Required]
    public int RoleId { get; set; }

    [ForeignKey(nameof(RoleId))]
    public virtual Role Role { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? DataAtualizacao { get; set; }
}