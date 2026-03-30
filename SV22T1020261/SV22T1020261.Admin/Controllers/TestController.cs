using Microsoft.AspNetCore.Mvc;
using SV22T1020261.DataLayers.SQLServer;
using SV22T1020261.Models.Common;

namespace SV22T1020261.Admin.Controllers
{
    public class TestController : Controller
    {
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, String searchValue = "")
        {
            var input = new PaginationSearchInput()
            {
                Page = page,
                PageSize = pageSize,
                SearchValue = searchValue
            };

            string connectionString = "Server=localhost;Database=LiteCommerceDB;Trusted_Connection=True;TrustServerCertificate=True;";
            var repo = new CustomerRepository(connectionString);
            var data = await repo.ListAsync(input);
            return Json(data);
        }
    }
}
