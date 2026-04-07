using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SV22T1020261.BusinessLayers;
using SV22T1020261.Models.Catalog;
using SV22T1020261.Models.Partner;
using SV22T1020261.Models.Sales;
using SV22T1020261.Models.Security;

namespace SV22T1020261.Shop.Controllers
{
    /// <summary>
    /// Các chức năng liên quan đến giỏ hàng
    /// </summary>
    [Authorize]
    public class CartController : Controller
    {
        /// <summary>
        /// Danh sách các sản phẩm trong giỏ hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var model = ApplicationContext.GetSessionData<List<Cart>>(ApplicationContext.CartSessionKey);

            return View(model);
        }

        /// <summary>
        /// Thêm vào giỏ hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Add(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
                return RedirectToAction("Index", "Home");

            var category = await CatalogDataService.GetCategoryAsync(product.CategoryID ?? -1);
            if (category == null)
                return RedirectToAction("Index", "Home");

            var carts = ApplicationContext.GetSessionData<List<Cart>>(ApplicationContext.CartSessionKey) ?? new List<Cart>();
            foreach (var item in carts)
            {
                if (item.ProductID == id)
                {
                    item.Quantity++;

                    ApplicationContext.SetSessionData(ApplicationContext.CartSessionKey, carts);
                    return RedirectToAction("Index");
                }
            }

            carts.Add(new Cart
            {
                Photo = (product.Photo == null ? "" : product.Photo),
                CategoryID = product.CategoryID,
                CategoryName = category.CategoryName,
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Price = product.Price,
                Quantity = 1
            });
            ApplicationContext.SetSessionData(ApplicationContext.CartSessionKey, carts);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Remove(int id)
        {
            var carts = ApplicationContext.GetSessionData<List<Cart>>(ApplicationContext.CartSessionKey) ?? new List<Cart>();
            carts.RemoveAll(c => c.ProductID == id);
            ApplicationContext.SetSessionData(ApplicationContext.CartSessionKey, carts);
            return RedirectToAction("Index");
        }

        public IActionResult Clear()
        {
            ApplicationContext.RemoveSessionData(ApplicationContext.CartSessionKey);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Update(Cart data)
        {
            var carts = ApplicationContext
                .GetSessionData<List<Cart>>(ApplicationContext.CartSessionKey)
                ?? new List<Cart>();

            var item = carts.FirstOrDefault(x => x.ProductID == data.ProductID);

            if (item != null)
            {
                if (data.Quantity <= 0)
                {
                    carts.Remove(item);
                }
                else
                {
                    item.Quantity = data.Quantity;
                }
            }

            ApplicationContext.SetSessionData(ApplicationContext.CartSessionKey, carts);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Customer model)
        {
            var carts = ApplicationContext
                .GetSessionData<List<Cart>>(ApplicationContext.CartSessionKey);

            if (carts == null || !carts.Any())
                return RedirectToAction("Index");

            // Kiểm tra thông tin người dùng từ Cookie
            var userData = User.GetUserData();
            if (userData == null || userData.UserId == null)
            {
                // Nếu không có thông tin user, yêu cầu đăng nhập lại
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(model.CustomerName))
            {
                ModelState.AddModelError(nameof(model.CustomerName), "Vui lòng nhập tên khách hàng");
            }

            if (string.IsNullOrWhiteSpace(model.Phone))
            {
                ModelState.AddModelError(nameof(model.Phone), "Vui lòng nhập số điện thoại");
            }

            if (string.IsNullOrWhiteSpace(model.Address))
            {
                ModelState.AddModelError(nameof(model.Address), "Vui lòng nhập địa chỉ");
            }

            if (!ModelState.IsValid)
                return View(model);

            // Cập nhật thông tin khách hàng (nếu cần)
            await PartnerDataService.UpdateDeliveryCustomerAsync(model);

            // Gán CustomerID từ Cookie vào Order
            var order = new Order
            {
                CustomerID = userData.UserId
            };

            // Lưu đơn hàng vào database
            var orderID = await SalesDataService.AddOrderAsync(order.CustomerID ?? 0,
                                                               model.Province ?? "",
                                                               model.Address ?? "");

            try
            {
                foreach (var item in carts)
                {
                    await SalesDataService.AddDetailAsync(new OrderDetail
                    {
                        OrderID = orderID,
                        ProductID = item.ProductID,
                        SalePrice = item.Price,
                        Quantity = item.Quantity
                    });
                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }

            // Xóa giỏ hàng
            ApplicationContext.RemoveSessionData(ApplicationContext.CartSessionKey);

            return RedirectToAction("Success");
        }

        public async Task<IActionResult> Checkout()
        {
            var carts = ApplicationContext
                .GetSessionData<List<Cart>>(ApplicationContext.CartSessionKey);

            if (carts == null || !carts.Any())
                return RedirectToAction("Index");

            // Lấy thông tin User từ Cookie thay vì Session
            var userData = User.GetUserData();
            if (userData == null || userData.UserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy chi tiết thông tin khách hàng từ Database dựa trên UserId trong Cookie
            var model = await PartnerDataService.GetCustomerAsync(userData.UserId.Value);

            if (model == null)
                return RedirectToAction("Index", "Home");

            return View(model);
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}