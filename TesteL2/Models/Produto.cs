using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesteL2.Models;

public class Produto
{
    [Key]
    public int Id { get; set; }
        
    [Required]
    [MaxLength(100)]
    public string ProdutoId { get; set; } = string.Empty;
        
    public int PedidoId { get; set; }
        
    [ForeignKey("PedidoId")]
    public virtual Pedido Pedido { get; set; } = null!;
        
    public int Altura { get; set; }
        
    public int Largura { get; set; }
        
    public int Comprimento { get; set; }
        
    [NotMapped]
    public int Volume => Altura * Largura * Comprimento;
}