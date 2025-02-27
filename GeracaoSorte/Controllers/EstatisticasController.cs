using GeracaoSorte.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GeracaoSorte.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstatisticasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EstatisticasController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("serie")]
        public async Task<ActionResult<IEnumerable<EstatisticasSerieOrdem>>> GetEstatisticasSerie()
        {
            var estatisticas = await _context.ParticipacoesSorte
                .GroupBy(c => c.Serie)
                .Select(c => new EstatisticasSerieOrdem
                {
                    Serie = c.Key,
                    Quantidade = c.Count()
                })
                .ToListAsync();
            if (estatisticas == null || !estatisticas.Any())
            {
                return NotFound();
            }
            return Ok(new { success = true, message = "Estatísticas encontradas com sucesso", estatistica = estatisticas });
        }

        [HttpGet("ordem")]
        public async Task<ActionResult<IEnumerable<EstatisticasSerieOrdem>>> GetEstatisticasOrdem()
        {
            var estatisticas = await _context.ParticipacoesSorte
                .GroupBy(c => c.Ordem)
                .Select(c => new EstatisticasSerieOrdem
                {
                    Serie = c.Key,
                    Quantidade = c.Count()
                })
                .ToListAsync();
            if (estatisticas == null || !estatisticas.Any())
            {
                return NotFound();
            }
            return Ok(new { success = true, message = "Estatísticas encontradas com sucesso", estatistica = estatisticas });
        }
    }
}
