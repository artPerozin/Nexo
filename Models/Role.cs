using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexo.Models
{
    [Table("roles")]
    public class Role
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public required string Nome { get; set; }
        
        [StringLength(200)]
        public string? Descricao { get; set; }
        
        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
        
        public virtual ICollection<RolePermissao> RolePermissoes { get; set; } = new List<RolePermissao>();
    }
}