using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020261.DataLayers.Interfaces;
using SV22T1020261.Models.Catalog;
using SV22T1020261.Models.Common;

namespace SV22T1020261.DataLayers.SQLServer
{
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        // =========================
        // PRODUCT
        // =========================

        public async Task<PagedResult<Product>> ListAsync(ProductSearchInput input)
        {
            using var connection = GetConnection();

            var result = new PagedResult<Product>()
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            string countSql = @"
                SELECT COUNT(*)
                FROM Products
                WHERE (@SearchValue = '' OR ProductName LIKE '%' + @SearchValue + '%')
                AND (@CategoryID = 0 OR CategoryID = @CategoryID)
                AND (@SupplierID = 0 OR SupplierID = @SupplierID)
                AND (@MinPrice = 0 OR Price >= @MinPrice)
                AND (@MaxPrice = 0 OR Price <= @MaxPrice)";

            result.RowCount = await connection.ExecuteScalarAsync<int>(countSql, input);

            if (result.RowCount == 0)
                return result;

            string sql = @"
                SELECT *
                FROM Products
                WHERE (@SearchValue = '' OR ProductName LIKE '%' + @SearchValue + '%')
                AND (@CategoryID = 0 OR CategoryID = @CategoryID)
                AND (@SupplierID = 0 OR SupplierID = @SupplierID)
                AND (@MinPrice = 0 OR Price >= @MinPrice)
                AND (@MaxPrice = 0 OR Price <= @MaxPrice)

                ORDER BY ProductName
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            var data = await connection.QueryAsync<Product>(sql, input);

            result.DataItems = data.ToList();

            return result;
        }

        public async Task<Product?> GetAsync(int productID)
        {
            using var connection = GetConnection();

            string sql = @"SELECT * FROM Products WHERE ProductID=@productID";

            return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { productID });
        }

        public async Task<int> AddAsync(Product data)
        {
            using var connection = GetConnection();

            string sql = @"
                INSERT INTO Products
                (
                    ProductName,
                    ProductDescription,
                    SupplierID,
                    CategoryID,
                    Unit,
                    Price,
                    Photo,
                    IsSelling
                )
                VALUES
                (
                    @ProductName,
                    @ProductDescription,
                    @SupplierID,
                    @CategoryID,
                    @Unit,
                    @Price,
                    @Photo,
                    @IsSelling
                );

                SELECT SCOPE_IDENTITY();";

            var id = await connection.ExecuteScalarAsync<decimal>(sql, data);

            return Convert.ToInt32(id);
        }

        public async Task<bool> UpdateAsync(Product data)
        {
            using var connection = GetConnection();

            string sql = @"
                UPDATE Products
                SET
                    ProductName=@ProductName,
                    ProductDescription=@ProductDescription,
                    SupplierID=@SupplierID,
                    CategoryID=@CategoryID,
                    Unit=@Unit,
                    Price=@Price,
                    Photo=@Photo,
                    IsSelling=@IsSelling
                WHERE ProductID=@ProductID";

            int rows = await connection.ExecuteAsync(sql, data);

            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int productID)
        {
            using var connection = GetConnection();

            string sql = @"DELETE FROM Products WHERE ProductID=@productID";

            int rows = await connection.ExecuteAsync(sql, new { productID });

            return rows > 0;
        }

        public async Task<bool> IsUsedAsync(int productID)
        {
            using var connection = GetConnection();

            string sql = @"SELECT COUNT(*) FROM OrderDetails WHERE ProductID=@productID";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { productID });

            return count > 0;
        }

        // =========================
        // PRODUCT ATTRIBUTES
        // =========================

        public async Task<List<ProductAttribute>> ListAttributesAsync(int productID)
        {
            using var connection = GetConnection();

            string sql = @"
                SELECT *
                FROM ProductAttributes
                WHERE ProductID=@productID
                ORDER BY DisplayOrder";

            var data = await connection.QueryAsync<ProductAttribute>(sql, new { productID });

            return data.ToList();
        }

        public async Task<ProductAttribute?> GetAttributeAsync(long attributeID)
        {
            using var connection = GetConnection();

            string sql = @"SELECT * FROM ProductAttributes WHERE AttributeID=@attributeID";

            return await connection.QueryFirstOrDefaultAsync<ProductAttribute>(sql, new { attributeID });
        }

        public async Task<long> AddAttributeAsync(ProductAttribute data)
        {
            using var connection = GetConnection();

            string sql = @"
                INSERT INTO ProductAttributes
                (
                    ProductID,
                    AttributeName,
                    AttributeValue,
                    DisplayOrder
                )
                VALUES
                (
                    @ProductID,
                    @AttributeName,
                    @AttributeValue,
                    @DisplayOrder
                );

                SELECT SCOPE_IDENTITY();";

            var id = await connection.ExecuteScalarAsync<decimal>(sql, data);

            return Convert.ToInt64(id);
        }

        public async Task<bool> UpdateAttributeAsync(ProductAttribute data)
        {
            using var connection = GetConnection();

            string sql = @"
                UPDATE ProductAttributes
                SET
                    AttributeName=@AttributeName,
                    AttributeValue=@AttributeValue,
                    DisplayOrder=@DisplayOrder
                WHERE AttributeID=@AttributeID";

            int rows = await connection.ExecuteAsync(sql, data);

            return rows > 0;
        }

        public async Task<bool> DeleteAttributeAsync(long attributeID)
        {
            using var connection = GetConnection();

            string sql = @"DELETE FROM ProductAttributes WHERE AttributeID=@attributeID";

            int rows = await connection.ExecuteAsync(sql, new { attributeID });

            return rows > 0;
        }

        // =========================
        // PRODUCT PHOTOS
        // =========================

        public async Task<List<ProductPhoto>> ListPhotosAsync(int productID)
        {
            using var connection = GetConnection();

            string sql = @"
                SELECT *
                FROM ProductPhotos
                WHERE ProductID=@productID
                ORDER BY DisplayOrder";

            var data = await connection.QueryAsync<ProductPhoto>(sql, new { productID });

            return data.ToList();
        }

        public async Task<ProductPhoto?> GetPhotoAsync(long photoID)
        {
            using var connection = GetConnection();

            string sql = @"SELECT * FROM ProductPhotos WHERE PhotoID=@photoID";

            return await connection.QueryFirstOrDefaultAsync<ProductPhoto>(sql, new { photoID });
        }

        public async Task<long> AddPhotoAsync(ProductPhoto data)
        {
            using var connection = GetConnection();

            string sql = @"
                INSERT INTO ProductPhotos
                (
                    ProductID,
                    Photo,
                    Description,
                    DisplayOrder,
                    IsHidden
                )
                VALUES
                (
                    @ProductID,
                    @Photo,
                    @Description,
                    @DisplayOrder,
                    @IsHidden
                );

                SELECT SCOPE_IDENTITY();";

            var id = await connection.ExecuteScalarAsync<decimal>(sql, data);

            return Convert.ToInt64(id);
        }

        public async Task<bool> UpdatePhotoAsync(ProductPhoto data)
        {
            using var connection = GetConnection();

            string sql = @"
                UPDATE ProductPhotos
                SET
                    Photo=@Photo,
                    Description=@Description,
                    DisplayOrder=@DisplayOrder,
                    IsHidden=@IsHidden
                WHERE PhotoID=@PhotoID";

            int rows = await connection.ExecuteAsync(sql, data);

            return rows > 0;
        }

        public async Task<bool> DeletePhotoAsync(long photoID)
        {
            using var connection = GetConnection();

            string sql = @"DELETE FROM ProductPhotos WHERE PhotoID=@photoID";

            int rows = await connection.ExecuteAsync(sql, new { photoID });

            return rows > 0;
        }
    }
}