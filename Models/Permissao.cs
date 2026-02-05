using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexo.Models
{
    [Table("permissoes")]
    public class Permissao
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string Nome { get; set; }
        
        [StringLength(200)]
        public string? Descricao { get; set; }
        
        public virtual ICollection<RolePermissao> RolePermissoes { get; set; } = new List<RolePermissao>();
    }
}