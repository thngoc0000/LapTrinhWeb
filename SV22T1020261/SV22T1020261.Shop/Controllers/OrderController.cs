using Microsoft.AspNetCore.Mvc;

namespace SV22T1020261.Shop.Controllers
{
    /// <summary>
    /// Các chức năng liên quan đến đơn hàng
    /// </summary>
    public class OrderController : Controller
    {
        /// <summary>
        /// Thanh toán đơn hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Checkout()
        {
            return View();
        }

        /// <summary>
        /// Lịch sử mua hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult History()
        {
            return View();
        }

        /// <summary>
        /// Theo dõi trạng thái xử lý của đơn hàng
        /// </summary>
        /// <returns></returns>
        public ActionResult Tracking()
        {
            return View();
        }

        /// <summary>
        /// Chi tiết đơn hàng
        /// </summary>
        /// <returns></returns>
        public ActionResult Detail(int id)
        {
            return View();
        }
    }
}
