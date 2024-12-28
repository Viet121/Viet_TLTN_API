using Back.DataAccess;
using Back.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CTHoaDonsController : ControllerBase
    {
        private IUnitOfWork context;
        public CTHoaDonsController(IUnitOfWork context)
        {
            this.context = context;
        }

        //hien thi toan bo 
        [HttpGet("[action]")]
        public async Task<IEnumerable<CTHoaDon>> Get()
        {
            return await context.CTHoaDonRepository.GetAsync();
        }
        //hien thi theo maHP
        [HttpGet("[action]/partial/{partialMaHD}")]
        public async Task<IActionResult> GetByPartialTenSP([FromRoute] string partialMaHD)
        {
            var result = await context.CTHoaDonRepository.GetCTHoaDonWithSanPhamByPartialMaHDAsync(partialMaHD);
            return Ok(result);
        }

        //them
        [HttpPost("[action]")]
        public async Task<ActionResult> Insert([FromBody] CTHoaDon cthoadon)
        {
            try
            {
                await context.CTHoaDonRepository.InsertAsync(cthoadon);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }
        // Trả về tổng số lượng dựa trên maSP
        [HttpGet("GetTotalSoLuongByMaSPCTHD/{maSP}")]
        public async Task<ActionResult<int>> GetTotalSoLuongByMaSPCTHD(string maSP)
        {
            try
            {
                // Gọi phương thức từ repository để lấy tổng số lượng
                var totalSoLuong = await context.CTHoaDonRepository.GetTotalSoLuongByMaSPAsyncCTHD(maSP);
                return Ok(totalSoLuong);
            }
            catch (Exception e)
            {
                // Xử lý lỗi và trả về mã lỗi 500
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        //top 5 sp
        [HttpGet("top-selling/{top}")]
        public async Task<ActionResult<List<TopSellingProductDto>>> GetTopSellingProducts(int top)
        {
            try
            {
                var result = await context.CTHoaDonRepository.GetTopSellingProductsAsync(top);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
