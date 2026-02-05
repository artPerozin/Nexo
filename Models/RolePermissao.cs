using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexo.Models
{
    [Table("role_permissoes")]
    public class RolePermissao
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int RoleId { get; set; }
        
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }
        
        [Required]
        public int PermissaoId { get; set; }
        
        [ForeignKey("PermissaoId")]
        public virtual Permissao Permissao { get; set; }
    }
}