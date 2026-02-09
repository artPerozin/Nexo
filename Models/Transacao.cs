using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexo.Models
{
    [Table("transacoes")]
    public class Transacao
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "A descrição é obrigatória")]
        [StringLength(200)]
        public required string Descricao { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Tipo { get; set; } = "Receita"; // Receita ou Despesa
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Valor { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string Categoria { get; set; }
        
        [Required]
        public DateTime Data { get; set; } = DateTime.Now;
        
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pendente"; // Pendente, Pago, Vencido
        
        public DateTime? DataVencimento { get; set; }
        
        public DateTime? DataPagamento { get; set; }
        
        [StringLength(50)]
        public string? FormaPagamento { get; set; }
        
        [StringLength(500)]
        public string? Observacoes { get; set; }
        
        public int? ProjetoId { get; set; }
        
        [ForeignKey("ProjetoId")]
        public virtual Projeto? Projeto { get; set; }
        
        public int? DealId { get; set; }
        
        [ForeignKey("DealId")]
        public virtual Deal? Deal { get; set; }
        
        public int CriadoPorId { get; set; }
        
        [ForeignKey("CriadoPorId")]
        public virtual Usuario? CriadoPor { get; set; }
        
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}
