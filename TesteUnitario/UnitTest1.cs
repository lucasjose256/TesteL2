// Em TesteL2.Tests/EmbalagemControllerTests.cs
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using TesteL2.Controllers;
using TesteL2.Models;
using TesteL2.Services; // Para IEmbalagemService
using System.Threading.Tasks;
using System.Collections.Generic;
using TesteL2.Data; // Para LojaContext
using Microsoft.EntityFrameworkCore; // Para DbContextOptions

namespace TesteL2.Tests
{
    public class EmbalagemControllerTests
    {
        private readonly Mock<IEmbalagemService> _mockEmbalagemService;
        private readonly Mock<ILogger<EmbalagemController>> _mockLogger;
        private readonly EmbalagemController _controller;
        private readonly Mock<LojaContext> _mockLojaContext;

        public EmbalagemControllerTests()
        {
            _mockEmbalagemService = new Mock<IEmbalagemService>();
            _mockLogger = new Mock<ILogger<EmbalagemController>>();
            
            var options = new DbContextOptions<LojaContext>(); 
            _mockLojaContext = new Mock<LojaContext>(options);

            _controller = new EmbalagemController(_mockEmbalagemService.Object, _mockLogger.Object, _mockLojaContext.Object);
        }

        [Fact]
        public async Task ProcessarPedidos_ComRequestValido_RetornaOkObjectResultComResultado()
        {
            var requestDto = new RequestDto { Pedidos = new List<PedidoInputDto> { new PedidoInputDto { PedidoId = 1, Produtos = new List<ProdutoInputDto> { new ProdutoInputDto { ProdutoId = "P1", Dimensoes = new DimensoesDto { Altura = 1, Largura = 1, Comprimento = 1 } } } } } };

            var expectedResponseDto = new ResponseDto { Pedidos = new List<PedidoResultadoDto> { new PedidoResultadoDto { PedidoId = 1 } } };

            _mockEmbalagemService.Setup(service => service.ProcessarPedidosAsync(requestDto))
                .ReturnsAsync(expectedResponseDto);

            var actionResult = await _controller.ProcessarPedidos(requestDto);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result); 
            var actualResponse = Assert.IsType<ResponseDto>(okResult.Value); 
      
            Assert.Equal(expectedResponseDto.Pedidos.Count, actualResponse.Pedidos.Count);
            if (expectedResponseDto.Pedidos.Any() && actualResponse.Pedidos.Any())
            {
                Assert.Equal(expectedResponseDto.Pedidos.First().PedidoId, actualResponse.Pedidos.First().PedidoId);
            }
        }

        [Fact]
        public async Task ProcessarPedidos_ComRequestNulo_RetornaBadRequest()
        {
            RequestDto? requestDto = null;

            var actionResult = await _controller.ProcessarPedidos(requestDto!);

            Assert.IsType<BadRequestObjectResult>(actionResult.Result); // Verifica o tipo do resultado
        }
        
        [Fact]
        public async Task ProcessarPedidos_ComPedidoSemProdutos_RetornaBadRequest()
        {
            var requestDto = new RequestDto 
            { 
                Pedidos = new List<PedidoInputDto> 
                { 
                    new PedidoInputDto { PedidoId = 1, Produtos = new List<ProdutoInputDto>() } 
                } 
            };

            var actionResult = await _controller.ProcessarPedidos(requestDto);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.Equal("Pedido 1 deve conter pelo menos um produto.", badRequestResult.Value);
        }

        [Fact]
        public async Task ProcessarPedidos_QuandoServicoLancaExcecao_RetornaStatusCode500()
        {
            var requestDto = new RequestDto { Pedidos = new List<PedidoInputDto> { new PedidoInputDto { PedidoId = 1, Produtos = new List<ProdutoInputDto> { new ProdutoInputDto { ProdutoId = "P1", Dimensoes = new DimensoesDto { Altura = 1, Largura = 1, Comprimento = 1 } } } } } };
            _mockEmbalagemService.Setup(service => service.ProcessarPedidosAsync(requestDto))
                .ThrowsAsync(new System.Exception("Erro simulado no serviço"));

            var actionResult = await _controller.ProcessarPedidos(requestDto);

            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result); 
            Assert.Equal(500, objectResult.StatusCode); 
            Assert.Equal("Erro interno do servidor ao processar os pedidos.", objectResult.Value);
        }
    }
}