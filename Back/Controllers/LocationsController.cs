using Back.DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public LocationsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        [HttpGet("provinces")]
        public async Task<IActionResult> GetProvinces()
        {
            var response = await _httpClient.GetAsync("https://provinces.open-api.vn/api/");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        // Proxy cho danh sách Quận/Huyện theo Tỉnh/Thành phố
        [HttpGet("districts/{provinceCode}")]
        public async Task<IActionResult> GetDistricts(string provinceCode)
        {
            var url = $"https://provinces.open-api.vn/api/p/{provinceCode}?depth=2";
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        // Proxy cho danh sách Phường/Xã theo Quận/Huyện
        [HttpGet("wards/{districtCode}")]
        public async Task<IActionResult> GetWards(string districtCode)
        {
            var url = $"https://provinces.open-api.vn/api/d/{districtCode}?depth=2";
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpGet("ProvincesAndDistrictAndWard/{provinceCode}/{districtCode}/{wardCode}")]
        public async Task<IActionResult> GetFullAddress(string provinceCode, string districtCode, string wardCode)
        {
            try
            {
                // Lấy thông tin tỉnh/thành phố
                var provinceResponse = await _httpClient.GetAsync($"https://provinces.open-api.vn/api/p/{provinceCode}");
                if (!provinceResponse.IsSuccessStatusCode) return NotFound("Province not found");
                var provinceJson = await provinceResponse.Content.ReadAsStringAsync();
                var province = JsonDocument.Parse(provinceJson).RootElement;
                var provinceName = province.GetProperty("name").GetString() ?? "Unknown";

                // Lấy thông tin quận/huyện
                var districtResponse = await _httpClient.GetAsync($"https://provinces.open-api.vn/api/d/{districtCode}");
                if (!districtResponse.IsSuccessStatusCode) return NotFound("District not found");
                var districtJson = await districtResponse.Content.ReadAsStringAsync();
                var district = JsonDocument.Parse(districtJson).RootElement;
                var districtName = district.GetProperty("name").GetString() ?? "Unknown";

                // Lấy thông tin phường/xã
                var wardResponse = await _httpClient.GetAsync($"https://provinces.open-api.vn/api/w/{wardCode}");
                if (!wardResponse.IsSuccessStatusCode) return NotFound("Ward not found");
                var wardJson = await wardResponse.Content.ReadAsStringAsync();
                var ward = JsonDocument.Parse(wardJson).RootElement;
                var wardName = ward.GetProperty("name").GetString() ?? "Unknown";

                // Trả về JSON thay vì text
                return Ok(new
                {
                    Province = provinceName,
                    District = districtName,
                    Ward = wardName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}
