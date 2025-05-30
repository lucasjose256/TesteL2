using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TesteL2.Data;
using TesteL2.Models;
using TesteL2.Services;

namespace TesteL2.Controllers;

using Microsoft.AspNetCore.Mvc;


    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize] // Adicione aqui para proteger todos os endpoints neste controller

    public class EmbalagemController : ControllerBase
    {
        private readonly LojaContext _context;
        private readonly IEmbalagemService _embalagemService;
        private readonly ILogger<EmbalagemController> _logger;

        public EmbalagemController(IEmbalagemService embalagemService, ILogger<EmbalagemController> logger, LojaContext context)
        {
            _embalagemService = embalagemService;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Processa uma lista de pedidos e retorna a otimização de embalagem
        /// </summary>
        /// <param name="request">Lista de pedidos com produtos e suas dimensões</param>
        /// <returns>Resultado da otimização com as caixas selecionadas para cada pedido</returns>
        /// <response code="200">Processamento realizado com sucesso</response>
        /// <response code="400">Dados de entrada inválidos</response>
        /// <response code="500">Erro interno do servidor</response>
        [HttpPost("processar")]
        [ProducesResponseType(typeof(ResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ResponseDto>> ProcessarPedidos([FromBody] RequestDto request)
        {
            try
            {
                if (request == null || !request.Pedidos.Any())
                {
                    return BadRequest("É necessário fornecer pelo menos um pedido.");
                }

                // Validação básica
                foreach (var pedido in request.Pedidos)
                {
                   

                    if (!pedido.Produtos.Any())
                    {
                        return BadRequest($"Pedido {pedido.PedidoId} deve conter pelo menos um produto.");
                    }

                    foreach (var produto in pedido.Produtos)
                    {
                        if (string.IsNullOrWhiteSpace(produto.ProdutoId))
                        {
                            return BadRequest($"Produto no pedido {pedido.PedidoId} deve ter um ID válido.");
                        }

                        if (produto.Dimensoes.Altura <= 0 || produto.Dimensoes.Largura <= 0 || produto.Dimensoes.Comprimento <= 0)
                        {
                            return BadRequest($"Produto {produto.ProdutoId} deve ter dimensões válidas (maiores que zero).");
                        }
                    }
                }

                _logger.LogInformation("Iniciando processamento de {Count} pedidos", request.Pedidos.Count);

                var resultado = await _embalagemService.ProcessarPedidosAsync(request);

                _logger.LogInformation("Processamento concluído com sucesso");

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pedidos");
                return StatusCode(500, "Erro interno do servidor ao processar os pedidos.");
            }
        }
        /// <summary>
        /// Deleta TODOS os pedidos e seus dados relacionados (Produtos, Logs de Processamento).
        /// ATENÇÃO: Esta é uma operação destrutiva.
        /// </summary>
        /// <returns>Confirmação da exclusão</returns>
        /// <response code="200">Todos os pedidos foram deletados com sucesso</response>
        /// <response code="500">Erro interno do servidor ao tentar deletar os pedidos</response>
        [HttpDelete("todos-pedidos")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletarTodosPedidos()
        {
            _logger.LogWarning("Requisição para deletar TODOS os pedidos recebida.");

            try
            {
                

                var numeroPedidos = await _context.Pedidos.CountAsync();
                if (numeroPedidos > 0)
                {

                    _context.Pedidos.RemoveRange(_context.Pedidos);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("{Count} pedidos e seus dados relacionados foram deletados.", numeroPedidos);
                    return Ok(new { message = $"Todos os {numeroPedidos} pedidos foram deletados com sucesso." });
                }
                else
                {
                    _logger.LogInformation("Nenhum pedido encontrado para deletar.");
                    return Ok(new { message = "Nenhum pedido encontrado para deletar." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao tentar deletar todos os pedidos.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao tentar deletar todos os pedidos.");
            }
        }

        /// <summary>
        /// Retorna informações sobre as caixas disponíveis
        /// </summary>
        /// <returns>Lista das caixas disponíveis com suas dimensões</returns>
        [HttpGet("caixas")]
        [ProducesResponseType(200)]
        public async Task<ActionResult<IEnumerable<Caixa>>> GetCaixasDisponiveis()
        {
            try
            {
                var caixasDoDb = await _context.Caixas.ToListAsync();

                if (!caixasDoDb.Any())
                {
                    return NotFound(new { message = "Nenhuma caixa encontrada no banco de dados." });
                }

                // Retorna as entidades Caixa diretamente. 
                // Se você quiser um formato específico (como o anterior com um objeto aninhado "caixas"),
                // você pode mapeá-las para um DTO ou objeto anônimo.
                // Exemplo retornando no formato anterior:
                // var resultadoFormatado = caixasDoDb.Select(c => new {
                //    Id = c.Nome, // Usando Nome como Id, como no exemplo original
                //    Dimensoes = new { c.Altura, c.Largura, c.Comprimento }
                // });
                // return Ok(new { caixas = resultadoFormatado });

                return Ok(caixasDoDb); // Retorna a lista de Caixas diretamente
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar caixas disponíveis do banco de dados.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao buscar as caixas disponíveis.");
            }
        }


          [HttpGet("pedidos-processados")]
        [ProducesResponseType(typeof(IEnumerable<PedidoResultadoDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PedidoResultadoDto>>> GetPedidosProcessados()
        {
            try
            {
                var logsProcessamento = await _context.ProcessamentosLog
                                                .OrderByDescending(log => log.DataProcessamento)
                                                .ToListAsync();

                if (!logsProcessamento.Any())
                {
                    return NotFound(new { message = "Nenhum pedido processado encontrado nos logs." });
                }

                var resultadosPedidos = new List<PedidoResultadoDto>();
                foreach (var log in logsProcessamento)
                {
                    try
                    {
                        var pedidoResultado = JsonSerializer.Deserialize<PedidoResultadoDto>(log.ResultadoJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (pedidoResultado != null)
                        {
                            resultadosPedidos.Add(pedidoResultado);
                        }
                        else
                        {
                             _logger.LogWarning("Não foi possível desserializar o ResultadoJson para o log Id {LogId} do PedidoId {PedidoId}", log.Id, log.PedidoId);
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError(jsonEx, "Erro ao desserializar ResultadoJson para o log Id {LogId} do PedidoId {PedidoId}", log.Id, log.PedidoId);
                    }
                }
                
                return Ok(resultadosPedidos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar logs de processamento de pedidos.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao buscar os pedidos processados.");
            }
        }

        
        
        public class PedidoResponseDto
        {
            public int PedidoId { get; set; }
            public DateTime DataCriacao { get; set; }
            public List<ProdutoResponseDto> Produtos { get; set; }
        }

        public class ProdutoResponseDto
        {
            public string ProdutoId { get; set; }
            public int Altura { get; set; }
            public int Largura { get; set; }
            public int Comprimento { get; set; }
        }
        [HttpGet("pedidos")]
        public async Task<ActionResult<IEnumerable<PedidoResponseDto>>> GetPedidos()
        {
            try
            {
                var pedidos = await _context.Pedidos
                    .Include(p => p.Produtos) // Carrega os produtos relacionados
                    .OrderByDescending(p => p.DataCriacao) // Ordena por data mais recente
                    .ToListAsync();

                var response = pedidos.Select(p => new PedidoResponseDto
                {
                    PedidoId = p.PedidoId,
                    DataCriacao = p.DataCriacao,
                    Produtos = p.Produtos.Select(prod => new ProdutoResponseDto
                    {
                        ProdutoId = prod.ProdutoId,
                        Altura = prod.Altura,
                        Largura = prod.Largura,
                        Comprimento = prod.Comprimento
                    }).ToList()
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pedidos");
                return StatusCode(500, "Ocorreu um erro ao processar sua requisição");
            }
        }
        
        
    }

   