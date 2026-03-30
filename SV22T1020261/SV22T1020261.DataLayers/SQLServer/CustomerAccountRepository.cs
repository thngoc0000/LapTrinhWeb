using Microsoft.AspNetCore.Identity;
using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020261.DataLayers.Interfaces;
using SV22T1020261.Models.Security;

namespace SV22T1020261.DataLayers.SQLServer
{
    public class CustomerAccountRepository : ISecurityRepository<CustomerAccount>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo đối tượng CategoryRepository
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối cơ sở dữ liệu</param>
        public CustomerAccountRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Kiểm tra đăng nhập
        /// </summary>
        public async Task<CustomerAccount?> AuthorizeAsync(string email, string password)
        {
            using var connection = GetConnection();

            string sql = @"
                SELECT CustomerID, CustomerName, ContactName, Email, Password, IsLocked
                FROM Customers
                WHERE Email = @Email AND Password=@Password";

            var account = await connection.QueryFirstOrDefaultAsync<CustomerAccount>(
                sql,
                new { Email = email, Password = password }
            );

            return account;
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        public async Task<bool> ChangePasswordAsync(string email, string password)
        {
            using var connection = GetConnection();

            string sql = @"
                UPDATE Customers
                SET Password = @password
                WHERE Email = @Email";

            int rows = await connection.ExecuteAsync(
                sql,
                new { email, password }
            );

            return rows > 0;
        }

        /// <summary>
        /// Đăng ký tài khoản khách hàng
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public async Task<bool> RegisterAsync(CustomerAccount account)
        {
            using var connection = GetConnection();

            string sql = @"
                INSERT INTO Customers (CustomerName, ContactName, Email, Password, IsLocked)
                VALUES (@CustomerName, @ContactName, @Email, @Password, 0)";

            int rows = await connection.ExecuteAsync(
                sql,
                new
                {
                    account.CustomerName,
                    account.ContactName,
                    account.Email,
                    account.Password
                }
            );

            return rows > 0;
        }
    }
}
