using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models
{
    [Table("LSDUYET")]
    public class LSDuyet
    {
        [Key]
        public int maLSD { get; set; }
        public string maHD { get; set; }
        public int idAdmin { get; set; }
        public DateTime ngayD { get; set; }
    }
}
