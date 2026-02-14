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

        [ForeignKey(nameof(RoleId))]
        public virtual Role Role { get; set; } = null!;

        [Required]
        public int PermissaoId { get; set; }

        [ForeignKey(nameof(PermissaoId))]
        public virtual Permissao Permissao { get; set; } = null!;
    }
}
