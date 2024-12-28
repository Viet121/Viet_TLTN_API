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
    public class CMTSsController : ControllerBase
    {
        private IUnitOfWork context;
        public CMTSsController(IUnitOfWork context)
        {
            this.context = context;
        }
        //them
        [Authorize(Roles = "admin")]
        [HttpPost("[action]")]
        public async Task<ActionResult> Insert([FromBody] CMTS cmt)
        {
            try
            {
                DateTime utcTime = DateTime.UtcNow;
                TimeZoneInfo vietNamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var Cmt = new CMTS
                {
                    id = cmt.id,
                    idCmt = cmt.idCmt,
                    name = cmt.name,
                    noiDung = cmt.noiDung,
                    thoiGian = TimeZoneInfo.ConvertTimeFromUtc(utcTime, vietNamTimeZone),
                };
                await context.CMTSRepository.InsertAsync(Cmt);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> GetAllCMT([FromRoute] int id)
        {
            var comments = await context.CMTSRepository.GetAsync(c => c.idCmt == id); // Lấy danh sách theo điều kiện

            var latestCMT = comments
                .OrderByDescending(c => c.thoiGian);

            if (latestCMT == null)
            {
                return NotFound("Không có Cmt");
            }

            return Ok(latestCMT);
        }
    }
}
