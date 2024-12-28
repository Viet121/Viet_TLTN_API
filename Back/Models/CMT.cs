using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models
{
    [Table("CMT")]
    public class CMT
    {
        [Key]
        public int id { get; set; }
        public string maSP { get; set; }

        public string name { get; set; }
        public string noiDung { get; set; }
        public DateTime? thoiGian { get; set; }
    }
}
