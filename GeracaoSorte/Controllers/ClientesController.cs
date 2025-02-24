using GeracaoSorte.Data;
using GeracaoSorte.Services.Clientes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeracaoSorte.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : Controller
    {
        private readonly ClientesService _clientesService;

        public ClientesController(ClientesService clientesService)
        {
            _clientesService = clientesService;
        }
        [HttpGet("PorArquivoNome")]
        public async Task<ActionResult<IEnumerable<ClienteComNumeros>>> GetClientesPorArquivoNome(string arquivoNome)
        {
            Console.WriteLine(arquivoNome);
            var clientes = await _clientesService.GetClientesPorArquivoNome(arquivoNome);

            if (clientes == null || !clientes.Any())
            {
                return NotFound();
            }
            return Ok(new { success = true, message = "Clientes encontrados com sucesso", cliente = clientes });
        }

        [HttpGet("ArquivoNome")]
        public async Task<ActionResult<IEnumerable<string>>> GetTodosrquivoNome()
        {
            var clientes = await _clientesService.GetTodosArquivoNome();

            if (clientes == null || !clientes.Any())
            {
                return NotFound();
            }

            return Ok(new { success = true, message = "Arquivos encontrados com sucesso", cliente = clientes });
        }
    }

    }
