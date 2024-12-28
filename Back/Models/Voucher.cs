using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Back.Models
{
    [Table("VOUCHER")]
    public class Voucher
    {
        [Key]
        public string code { get; set; }
        public int soLuong { get; set; }
        public int daDung { get; set; }
        public int phanTram { get; set; }
    }
}
