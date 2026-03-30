using Microsoft.AspNetCore.Mvc;
using SV22T1020261.Models.Common;
using SV22T1020261.Models.Partner;

namespace SV22T1020261.Admin.Controllers
{
    /// <summary>
    /// Các chức năng liên quan đến người giao hàng
    /// </summary>
    public class ShipperController : BaseSearchController
    {
        /// <summary>
        /// Tên của biến lưu trữ điều kiện tìm kiếm của người giao hàng trong Session
        /// </summary>
        private const string SEARCH_INPUT = "ShipperSearchInput";

        /// <summary>
        /// Nhập đầu vào tìm kiếm -> Hiển thị kết quả tìm kiếm
        /// Phần tìm kiếm này sẽ được giao cho hàm khác thực hiện
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var input = GetSearchInput(SEARCH_INPUT);
            return View(input);
        }

        /// <summary>
        /// Tìm kiếm và trả về kết quả
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            var result = await PartnerDataService.ListShippersAsync(input);
            ApplicationContext.SetSessionData(SEARCH_INPUT, input);
            return View(result);
        }

        /// <summary>
        /// Bổ sung người giao hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung người giao hàng";
            var model = new Shipper()
            {
                ShipperID = 0
            };
            return View("Edit", model);
        }

        /// <summary>
        /// Cập nhật thông tin giao hàng
        /// </summary>
        /// <param name="id">Mã người giao hàng cần cập nhật</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin người giao hàng";
            var model = await PartnerDataService.GetShipperAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(Shipper data)
        {
            try
            {
                ViewBag.Title = data.ShipperID == 0
                    ? "Bổ sung người giao hàng"
                    : "Cập nhật thông tin người giao hàng";

                // Validation
                if (string.IsNullOrWhiteSpace(data.ShipperName))
                    ModelState.AddModelError(nameof(data.ShipperName),
                        "Vui lòng nhập tên người giao hàng");

                if (string.IsNullOrWhiteSpace(data.Phone))
                    ModelState.AddModelError(nameof(data.Phone),
                        "Vui lòng nhập số điện thoại");

                if (!ModelState.IsValid)
                    return View("Edit", data);

                if (data.ShipperID == 0)
                    await PartnerDataService.AddShipperAsync(data);
                else
                    await PartnerDataService.UpdateShipperAsync(data);

                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("Error",
                    "Hệ thống hiện đang bận, vui lòng thử lại sau");
                return View("Edit", data);
            }
        }

        /// <summary>
        /// Xoá người giao hàng
        /// </summary>
        /// <param name="id">Mã người giao hàng cần xoá</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int id)
        {
            if (Request.Method == "POST")
            {
                try
                {
                    await PartnerDataService.DeleteShipperAsync(id);
                    return RedirectToAction("Index");
                }
                catch
                {
                    ModelState.AddModelError("Error",
                        "Hệ thống hiện đang bận, vui lòng thử lại sau");
                    return View();
                }
            }

            var model = await PartnerDataService.GetShipperAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            ViewBag.CanDelete = !await PartnerDataService.IsUsedShipperAsync(id);

            return View(model);
        }
    }
}
