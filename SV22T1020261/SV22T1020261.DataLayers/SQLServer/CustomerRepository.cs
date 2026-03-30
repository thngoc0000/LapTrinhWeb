using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020261.DataLayers.Interfaces;
using SV22T1020261.Models.Common;
using SV22T1020261.Models.Partner;

namespace SV22T1020261.DataLayers.SQLServer
{
    /// <summary>
    /// Cài đặt các thao tác xử lý dữ liệu đối với bảng Customers
    /// trong SQL Server sử dụng thư viện Dapper
    /// </summary>
    public class CustomerRepository : ICustomerRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo đối tượng CustomerRepository
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến cơ sở dữ liệu</param>
        public CustomerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Tạo kết nối đến SQL Server
        /// </summary>
        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Truy vấn danh sách khách hàng theo điều kiện tìm kiếm
        /// và trả về kết quả phân trang
        /// </summary>
        public async Task<PagedResult<Customer>> ListAsync(PaginationSearchInput input)
        {
            using var connection = GetConnection();

            var result = new PagedResult<Customer>()
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            string countSql = @"
                SELECT COUNT(*)
                FROM Customers
                WHERE (@SearchValue = '' 
                OR CustomerName LIKE '%' + @SearchValue + '%'
                OR ContactName LIKE '%' + @SearchValue + '%'
                OR Phone LIKE '%' + @SearchValue + '%')";

            result.RowCount = await connection.ExecuteScalarAsync<int>(countSql, new
            {
                input.SearchValue
            });

            if (result.RowCount == 0)
                return result;

            string dataSql = @"
                SELECT CustomerID,
                       CustomerName,
                       ContactName,
                       Province,
                       Address,
                       Phone,
                       Email,
                       IsLocked
                FROM Customers
                WHERE (@SearchValue = '' 
                OR CustomerName LIKE '%' + @SearchValue + '%'
                OR ContactName LIKE '%' + @SearchValue + '%'
                OR Phone LIKE '%' + @SearchValue + '%')
                ORDER BY CustomerName
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            var data = await connection.QueryAsync<Customer>(dataSql, new
            {
                input.SearchValue,
                Offset = input.Offset,
                input.PageSize
            });

            result.DataItems = data.ToList();
            return result;
        }

        /// <summary>
        /// Lấy thông tin của một khách hàng theo mã CustomerID
        /// </summary>
        public async Task<Customer?> GetAsync(int id)
        {
            using var connection = GetConnection();

            string sql = @"
                SELECT CustomerID,
                       CustomerName,
                       ContactName,
                       Province,
                       Address,
                       Phone,
                       Email,
                       IsLocked
                FROM Customers
                WHERE CustomerID = @id";

            return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { id });
        }

        /// <summary>
        /// Thêm mới một khách hàng vào cơ sở dữ liệu
        /// </summary>
        /// <returns>Mã CustomerID của bản ghi vừa tạo</returns>
        public async Task<int> AddAsync(Customer data)
        {
            using var connection = GetConnection();

            string sql = @"
                INSERT INTO Customers
                (
                    CustomerName,
                    ContactName,
                    Province,
                    Address,
                    Phone,
                    Email,
                    IsLocked
                )
                VALUES
                (
                    @CustomerName,
                    @ContactName,
                    @Province,
                    @Address,
                    @Phone,
                    @Email,
                    @IsLocked
                );

                SELECT SCOPE_IDENTITY();";

            var id = await connection.ExecuteScalarAsync<decimal>(sql, data);
            return Convert.ToInt32(id);
        }

        /// <summary>
        /// Cập nhật thông tin khách hàng
        /// </summary>
        public async Task<bool> UpdateAsync(Customer data)
        {
            using var connection = GetConnection();

            string sql = @"
                UPDATE Customers
                SET
                    CustomerName = @CustomerName,
                    ContactName = @ContactName,
                    Province = @Province,
                    Address = @Address,
                    Phone = @Phone,
                    Email = @Email,
                    IsLocked = @IsLocked
                WHERE CustomerID = @CustomerID";

            int rows = await connection.ExecuteAsync(sql, data);

            return rows > 0;
        }

        /// <summary>
        /// Xóa một khách hàng khỏi cơ sở dữ liệu
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = GetConnection();

            string sql = @"DELETE FROM Customers WHERE CustomerID = @id";

            int rows = await connection.ExecuteAsync(sql, new { id });

            return rows > 0;
        }

        /// <summary>
        /// Kiểm tra xem khách hàng có dữ liệu liên quan trong bảng Orders hay không
        /// </summary>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = GetConnection();

            string sql = @"SELECT COUNT(*) 
                           FROM Orders
                           WHERE CustomerID = @id";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { id });

            return count > 0;
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ của email khách hàng
        /// </summary>
        /// <param name="email">Email cần kiểm tra</param>
        /// <param name="id">
        /// id = 0: kiểm tra email khi thêm mới khách hàng  
        /// id ≠ 0: kiểm tra email khi cập nhật khách hàng
        /// </param>
        /// <returns>
        /// True nếu email hợp lệ (không bị trùng), False nếu email đã tồn tại
        /// </returns>
        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            using var connection = GetConnection();

            string sql;

            if (id == 0)
            {
                sql = @"SELECT COUNT(*)
                        FROM Customers
                        WHERE Email = @email";
                int count = await connection.ExecuteScalarAsync<int>(sql, new { email });
                return count == 0;
            }
            else
            {
                sql = @"SELECT COUNT(*)
                        FROM Customers
                        WHERE Email = @email
                        AND CustomerID <> @id";

                int count = await connection.ExecuteScalarAsync<int>(sql, new { email, id });
                return count == 0;
            }
        }
    }
}