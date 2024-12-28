using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models
{
    [Table("CTHOADON")]
    public class CTHoaDon
    {
        [Key]
        public int maCTHD { get; set; }
        public string maHD { get; set; }
        public string maSP { get; set; }
        public int soLuong { get; set; }
        public int giaSP { get; set; }
        public int maSize { get; set; }
        public int giaTong { get; set; }
        
    }
}
