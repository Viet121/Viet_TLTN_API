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
    public class GioHangsController : ControllerBase
    {
        private IUnitOfWork context;
        private readonly IWebHostEnvironment _env;
        public GioHangsController(IUnitOfWork context, IWebHostEnvironment env)
        {
            this.context = context;
            _env = env;
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> Insert([FromBody] GioHang giohang)
        {
            try
            {
                await context.GioHangRepository.InsertAsync(giohang);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        [HttpGet("[action]")]
        public async Task<bool> CheckExists(int idUser, string maSP, int maSize)
        {
            return await context.GioHangRepository.ExistsAsync(idUser, maSP, maSize);
        }

        [HttpGet("[action]")]
        public async Task<int?> GetSoLuong(int idUser, string maSP, int maSize)
        {
            return await context.GioHangRepository.GetSoLuongAsync(idUser, maSP, maSize);
        }

        [HttpPut("[action]")]
        public async Task<ActionResult> Update([FromBody] GioHang giohang)
        {
            try
            {
                context.GioHangRepository.Update(giohang);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateQuantity(int idUser, string maSP, int maSize, int newQuantity)
        {
            try
            {
                bool result = await context.GioHangRepository.UpdateQuantityAsync(idUser, maSP, maSize, newQuantity);

                if (result)
                {
                    return Ok();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpGet("[action]")]
        public async Task<IEnumerable<GioHang>> GetByPartialUserId([FromQuery] int userId)
        {
            var gioHangs = await context.GioHangRepository.GetAsync(gh => gh.idUser == userId);
            return gioHangs;
        }

        [HttpGet("[action]")]
        public async Task<IEnumerable<GioHangWithProductInfo>> GetGioHangWithProductInfoByPartialUserId([FromQuery] int userId)
        {
            var gioHangs = await context.GioHangRepository.GetAsync(gh => gh.idUser == userId);
            var gioHangsWithProductInfo = new List<GioHangWithProductInfo>();
            foreach (var gioHang in gioHangs)
            {
                var sanPham = await context.SanPhamRepository.GetSingleAsync(gioHang.maSP);
                if (sanPham != null)
                {
                    gioHangsWithProductInfo.Add(new GioHangWithProductInfo
                    {
                        maGH = gioHang.maGH,
                        idUser = gioHang.idUser,
                        maSP = gioHang.maSP,
                        soLuong = gioHang.soLuong,
                        maSize = gioHang.maSize,
                        tongTien = gioHang.tongTien,
                        tenSP = sanPham.tenSP,
                        kieuDang = sanPham.kieuDang,
                        chatLieu = sanPham.chatLieu,
                        image_URL = sanPham.image_URL
                    });
                }
            }

            return gioHangsWithProductInfo;
        }

        [HttpGet("user/{userId}/giohang")]
        public async Task<ActionResult<IEnumerable<GioHangWithProductInfo>>> GetGioHangWithProductInfoByPartialUserId2(int userId)
        {
            try
            {
                var gioHangsWithProductInfo = await context.GioHangRepository.GetGioHangWithProductInfoByPartialUserId(userId);
                return Ok(gioHangsWithProductInfo);
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        [HttpGet("total-quantity/{userId}")]
        public IActionResult GetTotalQuantityByUserId(int userId)
        {
            try
            {
                var totalQuantity = context.GioHangRepository.GetTotalQuantityByUserId(userId);
                return Ok(totalQuantity);
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        [HttpDelete("[action]/{maGH}")]
        public async Task<ActionResult> Delete(int maGH)
        {
            try
            {
                await context.GioHangRepository.DeleteAsync(maGH);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }
    }
}
