using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020261.DataLayers.Interfaces;
using SV22T1020261.Models.Common;
using SV22T1020261.Models.Catalog;

namespace SV22T1020261.DataLayers.SQLServer
{
    /// <summary>
    /// Cài đặt các thao tác xử lý dữ liệu đối với bảng Categories
    /// trong SQL Server sử dụng thư viện Dapper
    /// </summary>
    public class CategoryRepository : IGenericRepository<Category>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo đối tượng CategoryRepository
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối cơ sở dữ liệu</param>
        public CategoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Tạo kết nối đến SQL Server
        /// </summary>
        /// <returns>Đối tượng SqlConnection</returns>
        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Truy vấn danh sách loại hàng theo điều kiện tìm kiếm
        /// và trả về kết quả phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả phân trang chứa danh sách loại hàng</returns>
        public async Task<PagedResult<Category>> ListAsync(PaginationSearchInput input)
        {
            using var connection = GetConnection();

            var result = new PagedResult<Category>()
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            string whereClause = @"
        WHERE (@SearchValue = '' 
        OR CategoryName LIKE '%' + @SearchValue + '%')";

            // ===== 1️⃣ TRƯỜNG HỢP LẤY TOÀN BỘ (PageSize = 0) =====
            if (input.PageSize == 0)
            {
                string sql = $@"
            SELECT *
            FROM Categories
            {whereClause}
            ORDER BY CategoryName";

                var data = await connection.QueryAsync<Category>(sql, new
                {
                    input.SearchValue
                });

                result.DataItems = data.ToList();
                result.RowCount = result.DataItems.Count;

                return result;
            }

            // ===== 2️⃣ TRƯỜNG HỢP CÓ PHÂN TRANG =====

            // Đếm tổng số dòng
            string countSql = $@"
        SELECT COUNT(*)
        FROM Categories
        {whereClause}";

            result.RowCount = await connection.ExecuteScalarAsync<int>(countSql, new
            {
                input.SearchValue
            });

            if (result.RowCount == 0)
                return result;

            // Lấy dữ liệu theo trang
            string dataSql = $@"
        SELECT *
        FROM Categories
        {whereClause}
        ORDER BY CategoryName
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY";

            var dataPage = await connection.QueryAsync<Category>(dataSql, new
            {
                input.SearchValue,
                Offset = input.Offset,
                input.PageSize
            });

            result.DataItems = dataPage.ToList();

            return result;
        }

        /// <summary>
        /// Lấy thông tin của một loại hàng theo mã CategoryID
        /// </summary>
        /// <param name="id">Mã loại hàng</param>
        /// <returns>Thông tin loại hàng hoặc null nếu không tồn tại</returns>
        public async Task<Category?> GetAsync(int id)
        {
            using var connection = GetConnection();

            string sql = @"SELECT *
                           FROM Categories
                           WHERE CategoryID = @id";

            return await connection.QueryFirstOrDefaultAsync<Category>(sql, new { id });
        }

        /// <summary>
        /// Thêm mới một loại hàng vào cơ sở dữ liệu
        /// </summary>
        /// <param name="data">Thông tin loại hàng cần thêm</param>
        /// <returns>Mã CategoryID của bản ghi vừa được tạo</returns>
        public async Task<int> AddAsync(Category data)
        {
            using var connection = GetConnection();

            string sql = @"
                INSERT INTO Categories
                (
                    CategoryName,
                    Description
                )
                VALUES
                (
                    @CategoryName,
                    @Description
                );
                SELECT SCOPE_IDENTITY();";

            var id = await connection.ExecuteScalarAsync<decimal>(sql, data);
            return Convert.ToInt32(id);
        }

        /// <summary>
        /// Cập nhật thông tin loại hàng
        /// </summary>
        /// <param name="data">Thông tin loại hàng cần cập nhật</param>
        /// <returns>True nếu cập nhật thành công</returns>
        public async Task<bool> UpdateAsync(Category data)
        {
            using var connection = GetConnection();

            string sql = @"
                UPDATE Categories
                SET
                    CategoryName = @CategoryName,
                    Description = @Description
                WHERE CategoryID = @CategoryID";

            int rows = await connection.ExecuteAsync(sql, data);

            return rows > 0;
        }

        /// <summary>
        /// Xóa một loại hàng khỏi cơ sở dữ liệu
        /// </summary>
        /// <param name="id">Mã loại hàng cần xóa</param>
        /// <returns>True nếu xóa thành công</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = GetConnection();

            string sql = @"DELETE FROM Categories
                           WHERE CategoryID = @id";

            int rows = await connection.ExecuteAsync(sql, new { id });

            return rows > 0;
        }

        /// <summary>
        /// Kiểm tra xem loại hàng có đang được sử dụng trong bảng Products hay không
        /// </summary>
        /// <param name="id">Mã loại hàng</param>
        /// <returns>True nếu loại hàng đang được sử dụng</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = GetConnection();

            string sql = @"SELECT COUNT(*)
                           FROM Products
                           WHERE CategoryID = @id";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { id });

            return count > 0;
        }
    }
}