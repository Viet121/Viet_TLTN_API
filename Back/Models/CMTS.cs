using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Back.Models
{
    [Table("CMTS")]
    public class CMTS
    {
        [Key]
        public int id { get; set; }
        public int idCmt { get; set; }
        public string name { get; set; }
        public string noiDung { get; set; }
        public DateTime? thoiGian { get; set; }
    }
}
