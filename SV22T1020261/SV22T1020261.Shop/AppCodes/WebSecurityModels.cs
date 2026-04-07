using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace SV22T1020261.Shop
{
    /// <summary>
    /// Thông tin tài khoản người dùng được lưu trong phiên đăng nhập (cookie)
    /// </summary>
    public class WebUserData
    {
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }

        /// <summary>
        /// Lấy danh sách các Claim chứa thông tin của user
        /// </summary>
        /// <returns></returns>
        private List<Claim> Claims
        {
            get
            {
                List<Claim> claims = new List<Claim>()
                {
                    new Claim(nameof(UserId), UserId?.ToString() ?? "0"),
                    new Claim(nameof(UserName), UserName ?? ""),
                    new Claim(nameof(DisplayName), DisplayName ?? ""),
                    new Claim(nameof(Email), Email ?? ""),                 
                };
                return claims;
            }
        }

        /// <summary>
        /// Tạo Principal dựa trên thông tin của người dùng
        /// </summary>
        /// <returns></returns>
        public ClaimsPrincipal CreatePrincipal()
        {
            var claimIdentity = new ClaimsIdentity(Claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimPrincipal = new ClaimsPrincipal(claimIdentity);
            return claimPrincipal;
        }
    }

    /// <summary>
    /// Định nghĩa tên của các role sử dụng trong phân quyền chức năng cho nhân viên
    /// </summary>
    public class WebUserRoles
    {        
        /// <summary>
        /// Quản trị
        /// </summary>
        public const string Administrator = "admin";  
        /// <summary>
        /// Quản lý dữ liệu
        /// </summary>
        public const string DataManager = "datamanager";
        /// <summary>
        /// Quản lý bán hàng
        /// </summary>
        public const string Sales = "sales";
    }

    /// <summary>
    /// Extension các phương thức cho các đối tượng liên quan đến xác thực tài khoản người dùng
    /// </summary>
    public static class WebUserExtensions
    {
        /// <summary>
        /// Đọc thông tin của user từ principal
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public static WebUserData? GetUserData(this ClaimsPrincipal principal)
        {
            try
            {
                if (principal == null || principal.Identity == null || !principal.Identity.IsAuthenticated)
                    return null;

                var userData = new WebUserData();

                var userIdValue = principal.FindFirstValue(nameof(userData.UserId));

                if (int.TryParse(userIdValue, out int userId))
                    userData.UserId = userId;
                else
                    userData.UserId = null;

                userData.UserName = principal.FindFirstValue(nameof(userData.UserName));
                userData.DisplayName = principal.FindFirstValue(nameof(userData.DisplayName));
                userData.Email = principal.FindFirstValue(nameof(userData.Email));

                return userData;
            }
            catch
            {
                return null;
            }
        }
    }
}
