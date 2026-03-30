using Microsoft.AspNetCore.Mvc;
using SV22T1020261.BusinessLayers;
using SV22T1020261.Models.Catalog;
using SV22T1020261.Models.Partner;
using SV22T1020261.Models.Security;
using System.Text.RegularExpressions;

namespace SV22T1020261.Shop.Controllers
{
    /// <summary>
    /// Các chức năng liên quan đến tài khoản
    /// </summary>
    public class AccountController : Controller
    {
        /// <summary>
        /// Đăng ký tài khoản
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            var model = new CustomerAccount()
            {
                CustomerID = 0
            };

            return View(model);
        }

        /// <summary>
        /// Lưu thông tin đăng ký
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Register(CustomerAccount data)
        {
            try
            {
                // ===== VALIDATION =====
                if (string.IsNullOrWhiteSpace(data.CustomerName))
                    ModelState.AddModelError(nameof(data.CustomerName), "Vui lòng nhập tên khách hàng");

                if (string.IsNullOrWhiteSpace(data.Email))
                    ModelState.AddModelError(nameof(data.Email), "Email không được để trống");

                if (string.IsNullOrWhiteSpace(data.Password))
                    ModelState.AddModelError(nameof(data.Password), "Mật khẩu không được để trống");

                // (Tuỳ chọn) kiểm tra email đã tồn tại chưa
                if (!string.IsNullOrWhiteSpace(data.Email))
                {
                    bool isEmailUsed = await PartnerDataService.ValidateCustomerEmailAsync(data.Email);
                    if (!isEmailUsed)
                        ModelState.AddModelError(nameof(data.Email), "Email đã được sử dụng");
                }

                // Nếu có lỗi → trả lại View
                if (!ModelState.IsValid)
                    return View(data);

                var dataArr = data.CustomerName.Split(' ');
                data.ContactName = dataArr[dataArr.Length - 1];

                // ===== LƯU DỮ LIỆU =====
                bool result = await SecurityDataService.RegisterCustomerAccountAsync(data); // Lỗi có thể xảy ra ở đây nếu DB có vấn đề, hoặc email đã tồn tại (nếu không kiểm tra ở trên)

                if (!result)
                {
                    ModelState.AddModelError("Error", "Đăng ký thất bại. Vui lòng thử lại.");
                    return View(data);
                }

                // Thành công → chuyển sang Login
                ViewBag.Message = "Đăng ký thành công. Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", "Hệ thống hiện đang bận, vui lòng thử lại sau vài phút\n" + ex.StackTrace);
                return View(data);
            }
        }

        /// <summary>
        /// Hiển thị trang đăng nhập
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.Title = "Đăng nhập";
            return View();
        }

        /// <summary>
        /// Xử lý đăng nhập
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(CustomerAccount data)
        {
            ViewBag.Title = "Đăng nhập";

            try
            {
                var customerAccount =
                    await SecurityDataService.AuthorizeCustomerAccountAsync(data.Email, data.Password);

                if (customerAccount != null)
                {
                    ApplicationContext.SetSessionData(ApplicationContext.CustomerSessionKey, customerAccount);
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Error = "Đăng nhập thất bại. Vui lòng kiểm tra lại email và mật khẩu.";
            }
            catch (Exception)
            {
                ModelState.AddModelError("Error",
                    "Hệ thống hiện đang bận, vui lòng thử lại sau vài phút");
            }

            return View();
        }

        /// <summary>
        /// Đăng xuất
        /// </summary>
        /// <returns></returns>
        public IActionResult Logout()
        {
            ApplicationContext.RemoveSessionData(ApplicationContext.CustomerSessionKey);
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Hồ sơ cá nhân
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Profile()
        {
            var customerAccount = ApplicationContext.GetSessionData<CustomerAccount>(ApplicationContext.CustomerSessionKey);

            if (customerAccount != null)
            {
                var model = await PartnerDataService.GetCustomerAsync(customerAccount.CustomerID);
                return View(model);
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var customerAccount = ApplicationContext.GetSessionData<CustomerAccount>(ApplicationContext.CustomerSessionKey);

            if (customerAccount != null)
            {
                var model = await PartnerDataService.GetCustomerAsync(customerAccount.CustomerID);
                return View(model);
            }

            return RedirectToAction("Login");
        }

        /// <summary>
        /// Chỉnh sửa hồ sơ cá nhân POST
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditProfile(Customer data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(data.Phone))
                {
                    ModelState.AddModelError(nameof(data.Phone),
                        "Số điện thoại không được để trống");
                }
                else
                {
                    var phonePattern = @"^0\d{9}$";

                    if (!Regex.IsMatch(data.Phone, phonePattern))
                    {
                        ModelState.AddModelError(nameof(data.Phone),
                            "Số điện thoại phải gồm 10 số và bắt đầu bằng 0");
                    }
                }

                if (!ModelState.IsValid)
                    return View("EditProfile", data);


                data.ContactName = data.CustomerName.Split(' ').LastOrDefault() ?? data.CustomerName;
                var customerAccount = ApplicationContext.GetSessionData<CustomerAccount>(ApplicationContext.CustomerSessionKey);
                if (customerAccount != null)
                    data.Email = customerAccount.Email.Trim();

                await PartnerDataService.UpdateCustomerAsync(data);
                return RedirectToAction("Profile");
            }
            catch
            {
                //Ghi log lỗi dựa vào thông tin trong Exception (ex.Message, ex.StackTrace)
                ModelState.AddModelError("Error", "Hệ thống hiện đang bận, vui lòng thử lại sau vài phút");
                return View("EditProfile", data);
            }
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        /// <returns></returns>
        public IActionResult ChangePassword()
        {
            return View();
        }
    }
}
