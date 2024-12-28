using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models
{
    [Table("GIOHANG")]
    public class GioHang
    {
        [Key]
        public int maGH { get; set; }
        public int idUser { get; set; }
        public string maSP { get; set; }
        public int soLuong { get; set; }
        public int maSize { get; set; }
        public int tongTien { get; set; }
    }
}
