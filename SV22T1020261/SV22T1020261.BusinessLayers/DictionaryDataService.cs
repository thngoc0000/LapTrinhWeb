using SV22T1020261.DataLayers.Interfaces;
using SV22T1020261.DataLayers.SQLServer;
using SV22T1020261.Models.DataDictionary;

namespace SV22T1020261.BusinessLayers
{
    /// <summary>
    /// Lớp cung cấp các chức năng truy xuất dữ liệu dạng từ điển (Data Dictionary)
    /// Dùng cho các dữ liệu ít thay đổi, thường dùng để hiển thị dropdown
    /// Ví dụ: Tỉnh/Thành phố (Province)
    /// </summary>
    public static class DictionaryDataService
    {
        private static readonly IDataDictionaryRepository<Province> provinceDB;

        /// <summary>
        /// Constructor static, được gọi khi lớp được sử dụng lần đầu
        /// </summary>
        static DictionaryDataService()
        {
            provinceDB = new ProvinceRepository(Configuration.ConnectionString);
        }

        /// <summary>
        /// Lấy danh sách toàn bộ tỉnh/thành phố
        /// </summary>
        /// <returns>Danh sách Province</returns>
        public static async Task<List<Province>> ListProvincesAsync()
        {
            return await provinceDB.ListAsync();
        }
    }
}