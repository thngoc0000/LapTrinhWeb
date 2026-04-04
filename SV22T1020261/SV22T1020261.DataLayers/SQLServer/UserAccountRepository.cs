using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020261.DataLayers.Interfaces;
using SV22T1020261.Models.Security;

namespace SV22T1020261.DataLayers.SQLServer
{
    public class UserAccountRepository : ISecurityRepository<UserAccount>
    {
        private readonly string _connectionString;

        public UserAccountRepository(string connectionString)
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
        public async Task<UserAccount?> AuthorizeAsync(string userName, string password)
        {
            using var connection = GetConnection();

            string sql = @"
                SELECT
                    CAST(EmployeeID AS nvarchar(50)) AS UserId,
                    FullName,
                    Email,
                    Photo,
                    RoleNames
                FROM Employees
                WHERE Email = @userName
                AND Password = @password";

            return await connection.QueryFirstOrDefaultAsync<UserAccount>(
                sql,
                new { userName, password }
            );
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        public async Task<bool> ChangePasswordAsync(string userName, string password)
        {
            using var connection = GetConnection();

            string sql = @"
                UPDATE Employees
                SET Password = @password
                WHERE Email = @userName";

            int rows = await connection.ExecuteAsync(
                sql,
                new { userName, password }
            );

            return rows > 0;
        }

        public Task<bool> RegisterAsync(UserAccount account)
        {
            throw new NotImplementedException();
        }

        public async Task<List<string>> GetRoleNamesAsync(int id)
        {
            using var connection = GetConnection();

            // Chỉ cần lấy cột RoleNames từ DB
            string sql = "SELECT RoleNames FROM Employees WHERE EmployeeID = @id";

            // Lấy chuỗi thô (ví dụ: "admin,sales")
            string? roleString = await connection.ExecuteScalarAsync<string>(sql, new { id });

            // Nếu chuỗi rỗng hoặc null thì trả về danh sách trống
            if (string.IsNullOrEmpty(roleString))
                return new List<string>();

            // Cắt chuỗi thành List và dọn dẹp khoảng trắng
            return roleString.Split(',')
                             .Select(r => r.Trim())
                             .Where(r => !string.IsNullOrEmpty(r))
                             .ToList();
        }
    }
}