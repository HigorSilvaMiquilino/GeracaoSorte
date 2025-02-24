using GeracaoSorte.Services.Home;
using Microsoft.AspNetCore.Mvc;

namespace GeracaoSorte.Controllers
{
    [Route("api/[controller]")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HomeService _homeService;

        public HomeController(ILogger<HomeController> logger, HomeService homeService)
        {
            _logger = logger;
            _homeService = homeService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return PhysicalFile("wwwroot/html/index.html", "text/html");
        }
    }
}
