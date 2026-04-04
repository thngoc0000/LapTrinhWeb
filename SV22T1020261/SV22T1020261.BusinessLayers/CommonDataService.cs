using SV22T1020261.DataLayers.Interfaces;
using SV22T1020261.DataLayers.SQLServer;
using SV22T1020261.Models.Dashboard;
using SV22T1020261.Models.DataDictionary;

namespace SV22T1020261.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu dùng chung trong hệ thống
    /// </summary>
    public static partial class CommonDataService
    {
        private static readonly IDashboardRepository dashboardDB;
        /// <summary>
        /// Constructor static, được gọi khi lớp được sử dụng lần đầu
        /// </summary>
        static CommonDataService()
        {
            dashboardDB = new DashboardRepository(Configuration.ConnectionString);
        }
        /// <summary>
        /// Lấy thông tin tổng hợp cho Dashboard
        /// </summary>
        /// <returns>Thông tin Dashboard</returns>
        public static async Task<DashboardInfo> GetDashboardInfoAsync()
        {
            return await dashboardDB.GetDashboardInfoAsync();
        }
    }
}