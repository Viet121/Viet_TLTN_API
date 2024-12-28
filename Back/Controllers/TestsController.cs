using Back.DataAccess;
using Back.Helpers;
using Back.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestsController : ControllerBase
    {
        private IUnitOfWork context;
        public TestsController(IUnitOfWork context)
        {
            this.context = context;
        }

        //hien thi toan bo 
        [HttpGet("[action]")]
        public async Task<IEnumerable<Test>> Get()
        {
            return await context.TestRepository.GetAsync();
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> Insert([FromBody] Test2 data)
        {
            try
            {
                byte[] encryptedData = PasswordHasher.EncryptAES256(data.AES256);
                string passSHA256 = PasswordHasher.ComputeSHA256Hash(data.SHA256);
                string publicKeySeed = "47.01.104.238";
                string privateKeySeed = "47.01.104.238@";
                var (publicKey, privateKey) = PasswordHasher.GenerateKeys(publicKeySeed, privateKeySeed);
                byte[] encryptedBytes = PasswordHasher.EncryptRSA(publicKey, data.RSA);
                var test = new Test
                {
                    nameMH = "Test các thuật toán",
                    SHA256 = passSHA256,
                    AES256 = encryptedData,
                    RSA = encryptedBytes,
                };
                
                await context.TestRepository.InsertAsync(test);
                await context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        [HttpGet("[action]")]
        public async Task<IEnumerable<Test2>> GetAllDecryptedData()
        {
            var encryptedTests = await context.TestRepository.GetAsync(); // Lấy tất cả dữ liệu từ bảng Test

            // Tạo danh sách mới để lưu trữ dữ liệu đã giải mã
            List<Test> decryptedTests = new List<Test>();
            List<Test2> decryptedTest2s = new List<Test2>();

            foreach (var encryptedTest in encryptedTests)
            {
                // Giải mã dữ liệu AES256
                double decryptedAES256 = PasswordHasher.DecryptAES256(encryptedTest.AES256);

                string pass = "abc12345";

                // Giải mã dữ liệu RSA
                // double decryptedRSA = PasswordHasher.DecryptRSA(privateKey, encryptedTest.RSA); // Bạn cần phải có privateKey ở đây
                double decryptedRSA = 9;

             // Tạo đối tượng Test mới với dữ liệu đã giải mã
             Test2 decryptedTest = new Test2
                {
                    idTest = encryptedTest.idTest,
                    nameMH = encryptedTest.nameMH,
                    SHA256 = encryptedTest.SHA256,
                    check = PasswordHasher.VerifySHA256Hash(pass, encryptedTest.SHA256),
                    AES256 = decryptedAES256, // Chuyển đổi lại thành mảng byte[]
                    RSA = decryptedRSA // Chuyển đổi lại thành mảng byte[]
                };

                // Thêm đối tượng Test đã giải mã vào danh sách mới
                decryptedTest2s.Add(decryptedTest);
            }

            // Trả về danh sách các dữ liệu đã giải mã
            return decryptedTest2s;
        }

    }
}
