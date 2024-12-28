using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models
{
    [Table("SIZE")]
    public class Size
    {
        [Key]
        public int maSize { get; set; }
        public int soSize { get; set; }
    }
}
