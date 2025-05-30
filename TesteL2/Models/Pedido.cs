using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TesteL2.Models;

public class Pedido
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int PedidoId { get; set; }
        
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        
    public virtual ICollection<Produto> Produtos { get; set; } = new List<Produto>();
        
    public virtual ICollection<ProcessamentoLog> ProcessamentosLog { get; set; } = new List<ProcessamentoLog>();
}