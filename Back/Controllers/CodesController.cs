using Back.DataAccess;
using Back.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;

namespace Back.Controllers 
{
    [Route("api/[controller]")]
    [ApiController]
    public class CodesController : ControllerBase
    {
        private IUnitOfWork context;
        public CodesController(IUnitOfWork context)
        {
            this.context = context;
        }

        //them
        [HttpPost("[action]")]
        public async Task<ActionResult> Insert([FromBody] Code code)
        {
            try
            {
                await context.CodeRepository.InsertAsync(code);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }
        //sua
        [HttpPut("[action]")]
        public async Task<ActionResult> UpdateCodeByEmail(string email, string newCode)
        {
            try
            {
                // Tìm bản ghi dựa trên email
                var existingCode = await context.CodeRepository.GetSingleAsyncEmail(email);

                if (existingCode == null)
                {
                    return NotFound(new { message = "Email not found" });
                }

                // Cập nhật giá trị code
                existingCode.code = newCode;

                // Lưu thay đổi
                context.CodeRepository.Update(existingCode);
                await context.SaveChangesAsync();

                return Ok(new { message = "Code updated successfully" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        //xoa 
        [HttpDelete("[action]/{email}")]
        public async Task<ActionResult> DeleteByEmail(string email)
        {
            try
            {
                // Lấy bản ghi cần xóa dựa trên email
                var codeToDelete = await context.CodeRepository.GetSingleAsync2(c => c.email == email);

                if (codeToDelete != null)
                {
                    // Xóa bản ghi
                    await context.CodeRepository.DeleteAsync(codeToDelete.id);
                    await context.SaveChangesAsync();
                    return Ok($"Deleted record associated with email: {email}");
                }
                else
                {
                    return NotFound($"No record found for email: {email}");
                }
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }
        //them va gui gmail
        [HttpPost("[action]")]
        public async Task<ActionResult> InsertAndSendEmail([FromBody] Code code)
        {
            try
            {
                // Tạo mã 4 chữ số ngẫu nhiên
                var random = new Random();
                string newCode = random.Next(1000, 9999).ToString();

                code.code = newCode;

                await context.CodeRepository.InsertAsync(code);
                await context.SaveChangesAsync();
                var emailSent = await context.CodeRepository.SendEmailAsync(code.email, newCode);
                if (!emailSent)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, "Failed to send email.");
                }
                return Ok(new { message = "Code updated and email sent successfully." });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        //sua va gui gmail
        [HttpPut("[action]")]
        public async Task<ActionResult> UpdateCodeAndSendEmail(string email)
        {
            try
            {
                // Tìm bản ghi dựa trên email
                var existingCode = await context.CodeRepository.GetSingleAsyncEmail(email);

                if (existingCode == null)
                {
                    return NotFound(new { message = "Email not found" });
                }

                // Tạo mã 4 chữ số ngẫu nhiên
                var random = new Random();
                string newCode = random.Next(1000, 9999).ToString();

                // Cập nhật giá trị code
                existingCode.code = newCode;

                // Lưu thay đổi
                context.CodeRepository.Update(existingCode);
                await context.SaveChangesAsync();

                // Gửi email với mã mới
                var emailSent = await context.CodeRepository.SendEmailAsync(email, newCode);
                if (!emailSent)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, "Failed to send email.");
                }

                return Ok(new { message = "Code updated and email sent successfully." });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        //kiem tra xem co ton tai chua
        [HttpGet("[action]/{email}")]
        public async Task<ActionResult<bool>> CheckIfCodeEmailExists([FromRoute] string email)
        {
            try
            {
                // Sử dụng repository để kiểm tra tồn tại
                var existingLopHocPhan = await context.CodeRepository.GetSingleAsync2(c => c.email == email);

                // Nếu môn học tồn tại, trả về true, ngược lại trả về false
                return Ok(existingLopHocPhan != null);
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }
        //kiem tra xem code dung chua 
        [HttpGet("[action]/{email}/{code}")]
        public async Task<ActionResult<bool>> CheckIfEmailAndCodeExists([FromRoute] string email, [FromRoute] string code)
        {
            try
            {
                // Sử dụng repository để kiểm tra sự tồn tại
                var existingRecord = await context.CodeRepository.GetSingleAsync2(c => c.email == email && c.code == code);

                // Nếu tồn tại, trả về true, ngược lại trả về false
                return Ok(existingRecord != null);
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }


    }
}
