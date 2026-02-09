using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexo.Models
{
    [Table("projetos")]
    public class Projeto
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "O nome do projeto é obrigatório")]
        [StringLength(200)]
        public required string Nome { get; set; }
        
        [StringLength(1000)]
        public string? Descricao { get; set; }
        
        [Required]
        public DateTime DataInicio { get; set; }
        
        public DateTime? DataFim { get; set; }
        
        [Required]
        [Range(0, 100)]
        public int Progresso { get; set; } = 0;
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Planejamento";
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Valor { get; set; }
        
        public int? ClienteId { get; set; }
        
        public int? ResponsavelId { get; set; }
        
        [ForeignKey("ResponsavelId")]
        public virtual Usuario? Responsavel { get; set; }
        
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        
        public DateTime? DataAtualizacao { get; set; }
        
        public virtual ICollection<Tarefa> Tarefas { get; set; } = new List<Tarefa>();
    }
}
