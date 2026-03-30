using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020261.DataLayers.Interfaces;
using SV22T1020261.Models.Common;
using SV22T1020261.Models.Partner;

namespace SV22T1020261.DataLayers.SQLServer
{
    /// <summary>
    /// Cài đặt các thao tác xử lý dữ liệu đối với bảng Suppliers
    /// trong SQL Server sử dụng Dapper
    /// </summary>
    public class SupplierRepository : IGenericRepository<Supplier>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo repository với chuỗi kết nối tới SQL Server
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối cơ sở dữ liệu</param>
        public SupplierRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Tạo và trả về đối tượng kết nối SQL Server
        /// </summary>
        /// <returns>SqlConnection</returns>
        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Truy vấn danh sách nhà cung cấp theo điều kiện tìm kiếm
        /// và trả về kết quả dạng phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả phân trang chứa danh sách nhà cung cấp</returns>
        public async Task<PagedResult<Supplier>> ListAsync(PaginationSearchInput input)
        {
            using var connection = GetConnection();

            var result = new PagedResult<Supplier>()
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            string countSql = @"SELECT COUNT(*) 
                                FROM Suppliers
                                WHERE (@SearchValue = '' 
                                OR SupplierName LIKE '%' + @SearchValue + '%'
                                OR ContactName LIKE '%' + @SearchValue + '%'
                                OR Phone LIKE '%' + @SearchValue + '%')";

            result.RowCount = await connection.ExecuteScalarAsync<int>(countSql, new
            {
                input.SearchValue
            });

            if (result.RowCount == 0)
                return result;
            if (input.PageSize == 0)
            {
                string sql = @"
        SELECT *
        FROM Suppliers
        WHERE (@SearchValue = '' 
        OR SupplierName LIKE '%' + @SearchValue + '%'
        OR ContactName LIKE '%' + @SearchValue + '%'
        OR Phone LIKE '%' + @SearchValue + '%')
        ORDER BY SupplierName";

                var data1 = await connection.QueryAsync<Supplier>(sql, new
                {
                    input.SearchValue
                });

                result.DataItems = data1.ToList();
                result.RowCount = result.DataItems.Count;

                return result;
            }

            string dataSql = @"
                    SELECT *
                    FROM Suppliers
                    WHERE (@SearchValue = '' 
                    OR SupplierName LIKE '%' + @SearchValue + '%'
                    OR ContactName LIKE '%' + @SearchValue + '%'
                    OR Phone LIKE '%' + @SearchValue + '%')
                    ORDER BY SupplierName
                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY";

            var data = await connection.QueryAsync<Supplier>(dataSql, new
            {
                input.SearchValue,
                Offset = input.Offset,
                input.PageSize
            });

            result.DataItems = data.ToList();

            return result;
        }

        /// <summary>
        /// Lấy thông tin của một nhà cung cấp theo mã SupplierID
        /// </summary>
        /// <param name="id">Mã nhà cung cấp</param>
        /// <returns>Thông tin nhà cung cấp hoặc null nếu không tồn tại</returns>
        public async Task<Supplier?> GetAsync(int id)
        {
            using var connection = GetConnection();

            string sql = @"SELECT *
                           FROM Suppliers
                           WHERE SupplierID = @id";

            return await connection.QueryFirstOrDefaultAsync<Supplier>(sql, new { id });
        }

        /// <summary>
        /// Thêm mới một nhà cung cấp vào cơ sở dữ liệu
        /// </summary>
        /// <param name="data">Thông tin nhà cung cấp cần thêm</param>
        /// <returns>Mã SupplierID của bản ghi vừa được tạo</returns>
        public async Task<int> AddAsync(Supplier data)
        {
            using var connection = GetConnection();

            string sql = @"
                INSERT INTO Suppliers
                (
                    SupplierName,
                    ContactName,
                    Province,
                    Address,
                    Phone,
                    Email
                )
                VALUES
                (
                    @SupplierName,
                    @ContactName,
                    @Province,
                    @Address,
                    @Phone,
                    @Email
                );
                SELECT SCOPE_IDENTITY();
            ";

            var id = await connection.ExecuteScalarAsync<decimal>(sql, data);
            return Convert.ToInt32(id);
        }

        /// <summary>
        /// Cập nhật thông tin nhà cung cấp
        /// </summary>
        /// <param name="data">Thông tin nhà cung cấp cần cập nhật</param>
        /// <returns>True nếu cập nhật thành công</returns>
        public async Task<bool> UpdateAsync(Supplier data)
        {
            using var connection = GetConnection();

            string sql = @"
                UPDATE Suppliers
                SET
                    SupplierName = @SupplierName,
                    ContactName = @ContactName,
                    Province = @Province,
                    Address = @Address,
                    Phone = @Phone,
                    Email = @Email
                WHERE SupplierID = @SupplierID
            ";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Xóa một nhà cung cấp khỏi cơ sở dữ liệu
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần xóa</param>
        /// <returns>True nếu xóa thành công</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = GetConnection();

            string sql = @"DELETE FROM Suppliers
                           WHERE SupplierID = @id";

            int rows = await connection.ExecuteAsync(sql, new { id });
            return rows > 0;
        }

        /// <summary>
        /// Kiểm tra xem nhà cung cấp có được sử dụng trong bảng Products hay không
        /// </summary>
        /// <param name="id">Mã nhà cung cấp</param>
        /// <returns>True nếu nhà cung cấp đang được sử dụng</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = GetConnection();

            string sql = @"SELECT COUNT(*)
                           FROM Products
                           WHERE SupplierID = @id";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { id });

            return count > 0;
        }
    }
}