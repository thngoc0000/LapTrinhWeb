using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020261.DataLayers.Interfaces;
using SV22T1020261.Models.Common;
using SV22T1020261.Models.Sales;

namespace SV22T1020261.DataLayers.SQLServer
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;

        public OrderRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        // ======================================
        // ORDER
        // ======================================

        public async Task<PagedResult<OrderViewInfo>> ListAsync(OrderSearchInput input)
        {
            using var connection = GetConnection();

            var result = new PagedResult<OrderViewInfo>()
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            string countSql = @"
                SELECT COUNT(*)

                FROM Orders o

                LEFT JOIN Customers c ON o.CustomerID = c.CustomerID

                WHERE (@Status = 0 OR o.Status = @Status)
                AND (@DateFrom IS NULL OR o.OrderTime >= @DateFrom)
                AND (@DateTo IS NULL OR o.OrderTime <= @DateTo)
                AND (@SearchValue IS NULL OR c.CustomerName LIKE '%' + @SearchValue + '%')";

            result.RowCount = await connection.ExecuteScalarAsync<int>(countSql, input);

            if (result.RowCount == 0)
                return result;

            string sql = @"
                   SELECT 
                        o.OrderID,
                        o.OrderTime,
                        o.AcceptTime,
                        o.Status,
                        o.EmployeeID,
                        o.CustomerID,
                        o.ShipperID,

                        e.FullName AS EmployeeName,

                        c.CustomerName,
                        c.ContactName AS CustomerContactName,
                        c.Email AS CustomerEmail,
                        c.Phone AS CustomerPhone,
                        c.Address AS CustomerAddress,

                        s.ShipperName,
                        s.Phone AS ShipperPhone,

                        ISNULL(SUM(od.Quantity * od.SalePrice), 0) AS TotalAmount

                    FROM Orders o

                    LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
                    LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                    LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
                    LEFT JOIN OrderDetails od ON o.OrderID = od.OrderID

                    WHERE (@Status = 0 OR o.Status = @Status)
                    AND (@DateFrom IS NULL OR o.OrderTime >= @DateFrom)
                    AND (@DateTo IS NULL OR o.OrderTime <= @DateTo)
                    AND (@SearchValue IS NULL OR c.CustomerName LIKE '%' + @SearchValue + '%')

                    GROUP BY 
                        o.OrderID,
                        o.OrderTime,
                        o.AcceptTime,
                        o.Status,
                        o.EmployeeID,
                        o.CustomerID,
                        o.ShipperID,
                        e.FullName,
                        c.CustomerName,
                        c.ContactName,
                        c.Email,
                        c.Phone,
                        c.Address,
                        s.ShipperName,
                        s.Phone

                    ORDER BY o.OrderTime DESC

                    OFFSET @Offset ROWS
                    FETCH NEXT @PageSize ROWS ONLY";

            var data = await connection.QueryAsync<OrderViewInfo>(sql, input);

            result.DataItems = data.ToList();

            return result;
        }

        public async Task<OrderViewInfo?> GetAsync(int orderID)
        {
            using var connection = GetConnection();

            string sql = @"
                SELECT 
                    o.*,

                    e.FullName AS EmployeeName,

                    c.CustomerName,
                    c.ContactName AS CustomerContactName,
                    c.Email AS CustomerEmail,
                    c.Phone AS CustomerPhone,
                    c.Address AS CustomerAddress,

                    s.ShipperName,
                    s.Phone AS ShipperPhone

                FROM Orders o

                LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
                LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID

                WHERE o.OrderID = @orderID";

            return await connection.QueryFirstOrDefaultAsync<OrderViewInfo>(sql, new { orderID });
        }

        public async Task<int> AddAsync(Order data)
        {
            using var connection = GetConnection();

            string sql = @"
                INSERT INTO Orders
                (
                    CustomerID,
                    OrderTime,
                    DeliveryProvince,
                    DeliveryAddress,
                    EmployeeID,
                    AcceptTime,
                    ShipperID,
                    ShippedTime,
                    FinishedTime,
                    Status
                )
                VALUES
                (
                    @CustomerID,
                    @OrderTime,
                    @DeliveryProvince,
                    @DeliveryAddress,
                    @EmployeeID,
                    @AcceptTime,
                    @ShipperID,
                    @ShippedTime,
                    @FinishedTime,
                    @Status
                );

                SELECT SCOPE_IDENTITY();";

            var id = await connection.ExecuteScalarAsync<decimal>(sql, data);

            return Convert.ToInt32(id);
        }

        public async Task<bool> UpdateAsync(Order data)
        {
            using var connection = GetConnection();

            string sql = @"
                UPDATE Orders
                SET
                    CustomerID = @CustomerID,
                    DeliveryProvince = @DeliveryProvince,
                    DeliveryAddress = @DeliveryAddress,
                    EmployeeID = @EmployeeID,
                    AcceptTime = @AcceptTime,
                    ShipperID = @ShipperID,
                    ShippedTime = @ShippedTime,
                    FinishedTime = @FinishedTime,
                    Status = @Status
                WHERE OrderID = @OrderID";

            int rows = await connection.ExecuteAsync(sql, data);

            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int orderID)
        {
            using var connection = GetConnection();

            string sql = @"DELETE FROM Orders WHERE OrderID=@orderID";

            int rows = await connection.ExecuteAsync(sql, new { orderID });

            return rows > 0;
        }

        // ======================================
        // ORDER DETAILS
        // ======================================

        public async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            using var connection = GetConnection();

            string sql = @"
                SELECT
                    od.OrderID,
                    od.ProductID,
                    od.Quantity,
                    od.SalePrice,

                    p.ProductName,
                    p.Unit,
                    p.Photo

                FROM OrderDetails od
                JOIN Products p ON od.ProductID = p.ProductID

                WHERE od.OrderID = @orderID";

            var data = await connection.QueryAsync<OrderDetailViewInfo>(sql, new { orderID });

            return data.ToList();
        }

        public async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            using var connection = GetConnection();

            string sql = @"
                SELECT
                    od.OrderID,
                    od.ProductID,
                    od.Quantity,
                    od.SalePrice,

                    p.ProductName,
                    p.Unit,
                    p.Photo

                FROM OrderDetails od
                JOIN Products p ON od.ProductID = p.ProductID

                WHERE od.OrderID = @orderID
                AND od.ProductID = @productID";

            return await connection.QueryFirstOrDefaultAsync<OrderDetailViewInfo>(
                sql,
                new { orderID, productID }
            );
        }

        public async Task<bool> AddDetailAsync(OrderDetail data)
        {
            using var connection = GetConnection();

            string sql = @"
                INSERT INTO OrderDetails
                (
                    OrderID,
                    ProductID,
                    Quantity,
                    SalePrice
                )
                VALUES
                (
                    @OrderID,
                    @ProductID,
                    @Quantity,
                    @SalePrice
                )";

            int rows = await connection.ExecuteAsync(sql, data);

            return rows > 0;
        }

        public async Task<bool> UpdateDetailAsync(OrderDetail data)
        {
            using var connection = GetConnection();

            string sql = @"
                UPDATE OrderDetails
                SET
                    Quantity = @Quantity,
                    SalePrice = @SalePrice
                WHERE OrderID = @OrderID
                AND ProductID = @ProductID";

            int rows = await connection.ExecuteAsync(sql, data);

            return rows > 0;
        }

        public async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            using var connection = GetConnection();

            string sql = @"
                DELETE FROM OrderDetails
                WHERE OrderID=@orderID
                AND ProductID=@productID";

            int rows = await connection.ExecuteAsync(sql, new { orderID, productID });

            return rows > 0;
        }

        public async Task<decimal> GetTotalAmountAsync(int orderID)
        {
            using var connection = GetConnection();

            string sql = @"
        SELECT ISNULL(SUM(Quantity * SalePrice), 0)
        FROM OrderDetails
        WHERE OrderID = @orderID";

            return await connection.ExecuteScalarAsync<decimal>(sql, new { orderID });
        }

        public async Task<List<OrderViewInfo>> ListByAccountAsync(int CustomerID)
        {
            using var connection = GetConnection();


            string sql = @"
                   SELECT 
                        o.OrderID,
                        o.OrderTime,
                        o.AcceptTime,
                        o.Status,
                        o.EmployeeID,
                        o.CustomerID,
                        o.ShipperID,

                        e.FullName AS EmployeeName,

                        c.CustomerName,
                        c.ContactName AS CustomerContactName,
                        c.Email AS CustomerEmail,
                        c.Phone AS CustomerPhone,
                        c.Address AS CustomerAddress,

                        s.ShipperName,
                        s.Phone AS ShipperPhone,

                        ISNULL(SUM(od.Quantity * od.SalePrice), 0) AS TotalAmount

                    FROM Orders o

                    LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
                    LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                    LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
                    LEFT JOIN OrderDetails od ON o.OrderID = od.OrderID

                    WHERE @CustomerID = 0 OR o.CustomerID = @CustomerID

                    GROUP BY 
                        o.OrderID,
                        o.OrderTime,
                        o.AcceptTime,
                        o.Status,
                        o.EmployeeID,
                        o.CustomerID,
                        o.ShipperID,
                        e.FullName,
                        c.CustomerName,
                        c.ContactName,
                        c.Email,
                        c.Phone,
                        c.Address,
                        s.ShipperName,
                        s.Phone

                    ORDER BY o.OrderTime DESC";

            var data = await connection.QueryAsync<OrderViewInfo>(sql, new {CustomerID});

            return data.ToList();
        }
    }
}