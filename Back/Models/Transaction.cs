using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models
{
    [Table("TRANSACTIONS")]
    public class Transaction
    {
        [Key]
        public int transactionId { get; set; }
        public string orderId { get; set; } // FK đến Order
        public string vnPayTransactionId { get; set; }
        public decimal amount { get; set; }
        public string status { get; set; } // Pending, Success, Failed
        public DateTime createdAt { get; set; }
        public DateTime? updatedAt { get; set; }
        public string payDate { get; set; }
        
    }
}
