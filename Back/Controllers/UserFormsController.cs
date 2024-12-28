using Back.DataAccess;
using Back.Helpers;
using Back.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Octokit;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserFormsController : ControllerBase
    {
        private IUnitOfWork context;
        private readonly IWebHostEnvironment _env;
        public UserFormsController(IUnitOfWork context, IWebHostEnvironment env)
        {
            this.context = context;
            _env = env;
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> Insert([FromBody] UserForm userform)
        {
            if (userform == null)
                return BadRequest();

            var checkEmail = await context.UserFormRepository.GetUserByEmailAsync(userform.email);

            if (checkEmail != null)
                return NotFound(new { Message = "Email đã được đăng ký" });

            var passMessage = PasswordStrength.CheckPasswordStrength(userform.password);
            if (!string.IsNullOrEmpty(passMessage))
            {
                return BadRequest(new { Message = passMessage });
            }


            userform.password = PasswordHasher.HashPassword(userform.password);
            await context.UserFormRepository.InsertAsync(userform);
            await context.SaveChangesAsync();
            return Ok(new
            {
                Massage = "Đăng ký thành công!"
            });
        }
         
        [HttpGet("[action]/{email}")]
        public async Task<ActionResult<bool>> CheckIfEmailExists([FromRoute] string email)
        {
            try
            {
                // Sử dụng repository để kiểm tra tồn tại
                var existingLopHocPhan = await context.UserFormRepository.GetUserByEmailAsync(email);

                // Nếu môn học tồn tại, trả về true, ngược lại trả về false
                return Ok(existingLopHocPhan != null);
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserForm userform)
        {
            if (userform == null)
                return BadRequest();

            var checkEmail = await context.UserFormRepository.GetUserByEmailAsync(userform.email);

            if (checkEmail == null)
                return NotFound(new { Message = "Email không tồn tại" });

            if (!PasswordHasher.VerifyPassword(userform.password, checkEmail.password))
            {
                return BadRequest(new { Message = "Mật khẩu không đúng" });
            }

            var mail = checkEmail.email;
            var role = checkEmail.user_type;
            string Token = CreateJwt(checkEmail);
            return Ok(new
            {
                Token,
                mail ,
                role ,
                Massage = "Đăng nhập thành công"
            });
        }

        [HttpGet("[action]/{email}")]
        public async Task<UserForm> GetRole([FromRoute] string email)
        {
            return await context.UserFormRepository.GetByEmailAsync(email);
        }
        [HttpGet("[action]/{email}")]
        public async Task<ActionResult<object>> GetJRole([FromRoute] string email)
        {
            var checkEmail = await context.UserFormRepository.GetUserByEmailAsync(email);
            if (checkEmail == null)
            {
                return NotFound(new { message = "User not found" });
            }
            return Ok(new { role = checkEmail.user_type });
        }

        [HttpGet("[action]/{email}")]
        public async Task<ActionResult<int>> Get([FromRoute] string email)
        {
            var checkEmail = await context.UserFormRepository.GetUserByEmailAsync(email);
            if(checkEmail == null)
            {
                return NotFound("User not found");
            }
            return checkEmail.id;
        }
        [HttpGet("[action]/{email}")]
        public async Task<ActionResult<object>> GetName([FromRoute] string email)
        {
            var checkEmail = await context.UserFormRepository.GetUserByEmailAsync(email);
            if (checkEmail == null)
            {
                return NotFound(new { message = "User not found" });
            }
            return Ok(new { name = checkEmail.name });
        }

        //Doi mat khau
        [HttpPut("[action]")]
        public async Task<ActionResult> Update([FromBody] UserForm userform)
        {
            if (userform == null || string.IsNullOrEmpty(userform.email) || string.IsNullOrEmpty(userform.password))
            {
                return BadRequest(new { Message = "Email và mật khẩu không được để trống." });
            }

            try
            {
                // Kiểm tra xem email có tồn tại trong cơ sở dữ liệu không
                var existingUserForm = await context.UserFormRepository.GetUserByEmailAsync(userform.email);

                if (existingUserForm == null)
                {
                    return NotFound(new { Message = "Email không tồn tại." });
                }

                // Kiểm tra độ mạnh mật khẩu
                var passMessage = PasswordStrength.CheckPasswordStrength(userform.password);
                if (!string.IsNullOrEmpty(passMessage))
                {
                    return BadRequest(new { Message = passMessage });
                }

                // Mã hóa mật khẩu trước khi lưu vào cơ sở dữ liệu
                userform.password = PasswordHasher.HashPassword(userform.password);

                // Cập nhật mật khẩu
                existingUserForm.password = userform.password;
                context.UserFormRepository.Update(existingUserForm);

                // Lưu thay đổi vào cơ sở dữ liệu
                await context.SaveChangesAsync();

                return Ok(new { Message = "Đổi mật khẩu thành công" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { Message = e.Message });
            }
        }

        //Doi role acount
        [Authorize(Roles = "admin")]
        [HttpPut("[action]")]
        public async Task<ActionResult> UpdateType([FromBody] UserForm userform)
        {
            if (userform == null || string.IsNullOrEmpty(userform.email) )
            {
                return BadRequest(new { Message = "Email không được để trống." });
            }

            try
            {
                // Kiểm tra xem email có tồn tại trong cơ sở dữ liệu không
                var existingUserForm = await context.UserFormRepository.GetUserByEmailAsync(userform.email);

                if (existingUserForm == null)
                {
                    return NotFound(new { Message = "Email không tồn tại." });
                }

                existingUserForm.user_type = "user";

        
                context.UserFormRepository.Update(existingUserForm);

                // Lưu thay đổi vào cơ sở dữ liệu
                await context.SaveChangesAsync();

                return Ok(new { Message = "Đổi quyền thành công" });
            }
            catch (Exception e)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { Message = e.Message });
            }
        }

        // excel danh sach
        [Authorize(Roles = "admin")]
        [HttpPost("import")]
        public async Task<IActionResult> ImportNhanVienData([FromBody] List<UserForm> nvData)
        {
            try
            {
                await context.UserFormRepository.ImportNVDataAsync(nvData);

                return Ok(new { Massage = "Thêm danh sách nhân viên thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Import thất bại: {ex.Message}");
            }
        }
        // danh sach admin
        [Authorize(Roles = "admin")]
        [HttpGet("[action]/{user_type}/{page}/{pageSize}")]
        public async Task<IActionResult> GetPagedEmployee(string user_type, int page, int pageSize)
        {
            var result = await context.HoaDonRepository.GetPagedEmployee(
                user_type,
                page,
                pageSize
            );

            return Ok(new
            {
                data = result.Item1,         // Danh sách hóa đơn
                totalRecords = result.Item2 // Tổng số bản ghi
            });
        }

        private string CreateJwt(UserForm user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("myverysecurekeythatisatleast32byteslong!");
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, user.user_type),
                new Claim(ClaimTypes.Email, user.email),
                new Claim(ClaimTypes.Name,$"{user.name}")
            });

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.Now.AddDays(2),
                SigningCredentials = credentials
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }
    }
}
