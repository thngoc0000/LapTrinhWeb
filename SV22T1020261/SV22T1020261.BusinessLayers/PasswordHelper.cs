using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace SV22T1020261.BusinessLayers
{
    /// <summary>
    /// Cung cấp các phương thức hỗ trợ mã hóa (hash) và kiểm tra mật khẩu
    /// sử dụng cơ chế PasswordHasher của ASP.NET Core Identity.
    /// </summary>
    /// <remarks>
    /// Lớp này sử dụng thuật toán PBKDF2 có kèm Salt và nhiều vòng lặp,
    /// đảm bảo tính bảo mật cao khi lưu trữ mật khẩu trong cơ sở dữ liệu.
    /// Không nên tự triển khai thuật toán hash thủ công.
    /// </remarks>
    public static class PasswordHelper
    {
        private static readonly PasswordHasher<object> _hasher
            = new PasswordHasher<object>();

        /// <summary>
        /// Thực hiện hash mật khẩu đầu vào trước khi lưu vào cơ sở dữ liệu.
        /// </summary>
        /// <param name="password">Mật khẩu dạng plain text.</param>
        /// <returns>Chuỗi mật khẩu đã được hash.</returns>
        public static string Hash(string password)
        {
            return _hasher.HashPassword(null, password);
        }
        /// <summary>
        /// Thực hiện hash mật khẩu đầu vào trước khi lưu vào cơ sở dữ liệu. (Áp dụng cho NVARCHAR(50))
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string HashSHA256(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Kiểm tra mật khẩu người dùng nhập vào có khớp với mật khẩu đã hash hay không.
        /// </summary>
        /// <param name="hashedPassword">Mật khẩu đã được hash lưu trong cơ sở dữ liệu.</param>
        /// <param name="inputPassword">Mật khẩu người dùng nhập vào.</param>
        /// <returns>
        /// True nếu mật khẩu hợp lệ; ngược lại False.
        /// </returns>
        public static bool Verify(string hashedPassword, string inputPassword)
        {
            var result = _hasher.VerifyHashedPassword(
                null,
                hashedPassword,
                inputPassword);

            return result == PasswordVerificationResult.Success;
        }
    }
}