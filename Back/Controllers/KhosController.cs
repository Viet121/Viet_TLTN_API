using Back.DataAccess;
using Back.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhosController : ControllerBase
    {
        private IUnitOfWork context;
        public KhosController(IUnitOfWork context)
        {
            this.context = context;
        }

        //hien thi toan bo 
        [HttpGet("[action]")]
        public async Task<IEnumerable<Kho>> Get()
        {
            return await context.KhoRepository.GetAsync();
        }

        [HttpGet("[action]/{maSP}")]
        public async Task<ActionResult<IEnumerable<int>>> GetSizesByProduct(string maSP)
        {
            var sizes = await context.KhoRepository.GetSizesByProductAsync(maSP);
            if (sizes == null || !sizes.Any())
            {
                return NotFound();
            }
            return Ok(sizes);
        }

        [HttpGet("[action]/{maSP}/{maSize}")]
        public async Task<ActionResult<int?>> GetQuantityByProductAndSize(string maSP, int maSize)
        {
            var quantity = await context.KhoRepository.GetQuantityByProductAndSizeAsync(maSP, maSize);
            if (quantity == null)
            {
                return NotFound();
            }
            return Ok(quantity);
        }

        [HttpGet("[action]/{maSP}")]
        public async Task<ActionResult<int>> GetTotalQuantityByProduct(string maSP)
        {
            var totalQuantity = await context.KhoRepository.GetTotalQuantityByProductAsync(maSP);
            return Ok(totalQuantity);
        }

        [HttpGet("[action]")]
        public async Task<int?> GetSoLuongKhoByMaSP( string maSP, int maSize)
        {
            return await context.KhoRepository.GetSoLuongAsyncKho( maSP, maSize);
        }

        [HttpPut("[action]")]
        public async Task<ActionResult> Update([FromBody] Kho kho)
        {
            try
            {
                context.KhoRepository.Update(kho);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }
        [HttpPost("[action]")]
        public async Task<ActionResult> Insert([FromBody] Kho kho)
        {
            try
            {
                await context.KhoRepository.InsertAsync(kho);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }
        [HttpDelete("[action]/{maSP}")]
        public async Task<ActionResult> DeleteKhoByMaSP(string maSP)
        {
            try
            {
                var result = await context.KhoRepository.DeleteKhoByMaSPAsync(maSP);
                if (result)
                {
                    return Ok(); // Trả về 200 OK nếu xóa thành công
                }
                else
                {
                    return NotFound(); // Trả về 404 Not Found nếu không tìm thấy dữ liệu để xóa
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần thiết
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
