using System.ComponentModel.DataAnnotations;

namespace Back.Models
{
    public class SanPhamWithSizeQuantity
    {
        [Key]
        public string maSP { get; set; }
        public string tenSP { get; set; }
        public string gioiTinh { get; set; }
        public string trangThai { get; set; }
        public string kieuDang { get; set; }
        public int giaSP { get; set; }
        public string chatLieu { get; set; }
        public string mauSac { get; set; }
        public string image_URL { get; set; }
        public int soLuong38 { get; set; } // Số lượng size 38
        public int soLuong39 { get; set; } // Số lượng size 39
        public int soLuong40 { get; set; } // Số lượng size 40
        public int soLuong41 { get; set; } // Số lượng size 41
        public int soLuong42 { get; set; } // Số lượng size 42
        public int soLuong43 { get; set; } // Số lượng size 43
    }
}
