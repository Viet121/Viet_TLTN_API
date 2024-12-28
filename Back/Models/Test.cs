using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models
{
    [Table("Test")]
    public class Test
    {
        [Key]
        public int idTest { get; set; }
        public string nameMH { get; set; }

        public string SHA256 { get; set; } 

        [Column(TypeName = "varbinary(max)")]
        public byte[] AES256 { get; set; }

        [Column(TypeName = "varbinary(max)")]
        public byte[] RSA { get; set; }
    }
}
