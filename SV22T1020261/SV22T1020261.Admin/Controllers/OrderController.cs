using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using SV22T1020261.BusinessLayers;
using SV22T1020261.Models.Catalog;
using SV22T1020261.Models.Common;
using SV22T1020261.Models.Partner;
using SV22T1020261.Models.Sales;
using System.Threading.Tasks;

namespace SV22T1020261.Admin.Controllers
{
    /// <summary>
    /// Các chức năng liên quan đến đơn hàng
    /// </summary>
    [Authorize(Roles = $"{WebUserRoles.Sales},{WebUserRoles.Administrator}")]
    public class OrderController : BaseSearchController
    {
        /// <summary>
        /// 
        /// </summary>
        private const string PRODUCT_SEARCH = "SearchProductToSale";

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
            var input = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH);
            if (input == null)
            {
                input = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = 3,
                    SearchValue = ""
                };
            }
            return View(input);
        }

        /// <summary>
        /// Tìm kiếm và trả về danh sách mặt hàng cần bán
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<IActionResult> SearchProduct(ProductSearchInput input)
        {
            var result = await CatalogDataService.ListProductsAsync(input);
            ApplicationContext.SetSessionData(PRODUCT_SEARCH, input);

            return View(result);
        }

        /// <summary>
        /// Hiển thị giỏ hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult ShowCart()
        {
            var cart = ShoppingCartService.GetShoppingCart();

            return View(cart);
        }
        /// <summary>
        /// Thêm hàng vào giỏ hàng
        /// </summary>
        /// <param name="productID"></param>
        /// <param name="quantity"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddCartItem(int productID, int quantity, decimal price)
        {
            if (quantity <= 0)
                return Json(new ApiResult(0, "Số lượng không hợp lệ"));

            if(price < 0)
                return Json(new ApiResult(0, "Giá không hợp lệ"));

            var product = await CatalogDataService.GetProductAsync(productID);

            if(product == null)
                return Json(new ApiResult(0, "Sản phẩm không tồn tại"));

            if(!product.IsSelling)
                return Json(new ApiResult(0, "Sản phẩm không còn bán"));

            var item = new OrderDetailViewInfo()
            {
                ProductID = productID,
                ProductName = product.ProductName,
                Quantity = quantity,
                SalePrice = price,
                Unit = product.Unit,
                Photo = product.Photo ?? "nophoto.png"
            };
            ShoppingCartService.AddCartItem(item);

            return Json(new ApiResult(1));
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
        /// <param name="orderID">Mã đơn hàng cần xoá</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int orderID)
        {
            if(Request.Method == "POST")
            {
                try
                {
                    await SalesDataService.DeleteOrderAsync(orderID);
                    return Json(new ApiResult(1));
                }
                catch
                {
                    return Json(new ApiResult(0, "Xoá đơn hàng thất bại"));
                }
            }

            var model = await SalesDataService.GetOrderAsync(orderID);
            return PartialView(model);
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
        /// Hiển thị thông tin mặt hàng cần cập nhật trong giỏ hàng
        /// </summary>
        /// <param name="productId">Mã sản phẩm cần cập nhật</param>
        /// <returns></returns>
        public async Task<IActionResult> EditCartItem(int orderID = 0, int productId = 0)
        {
            var item = ShoppingCartService.GetCartItem(productId);

            // Nếu orderID khác 0 -> Lấy thông tin chi tiết từ CSDL -> Cập nhật cho đơn hàng
            if (orderID != 0)
            {
                item = await SalesDataService.GetDetailAsync(orderID, productId);
            }
            return View(item);
        }

        public async Task<IActionResult> UpdateCartItem(int productID, int quantity, decimal salePrice, int orderID = 0)
        {
            if (quantity <= 0)
                return Json(new ApiResult(0, "Số lượng không hợp lệ"));

            if(salePrice < 0)
                return Json(new ApiResult(0, "Giá không hợp lệ"));

            // Nếu orderID khác 0 -> Cập nhật thông tin chi tiết trong CSDL -> Cập nhật cho đơn hàng
            if (orderID != 0)
                await SalesDataService.UpdateDetailAsync(new OrderDetail()
                {
                    OrderID = orderID,
                    ProductID = productID,
                    Quantity = quantity,
                    SalePrice = salePrice
                });
            else
                ShoppingCartService.UpdateCartItem(productID, quantity, salePrice);
            return Json(new ApiResult(1));
        }

        /// <summary>
        /// Xoá sản phẩm trong giỏ hàng
        /// </summary>
        /// <param name="productId">Mã sản phẩm cần xoá</param>
        /// <returns></returns>
        public async Task<IActionResult> DeleteCartItem(int orderID = 0, int productId = 0)
        {
            if (Request.Method == "POST")
            {
                // Nếu orderID khác 0 -> Xoá thông tin chi tiết trong CSDL -> Cập nhật cho đơn hàng
                if (orderID != 0)
                {
                    await SalesDataService.DeleteDetailAsync(orderID, productId);

                    var lst = await SalesDataService.ListDetailsAsync(orderID);
                    if (lst.Count == 0)
                    {
                        await SalesDataService.DeleteOrderAsync(orderID);
                        return Json(new ApiResult(2, "Đơn hàng đã bị xoá do không còn sản phẩm nào"));
                    }
                }
                else
                    ShoppingCartService.RemoveCartItem(productId);
                return Json(new ApiResult(1));
            }

            var item = ShoppingCartService.GetCartItem(productId);

            // Nếu orderID khác 0 -> Lấy thông tin chi tiết từ CSDL -> Cập nhật cho đơn hàng
            if (orderID != 0)
            {
                item = await SalesDataService.GetDetailAsync(orderID, productId);
            }

            return PartialView(item);
        }

        /// <summary>
        /// Làm sạch giỏ hàng (Xoá tất cả sản phẩm trong giỏ hàng)
        /// </summary>
        /// <returns></returns>
        public IActionResult ClearCart()
        {
            if(Request.Method == "POST")
            {
                ShoppingCartService.ClearCart();
                return Json(new ApiResult(1));

            }

            return PartialView();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateOrder(int customerID = 0, string province = "", string address = "")
        {
            var cart = ShoppingCartService.GetShoppingCart();
            if (cart.Count == 0)
            {
                return Json(new ApiResult(0, "Giỏ hàng rỗng"));
            }

            //TODO: Kiểm tra dữ liệu hợp lệ ...

            //Tạo 1 đơn hàng mới và bổ sung vào CSDL
            //var order = new Order()
            //{
            //    CustomerID = customerID == 0 ? null : customerID,
            //    DeliveryProvince = province,
            //    DeliveryAddress = address
            //};

            int orderID = await SalesDataService.AddOrderAsync(customerID, province, address);

            //TODO: Kiểm tra tạo đơn hàng thành công hay không

            //Bổ sung chi tiết vào đơn hàng
            foreach (var item in cart)
            {
                //var detail = new OrderDetail()
                //{
                //    OrderID = orderID,
                //    ProductID = item.ProductID,
                //    Quantity = item.Quantity,
                //    SalePrice = item.SalePrice
                //};
                item.OrderID = orderID;
                await SalesDataService.AddDetailAsync(item);
            }

            //Clear cart
            ShoppingCartService.ClearCart();

            return Json(new ApiResult(orderID));
        }

        /// <summary>
        /// Chuyển trạng thái đơn hàng -> Đã chấp nhận
        /// </summary>
        /// <param name="id">Mã đơn hàng cần chuyển</param>
        /// <returns></returns>
        public async Task<IActionResult> Accept(int id)
        {
            if (Request.Method == "POST")
            {
                // TODO: Cập nhật trạng thái sang Đã chấp nhận
                // Lưu ý: Cần truyền thêm mã nhân viên xử lý đơn hàng vào hàm AcceptOrderAsync
                int employeeID = Convert.ToInt32(User.GetUserData()?.UserId);
                var kt = await SalesDataService.AcceptOrderAsync(id, employeeID); 
                if(!kt)
                    return Json(new ApiResult(0, "Không thể chấp nhận đơn hàng ở trạng thái hiện tại"));
                return Json(new ApiResult(1));
            }
            var model = await SalesDataService.GetOrderAsync(id);
            return View(model);
        }

        /// <summary>
        /// Chuyển trạng thái đơn hàng -> Đang giao
        /// </summary>
        /// <param name="id">Mã đơn hàng cần chuyển</param>
        /// <returns></returns>
        public async Task<IActionResult> Shipping(int orderID, int shipperID = 0)
        {
            if (Request.Method == "POST")
            {
                if(shipperID <= 0)
                    return Json(new ApiResult(0, "Vui lòng chọn đơn vị vận chuyển")); 

                var kt = await SalesDataService.ShipOrderAsync(orderID, shipperID);
                if(!kt)
                    return Json(new ApiResult(0, string.Empty));
                return Json(new ApiResult(1));
            }
            var model = await SalesDataService.GetOrderAsync(orderID);
            return View(model);
        }

        /// <summary>
        /// Chuyển trạng thái đơn hàng -> Đã hoàn thành
        /// </summary>
        /// <param name="id">Mã đơn hàng cần chuyển</param>
        /// <returns></returns>
        public async Task<IActionResult> Finish(int id)
        {
            if (Request.Method == "POST")
            {
                var kt = await SalesDataService.CompleteOrderAsync(id);
                if(!kt)
                    return Json(new ApiResult(0, "Không thể hoàn thành đơn hàng ở trạng thái hiện tại"));
                return Json(new ApiResult(1));
            }
            var model = await SalesDataService.GetOrderAsync(id);
            return View(model);
        }

        /// <summary>
        /// Chuyển trạng thái đơn hàng -> Đã từ chối
        /// </summary>
        /// <param name="id">Mã đơn hàng cần chuyển</param>
        public async Task<IActionResult> Reject(int id)
        {
            if (Request.Method == "POST")
            {
                var kt = await SalesDataService.RejectOrderAsync(id, 1); //Hiện tại chưa có thông tin về nhân viên -> Tạm thời truyền vào mã nhân viên là 1
                if(!kt)
                    return Json(new ApiResult(0, "Từ chối đơn hàng thất bại"));
                return Json(new ApiResult(1));
            }

            var model = await SalesDataService.GetOrderAsync(id);
            return View(model);
        }

        /// <summary>
        /// Chuyển trạng thái đơn hàng -> Đã huỷ
        /// </summary>
        /// <param name="id">Mã đơn hàng cần chuyển</param>
        public async Task<IActionResult> Cancel(int id)
        {
            if (Request.Method == "POST")
            {
                var kt = await SalesDataService.CancelOrderAsync(id);
                if(!kt)
                    return Json(new ApiResult(0, "Hủy đơn hàng thất bại"));

                return Json(new ApiResult(1));
            }

            var model = await SalesDataService.GetOrderAsync(id);
            return View(model);
        }

    }
}
