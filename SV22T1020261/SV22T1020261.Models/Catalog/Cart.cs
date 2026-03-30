using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1020261.Models.Catalog
{
    public class Cart
    {
        /// <summary>
        /// Ảnh
        /// </summary>
        public string Photo { get; set; } = string.Empty;
        /// <summary>
        /// Mã mặt hàng
        /// </summary>
        public int ProductID { get; set; }
        /// <summary>
        /// Tên mặt hàng
        /// </summary>
        public string ProductName { get; set; } = string.Empty;
        /// <summary>
        /// Mã loại hàng
        /// </summary>
        public int? CategoryID { get; set; }
        /// <summary>
        /// Tên loại hàng
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;
        /// <summary>
        /// Giá
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Số lượng mua
        /// </summary>
        public int Quantity { get; set; }
    }
}
