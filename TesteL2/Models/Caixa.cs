using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TesteL2.Models;

public class Caixa
{
    [Key]
    public int Id { get; set; }
        
    [Required]
    [MaxLength(50)]
    public string Nome { get; set; } = string.Empty;
        
    public int Altura { get; set; }
        
    public int Largura { get; set; }
        
    public int Comprimento { get; set; }
        
    [NotMapped]
    public int Volume => Altura * Largura * Comprimento;
        
    public bool ProdutoCabe(Produto produto)
    {
        // Verifica se o produto cabe em qualquer orientação da caixa
        var produtoDimensoes = new[] { produto.Altura, produto.Largura, produto.Comprimento }.OrderByDescending(x => x).ToArray();
        var caixaDimensoes = new[] { Altura, Largura, Comprimento }.OrderByDescending(x => x).ToArray();
            
        return produtoDimensoes[0] <= caixaDimensoes[0] && 
               produtoDimensoes[1] <= caixaDimensoes[1] && 
               produtoDimensoes[2] <= caixaDimensoes[2];
    }
}