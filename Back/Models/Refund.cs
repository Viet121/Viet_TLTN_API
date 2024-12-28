using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models
{
    [Table("REFUNDS")]
    public class Refund
    {
        [Key]
        public int refundId { get; set; }
        public int transactionId { get; set; } // FK 
        public decimal refundAmount { get; set; }
        public string refundReason { get; set; } 
        public string status { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime? updatedAt { get; set; }
        public string vnPayRefundId { get; set; }
    }
}
