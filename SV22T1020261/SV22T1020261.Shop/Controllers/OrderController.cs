using Microsoft.AspNetCore.Mvc;
using SV22T1020261.BusinessLayers;
using SV22T1020261.Models.Sales;
using SV22T1020261.Models.Security;

namespace SV22T1020261.Shop.Controllers
{
    /// <summary>
    /// Các chức năng liên quan đến đơn hàng
    /// </summary>
    public class OrderController : Controller
    {
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
        public async Task<ActionResult> Tracking()
        {
            // GET: 4 trạng thái xử lý của đơn hàng: Đang xử lý, Đang giao hàng, Đã giao hàng, Đã hủy
            // Các thông tin: orderId, orderDate, status, DeliveryAdress, DeliveryProvice, tổng tiền thanh toán
            var customerID = ApplicationContext.GetSessionData<CustomerAccount>(ApplicationContext.CustomerSessionKey)?.CustomerID;
            var model = await SalesDataService.ListOrderByAccountAsync(customerID ?? 0);

            return View(model);
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
