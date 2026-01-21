using Microsoft.AspNetCore.Mvc;

namespace MyFirstApp.HRM.Controllers
{
    public class EmployeeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public string SayHello()
        {
            return "Hello from EmployeeController!";
        }
    }
}
