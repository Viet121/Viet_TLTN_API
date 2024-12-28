using Back.DataAccess;
using Back.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CMTsController : ControllerBase
    {
        private IUnitOfWork context;
        public CMTsController(IUnitOfWork context)
        {
            this.context = context;
        }
        //them
        [HttpPost("[action]")]
        public async Task<ActionResult> Insert([FromBody] CMT cmt)
        {
            try
            {
                DateTime utcTime = DateTime.UtcNow;
                TimeZoneInfo vietNamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var Cmt = new CMT {
                    id = cmt.id,
                    maSP = cmt.maSP,
                    name = cmt.name,
                    noiDung = cmt.noiDung,
                    thoiGian = TimeZoneInfo.ConvertTimeFromUtc(utcTime, vietNamTimeZone),
            };
                await context.CMTRepository.InsertAsync(Cmt);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }
        [HttpGet("[action]/{maSP}")]
        public async Task<IActionResult> GetLatestCMT([FromRoute] string maSP)
        {
            var result = await context.HoaDonRepository.GetAsyncCountReplies(
                maSP,
                orderBy: q => q.OrderByDescending(hd => hd.thoiGian)
            );

            if (result == null || !result.Any())
            {
                return NotFound(new { message = "Không có đánh giá nào." });
            }

            // Lấy phần tử đầu tiên
            var latestCMT = result.FirstOrDefault();

            return Ok(latestCMT);

        }

        [HttpGet("[action]/{maSP}")]
        public async Task<IActionResult> GetAllCMT([FromRoute] string maSP)
        {
            var comments = await context.CMTRepository.GetAsync(c => c.maSP == maSP); // Lấy danh sách theo điều kiện

            var latestCMT = comments
                .OrderByDescending(c => c.thoiGian);

            if (latestCMT == null)
            {
                return NotFound("Không có Cmt");
            }

            return Ok(latestCMT);
        }

        [HttpGet("[action]/{maSP}/{page}/{pageSize}")]
        public async Task<IActionResult> GetPagedCMT(string maSP, int page, int pageSize)
        {
            var result = await context.HoaDonRepository.GetPagedCmtWithReplyCount(
                maSP,
                page,
                pageSize,
                orderBy: q => q.OrderByDescending(hd => hd.thoiGian)
            );

            return Ok(new
            {
                data = result.Item1,         // Danh sách hóa đơn
                totalRecords = result.Item2 // Tổng số bản ghi
            });
        }

    }
}
