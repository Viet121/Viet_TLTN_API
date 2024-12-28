using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models
{
    [Table("CODE")]
    public class Code 
    {
        [Key]
        public int id { get; set; }
        public string email { get; set; }
        public string code { get; set; }
       
    }
}
