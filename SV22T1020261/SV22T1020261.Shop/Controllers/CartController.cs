using Microsoft.AspNetCore.Mvc;
using SV22T1020261.BusinessLayers;
using SV22T1020261.Models.Catalog;
using SV22T1020261.Models.Sales;
using SV22T1020261.Models.Security;

namespace SV22T1020261.Shop.Controllers
{
    /// <summary>
    /// Các chức năng liên quan đến giỏ hàng
    /// </summary>
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
        [CustomerAuthorize]
        public async Task<IActionResult> Checkout(CheckoutModel model)
        {
            var carts = ApplicationContext
                .GetSessionData<List<Cart>>(ApplicationContext.CartSessionKey);

            if (carts == null || !carts.Any())
                return RedirectToAction("Index");

            if (!ModelState.IsValid)
                return View(model);

            // TODO: Lưu đơn hàng vào database ở đây
            var order = new Order
            {
                CustomerID = ApplicationContext.GetSessionData<CustomerAccount>(ApplicationContext.CustomerSessionKey)?.CustomerID
            };
            var orderID = await SalesDataService.AddOrderAsync(order);

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

        [CustomerAuthorize]
        public async Task<IActionResult> Checkout()
        {
            var carts = ApplicationContext
                .GetSessionData<List<Cart>>(ApplicationContext.CartSessionKey);

            if (carts == null || !carts.Any())
                return RedirectToAction("Index");

            var customerAccount = ApplicationContext.GetSessionData<CustomerAccount>(ApplicationContext.CustomerSessionKey);

            var model = await PartnerDataService.GetCustomerAsync(customerAccount?.CustomerID ?? -1);

            return View(model);
        }

        [CustomerAuthorize]
        public IActionResult Success()
        {
            return View();
        }
    }
}