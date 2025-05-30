using TesteL2.Data;
using TesteL2.Models;

namespace TesteL2.Services;

using Microsoft.EntityFrameworkCore;

using System.Text.Json;


    public interface IEmbalagemService
    {
        Task<ResponseDto> ProcessarPedidosAsync(RequestDto request);
    }

    public class EmbalagemService : IEmbalagemService
    {
        private readonly LojaContext _context;
        private readonly ILogger<EmbalagemService> _logger;

        public EmbalagemService(LojaContext context, ILogger<EmbalagemService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ResponseDto> ProcessarPedidosAsync(RequestDto request)
        {
            var response = new ResponseDto();
            var caixas = await _context.Caixas.ToListAsync();

            foreach (var pedidoInput in request.Pedidos)
            {
                var pedidoResultado = await ProcessarPedidoAsync(pedidoInput, caixas);
                response.Pedidos.Add(pedidoResultado);
            }

            return response;
        }

        private async Task<PedidoResultadoDto> ProcessarPedidoAsync(PedidoInputDto pedidoInput, List<Caixa> caixasDisponiveis)
        {
            try
            {
                var pedido = await SalvarPedidoAsync(pedidoInput);
                var resultado = OtimizarEmbalagem(pedidoInput, caixasDisponiveis);
                await SalvarLogProcessamentoAsync(pedido.PedidoId, resultado);
                
                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pedido {PedidoId}", pedidoInput.PedidoId);
                throw;
            }
        }

        private async Task<Pedido> SalvarPedidoAsync(PedidoInputDto pedidoInput)
        {
            var pedidoExistente = await _context.Pedidos
                .Include(p => p.Produtos)
                .FirstOrDefaultAsync(p => p.PedidoId == pedidoInput.PedidoId);

            if (pedidoExistente != null)
            {
                return pedidoExistente;
            }

            var pedido = new Pedido
            {
                PedidoId = pedidoInput.PedidoId,
                DataCriacao = DateTime.UtcNow,
                Produtos = pedidoInput.Produtos.Select(p => new Produto
                {
                    ProdutoId = p.ProdutoId,
                    Altura = p.Dimensoes.Altura,
                    Largura = p.Dimensoes.Largura,
                    Comprimento = p.Dimensoes.Comprimento
                }).ToList()
            };

 
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();
            return pedido;
        }

        private async Task SalvarLogProcessamentoAsync(int pedidoId, PedidoResultadoDto resultado)
        {
            var log = new ProcessamentoLog
            {
                PedidoId = pedidoId,
                ResultadoJson = JsonSerializer.Serialize(resultado, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true 
                })
            };

            _context.ProcessamentosLog.Add(log);
            await _context.SaveChangesAsync();
        }

        private PedidoResultadoDto OtimizarEmbalagem(PedidoInputDto pedidoInput, List<Caixa> caixasDisponiveis)
        {
            var resultado = new PedidoResultadoDto { PedidoId = pedidoInput.PedidoId };
            var produtosRestantes = pedidoInput.Produtos.ToList();

            //ordena caixas por volume (menor para maior para otimizar)
            var caixasOrdenadas = caixasDisponiveis.OrderBy(c => c.Volume).ToList();

            while (produtosRestantes.Any())
            {
                var melhorCombinacao = EncontrarMelhorCombinacao(produtosRestantes, caixasOrdenadas);
                
                if (melhorCombinacao.caixa == null)
                {
                    //produto não cabem em nenhuma caixa
                    resultado.Caixas.Add(new CaixaResultadoDto
                    {
                        CaixaId = null,
                        Produtos = produtosRestantes.Select(p => p.ProdutoId).ToList(),
                        Observacao = "Produto não cabe em nenhuma caixa disponível."
                    });
                    break;
                }

                resultado.Caixas.Add(new CaixaResultadoDto
                {
                    CaixaId = melhorCombinacao.caixa.Nome,
                    Produtos = melhorCombinacao.produtos.Select(p => p.ProdutoId).ToList()
                });

                //remove produtos já embalados
                foreach (var produto in melhorCombinacao.produtos)
                {
                    produtosRestantes.Remove(produto);
                }
            }

            return resultado;
        }

        private (Caixa? caixa, List<ProdutoInputDto> produtos) EncontrarMelhorCombinacao(
            List<ProdutoInputDto> produtos, List<Caixa> caixas)
        {
            //tenta encontrar a melhor combinação de produtos para uma caixa
            foreach (var caixa in caixas)
            {
                var combinacao = EncontrarMelhorCombinacaoParaCaixa(produtos, caixa);
                if (combinacao.Any())
                {
                    return (caixa, combinacao);
                }
            }

            return (null, new List<ProdutoInputDto>());
        }

        private List<ProdutoInputDto> EncontrarMelhorCombinacaoParaCaixa(
            List<ProdutoInputDto> produtos, Caixa caixa)
        {
            //implementação simplificada do algoritmo de bin packing
            var combinacao = new List<ProdutoInputDto>();
            var produtosOrdenados = produtos
                .Where(p => ProdutoCabeNaCaixa(p, caixa))
                .OrderByDescending(p => p.Dimensoes.Altura * p.Dimensoes.Largura * p.Dimensoes.Comprimento)
                .ToList();

            var volumeUsado = 0;
            var volumeCaixa = caixa.Volume;

            foreach (var produto in produtosOrdenados)
            {
                var volumeProduto = produto.Dimensoes.Altura * produto.Dimensoes.Largura * produto.Dimensoes.Comprimento;
                
                if (volumeUsado + volumeProduto <= volumeCaixa && 
                    ProdutosCabemJuntos(combinacao.Concat(new[] { produto }).ToList(), caixa))
                {
                    combinacao.Add(produto);
                    volumeUsado += volumeProduto;
                }
            }

            return combinacao;
        }

        private bool ProdutoCabeNaCaixa(ProdutoInputDto produto, Caixa caixa)
        {
            var produtoDimensoes = new[] { produto.Dimensoes.Altura, produto.Dimensoes.Largura, produto.Dimensoes.Comprimento }
                .OrderByDescending(x => x).ToArray();
            var caixaDimensoes = new[] { caixa.Altura, caixa.Largura, caixa.Comprimento }
                .OrderByDescending(x => x).ToArray();

            return produtoDimensoes[0] <= caixaDimensoes[0] &&
                   produtoDimensoes[1] <= caixaDimensoes[1] &&
                   produtoDimensoes[2] <= caixaDimensoes[2];
        }

        private bool ProdutosCabemJuntos(List<ProdutoInputDto> produtos, Caixa caixa)
        {
            //verifica se o volume total não excede
            var volumeTotal = produtos.Sum(p => p.Dimensoes.Altura * p.Dimensoes.Largura * p.Dimensoes.Comprimento);
            return volumeTotal <= caixa.Volume * 0.8; //80% de eficiência de empacotamento
        }
    }
