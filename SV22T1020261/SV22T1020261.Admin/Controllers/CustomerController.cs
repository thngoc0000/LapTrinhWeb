using Microsoft.AspNetCore.Mvc;
using SV22T1020261.Models.Common;
using SV22T1020261.Models.Partner;
using System.Threading.Tasks;

namespace SV22T1020261.Admin.Controllers
{
    /// <summary>
    /// Các chức năng liên quan đến Khách hàng
    /// </summary>
    public class CustomerController : Controller
    {
        //private const int PAGE_SIZE = 10; // Hard code: code bị cứng, khó sửa. Nếu muốn sửa, phải sửa lại code, sau đó biên dịch lại. Cách làm này không tốt.
        /// <summary>
        /// Tên của biến lưu trữ điều kiện tìm kiếm của khách hàng trong Session
        /// </summary>
        private const string CUSTOMER_SEARCH_INPUT = "CustomerSearchInput";

        /// <summary>
        /// Nhập đầu vào tìm kiếm -> Hiển thị kết quả tìm kiếm
        /// Phần tìm kiếm này sẽ được giao cho hàm khác thực hiện
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(CUSTOMER_SEARCH_INPUT);
            if (input == null)
                input = new PaginationSearchInput()
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
        /// <returns></returns>
        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            var result = await PartnerDataService.ListCustomersAsync(input);
            ApplicationContext.SetSessionData(CUSTOMER_SEARCH_INPUT, input);
            return View(result);
        }

        /// <summary>
        /// Bổ sung khách hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung khách hàng";
            var model = new Customer()
            {
                CustomerID = 0
            };

            return View("Edit", model);
        }

        /// <summary>
        /// Cập nhật thông tin khách hàng
        /// </summary>
        /// <param name="id">Mã khách hàng cần cập nhật</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin khách hàng";
            var model = await PartnerDataService.GetCustomerAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(Customer data)
        {
            try
            {
                ViewBag.Title = data.CustomerID == 0 ? "Bổ sung khách hàng" : "Cập nhật thông tin khách hàng";

                //TODO: Kiểm tra tính hợp lệ của dữ liệu (validation)
                //Sử dụng MOdelState để lưu các tình huống lỗi và thông báo lỗi cho người dùng (trên View)
                //Giả định: chỉ yêu cầu nhập tên, email, tỉnh/thành
                if (string.IsNullOrWhiteSpace(data.CustomerName))
                    ModelState.AddModelError("CustomerName", "Vui lòng nhập tên khách hàng");

                if (string.IsNullOrWhiteSpace(data.Email))
                    ModelState.AddModelError(nameof(data.Email), "Email không được để trống");
                else if (!await PartnerDataService.ValidateCustomerEmailAsync(data.Email, data.CustomerID))
                    ModelState.AddModelError(nameof(data.Email), "Email đã được sử dụng bởi khách hàng khác");

                if (string.IsNullOrEmpty(data.Province))
                    ModelState.AddModelError(nameof(data.Province), "Vui lòng chọn tỉnh/thành phố");

                if (!ModelState.IsValid)
                {
                    //Nếu có lỗi, trả về View Edit để hiển thị lỗi
                    return View("Edit", data);
                }

                //(Tuỳ chọn) Hiệu chỉnh dữ liệu theo qui định của hệ thống
                if (string.IsNullOrWhiteSpace(data.ContactName)) data.ContactName = data.CustomerName;
                if (string.IsNullOrEmpty(data.Phone)) data.Phone = "";
                if (string.IsNullOrEmpty(data.Address)) data.Address = "";

                //Lưu dữ liệu vào CSDL
                if (data.CustomerID == 0)
                    await PartnerDataService.AddCustomerAsync(data);
                else
                    await PartnerDataService.UpdateCustomerAsync(data);

                return RedirectToAction("Index");

            }
            catch
            {
                //Ghi log lỗi dựa vào thông tin trong Exception (ex.Message, ex.StackTrace)
                ModelState.AddModelError("Error", "Hệ thống hiện đang bận, vui lòng thử lại sau vài phút");
                return View("Edit", data);
            }

        }

        /// <summary>
        /// Xoá khách hàng
        /// </summary>
        /// <param name="id">Mã khách hàng cần xoá</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int id)
        {
            //Nếu method là POST thì xoá
            if (Request.Method == "POST")
            {
                try
                {
                    await PartnerDataService.DeleteCustomerAsync(id);
                    return RedirectToAction("Index");
                }
                catch
                {
                    //Ghi log lỗi dựa vào thông tin trong Exception (ex.Message, ex.StackTrace)
                    ModelState.AddModelError("Error", "Hệ thống hiện đang bận, vui lòng thử lại sau vài phút");
                    return View();
                }
            }

            //GET: Hiển thị thông tin khách hàng cần xoá 
            var model = await PartnerDataService.GetCustomerAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            ViewBag.CanDelete = !await PartnerDataService.IsUsedCustomerAsync(id);

            return View(model);
        }

        /// <summary>
        /// Đổi mật khẩu của khách hàng
        /// </summary>
        /// <param name="id">Mã khách hàng cần đổi mật khẩu</param>
        /// <returns></returns>
        public IActionResult ChangePassword(int id)
        {
            return View();
        }
    }
}
