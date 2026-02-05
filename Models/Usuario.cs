using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexo.Models
{
    [Table("usuarios")]
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100)]
        [Display(Name = "Nome Completo")]
        public required string Nome { get; set; }
        
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "E-mail")]
        public required string Email { get; set; }
        
        [Required(ErrorMessage = "A senha é obrigatória")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
        public required string Senha { get; set; }
        
        [NotMapped]
        [Display(Name = "Confirmar Senha")]
        [DataType(DataType.Password)]
        [Compare("Senha", ErrorMessage = "As senhas não coincidem")]
        public string? ConfirmarSenha { get; set; }
        
        [Required]
        public bool Ativo { get; set; } = true;
        
        [Required]
        public int RoleId { get; set; }
        
        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }
        
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        
        public DateTime? DataAtualizacao { get; set; }
    }
    
    public class LoginViewModel
    {
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "E-mail")]
        public required string Email { get; set; }
        
        [Required(ErrorMessage = "A senha é obrigatória")]
        [DataType(DataType.Password)]
        public required string Senha { get; set; }
        
        [Display(Name = "Lembrar-me")]
        public bool LembrarMe { get; set; }
    }
}