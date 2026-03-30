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
                    UserId,
                    UserName,
                    DisplayName,
                    Email,
                    Photo,
                    RoleNames
                FROM Users
                WHERE UserName = @userName
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
                UPDATE Users
                SET Password = @password
                WHERE UserName = @userName";

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
    }
}