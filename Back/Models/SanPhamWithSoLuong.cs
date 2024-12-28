using System.ComponentModel.DataAnnotations;

namespace Back.Models
{
    public class SanPhamWithSoLuong
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
        public int totalSoLuong { get; set; }
    }
}
