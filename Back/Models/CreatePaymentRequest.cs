namespace Back.Models
{
    public class CreatePaymentRequest
    {
        public decimal amount { get; set; }             // Số tiền thanh toán (VNĐ)
        public string orderId { get; set; }          // Thông tin đơn hàng
        public string returnUrl { get; set; }          // URL chuyển hướng sau thanh toán
        public string transactionReference { get; set; } // Mã giao dịch hệ thống
        public string ipAddress { get; set; }          // Địa chỉ IP của người dùng
        public string locale { get; set; } = "vn";     // Ngôn ngữ giao diện mặc định
    }

}
