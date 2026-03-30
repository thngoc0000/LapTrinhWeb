using Microsoft.AspNetCore.Mvc;

namespace SV22T1020261.Shop.Controllers
{
    /// <summary>
    /// Các chức năng liên quan đến giỏ hàng
    /// </summary>
    public class CartController : Controller
    {
        /// <summary>
        /// Danh sách các sản phẩm trong giỏ hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        
    }
}
