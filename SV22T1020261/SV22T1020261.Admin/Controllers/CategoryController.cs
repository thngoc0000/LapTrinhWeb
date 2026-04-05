using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020261.BusinessLayers;
using SV22T1020261.Models.Catalog;
using SV22T1020261.Models.Common;

namespace SV22T1020261.Admin.Controllers
{
    /// <summary>
    /// Các chức năng liên quan đến loại hàng
    /// </summary>
    [Authorize(Roles = $"{WebUserRoles.DataManager},${WebUserRoles.Administrator}")]
    public class CategoryController : BaseSearchController
    {
        /// <summary>
        /// Tên của biến lưu trữ điều kiện tìm kiếm của loại hàng trong Session
        /// </summary>
        private const string CATEGORY_SEARCH_INPUT = "CategorySearchInput";

        /// <summary>
        /// Nhập đầu vào tìm kiếm -> Hiển thị kết quả tìm kiếm
        /// Phần tìm kiếm này sẽ được giao cho hàm khác thực hiện
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var input = GetSearchInput(CATEGORY_SEARCH_INPUT);
            return View(input);
        }

        /// <summary>
        /// Tìm kiếm và trả về kết quả
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            var result = await CatalogDataService.ListCategoriesAsync(input);
            ApplicationContext.SetSessionData(CATEGORY_SEARCH_INPUT, input);
            return View(result);
        }

        /// <summary>
        /// Bổ sung loại hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung loại hàng";
            return View("Edit", new Category());
        }

        /// <summary>
        /// Cập nhật thông tin loại hàng
        /// </summary>
        /// <param name="id">Mã id loại hàng cần cập nhật</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin loại hàng";
            var data = await CatalogDataService.GetCategoryAsync(id);
            if (data == null)
                return RedirectToAction("Index");

            return View(data);
        }

        /// <summary>
        /// Lưu dữ liệu loại hàng (Thêm / Cập nhật)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveData(Category model)
        {
            try
            {
                ViewBag.Title = model.CategoryID == 0 ?
                    "Bổ sung loại hàng" :
                    "Cập nhật thông tin loại hàng";

                // ===== VALIDATION =====
                if (string.IsNullOrWhiteSpace(model.CategoryName))
                    ModelState.AddModelError(nameof(model.CategoryName),
                        "Vui lòng nhập tên loại hàng");

                if (!ModelState.IsValid)
                {
                    return View("Edit", model);
                }

                // ===== HIỆU CHỈNH DỮ LIỆU =====
                if (string.IsNullOrWhiteSpace(model.Description))
                    model.Description = "";

                // ===== LƯU CSDL =====
                if (model.CategoryID == 0)
                    await CatalogDataService.AddCategoryAsync(model);
                else
                    await CatalogDataService.UpdateCategoryAsync(model);

                return RedirectToAction("Index");
            }
            catch
            {
                // Ghi log lỗi dựa vào thông tin trong Exception (ex.Message, ex.StackTrace)
                ModelState.AddModelError("Error",
                    "Hệ thống hiện đang bận, vui lòng thử lại sau vài phút");

                return View("Edit", model);
            }
        }

        /// <summary>
        /// Xoá loại hàng
        /// </summary>
        /// <param name="id">Mã loại hàng cần xoá</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int id)
        {
            // Nếu method là POST thì xoá
            if (Request.Method == "POST")
            {
                try
                {
                    await CatalogDataService.DeleteCategoryAsync(id);
                    return RedirectToAction("Index");
                }
                catch
                {
                    // Ghi log lỗi dựa vào thông tin trong Exception (ex.Message, ex.StackTrace)
                    ModelState.AddModelError("Error", "Hệ thống hiện đang bận, vui lòng thử lại sau vài phút");
                    return View();
                }
            }

            // GET: Hiển thị thông tin loại hàng cần xoá 
            var model = await CatalogDataService.GetCategoryAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            ViewBag.CanDelete = !await CatalogDataService.IsUsedCategoryAsync(id);

            return View(model);
        }
    }
}