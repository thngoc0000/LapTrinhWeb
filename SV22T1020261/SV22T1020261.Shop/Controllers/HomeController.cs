using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SV22T1020261.BusinessLayers;
using SV22T1020261.Models.Catalog;
using SV22T1020261.Models.Security;

namespace SV22T1020261.Shop.Controllers
{
    /// <summary>
    /// Các chức năng liên quan đến trang chủ
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Tên của biến lưu trữ điều kiện tìm kiếm của mặt hàng trong Session
        /// </summary>
        private const string PRODUCT_SEARCH_INPUT = "ProductSearchInput";

        /// <summary>
        /// Danh sách mặt hàng; Nhập đầu vào tìm kiếm -> Hiển thị kết quả tìm kiếm
        /// </summary>
        /// <returns></returns>
        public IActionResult Index(ProductSearchInput? input)
        {
            if (input != null && input.SearchValue.IsNullOrEmpty())
                input = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH_INPUT);
            if (input == null)
                input = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = ""
                };
            return View(input);
        }

        /// <summary>
        /// Tìm kiếm và trả về kết quả
        /// </summary>
        public async Task<IActionResult> Search(ProductSearchInput input)
        {
            var result = await CatalogDataService.ListProductsAsync(input);
            ApplicationContext.SetSessionData(PRODUCT_SEARCH_INPUT, input);

            return View(result);
        }

        /// <summary>
        /// Chi tiết mặt hàng
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Detail(int id)
        {
            var result = await CatalogDataService.GetProductAsync(id);
            if(result != null)
                ViewBag.Category = await CatalogDataService.GetCategoryAsync(result.CategoryID ?? -1);
            ViewBag.Attributes = await CatalogDataService.ListAttributesAsync(id);
            ViewBag.Photos = await CatalogDataService.ListPhotosAsync(id);
            return View(result);
        }
    }
}
