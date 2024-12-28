using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models
{
    [Table("KHO")]
    public class Kho
    {
        [Key]
        public string maSP { get; set; }
        [Key]
        public int maSize { get; set; }
        public int soLuong { get; set; }
    }
}
