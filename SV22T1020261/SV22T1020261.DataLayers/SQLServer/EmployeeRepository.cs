using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020261.DataLayers.Interfaces;
using SV22T1020261.Models.Common;
using SV22T1020261.Models.HR;

namespace SV22T1020261.DataLayers.SQLServer
{
    /// <summary>
    /// Cài đặt các thao tác xử lý dữ liệu đối với bảng Employees
    /// trong SQL Server sử dụng thư viện Dapper
    /// </summary>
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo đối tượng EmployeeRepository
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến cơ sở dữ liệu</param>
        public EmployeeRepository(string connectionString)
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
        /// Truy vấn danh sách nhân viên theo điều kiện tìm kiếm
        /// và trả về kết quả phân trang
        /// </summary>
        public async Task<PagedResult<Employee>> ListAsync(PaginationSearchInput input)
        {
            using var connection = GetConnection();

            var result = new PagedResult<Employee>()
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            string countSql = @"
                SELECT COUNT(*)
                FROM Employees
                WHERE (@SearchValue = ''
                OR FullName LIKE '%' + @SearchValue + '%'
                OR Phone LIKE '%' + @SearchValue + '%'
                OR Email LIKE '%' + @SearchValue + '%')";

            result.RowCount = await connection.ExecuteScalarAsync<int>(countSql, new
            {
                input.SearchValue
            });

            if (result.RowCount == 0)
                return result;

            string dataSql = @"
                SELECT EmployeeID,
                       FullName,
                       BirthDate,
                       Address,
                       Phone,
                       Email,
                       Photo,
                       IsWorking
                FROM Employees
                WHERE (@SearchValue = ''
                OR FullName LIKE '%' + @SearchValue + '%'
                OR Phone LIKE '%' + @SearchValue + '%'
                OR Email LIKE '%' + @SearchValue + '%')
                ORDER BY FullName
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            var data = await connection.QueryAsync<Employee>(dataSql, new
            {
                input.SearchValue,
                Offset = input.Offset,
                input.PageSize
            });

            result.DataItems = data.ToList();
            return result;
        }

        /// <summary>
        /// Lấy thông tin của một nhân viên theo mã EmployeeID
        /// </summary>
        public async Task<Employee?> GetAsync(int id)
        {
            using var connection = GetConnection();

            string sql = @"
                SELECT EmployeeID,
                       FullName,
                       BirthDate,
                       Address,
                       Phone,
                       Email,
                       Photo,
                       IsWorking
                FROM Employees
                WHERE EmployeeID = @id";

            return await connection.QueryFirstOrDefaultAsync<Employee>(sql, new { id });
        }

        /// <summary>
        /// Thêm mới một nhân viên vào cơ sở dữ liệu
        /// </summary>
        /// <returns>Mã EmployeeID của bản ghi vừa tạo</returns>
        public async Task<int> AddAsync(Employee data)
        {
            using var connection = GetConnection();

            string sql = @"
                INSERT INTO Employees
                (
                    FullName,
                    BirthDate,
                    Address,
                    Phone,
                    Email,
                    Photo,
                    IsWorking
                )
                VALUES
                (
                    @FullName,
                    @BirthDate,
                    @Address,
                    @Phone,
                    @Email,
                    @Photo,
                    @IsWorking
                );

                SELECT SCOPE_IDENTITY();";

            var id = await connection.ExecuteScalarAsync<decimal>(sql, data);
            return Convert.ToInt32(id);
        }

        /// <summary>
        /// Cập nhật thông tin nhân viên
        /// </summary>
        public async Task<bool> UpdateAsync(Employee data)
        {
            using var connection = GetConnection();

            string sql = @"
                UPDATE Employees
                SET
                    FullName = @FullName,
                    BirthDate = @BirthDate,
                    Address = @Address,
                    Phone = @Phone,
                    Email = @Email,
                    Photo = @Photo,
                    IsWorking = @IsWorking
                WHERE EmployeeID = @EmployeeID";

            int rows = await connection.ExecuteAsync(sql, data);

            return rows > 0;
        }

        /// <summary>
        /// Xóa một nhân viên khỏi cơ sở dữ liệu
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = GetConnection();

            string sql = @"DELETE FROM Employees WHERE EmployeeID = @id";

            int rows = await connection.ExecuteAsync(sql, new { id });

            return rows > 0;
        }

        /// <summary>
        /// Kiểm tra xem nhân viên có dữ liệu liên quan trong bảng Orders hay không
        /// </summary>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = GetConnection();

            string sql = @"SELECT COUNT(*)
                           FROM Orders
                           WHERE EmployeeID = @id";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { id });

            return count > 0;
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ của email nhân viên
        /// </summary>
        /// <param name="email">Email cần kiểm tra</param>
        /// <param name="id">
        /// id = 0: kiểm tra khi thêm mới nhân viên  
        /// id ≠ 0: kiểm tra khi cập nhật nhân viên
        /// </param>
        /// <returns>True nếu email hợp lệ (không trùng), False nếu đã tồn tại</returns>
        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            using var connection = GetConnection();

            string sql;

            if (id == 0)
            {
                sql = @"SELECT COUNT(*)
                        FROM Employees
                        WHERE Email = @email";

                int count = await connection.ExecuteScalarAsync<int>(sql, new { email });
                return count == 0;
            }
            else
            {
                sql = @"SELECT COUNT(*)
                        FROM Employees
                        WHERE Email = @email
                        AND EmployeeID <> @id";

                int count = await connection.ExecuteScalarAsync<int>(sql, new { email, id });
                return count == 0;
            }
        }
    }
}