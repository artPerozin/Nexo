using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexo.Models
{
    [Table("tarefas")]
    public class Tarefa
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int ProjetoId { get; set; }
        
        [ForeignKey("ProjetoId")]
        public virtual Projeto? Projeto { get; set; }
        
        [Required(ErrorMessage = "O título da tarefa é obrigatório")]
        [StringLength(200)]
        public required string Titulo { get; set; }
        
        [StringLength(1000)]
        public string? Descricao { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pendente";
        
        [Required]
        [StringLength(20)]
        public string Prioridade { get; set; } = "Media";
        
        public DateTime? DataVencimento { get; set; }
        
        public int? ResponsavelId { get; set; }
        
        [ForeignKey("ResponsavelId")]
        public virtual Usuario? Responsavel { get; set; }
        
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        
        public DateTime? DataConclusao { get; set; }
    }
}