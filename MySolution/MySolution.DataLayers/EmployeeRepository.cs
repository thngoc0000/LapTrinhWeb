using Dapper;
using MySolution.DomainModels;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace MySolution.DataLayers
{
    public class EmployeeRepository
    {
        private readonly string _connectionString;

        public EmployeeRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);

        // 1️⃣ Lấy danh sách nhân viên
        public async Task<List<Employee>> ListAsync()
        {
            const string sql = @"SELECT EmployeeId, FullName, BirthDate, Address, Email, Phone, Department
                                 FROM Employee";

            using var connection = CreateConnection();
            var result = await connection.QueryAsync<Employee>(sql);
            return result.AsList();
        }

        // 2️⃣ Lấy 1 nhân viên theo ID
        public async Task<Employee?> GetAsync(string id)
        {
            const string sql = @"SELECT EmployeeId, FullName, BirthDate, Address, Email, Phone, Department
                                 FROM Employee
                                 WHERE EmployeeId = @Id";

            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Employee>(sql, new { Id = id });
        }

        // 3️⃣ Thêm nhân viên
        public async Task<int> AddAsync(Employee employee)
        {
            const string sql = @"INSERT INTO Employee(EmployeeId, FullName, BirthDate, Address, Email, Phone, Department)
                                 VALUES(@EmployeeId, @FullName, @BirthDate, @Address, @Email, @Phone, @Department)";

            using var connection = CreateConnection();
            return await connection.ExecuteAsync(sql, employee);
        }

        // 4️⃣ Cập nhật nhân viên (KHÔNG cập nhật EmployeeId)
        public async Task<int> UpdateAsync(Employee employee)
        {
            const string sql = @"UPDATE Employee
                                 SET FullName = @FullName,
                                     BirthDate = @BirthDate,
                                     Address = @Address,
                                     Email = @Email,
                                     Phone = @Phone,
                                     Department = @Department
                                 WHERE EmployeeId = @EmployeeId";

            using var connection = CreateConnection();
            return await connection.ExecuteAsync(sql, employee);
        }

        // 5️⃣ Xóa nhân viên theo ID
        public async Task<int> DeleteAsync(string id)
        {
            const string sql = @"DELETE FROM Employee WHERE EmployeeId = @Id";

            using var connection = CreateConnection();
            return await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}
