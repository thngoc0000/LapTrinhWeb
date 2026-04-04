using SV22T1020261.DataLayers.Interfaces;
using SV22T1020261.DataLayers.SQLServer;
using SV22T1020261.Models.Security;

namespace SV22T1020261.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng liên quan tới tài khoản
    /// Bao gồm: đăng nhập, đổi mật khẩu, đăng ký
    /// </summary>
    public static class SecurityDataService
    {
        private static readonly ISecurityRepository<CustomerAccount> customerAccountDB;
        private static readonly ISecurityRepository<UserAccount> userAccountDB;

        /// <summary>
        /// Constructor
        /// </summary>
        static SecurityDataService()
        {
            customerAccountDB = new CustomerAccountRepository(Configuration.ConnectionString);
            userAccountDB = new UserAccountRepository(Configuration.ConnectionString);
        }

        /// <summary>
        /// Lấy về thông tin tài khoản khách hàng nếu email và mật khẩu hợp lệ
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<CustomerAccount?> AuthorizeCustomerAccountAsync(string email, string password)
        {
            return await customerAccountDB.AuthorizeAsync(email, PasswordHelper.HashSHA256(password));
        }

        /// <summary>
        /// Đổi mật khẩu cho tài khoản khách hàng
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<bool> ChangeCustomerPasswordAsync(string email, string password)
        {
            return await customerAccountDB.ChangePasswordAsync(email, PasswordHelper.HashSHA256(password));
        }

        /// <summary>
        /// Đăng ký tài khoản khách hàng mới
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<bool> RegisterCustomerAccountAsync(CustomerAccount data)
        {
            data.Password = PasswordHelper.HashSHA256(data.Password);
            return await customerAccountDB.RegisterAsync(data);
        }

        /// <summary>
        /// Lấy về thông tin tài khoản nếu email và mật khẩu hợp lệ
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<UserAccount?> AuthorizeUserAccountAsync(string email, string password)
        {
            return await userAccountDB.AuthorizeAsync(email, password);
        }

        /// <summary>
        /// Đổi mật khẩu cho tài khoản 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<bool> ChangePasswordAsync(string email, string password)
        {
            return await userAccountDB.ChangePasswordAsync(email, password);
        }

        /// <summary>
        /// Đăng ký tài khoản  mới
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<bool> RegisterAccountAsync(UserAccount data)
        {
            return await userAccountDB.RegisterAsync(data);
        }

        public static async Task<List<string>> GetRoleNamesUserAsync(int id)
        {
            return await userAccountDB.GetRoleNamesAsync(id);
        }
    }
}