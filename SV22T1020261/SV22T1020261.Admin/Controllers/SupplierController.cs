using Microsoft.AspNetCore.Mvc;
using SV22T1020261.BusinessLayers;
using SV22T1020261.Models.Common;
using SV22T1020261.Models.Partner;

namespace SV22T1020261.Admin.Controllers
{
    /// <summary>
    /// Các chức năng liên quan đến nhà cung cấp
    /// </summary>
    public class SupplierController : BaseSearchController
    {
        private const string SEARCH_INPUT = "SupplierSearchInput";

        public IActionResult Index()
        {
            var input = GetSearchInput(SEARCH_INPUT);
            return View(input);
        }

        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            var result = await PartnerDataService.ListSuppliersAsync(input);
            ApplicationContext.SetSessionData(SEARCH_INPUT, input);
            return View(result);
        }

        /// <summary>
        /// Bổ sung nhà cung cấp
        /// </summary>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung nhà cung cấp";

            var model = new Supplier()
            {
                SupplierID = 0
            };

            return View("Edit", model);
        }

        /// <summary>
        /// Cập nhật thông tin nhà cung cấp
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin nhà cung cấp";

            var model = await PartnerDataService.GetSupplierAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        /// <summary>
        /// Lưu dữ liệu (thêm/sửa)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveData(Supplier data)
        {
            try
            {
                ViewBag.Title = data.SupplierID == 0
                    ? "Bổ sung nhà cung cấp"
                    : "Cập nhật thông tin nhà cung cấp";

                // ===== VALIDATION =====
                if (string.IsNullOrWhiteSpace(data.SupplierName))
                    ModelState.AddModelError(nameof(data.SupplierName), "Vui lòng nhập tên nhà cung cấp");

                if (string.IsNullOrWhiteSpace(data.ContactName))
                    ModelState.AddModelError(nameof(data.ContactName), "Vui lòng nhập người liên hệ");

                if (string.IsNullOrWhiteSpace(data.Province))
                    ModelState.AddModelError(nameof(data.Province), "Vui lòng chọn tỉnh/thành");

                //if (!string.IsNullOrEmpty(data.Email))
                //{
                //    if (!await PartnerDataService.ValidateCustomerEmailAsync(data.Email, data.SupplierID))
                //        ModelState.AddModelError(nameof(data.Email), "Email đã tồn tại");
                //}

                if (!ModelState.IsValid)
                    return View("Edit", data);

                // ===== CHUẨN HOÁ DỮ LIỆU =====
                if (string.IsNullOrEmpty(data.Address)) data.Address = "";
                if (string.IsNullOrEmpty(data.Phone)) data.Phone = "";

                // ===== LƯU DB =====
                if (data.SupplierID == 0)
                    await PartnerDataService.AddSupplierAsync(data);
                else
                    await PartnerDataService.UpdateSupplierAsync(data);

                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("Error", "Hệ thống bận, vui lòng thử lại sau");
                return View("Edit", data);
            }
        }

        /// <summary>
        /// Xoá nhà cung cấp
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            // POST: Xoá
            if (Request.Method == "POST")
            {
                try
                {
                    await PartnerDataService.DeleteSupplierAsync(id);
                    return RedirectToAction("Index");
                }
                catch
                {
                    ModelState.AddModelError("Error", "Không thể xoá dữ liệu");
                    return View();
                }
            }

            // GET: Hiển thị
            var model = await PartnerDataService.GetSupplierAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            ViewBag.CanDelete = !await PartnerDataService.IsUsedSupplierAsync(id);

            return View(model);
        }
    }
}