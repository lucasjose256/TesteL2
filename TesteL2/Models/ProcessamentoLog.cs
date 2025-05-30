using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesteL2.Models;

public class ProcessamentoLog
{
    [Key]
    public int Id { get; set; }
        
    public int PedidoId { get; set; }
        
    [ForeignKey("PedidoId")]
    public virtual Pedido Pedido { get; set; } = null!;
        
    public DateTime DataProcessamento { get; set; } = DateTime.UtcNow;
        
    [Required]
    public string ResultadoJson { get; set; } = string.Empty;
}