using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models
{
    [Table("USER_FORM")]
    public class UserForm
    {
        [Key]
        public int id { get; set; } 
        public string name { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string user_type { get; set; }
    }
}
