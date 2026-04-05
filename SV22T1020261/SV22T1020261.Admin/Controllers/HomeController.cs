using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020261.Admin.Models;
using SV22T1020261.BusinessLayers;
using System.Diagnostics;

namespace SV22T1020261.Admin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = await CommonDataService.GetDashboardInfoAsync();
            return View(model);
        }
    }
}
