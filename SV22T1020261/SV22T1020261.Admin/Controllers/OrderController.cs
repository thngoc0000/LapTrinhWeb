using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using SV22T1020261.BusinessLayers;
using SV22T1020261.Models.Common;
using SV22T1020261.Models.Sales;

namespace SV22T1020261.Admin.Controllers
{
    /// <summary>
    /// Các chức năng liên quan đến đơn hàng
    /// </summary>
    public class OrderController : BaseSearchController
    {
        /// <summary>
        /// Tên của biến lưu trữ điều kiện tìm kiếm của đơn hàng trong Session
        /// </summary>
        private const string SEARCH_INPUT = "OrderSearchInput";

        /// <summary>
        /// Nhập đầu vào tìm kiếm -> Hiển thị kết quả tìm kiếm
        /// Phần tìm kiếm này sẽ được giao cho hàm khác thực hiện
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<OrderSearchInput>(SEARCH_INPUT);
            if (input == null)
            {
                input = new OrderSearchInput()
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = ""
                };
            }
            return View(input);
        }

        /// <summary>
        /// Tìm kiếm và trả về kết quả
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Search(OrderSearchInput input)
        {
            var result = await SalesDataService.ListOrdersAsync(input);
            ApplicationContext.SetSessionData(SEARCH_INPUT, input);

            return View(result);
        }

        /// <summary>
        /// Chi tiết đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng cần xem chi tiết</param>
        /// <returns></returns>
        public async Task<IActionResult> Detail(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);

            ViewBag.ListDetails = await SalesDataService.ListDetailsAsync(id);

            return View(order);
        }

        /// <summary>
        /// Lập đơn hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Cập nhật đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng cần cập nhật</param>
        /// <returns></returns>
        public IActionResult Edit(int id)
        {            
            return View();
        }

        /// <summary>
        /// Xoá đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng cần xoá</param>
        /// <returns></returns>
        public IActionResult Delete(int id)
        {            
            return View();
        }

        /// <summary>
        /// Tìm kiếm đơn hàng
        /// </summary>
        /// <returns></returns>
        //public IActionResult Search()
        //{
        //    return View();
        //}

        /// <summary>
        /// Cập nhật sản phẩm trong giỏ hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng cần cập nhật</param>
        /// <param name="productId">Mã sản phẩm cần cập nhật</param>
        /// <returns></returns>
        public IActionResult EditCartItem(int id, int productId)
        {
            return View();
        }
        /// <summary>
        /// Xoá sản phẩm trong giỏ hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng cần xoá</param>
        /// <param name="productId">Mã sản phẩm cần xoá</param>
        /// <returns></returns>
        public IActionResult DeleteCartItem(int id, int productId)
        {
            return View();
        }

        /// <summary>
        /// Làm sạch giỏ hàng (Xoá tất cả sản phẩm trong giỏ hàng)
        /// </summary>
        /// <returns></returns>
        public IActionResult ClearCart()
        {
            return View();
        }

        /// <summary>
        /// Chuyển trạng thái đơn hàng -> Đã chấp nhận
        /// </summary>
        /// <param name="id">Mã đơn hàng cần chuyển</param>
        /// <returns></returns>
        public IActionResult Accept(int id)
        {
            return View();
        }

        /// <summary>
        /// Chuyển trạng thái đơn hàng -> Đang giao
        /// </summary>
        /// <param name="id">Mã đơn hàng cần chuyển</param>
        /// <returns></returns>
        public IActionResult Shipping(int id)
        {
            return View();
        }

        /// <summary>
        /// Chuyển trạng thái đơn hàng -> Đã hoàn thành
        /// </summary>
        /// <param name="id">Mã đơn hàng cần chuyển</param>
        /// <returns></returns>
        public IActionResult Finish(int id)
        {
            // TODO: Chuyển trạng thái sang Hoàn tất

            return View();
        }

        /// <summary>
        /// Chuyển trạng thái đơn hàng -> Đã từ chối
        /// </summary>
        /// <param name="id">Mã đơn hàng cần chuyển</param>
        /// <returns></returns>
        public IActionResult Reject(int id)
        {
            // TODO: Từ chối đơn hàng

            return View();
        }

        /// <summary>
        /// Chuyển trạng thái đơn hàng -> Đã huỷ
        /// </summary>
        /// <param name="id">Mã đơn hàng cần chuyển</param>
        /// <returns></returns>
        public IActionResult Cancel(int id)
        {
            // TODO: Hủy đơn hàng

            return View();
        }

    }
}
