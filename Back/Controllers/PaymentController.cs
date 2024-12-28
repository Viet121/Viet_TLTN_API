using Back.DataAccess;
using Back.Helpers;
using Back.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Octokit;
using Org.BouncyCastle.Asn1.X9;
using System.Net;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private IUnitOfWork context;
        public PaymentController(IUnitOfWork context)
        {
            this.context = context;
        }

        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            // 1. Tạo giao dịch và lưu vào DB
            var transaction = new Transaction
            {
                orderId = request.orderId,
                amount = request.amount,
                status = "Pending",
                createdAt = DateTime.UtcNow,
                vnPayTransactionId = "0",
                updatedAt = DateTime.UtcNow,
                payDate = "0",
            };
            var createdTransaction = await context.TransactionRepository.CreateTransactionAsync(transaction);

            // 2. Khởi tạo URL thanh toán VNPAY
            var paymentUrl = GenerateVNPayUrl(request, createdTransaction.transactionId);

            return Ok(new { PaymentUrl = paymentUrl });
        }
        
        [HttpGet("[action]/{vnp_TxnRef}")]
        public async Task<Transaction> Get([FromRoute] string vnp_TxnRef)
        {
            int.TryParse(vnp_TxnRef, out var TransactionId);
            return await context.TransactionRepository.GetSingleAsync(TransactionId);
        }

        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VNPayReturn([FromQuery] VNPayReturnRequest request)
        {
            string secretKey = "TSTJ49OGEXRTDFUGHZ2A9FVOC1HOGR7T"; // Thay bằng secret key do VNPAY cung cấp
            var pay = new VNPayLibrary();
            var isValidSignature = pay.VerifyVNPaySignature(request);

            // 1. Xác thực chữ ký số

            if (!isValidSignature)
            {
                return BadRequest(new { Message = "Chữ ký không hợp lệ" });
            }

            // 2. Kiểm tra trạng thái giao dịch
            if (request.vnp_ResponseCode == "00") // "00" là mã thành công từ VNPAY
            {
                if (!int.TryParse(request.vnp_TxnRef, out var transactionId))
                {
                    return BadRequest(new { Message = "Mã giao dịch không hợp lệ (không phải số)" });
                }
                // 2.1 Tìm giao dịch trong cơ sở dữ liệu
                var transaction = await context.TransactionRepository.GetSingleAsync(transactionId);
                if (transaction == null)
                {
                    return NotFound(new { Message = "Không tìm thấy giao dịch" });
                }

                // 2.2 Kiểm tra trạng thái giao dịch để tránh xử lý lại
                if (transaction.status == "Success")
                {
                    return Ok(new { Message = "Giao dịch đã được xử lý thành công trước đó" });
                }

                // 2.3 Cập nhật trạng thái giao dịch
                transaction.status = "Success";
                transaction.vnPayTransactionId = request.vnp_TransactionNo;
                transaction.updatedAt = DateTime.UtcNow;
                transaction.payDate = request.vnp_PayDate;
                context.TransactionRepository.Update(transaction);
                await context.SaveChangesAsync();
                

                return Ok(new { Message = "Giao dịch thành công", orderId = transaction.orderId });
            }

            // 3. Trường hợp giao dịch không thành công
            return BadRequest(new { Message = "Giao dịch thất bại", ResponseCode = request.vnp_ResponseCode });
        }


        /*[HttpPost("refund")]
        public IActionResult RefundTransaction([FromBody] RefundRequest request)
        {
            try
            {
                // 1. Lấy thông tin giao dịch
                var transaction = await context.TransactionRepository.GetSingleAsync(request.transactionId);
                if (transaction == null)
                    return NotFound(new { Message = "Giao dịch không tồn tại" });

                if (transaction.status != "Success")
                    return BadRequest(new { Message = "Chỉ có thể hoàn tiền giao dịch thành công" });

                // Load configuration
                string vnp_Api = "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction";
                string vnp_TmnCode = "61PX58QM";
                string vnp_HashSecret = "TSTJ49OGEXRTDFUGHZ2A9FVOC1HOGR7T";

                // Generate required parameters
                var vnp_RequestId = DateTime.Now.Ticks.ToString();
                var vnp_Version = "2.1.0";
                var vnp_Command = "refund";
                var vnp_TransactionType = "02";
                var vnp_Amount = ("vnp_Amount", ((int)(transaction.amount * 100)).ToString());
                var vnp_TxnRef = transaction.transactionId;
                var vnp_OrderInfo = $"Hoan tien don hang {transaction.orderId}";
                var vnp_TransactionNo = transaction.vnPayTransactionId;
                var vnp_TransactionDate = transaction.payDate;
                var vnp_CreateDate = DateTime.Now.ToString("yyyyMMddHHmmss");
                var vnp_CreateBy = request.name;
                var vnp_IpAddr = "127.0.0.1";

                // Create sign data
                var signData = $"{vnp_RequestId}|{vnp_Version}|{vnp_Command}|{vnp_TmnCode}|{vnp_TransactionType}|{vnp_TxnRef}|{vnp_Amount}|{vnp_TransactionNo}|{vnp_TransactionDate}|{vnp_CreateBy}|{vnp_CreateDate}|{vnp_IpAddr}|{vnp_OrderInfo}";
                var vnp_SecureHash = VNPayLibrary.HmacSHA512(vnp_HashSecret, signData);

                // Prepare request data
                var rfData = new
                {
                    vnp_RequestId,
                    vnp_Version,
                    vnp_Command,
                    vnp_TmnCode,
                    vnp_TransactionType,
                    vnp_TxnRef,
                    vnp_Amount,
                    vnp_OrderInfo,
                    vnp_TransactionNo,
                    vnp_TransactionDate,
                    vnp_CreateBy,
                    vnp_CreateDate,
                    vnp_IpAddr,
                    vnp_SecureHash
                };

                var jsonData = JsonConvert.SerializeObject(rfData);

                // Send HTTP POST request
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(vnp_Api);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(jsonData);
                }

                // Get response
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    return Ok(new { message = "Refund request sent successfully.", response = result });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error processing refund request.", error = ex.Message });
            }
        }*/
        
        [HttpPost("refund")]
        public IActionResult RefundTransaction([FromBody] RefundRequest request)
        {
            try
            {
                var result = context.TransactionRepository.RefundTransactionAsync(request).Result;
                return Ok(new { message = "Refund request sent successfully.", response = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error processing refund request.", error = ex.Message });
            }
        }

        private string GenerateVNPayUrl(CreatePaymentRequest request, int transactionId)
        {
            string vnp_TmnCode = "61PX58QM";
            string vnp_HashSecret = "TSTJ49OGEXRTDFUGHZ2A9FVOC1HOGR7T";
            string vnp_Url = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            string vnp_ReturnUrl = "http://localhost:4200/vnpay-return";

            var pay = new VNPayLibrary();
            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            pay.AddRequestData("vnp_Locale", "vn");
            pay.AddRequestData("vnp_Amount", (request.amount * 100).ToString()); // Chuyển thành đơn vị VND * 100
            pay.AddRequestData("vnp_CurrCode", "VND");
            pay.AddRequestData("vnp_TxnRef", transactionId.ToString()); //Id nay la ma tang tu dong, (luu vo transaction trc goi thuc hien thanh toan onl roi update tru tien)
            pay.AddRequestData("vnp_OrderInfo", request.orderId); 
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);
            pay.AddRequestData("vnp_IpAddr", "127.0.0.1");
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));

            string paymentUrl = pay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return paymentUrl;
        }
        


    }
}
