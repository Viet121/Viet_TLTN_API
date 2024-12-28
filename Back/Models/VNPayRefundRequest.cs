namespace Back.Models
{
    public class VNPayRefundRequest
    {
        // Mã phản hồi từ VNPAY khi thực hiện hoàn tiền
        public string vnp_ResponseId { get; set; }
        public string vnp_Command { get; set; }
        public string vnp_TmnCode { get; set; }
        public string vnp_TxnRef { get; set; }
        public string vnp_Amount { get; set; }
        public string vnp_OrderInfo { get; set; }
        public string vnp_ResponseCode { get; set; }
        public string vnp_Message { get; set; }
        public string vnp_BankCode { get; set; }
        public string vnp_PayDate { get; set; }
        public string vnp_TransactionNo { get; set; } // La vnPayTransactionId ở bảng Transaction, ma tra ve khi thuc hien thanh toan success
        public string vnp_TransactionType { get; set; }
        public string vnp_TransactionStatus { get; set; }
        public string vnp_SecureHash { get; set; }

       
    }
}
