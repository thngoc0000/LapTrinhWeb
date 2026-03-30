using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SV22T1020261.Admin.Models;

namespace SV22T1020261.Admin.Controllers
{
    /// <summary>
    /// Các chức năng liên quan đến trang chủ
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Trang chủ/Dashboard
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }
    }
}
