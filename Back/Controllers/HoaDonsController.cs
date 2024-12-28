using Back.DataAccess;
using Back.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HoaDonsController : ControllerBase
    {
        private IUnitOfWork context;
        public HoaDonsController(IUnitOfWork context)
        {
            this.context = context;
        }

        //hien thi toan bo 
        [HttpGet("[action]")]
        public async Task<IEnumerable<HoaDon>> Get()
        {
            return await context.HoaDonRepository.GetAsync();
        }
        //hien thi 1 hoa don
        [HttpGet("[action]/{maHD}")]
        public async Task<HoaDon> Get([FromRoute] string maHD)
        {
            return await context.HoaDonRepository.GetSingleAsync(maHD);
        }
        //hien thi toan bo theo tg
        [HttpGet("[action]")]
        public async Task<IEnumerable<HoaDon>> GetSortedByDateAsc()
        {
            return await context.HoaDonRepository.GetAsync2(orderBy: q => q.OrderBy(hd => hd.thoiGian));
        }
        //hien thi theo tinhTrang 
        [HttpGet("[action]/{tinhTrang}")]
        public async Task<IEnumerable<HoaDon>> GetByTinhTrang(int tinhTrang)
        {
            return await context.HoaDonRepository.GetByTinhTrangHD(
                tinhTrang,
                orderBy: q => q.OrderBy(hd => hd.thoiGian)
            );
        }
        //hien thi theo so trang dk tinhTrang
        [HttpGet("[action]/{tinhTrang}/{page}/{pageSize}")]
        public async Task<IActionResult> GetPagedByTinhTrang(int tinhTrang, int page, int pageSize)
        {
            var result = await context.HoaDonRepository.GetPagedByTinhTrangAsync(
                tinhTrang,
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
        //hien thi theo so trang dk id
        [HttpGet("[action]/{id}/{page}/{pageSize}")]
        public async Task<IActionResult> GetPagedById(int id, int page, int pageSize)
        {
            var result = await context.HoaDonRepository.GetPagedByIdAsync(
                id,
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

        //them
        [HttpPost("[action]")]
        public async Task<ActionResult> Insert([FromBody] HoaDon hoadon)
        {
            try
            {
                await context.HoaDonRepository.InsertAsync(hoadon);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        //tao moi maHD
        [HttpGet("[action]")]
        public IActionResult GenerateMaHDCode()
        {
            var productCode = context.SanPhamRepository.GenerateMaHDCode();
            return Ok(new { productCode });
        }

        //thong ke 12 thang
        [HttpGet("[action]/{year}")]
        public async Task<ActionResult<List<int>>> GetTongTienByYear([FromRoute] int year)
        {
            try
            {
                var result = await context.HoaDonRepository.GetTongTienByYearAsync(year);
                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }
        // update tinh trang
        [HttpPut("[action]/{maHD}/{tinhTrang}")]
        public async Task<IActionResult> UpdateTinhTrang(string maHD, int tinhTrang)
        {
            var result = await context.HoaDonRepository.UpdateTinhTrangAsync(maHD, tinhTrang);

            if (!result)
            {
                return NotFound(new { Message = "Hóa đơn không tồn tại!" });
            }

            return Ok(new { Message = "Cập nhật tình trạng thành công!" });
        }
        //update tamtinh
        [HttpPut("[action]/{maHD}/{tamTinh}")]
        public async Task<IActionResult> UpdateTamTinh(string maHD, int tamTinh)
        {
            var result = await context.HoaDonRepository.UpdateTamTinhAsync(maHD, tamTinh);

            if (!result)
            {
                return NotFound(new { Message = "Hóa đơn không tồn tại!" });
            }


            return Ok(new { Message = "Cập nhật tạm tính thành công!" });
        }
        //kiem tra xem don duyet chua 
        [HttpGet("[action]/{maHD}/{tinhTrang}")]
        public async Task<ActionResult<bool>> CheckIfTinhTrang0([FromRoute] string maHD, [FromRoute] int tinhTrang)
        {
            try
            {
                // Sử dụng repository để kiểm tra sự tồn tại
                var existingRecord = await context.HoaDonRepository.GetSingleAsync2(c => c.maHD == maHD && c.tinhTrang == tinhTrang);

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
