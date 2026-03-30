namespace SV22T1020261.BusinessLayers
{
    /// <summary>
    /// Lớp lưu giữ các thông tin cấu hình sử dụng cho BusinessLayer
    /// </summary>
    public static class Configuration
    {
        private static string _connectionString = "";

        /// <summary>
        /// Khởi tạo cấu hình cho BusinessLayer
        /// (hàm này phải gọi trước khi chạy ứng dụng)
        /// </summary>
        /// <param name="connectionString"></param>
        public static void Initialize(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Lấy chuỗi tham số kết nối đến cơ sở dữ liệu sử dụng trong hệ thống
        /// </summary>
        public static string ConnectionString => _connectionString;
    }
}
