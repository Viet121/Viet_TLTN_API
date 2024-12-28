using Back.DataAccess;
using Back.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LSDuyetsController : ControllerBase
    {
        private IUnitOfWork context;
        public LSDuyetsController(IUnitOfWork context)
        {
            this.context = context;
        }
        //hien thi toan bo
        [HttpGet("[action]")]
        public async Task<IEnumerable<LSDuyet>> Get()
        {
            return await context.LSDuyetRepository.GetAsync();
        }

        //THEM
        [HttpPost("[action]")]
        public async Task<ActionResult> Insert([FromBody] LSDuyet lsduyet)
        {
            try
            {
                lsduyet.ngayD = DateTime.Now;
                await context.LSDuyetRepository.InsertAsync(lsduyet);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }
        //tinh trang hoa don 1 de gui don
        [HttpGet("[action]/{tinhTrang}/{idAdmin}/{page}/{pageSize}")]
        public async Task<IActionResult> GetPagedByTinhTrangAndAdmin(int tinhTrang, int idAdmin, int page, int pageSize)
        {
            var result = await context.LSDuyetRepository.GetPagedByTinhTrangAndAdminAsync(
                tinhTrang,
                idAdmin,
                page,
                pageSize,
                orderBy: q => q.OrderBy(hd => hd.thoiGian)
            );

            return Ok(new
            {
                data = result.Item1,         // Danh sách hóa đơn
                totalRecords = result.Item2 // Tổng số bản ghi
            });
        }
    }
}
