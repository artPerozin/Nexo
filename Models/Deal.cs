using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexo.Models
{
    [Table("deals")]
    public class Deal
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "O nome do negócio é obrigatório")]
        [StringLength(200)]
        public required string Nome { get; set; }
        
        [StringLength(1000)]
        public string? Descricao { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Valor { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Estagio { get; set; } = "Prospecção";
        
        [StringLength(200)]
        public string? Cliente { get; set; }
        
        [StringLength(100)]
        public string? EmailCliente { get; set; }
        
        [StringLength(20)]
        public string? TelefoneCliente { get; set; }
        
        public int? ResponsavelId { get; set; }
        
        [ForeignKey("ResponsavelId")]
        public virtual Usuario? Responsavel { get; set; }
        
        public DateTime? DataFechamentoEstimada { get; set; }
        
        public DateTime? DataFechamento { get; set; }
        
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        
        public DateTime? DataAtualizacao { get; set; }
        
        [Range(0, 100)]
        public int Probabilidade { get; set; } = 0;
    }
}
