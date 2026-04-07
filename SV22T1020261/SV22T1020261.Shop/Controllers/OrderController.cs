using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020261.BusinessLayers;
using SV22T1020261.Models.Sales;
using SV22T1020261.Models.Security;

namespace SV22T1020261.Shop.Controllers
{
    /// <summary>
    /// Các chức năng liên quan đến đơn hàng
    /// </summary>
    [Authorize]
    public class OrderController : Controller
    {
        /// <summary>
        /// Lịch sử mua hàng
        /// </summary>
        public async Task<IActionResult> History()
        {
            var input = ApplicationContext.GetSessionData<OrderSearchInput>(ApplicationContext.OrderSessionKey);

            if (input == null)
                input = new OrderSearchInput()
                {
                    Page = 1,
                    PageSize = 10,
                    SearchValue = "",
                };

            return View(input);
        }

        /// <summary>
        /// Tìm kiếm đơn hàng của khách hàng hiện tại
        /// </summary>
        public async Task<IActionResult> Search(OrderSearchInput input)
        {
            // Lấy thông tin người dùng từ Cookie
            var userData = User.GetUserData();

            // Kiểm tra lỗi null và lấy UserId (mặc định là 0 nếu không tìm thấy)
            int customerID = userData?.UserId ?? 0;

            // Nếu không có ID hợp lệ, có thể yêu cầu đăng nhập lại hoặc trả về danh sách trống
            if (customerID == 0)
                return RedirectToAction("Login", "Account");

            var model = await SalesDataService.ListOrdersAsync(input, customerID);

            // Lưu lại tham số tìm kiếm vào session để dùng cho lần sau
            ApplicationContext.SetSessionData(ApplicationContext.OrderSessionKey, input);

            return View(model);
        }

        /// <summary>
        /// Theo dõi trạng thái xử lý của đơn hàng
        /// </summary>
        public async Task<IActionResult> Tracking()
        {
            var userData = User.GetUserData();
            int customerID = userData?.UserId ?? 0;

            if (customerID == 0)
                return RedirectToAction("Login", "Account");

            var model = await SalesDataService.ListOrderByAccountAsync(customerID);

            return View(model);
        }

        /// <summary>
        /// Chi tiết đơn hàng
        /// </summary>
        public async Task<IActionResult> Detail(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);

            // Kiểm tra bảo mật: Đảm bảo khách hàng chỉ xem được đơn hàng của chính mình
            var userData = User.GetUserData();
            if (order == null || order.CustomerID != (userData?.UserId ?? 0))
            {
                return RedirectToAction("History");
            }

            var model = await SalesDataService.ListDetailsAsync(id);
            ViewBag.Order = order;
            return View(model);
        }
    }
}