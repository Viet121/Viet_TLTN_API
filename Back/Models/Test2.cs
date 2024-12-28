using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Back.Models
{
    public class Test2
    {
        [Key]
        public int idTest { get; set; }
        public string nameMH { get; set; }

        public string SHA256 { get; set; } // Mat khau vi no la bam

        public double AES256 { get; set; }

        public double RSA { get; set; }

        public bool check { get; set; }
    }
}
