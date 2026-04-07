using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020261.BusinessLayers;
using SV22T1020261.Models.Catalog;
using SV22T1020261.Models.Common;

namespace SV22T1020261.Admin.Controllers
{
    /// <summary>
    /// Các chức năng liên quan đến mặt hàng
    /// </summary>
    [Authorize(Roles = $"{WebUserRoles.DataManager},{WebUserRoles.Administrator}")]
    public class ProductController : Controller
    {
        /// <summary>
        /// Tên của biến lưu trữ điều kiện tìm kiếm của mặt hàng trong Session
        /// </summary>
        private const string PRODUCT_SEARCH_INPUT = "ProductSearchInput";

        /// <summary>
        /// Nhập đầu vào tìm kiếm -> Hiển thị kết quả tìm kiếm
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH_INPUT);
            if (input == null)
                input = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = ""
                };
            //ViewBag.Categories = await CatalogDataService.ListCategoriesAsync(input);
            //ViewBag.Suppliers = await PartnerDataService.ListSuppliersAsync(input);
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
        /// Bổ sung mặt hàng
        /// </summary>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung mặt hàng";
            var model = new Product()
            {
                ProductID = 0,
                IsSelling = true
            };
            return View("Edit", model);
        }

        /// <summary>
        /// Cập nhật thông tin mặt hàng
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin mặt hàng";
            var model = await CatalogDataService.GetProductAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            ViewBag.ProductID = id;

            var photos = await CatalogDataService.ListPhotosAsync(id);
            ViewBag.Photos = photos;

            var attributes = await CatalogDataService.ListAttributesAsync(id);
            ViewBag.Attributes = attributes;

            return View(model);
        }

        /// <summary>
        /// Lưu dữ liệu mặt hàng (Thêm / Cập nhật)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveData(Product data, IFormFile? uploadPhoto)
        {
            try
            {
                ViewBag.Title = data.ProductID == 0
                    ? "Bổ sung mặt hàng"
                    : "Cập nhật thông tin mặt hàng";

                // ===== VALIDATION =====
                // Bắt lỗi Loại hàng
                if (data.CategoryID <= 0)
                    ModelState.AddModelError(nameof(data.CategoryID), "Vui lòng chọn loại hàng");

                // Bắt lỗi Nhà cung cấp
                if (data.SupplierID <= 0)
                    ModelState.AddModelError(nameof(data.SupplierID), "Vui lòng chọn nhà cung cấp");

                if (string.IsNullOrWhiteSpace(data.ProductName))
                    ModelState.AddModelError(nameof(data.ProductName),
                        "Vui lòng nhập tên mặt hàng");

                if (data.SupplierID == null)
                    ModelState.AddModelError(nameof(data.SupplierID),
                        "Vui lòng chọn nhà cung cấp");

                if (data.CategoryID == null)
                    ModelState.AddModelError(nameof(data.CategoryID),
                        "Vui lòng chọn loại hàng");

                if (string.IsNullOrWhiteSpace(data.Unit))
                    ModelState.AddModelError(nameof(data.Unit),
                        "Vui lòng nhập đơn vị tính");

                if (data.Price <= 0)
                    ModelState.AddModelError(nameof(data.Price),
                        "Giá phải lớn hơn 0");

                if (!ModelState.IsValid)
                    return View("Edit", data);

                // Xử lý upload ảnh
                if (uploadPhoto != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(uploadPhoto.FileName)}";

                    var filePath = Path.Combine(
                        ApplicationContext.WWWRootPath,
                        "images/products",
                        fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadPhoto.CopyToAsync(stream);
                    }

                    data.Photo = fileName;
                }

                // ===== TIỀN XỬ LÝ DỮ LIỆU =====
                if (string.IsNullOrWhiteSpace(data.ProductDescription))
                    data.ProductDescription = "";

                if (string.IsNullOrWhiteSpace(data.Photo))
                    data.Photo = "nophoto.png";

                // ===== LƯU DATABASE =====
                if (data.ProductID == 0)
                {
                    await CatalogDataService.AddProductAsync(data);
                }
                else
                {
                    await CatalogDataService.UpdateProductAsync(data);
                }

                return RedirectToAction("Index");
            }
            catch
            {
                // TODO: Có thể ghi log tại đây
                ModelState.AddModelError(string.Empty,
                    "Hệ thống đang bận hoặc dữ liệu không hợp lệ. Vui lòng thử lại sau");

                return View("Edit", data);
            }
        }

        /// <summary>
        /// Xoá mặt hàng
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            if (Request.Method == "POST")
            {
                try
                {
                    await CatalogDataService.DeleteProductAsync(id);
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("Error",
                        "Hệ thống hiện đang bận, vui lòng thử lại sau vài phút");
                    return View();
                }
            }

            var model = await CatalogDataService.GetProductAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            ViewBag.CanDelete = !await CatalogDataService.IsUsedProductAsync(id);

            return View(model);
        }

        /// <summary>
        /// Chi tiết mặt hàng
        /// </summary>
        public async Task<IActionResult> Detail(int id)
        {
            var model = await CatalogDataService.GetProductAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        /// <summary>
        /// Danh sách ảnh của mặt hàng
        /// </summary>
        public async Task<IActionResult> ListPhotos(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
                return RedirectToAction("Index");

            return View();
        }

        /// <summary>
        /// Bổ sung ảnh cho mặt hàng
        /// </summary>
        public IActionResult CreatePhoto(int id)
        {
            ViewBag.Title = "Bổ sung hình ảnh";

            var model = new ProductPhoto()
            {
                ProductID = id,
                DisplayOrder = 0,
                IsHidden = false
            };

            return View("EditPhoto", model);
        }

        /// <summary>
        /// Cập nhật ảnh của mặt hàng
        /// </summary>
        public async Task<IActionResult> EditPhoto(int id, int photoId)
        {
            ViewBag.Title = "Cập nhật hình ảnh";

            var model = await CatalogDataService.GetPhotoAsync(photoId);
            if (model == null || model.ProductID != id)
                return RedirectToAction("Edit", new { id });

            return View(model);
        }

        /// <summary>
        /// Lưu ảnh mặt hàng (Thêm / Cập nhật)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SavePhoto(ProductPhoto data, IFormFile? uploadPhoto)
        {
            try
            {
                ViewBag.Title = data.PhotoID == 0 ? "Bổ sung ảnh" : "Cập nhật ảnh";

                // 1. Kiểm tra ID mặt hàng (Trường hợp can thiệp URL)
                if (data.ProductID <= 0)
                    ModelState.AddModelError(nameof(data.ProductID), "Mặt hàng không tồn tại hoặc không hợp lệ.");

                // 2. Bắt lỗi bắt buộc chọn ảnh khi THÊM MỚI (PhotoID == 0)
                if (data.PhotoID == 0 && (uploadPhoto == null || uploadPhoto.Length == 0))
                {
                    ModelState.AddModelError("uploadPhoto", "Vui lòng chọn file ảnh để tải lên.");
                }

                // 3. Kiểm tra định dạng file (Chỉ cho phép ảnh)
                if (uploadPhoto != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var extension = Path.GetExtension(uploadPhoto.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("uploadPhoto", "Định dạng file không hỗ trợ. Vui lòng chọn .jpg, .png, .gif hoặc .webp");
                    }

                    // Kiểm tra dung lượng (Ví dụ tối đa 2MB)
                    if (uploadPhoto.Length > 2 * 1024 * 1024)
                    {
                        ModelState.AddModelError("uploadPhoto", "Dung lượng ảnh không được vượt quá 2MB.");
                    }
                }

                // 4. Bắt lỗi logic cho các trường khác
                if (string.IsNullOrWhiteSpace(data.Description))
                    ModelState.AddModelError(nameof(data.Description), "Vui lòng nhập mô tả cho ảnh.");

                if (data.DisplayOrder <= 0)
                    ModelState.AddModelError(nameof(data.DisplayOrder), "Thứ tự hiển thị phải là số lớn hơn 0.");

                // ===== NẾU CÓ LỖI: TRẢ VỀ VIEW NGAY =====
                if (!ModelState.IsValid)
                {
                    return View("EditPhoto", data);
                }

                // ===== XỬ LÝ LƯU DỮ LIỆU KHI MỌI THỨ ĐÃ HỢP LỆ =====
                if (uploadPhoto != null && uploadPhoto.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(uploadPhoto.FileName)}";
                    var filePath = Path.Combine(ApplicationContext.WWWRootPath, "images/products", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadPhoto.CopyToAsync(stream);
                    }

                    // Nếu là cập nhật, có thể xóa file cũ ở đây để dọn dẹp bộ nhớ (tùy chọn)
                    data.Photo = fileName;
                }
                else if (data.PhotoID != 0)
                {
                    // Cập nhật mà không đổi ảnh -> lấy lại tên ảnh cũ từ DB
                    var oldPhoto = await CatalogDataService.GetPhotoAsync(data.PhotoID);
                    data.Photo = oldPhoto?.Photo ?? "nophoto.png";
                }

                // Lưu vào Database
                if (data.PhotoID == 0)
                    await CatalogDataService.AddPhotoAsync(data);
                else
                    await CatalogDataService.UpdatePhotoAsync(data);

                return RedirectToAction("Edit", new { id = data.ProductID });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi tại đây (ví dụ: _logger.LogError(ex, "Error saving photo"))
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi trong quá trình lưu dữ liệu. Vui lòng thử lại.");
                return View("EditPhoto", data);
            }
        }

        /// <summary>
        /// Xoá ảnh của mặt hàng
        /// </summary>
        public async Task<IActionResult> DeletePhoto(int id, int photoId)
        {
            try
            {
                await CatalogDataService.DeletePhotoAsync(photoId);
                return RedirectToAction("Edit", new { id });
            }
            catch (Exception)
            {
                ModelState.AddModelError("Error",
                    "Hệ thống hiện đang bận, vui lòng thử lại sau vài phút");
            }

            var model = await CatalogDataService.GetProductAsync(id);
            if (model == null || model.ProductID != id)
                return RedirectToAction("Edit", new { id });

            return View();
        }

        /// <summary>
        /// Danh sách thuộc tính của mặt hàng
        /// </summary>
        public async Task<IActionResult> ListAttributes(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
                return RedirectToAction("Index");

            ViewBag.Product = product;

            var attributes = await CatalogDataService.ListAttributesAsync(id);
            return View(attributes);
        }


        /// <summary>
        /// Bổ sung thuộc tính cho mặt hàng
        /// </summary>
        public IActionResult CreateAttribute(int id)
        {
            ViewBag.Title = "Bổ sung thuộc tính";

            var model = new ProductAttribute()
            {
                ProductID = id,
                DisplayOrder = 0
            };

            return View("EditAttribute", model);
        }

        /// <summary>
        /// Cập nhật thuộc tính của mặt hàng
        /// </summary>
        public async Task<IActionResult> EditAttribute(int id, int attributeId)
        {
            ViewBag.Title = "Cập nhật thuộc tính";

            var model = await CatalogDataService.GetAttributeAsync(attributeId);
            if (model == null || model.ProductID != id)
                return RedirectToAction("Edit", new { id });

            return View(model);
        }

        /// <summary>
        /// Lưu thuộc tính (Thêm / Cập nhật)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveAttribute(ProductAttribute data)
        {
            try
            {
                // ===== VALIDATION =====
                if (string.IsNullOrWhiteSpace(data.AttributeName))
                    ModelState.AddModelError(nameof(data.AttributeName),
                        "Vui lòng nhập tên thuộc tính");

                if (string.IsNullOrWhiteSpace(data.AttributeValue))
                    ModelState.AddModelError(nameof(data.AttributeValue),
                        "Vui lòng nhập giá trị thuộc tính");

                if (data.DisplayOrder <= 0)
                    ModelState.AddModelError(nameof(data.DisplayOrder),
                        "Thứ tự hiển thị không hợp lệ");

                if (!ModelState.IsValid)
                    return View("EditAttribute", data);

                // ===== LƯU DATABASE =====
                if (data.AttributeID == 0)
                    await CatalogDataService.AddAttributeAsync(data);
                else
                    await CatalogDataService.UpdateAttributeAsync(data);

                return RedirectToAction("Edit", new { id = data.ProductID });
            }
            catch
            {
                ModelState.AddModelError("Error",
                    "Không thể lưu thuộc tính. Vui lòng thử lại.");

                return View("EditAttribute", data);
            }
        }

        /// <summary>
        /// Xoá thuộc tính của mặt hàng
        /// </summary>
        public async Task<IActionResult> DeleteAttribute(int id, int attributeId)
        {
            try
            {
                await CatalogDataService.DeleteAttributeAsync(attributeId);
                return RedirectToAction("Edit", new { id });
            }
            catch (Exception)
            {
                ModelState.AddModelError("Error",
                    "Hệ thống hiện đang bận, vui lòng thử lại sau vài phút");
            }

            var model = await CatalogDataService.GetAttributeAsync(attributeId);
            if (model == null || model.ProductID != id)
                return RedirectToAction("Edit", new { id });

            return View(model);
        }
    }
}