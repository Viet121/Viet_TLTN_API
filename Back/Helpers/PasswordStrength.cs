namespace Back.Helpers
{
    public class PasswordStrength
    {
        public static bool IsPasswordStrong(string password)
        {
            // Kiểm tra chiều dài của mật khẩu
            if (password.Length < 8)
            {
                return false;
            }
            if (password.Length > 20)
            {
                return false;
            }
            /*
            // Kiểm tra có ít nhất một chữ cái viết hoa
            if (!password.Any(char.IsUpper))
            {
                return false;
            }

            // Kiểm tra có ít nhất một chữ cái viết thường
            if (!password.Any(char.IsLower))
            {
                return false;
            }

            // Kiểm tra có ít nhất một số
            if (!password.Any(char.IsDigit))
            {
                return false;
            }

            // Kiểm tra có ít nhất một ký tự đặc biệt
            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                return false;
            }*/

            // Mật khẩu đạt yêu cầu độ mạnh
            return true;
        }

        public static string CheckPasswordStrength(string password)
        {
            // Kiểm tra chiều dài của mật khẩu
            if (password.Length < 8)
            {
                return "Mật khẩu phải dài ít nhất 7 ký tự.";
            }
            if (password.Length > 20)
            {
                return "Độ dài mật khẩu tối đa 20 ký tự.";
            }
            /*
            // Kiểm tra có ít nhất một chữ cái viết hoa
            if (!password.Any(char.IsUpper))
            {
                return "Password must contain at least one uppercase letter.";
            }

            // Kiểm tra có ít nhất một chữ cái viết thường
            if (!password.Any(char.IsLower))
            {
                return "Password must contain at least one lowercase letter.";
            }

            // Kiểm tra có ít nhất một số
            if (!password.Any(char.IsDigit))
            {
                return "Password must contain at least one digit.";
            }

            // Kiểm tra có ít nhất một ký tự đặc biệt
            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                return "Password must contain at least one special character.";
            }*/

            // Mật khẩu đạt yêu cầu độ mạnh
            return string.Empty; // Không có thông báo lỗi
        }
    }
}
