using System.Security.Cryptography;
using System.Text;


namespace Back.Helpers
{
    public class PasswordHasher
    {
        private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
        private static readonly int SaltSize = 16;
        private static readonly int HashSize = 20;
        private static readonly int Iterations = 10000;
        private static readonly byte[] KeyAES256 = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20 };
        private static readonly byte[] IVAES256 = new byte[] { 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28, 0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F, 0x30 };


        //Băm mật khẩu với muối
        public static string HashPassword(string password)
        {
            byte[] salt;
            rngCsp.GetBytes(salt = new byte[SaltSize]);

            var key = new Rfc2898DeriveBytes(password, salt, Iterations);
            var hash = key.GetBytes(HashSize);

            var hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            var base64Hash = Convert.ToBase64String(hashBytes);

            return base64Hash;
        }
        //Kiểm tra mật khẩu đã băm với muối 
        public static bool VerifyPassword(string password, string base64Hash)
        {
            var hashBytes = Convert.FromBase64String(base64Hash);

            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            var key = new Rfc2898DeriveBytes(password, salt, Iterations);
            byte[] hash = key.GetBytes(HashSize);

            for (var i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + SaltSize] != hash[i])
                    return false;
            }
            return true;
        }

        //-----------------------------------------------------------------------------------------------//

        //Băm bằng thuật toán SHA256
        public static string ComputeSHA256Hash(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
        public static bool VerifySHA256Hash(string password, string hashedPassword)
        {
            // Tính toán giá trị băm SHA-256 của chuỗi đầu vào
            string hashedInput = ComputeSHA256Hash(password);

            // So sánh giá trị băm đã tính với giá trị băm đã lưu trữ
            return string.Equals(hashedInput, hashedPassword, StringComparison.OrdinalIgnoreCase);
        }

        //-----------------------------------------------------------------------------------------------//

        //Mã hóa dữ liệu bằng thuật toán AES256
        public static byte[] EncryptAES256(double plainDouble)
        {
            // Chuyển đổi double thành byte array
            byte[] plainBytes = BitConverter.GetBytes(plainDouble);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = KeyAES256;
                aesAlg.IV = IVAES256;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        // Ghi byte array của double vào CryptoStream để mã hóa
                        csEncrypt.Write(plainBytes, 0, plainBytes.Length);
                    }
                    return msEncrypt.ToArray();
                }
            }
        }

        public static double DecryptAES256(byte[] cipherText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = KeyAES256;
                aesAlg.IV = IVAES256;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        // Đọc byte array từ CryptoStream và chuyển đổi thành double
                        byte[] decryptedBytes = new byte[sizeof(double)];
                        int bytesRead = csDecrypt.Read(decryptedBytes, 0, decryptedBytes.Length);
                        if (bytesRead == sizeof(double))
                        {
                            return BitConverter.ToDouble(decryptedBytes, 0);
                        }
                        else
                        {
                            // Trường hợp không thành công, có thể làm xử lý khác tùy theo yêu cầu của bạn
                            throw new InvalidOperationException("Failed to decrypt the double.");
                        }
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------//

        public static byte[] EncryptRSA(string publicKey, double data)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(publicKey);
                byte[] bytesToEncrypt = DoubleToBytes(data);
                byte[] encryptedBytes = rsa.Encrypt(bytesToEncrypt, false);
                return encryptedBytes;
            }
        }

        public static double DecryptRSA(string privateKey, byte[] encryptedData)
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(privateKey);
                byte[] decryptedBytes = rsa.Decrypt(encryptedData, false);
                return BytesToDouble(decryptedBytes);
            }
        }

        public static (string publicKey, string privateKey) GenerateKeys(string publicKeySeed, string privateKeySeed)
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                // Tạo Key Container Name từ seed bằng cách sử dụng hàm băm SHA256
                string publicKeyContainerName = HashString(publicKeySeed);
                string privateKeyContainerName = HashString(privateKeySeed);

                // Cấu hình CspParameters với tên Key Container duy nhất
                CspParameters publicKeyParams = new CspParameters { KeyContainerName = publicKeyContainerName };
                CspParameters privateKeyParams = new CspParameters { KeyContainerName = privateKeyContainerName };

                rsa.PersistKeyInCsp = true;

                // Xuất khóa công khai
                string publicKeyXml = rsa.ToXmlString(false);

                // Xuất khóa riêng tư
                string privateKeyXml = rsa.ToXmlString(true);

                return (publicKeyXml, privateKeyXml);
            }
        }

        // Hàm băm chuỗi sử dụng SHA256
        private static string HashString(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hashBytes);
            }
        }
        private static byte[] DoubleToBytes(double data)
        {
            long value = BitConverter.DoubleToInt64Bits(data);
            return BitConverter.GetBytes(value);
        }

        private static double BytesToDouble(byte[] bytes)
        {
            long value = BitConverter.ToInt64(bytes, 0);
            return BitConverter.Int64BitsToDouble(value);
        }

    }
}
