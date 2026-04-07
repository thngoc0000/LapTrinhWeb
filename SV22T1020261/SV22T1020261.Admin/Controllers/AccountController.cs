using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SV22T1020261.BusinessLayers;
using SV22T1020261.Models.Security;

namespace SV22T1020261.Admin.Controllers
{
    /// <summary>
    /// Các chức năng liên quan đến tài khoản
    /// </summary>
    [Authorize]
    public class AccountController : Controller
    {
        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            ViewBag.UserName = username;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Tên đăng nhập và mật khẩu không được để trống");
                return View();
            }

            string hasedPassword = CryptHelper.HashMD5(password);

            //TODO: Kiểm tra username và password trong database
            var userAccount = await SecurityDataService.AuthorizeUserAccountAsync(username, hasedPassword);

            //Giả lập
            //var userAccount = new UserAccount()
            //{
            //    UserId = "1",
            //    UserName = username,
            //    DisplayName = username,
            //    Email = username,
            //    Photo = "nophoto.png",
            //    RoleNames = $"{WebUserRoles.Sales},${WebUserRoles.Administrator}"
            //};
            if (userAccount != null)
            {
                userAccount.DisplayName = userAccount.FullName ?? "";
                userAccount.UserName = userAccount.FullName ?? "";
            }

            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Tên đăng nhập hoặc mật khẩu không đúng");
                return View();
            }

            //Chuẩn bị thông tin để ghi lên "giấy chứng nhận"
            var userData = new WebUserData()
            {
                UserId = userAccount.UserId,
                UserName = userAccount.UserName,
                DisplayName = userAccount.DisplayName,
                Email = userAccount.Email,
                Photo = userAccount.Photo,
                Roles = userAccount.RoleNames.Split(',').ToList()
            };

            //Tạo giấy chứng nhận (ClaimsPrincipal)
            var principal = userData.CreatePrincipal();

            //Cấp giấy chứng nhận cho người dùng (đăng nhập)
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }
        /// <summary>
        /// Đăng xuất
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        /// <returns></returns>
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {

            try
            {
                var id = User.GetUserData()?.UserId;
                var Email = User.GetUserData()?.Email;

                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(Email))
                {
                    ModelState.AddModelError(string.Empty, "Không tìm thấy thông tin người dùng");
                    return View();
                }
                var kt = await HRDataService.ValidateEmployeeEmailAsync(Email, int.Parse(id));
                if (!kt)
                {
                    ModelState.AddModelError(string.Empty, "Email không tồn tại hoặc đã được sử dụng bởi nhân viên khác");
                    return View();
                }

                // ===== VALIDATION =====
                if (string.IsNullOrWhiteSpace(CurrentPassword))
                    ModelState.AddModelError("CurrentPassword", "Vui lòng nhập mật khẩu cũ");

                if (string.IsNullOrWhiteSpace(NewPassword))
                    ModelState.AddModelError("NewPassword", "Vui lòng nhập mật khẩu mới");

                if (string.IsNullOrWhiteSpace(ConfirmPassword))
                    ModelState.AddModelError("ConfirmPassword", "Vui lòng xác nhận mật khẩu");

                if (NewPassword == CurrentPassword)
                    ModelState.AddModelError("NewPassword", "Mật khẩu không được trùng mật khẩu cũ");

                if (NewPassword != ConfirmPassword)
                    ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp");

                var userAccount = await SecurityDataService.AuthorizeUserAccountAsync(Email, CurrentPassword);
                if(userAccount == null)
                    ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng");

                if (!ModelState.IsValid)
                    return View();

                string hashedPassword = CryptHelper.HashMD5(NewPassword);
                var isValid = await SecurityDataService.ChangePasswordAsync(Email, hashedPassword);
                if (!isValid)
                {
                    ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng");
                    return View();
                }
                ViewBag.Message = "Đổi mật khẩu thành công";
            }
            catch
            {
                //Ghi log lỗi dựa vào thông tin trong Exception (ex.Message, ex.StackTrace)
                ModelState.AddModelError(string.Empty, "Hệ thống hiện đang bận, vui lòng thử lại sau vài phút");
            }
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
