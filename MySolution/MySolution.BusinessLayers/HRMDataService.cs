using MySolution.DataLayers;
using MySolution.DomainModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MySolution.BusinessLayers
{
    public static class HRMDataService
    {
        private static EmployeeRepository? _employeeRepo;

        // 🔹 Khởi tạo Repository (gọi 1 lần khi app start)
        public static void Initialize(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string không hợp lệ");

            _employeeRepo = new EmployeeRepository(connectionString);
        }

        // 🔸 Kiểm tra đã init chưa
        private static EmployeeRepository Repo
        {
            get
            {
                if (_employeeRepo == null)
                    throw new Exception("HRMDataService chưa được khởi tạo. Hãy gọi Initialize() trước.");
                return _employeeRepo;
            }
        }

        // ================= NHÂN SỰ =================

        // 1️⃣ Danh sách nhân viên
        public static Task<List<Employee>> ListEmployeesAsync()
        {
            return Repo.ListAsync();
        }

        // 2️⃣ Lấy 1 nhân viên
        public static Task<Employee?> GetEmployeeAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("EmployeeId không hợp lệ");

            return Repo.GetAsync(id);
        }

        // 3️⃣ Cập nhật nhân viên (Business rule kiểm tra trước)
        public static async Task<bool> UpdateEmployeeAsync(Employee emp)
        {
            ValidateEmployee(emp);

            var existed = await Repo.GetAsync(emp.EmployeeId);
            if (existed == null)
                throw new Exception("Nhân viên không tồn tại");

            var rows = await Repo.UpdateAsync(emp);
            return rows > 0;
        }

        // 4️⃣ Xóa nhân viên
        public static async Task<bool> DeleteEmployeeAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("EmployeeId không hợp lệ");

            var existed = await Repo.GetAsync(id);
            if (existed == null)
                throw new Exception("Nhân viên không tồn tại");

            var rows = await Repo.DeleteAsync(id);
            return rows > 0;
        }

        // ============= BUSINESS VALIDATION =============

        private static void ValidateEmployee(Employee emp)
        {
            if (emp == null)
                throw new ArgumentNullException(nameof(emp));

            if (string.IsNullOrWhiteSpace(emp.EmployeeId))
                throw new Exception("EmployeeId không được rỗng");

            if (string.IsNullOrWhiteSpace(emp.FullName))
                throw new Exception("Họ tên không được rỗng");

            if (emp.Email != null && !emp.Email.Contains("@"))
                throw new Exception("Email không hợp lệ");

            if (emp.BirthDate != null && emp.BirthDate > DateTime.Now)
                throw new Exception("Ngày sinh không hợp lệ");
        }
    }
}
