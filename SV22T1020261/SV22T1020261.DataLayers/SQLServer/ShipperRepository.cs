using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020261.DataLayers.Interfaces;
using SV22T1020261.Models.Common;
using SV22T1020261.Models.Partner;

namespace SV22T1020261.DataLayers.SQLServer
{
    /// <summary>
    /// Cài đặt các thao tác xử lý dữ liệu đối với bảng Shippers
    /// trong SQL Server sử dụng thư viện Dapper
    /// </summary>
    public class ShipperRepository : IGenericRepository<Shipper>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo đối tượng ShipperRepository
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến cơ sở dữ liệu</param>
        public ShipperRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Tạo kết nối tới SQL Server
        /// </summary>
        /// <returns>Đối tượng SqlConnection</returns>
        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Truy vấn danh sách người giao hàng theo điều kiện tìm kiếm
        /// và trả về kết quả phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả phân trang chứa danh sách người giao hàng</returns>
        public async Task<PagedResult<Shipper>> ListAsync(PaginationSearchInput input)
        {
            using var connection = GetConnection();

            var result = new PagedResult<Shipper>()
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            string countSql = @"
                SELECT COUNT(*)
                FROM Shippers
                WHERE (@SearchValue = '' 
                OR ShipperName LIKE '%' + @SearchValue + '%'
                OR Phone LIKE '%' + @SearchValue + '%')";

            result.RowCount = await connection.ExecuteScalarAsync<int>(countSql, new
            {
                input.SearchValue
            });

            if (result.RowCount == 0)
                return result;

            string dataSql = @"
                SELECT *
                FROM Shippers
                WHERE (@SearchValue = '' 
                OR ShipperName LIKE '%' + @SearchValue + '%'
                OR Phone LIKE '%' + @SearchValue + '%')
                ORDER BY ShipperName
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            var data = await connection.QueryAsync<Shipper>(dataSql, new
            {
                input.SearchValue,
                Offset = input.Offset,
                input.PageSize
            });

            result.DataItems = data.ToList();

            return result;
        }

        /// <summary>
        /// Lấy thông tin của một người giao hàng theo mã ShipperID
        /// </summary>
        /// <param name="id">Mã người giao hàng</param>
        /// <returns>Thông tin người giao hàng hoặc null nếu không tồn tại</returns>
        public async Task<Shipper?> GetAsync(int id)
        {
            using var connection = GetConnection();

            string sql = @"SELECT *
                           FROM Shippers
                           WHERE ShipperID = @id";

            return await connection.QueryFirstOrDefaultAsync<Shipper>(sql, new { id });
        }

        /// <summary>
        /// Thêm mới một người giao hàng vào cơ sở dữ liệu
        /// </summary>
        /// <param name="data">Thông tin người giao hàng cần thêm</param>
        /// <returns>Mã ShipperID của bản ghi vừa được tạo</returns>
        public async Task<int> AddAsync(Shipper data)
        {
            using var connection = GetConnection();

            string sql = @"
                INSERT INTO Shippers
                (
                    ShipperName,
                    Phone
                )
                VALUES
                (
                    @ShipperName,
                    @Phone
                );
                SELECT SCOPE_IDENTITY();
            ";

            var id = await connection.ExecuteScalarAsync<decimal>(sql, data);
            return Convert.ToInt32(id);
        }

        /// <summary>
        /// Cập nhật thông tin người giao hàng
        /// </summary>
        /// <param name="data">Thông tin người giao hàng cần cập nhật</param>
        /// <returns>True nếu cập nhật thành công</returns>
        public async Task<bool> UpdateAsync(Shipper data)
        {
            using var connection = GetConnection();

            string sql = @"
                UPDATE Shippers
                SET
                    ShipperName = @ShipperName,
                    Phone = @Phone
                WHERE ShipperID = @ShipperID";

            int rows = await connection.ExecuteAsync(sql, data);

            return rows > 0;
        }

        /// <summary>
        /// Xóa một người giao hàng khỏi cơ sở dữ liệu
        /// </summary>
        /// <param name="id">Mã người giao hàng cần xóa</param>
        /// <returns>True nếu xóa thành công</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = GetConnection();

            string sql = @"DELETE FROM Shippers
                           WHERE ShipperID = @id";

            int rows = await connection.ExecuteAsync(sql, new { id });

            return rows > 0;
        }

        /// <summary>
        /// Kiểm tra xem người giao hàng có được sử dụng trong bảng Orders hay không
        /// </summary>
        /// <param name="id">Mã người giao hàng</param>
        /// <returns>True nếu đang được sử dụng</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = GetConnection();

            string sql = @"SELECT COUNT(*)
                           FROM Orders
                           WHERE ShipperID = @id";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { id });

            return count > 0;
        }
    }
}