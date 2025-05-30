using System.Text.Json.Serialization;

namespace TesteL2.Models;

    public class PedidoInputDto
    {
        [JsonPropertyName("pedido_id")] 
        public int PedidoId { get; set; }

        [JsonPropertyName("produtos")] 
        public List<ProdutoInputDto> Produtos { get; set; } = new List<ProdutoInputDto>();
    }

    public class ProdutoInputDto
    {
        [JsonPropertyName("produto_id")] 
        public string ProdutoId { get; set; } = string.Empty;

        [JsonPropertyName("dimensoes")] 
        public DimensoesDto Dimensoes { get; set; } = new DimensoesDto();
    }


    public class DimensoesDto
    {
        [JsonPropertyName("altura")] 
        public int Altura { get; set; }

        [JsonPropertyName("largura")] 
        public int Largura { get; set; }

        [JsonPropertyName("comprimento")] 
        public int Comprimento { get; set; }
    }
    public class RequestDto
    {
        [JsonPropertyName("pedidos")] 
        public List<PedidoInputDto> Pedidos { get; set; } = new List<PedidoInputDto>();
    }

    public class ResponseDto
    {
        public List<PedidoResultadoDto> Pedidos { get; set; } = new List<PedidoResultadoDto>();
    }

    public class PedidoResultadoDto
    {
        public int PedidoId { get; set; }
        public List<CaixaResultadoDto> Caixas { get; set; } = new List<CaixaResultadoDto>();
    }

    public class CaixaResultadoDto
    {
        public string? CaixaId { get; set; }
        public List<string> Produtos { get; set; } = new List<string>();
        public string? Observacao { get; set; }
    }
