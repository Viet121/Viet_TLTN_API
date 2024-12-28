using System.ComponentModel.DataAnnotations;

namespace Back.Models
{
    public class RefundRequest
    {
        public int transactionId { get; set; } // 
        public string orderInfo { get; set; } // noidung
        public string name { get; set; }
        
    }
}
