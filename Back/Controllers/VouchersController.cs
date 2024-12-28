using Back.DataAccess;
using Back.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Net;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VouchersController : ControllerBase
    {
        private IUnitOfWork context;
        public VouchersController(IUnitOfWork context)
        {
            this.context = context;
        }
        // danh sach
        [HttpGet("[action]")]
        public async Task<IEnumerable<Voucher>> Get()
        {
            return await context.VoucherRepository.GetAsync();
        }

        //danh sach theo trang
        [HttpGet("[action]/{page}/{pageSize}")]
        public async Task<ActionResult<IEnumerable<Voucher>>> GetAllVoucherPage(int page, int pageSize)
        {
            try
            {
                var result = await context.VoucherRepository.GetPageAll(
                page,
                pageSize
            );

                return Ok(new
                {
                    data = result.Item1,         // Danh sách hóa đơn
                    totalRecords = result.Item2 // Tổng số bản ghi
                });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần thiết
                return StatusCode(500, "Internal server error");
            }
        }
        //them
        [Authorize(Roles = "admin")]
        [HttpPost("[action]")]
        public async Task<ActionResult> Insert([FromBody] Voucher voucher)
        {
            try
            {
                await context.VoucherRepository.InsertAsync(voucher);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }
        //sua
        [Authorize(Roles = "admin")]
        [HttpPut("[action]")]
        public async Task<ActionResult> Update([FromBody] Voucher voucher)
        {
            try
            {

                context.VoucherRepository.Update(voucher);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }
        [HttpPut("[action]")]
        public async Task<ActionResult> Auto([FromBody] Voucher voucher)
        {
            try
            {
                var record = await context.VoucherRepository.GetSingleAsync(voucher.code);
                if (record == null)
                {
                    return NotFound(new { Message = "Voucher không tồn tại." });
                }

                record.daDung = record.daDung+1;
                context.VoucherRepository.Update(record);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }
        // xoa
        [Authorize(Roles = "admin")]
        [HttpDelete("[action]/{code}")]
        public async Task<ActionResult> Delete(string code)
        {
            try
            {
                await context.VoucherRepository.DeleteAsync(code);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        // get 1 voucher
        [HttpGet("[action]/{code}")]
        public async Task<Voucher> Get([FromRoute] string code)
        {
            return await context.VoucherRepository.GetSingleAsync(code);
        }


        [HttpPost("CheckIfVoucherSL")]
        public async Task<IActionResult> CheckIfVoucherSL(string code)
        {
            if (code == null)
                return BadRequest();

            var existingRecord = await context.VoucherRepository.GetSingleAsync(code);

            if (existingRecord == null)
                return NotFound(new { Message = "Mã giảm không tồn tại" });

            if (existingRecord.soLuong <= existingRecord.daDung)
            {
                return BadRequest(new { Message = "Số lượt sử dụng mã này đã hết " });
            }

            var phanTram = existingRecord.phanTram;
            return Ok(new
            {
                phanTram,
                Massage = "Gắn mã giảm cho đơn hàng này thành công!"
            });
        }
    }
}
