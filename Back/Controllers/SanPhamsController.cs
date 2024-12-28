using Back.DataAccess;
using Back.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Octokit.Internal;
using System.Net;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SanPhamsController : ControllerBase
    {
        private IUnitOfWork context;
        private readonly IWebHostEnvironment _env;
        public SanPhamsController(IUnitOfWork context, IWebHostEnvironment env)
        {
            this.context = context;
            _env = env;
        }

        // hien thi toan bo san pham
        [HttpGet("[action]")]
        public async Task<IEnumerable<SanPham>> Get()
        {
            return await context.SanPhamRepository.GetAsync();
        }

        // Hien thi toan bo san pham voi tinh trang san pham va phan trang
        [HttpGet("[action]/{page}/{pageSize}")]
        public async Task<ActionResult<IEnumerable<SanPhamWithSoLuong>>> GetSanPhamWithSoLuong(int page, int pageSize)
        {
            try
            {
                var result = await context.SanPhamRepository.GetSanPhamWithSoLuongPagedAsync(
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
        // Hien thi toan bo sp theo tensp vs tinh trang va phan trang 
        [HttpGet("[action]/{tenSP}/{page}/{pageSize}/withTotalQuantity")]
        public async Task<ActionResult<SanPhamWithSoLuong>> GetSanPhamWithTotalQuantity(string tenSP,int page, int pageSize)
        {
            try
            {
                var sanPhamWithTotalQuantity = await context.SanPhamRepository.GetSanPhamWithTotalQuantityByTenSPPagedAsync(tenSP,page,pageSize);

                return Ok(new
                {
                    data = sanPhamWithTotalQuantity.Item1,         // Danh sách hóa đơn
                    totalRecords = sanPhamWithTotalQuantity.Item2 // Tổng số bản ghi
                });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần thiết
                return StatusCode(500, "Internal server error");
            }
        }
        // Hien thi toan bo sp theo gioiTinh vs tinh trang va phan trang 
        [HttpGet("[action]/{gioiTinh}/{page}/{pageSize}/withTotalQuantity")]
        public async Task<ActionResult<SanPhamWithSoLuong>> GetSanPhamGioiTinhWithTotalQuantity(string gioiTinh, int page, int pageSize)
        {
            try
            {
                var sanPhamWithTotalQuantity = await context.SanPhamRepository.GetSanPhamWithTotalQuantityByGioiTinhSPPagedAsync(gioiTinh, page, pageSize);

                return Ok(new
                {
                    data = sanPhamWithTotalQuantity.Item1,         // Danh sách hóa đơn
                    totalRecords = sanPhamWithTotalQuantity.Item2 // Tổng số bản ghi
                });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần thiết
                return StatusCode(500, "Internal server error");
            }
        }
        // Hien thi toan bo sp theo gioiTinh vs tinh trang va phan trang 
        [HttpGet("[action]/{trangThai}/{page}/{pageSize}/withTotalQuantity")]
        public async Task<ActionResult<SanPhamWithSoLuong>> GetSanPhamTrangThaiWithTotalQuantity(string trangThai, int page, int pageSize)
        {
            try
            {
                var sanPhamWithTotalQuantity = await context.SanPhamRepository.GetSanPhamWithTotalQuantityByTrangThaiSPPagedAsync(trangThai, page, pageSize);

                return Ok(new
                {
                    data = sanPhamWithTotalQuantity.Item1,         // Danh sách hóa đơn
                    totalRecords = sanPhamWithTotalQuantity.Item2 // Tổng số bản ghi
                });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần thiết
                return StatusCode(500, "Internal server error");
            }
        }

        // hien thi san pham theo maSP
        [HttpGet("[action]/{maSP}")]
        public async Task<SanPham> Get([FromRoute] string maSP)
        {
            return await context.SanPhamRepository.GetSingleAsync(maSP);
        }

        // xoa san pham theo maSP
        [Authorize(Roles = "admin")]
        [HttpDelete("[action]/{maSP}")]
        public async Task<ActionResult> Delete(string maSP)
        {
            try
            {
                var imageFolderPath = "wwwroot/Upload/product"; // Đường dẫn của thư mục chứa hình ảnh
                var imagePath = Path.Combine(imageFolderPath, maSP + ".jpg"); // Đường dẫn đến hình ảnh cần xóa

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath); // Xóa hình ảnh
                }
                await context.SanPhamRepository.DeleteAsync(maSP);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        //update sanpham 
        [Authorize(Roles = "admin")]
        [HttpPut("[action]")]
        public async Task<ActionResult> Update([FromBody] SanPham sanpham)
        {
            try
            {
                context.SanPhamRepository.Update(sanpham);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        //hien thi danh sach san pham theo tenSP
        [HttpGet("[action]/partial/{partialTenSP}")]
        public async Task<IEnumerable<SanPham>> GetByPartialTenSP([FromRoute] string partialTenSP)
        {
            var sanPhams = await context.SanPhamRepository.GetAsync(sp => EF.Functions.Like(sp.tenSP, $"%{partialTenSP}%"));
            return sanPhams;
        }

        [HttpGet("[action]/{maSP}/withSizeQuantity")]
        public async Task<ActionResult<SanPhamWithSizeQuantity>> GetSanPhamWithSizeQuantity(string maSP)
        {
            try
            {
                var sanPhamWithSizeQuantity = await context.SanPhamRepository.GetSanPhamWithSizeQuantityAsync(maSP);
                if (sanPhamWithSizeQuantity == null)
                {
                    return NotFound(); // Trả về 404 nếu không tìm thấy sản phẩm
                }
                return Ok(sanPhamWithSizeQuantity);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần thiết
                return StatusCode(500, "Internal server error");
            }
        }

        // tao maSP moi
        [HttpGet("[action]")]
        public IActionResult GenerateProductCode()
        {
            var productCode = context.SanPhamRepository.GenerateProductCode();
            return Ok(new { productCode });
        }

        //them san pham moi
        [Authorize(Roles = "admin")]
        [HttpPost("[action]")]
        public async Task<ActionResult> Insert([FromBody] SanPham sanpham)
        {
            try
            {
                await context.SanPhamRepository.InsertAsync(sanpham);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        //them hinh anh nhung front end gui ko dc
        [HttpPost("[action]")]
        public async Task<IActionResult> UploadImage(IFormFile file, string name)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty");
            }

            try
            {
                // Xử lý lưu trữ hình ảnh vào một vị trí cụ thể trên máy chủ
                var filePath = "wwwroot/Upload/product"; // Đường dẫn lưu trữ hình ảnh
                var fileName = name + Path.GetExtension(file.FileName);
                var fullPath = Path.Combine(filePath, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Trả về đường dẫn của hình ảnh đã được lưu trữ (nếu cần)
                var imageUrl = $"https://yourdomain.com/{fileName}";

                return Ok(new { imageUrl });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        //them hinh anh theo dang gui form 
        [HttpPost("[action]")]
        public async Task<IActionResult> UploadFile()
        {
            try
            {
                var httpRequest = Request.Form;
                var postedFile = httpRequest.Files[0];
                string filename = postedFile.FileName;
                //var physicalPath = _env.ContentRootPath + "/Photos/" + filename;
                var tenSP = Request.Form["tenSP"];
                string extension = Path.GetExtension(postedFile.FileName);
                string newFileName = tenSP + extension;
                var physicalPath = Path.Combine(_env.ContentRootPath, "wwwroot/Upload/product", newFileName);
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
                using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    await postedFile.CopyToAsync(stream);
                }
                return new JsonResult(filename);
            }
            catch (Exception)
            {
                return new JsonResult("Ko dc !!!!");
            }
            
        }

        //hien thi toan bo anh san pham
        [HttpGet("[action]")]
        public IActionResult GetAllImagePaths()
        {
            var imageFolderPath = "wwwroot/Upload/product"; // Đường dẫn của thư mục chứa hình ảnh
            if (!Directory.Exists(imageFolderPath))
            {
                return BadRequest("Image folder does not exist");
            }

            try
            {
                var imageFiles = Directory.GetFiles(imageFolderPath)
                                           .Where(file => file.ToLower().EndsWith(".jpg") || file.ToLower().EndsWith(".png")) // Chỉ lấy các file có phần mở rộng là .jpg hoặc .png
                                           .Select(file => $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}/Upload/product/{Path.GetFileName(file)}"); // Tạo đường dẫn URL cho mỗi file

                return Ok(imageFiles);
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        //xoa hinh anh 
        [HttpDelete("[action]")]
        public IActionResult DeleteImage(string imageName)
        {
            var imageFolderPath = "wwwroot/Upload/product"; // Đường dẫn của thư mục chứa hình ảnh
            var imagePath = Path.Combine(imageFolderPath, imageName + ".jpg"); // Đường dẫn đến hình ảnh cần xóa

            try
            {
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath); // Xóa hình ảnh 
                    return Ok("Image deleted successfully");
                }
                else
                {
                    return NotFound("Image not found");
                }
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        //hien thi danh sach san pham theo gia
        [HttpGet("[action]")]
        public async Task<IEnumerable<SanPham>> GetByPriceCondition([FromQuery] string condition)
        {
            return await context.SanPhamRepository.GetByPriceGreaterThanAsync(condition);
        }


        //hien thi danh sach san pham theo tim kiem chon loc
        //[HttpGet("[action]")]
        //public async Task<IEnumerable<SanPham>> LocSanPhams([FromQuery] string[] trangThai, [FromQuery] string[] kieuDang,[FromQuery] string[] gia, [FromQuery] string[] chatLieu)
        //{
        //    return await context.SanPhamRepository.GetByPriceAndMaterialAsync(trangThai, kieuDang, gia, chatLieu);
        //}

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<SanPhamWithSoLuong>>> GetByPriceAndMaterial([FromQuery] string[] trangThai = null, [FromQuery] string[] kieuDang = null, [FromQuery] string[] gia = null, [FromQuery] string[] chatLieu = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 9)
        {
            try
            {
                var sanPhamWithTotalQuantity = await context.SanPhamRepository.GetByPriceAndMaterialPagedAsync(trangThai, kieuDang, gia, chatLieu, page,pageSize);
                return Ok(new
                {
                    data = sanPhamWithTotalQuantity.Item1,         // Danh sách hóa đơn
                    totalRecords = sanPhamWithTotalQuantity.Item2 // Tổng số bản ghi
                });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần thiết
                return StatusCode(500, "Internal server error");
            }
        }
    }
}