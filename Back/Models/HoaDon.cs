using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models
{
    [Table("HOADON")]
    public class HoaDon
    {
        [Key]
        public string maHD { get; set; }
        public int idUser { get; set; }
        public int tongTien { get; set; }
        public DateTime thoiGian { get; set; }
        public string hoTen { get; set; }
        public string sdt { get; set; }
        public string email { get; set; }
        public string diaChi { get; set; }
        public int tinhTrang { get; set;}
        public int thanhToan { get; set; }
        public int tamTinh { get; set; }
    }
}
