using System.ComponentModel.DataAnnotations;

namespace Back.Models
{
    public class GioHangWithProductInfo
    {
        [Key]
        public int maGH { get; set; }
        public int idUser { get; set; }
        public string maSP { get; set; }
        public int soLuong { get; set; }
        public int maSize { get; set; }
        public int tongTien { get; set; }
        public string tenSP { get; set; }
        public string kieuDang { get; set; }
        public string chatLieu { get; set; }
        public string image_URL { get; set; }
        public int soLuongKho { get; set;} //so luong hang trong kho tuong ung voi maSP va size
        public string trangThai { get; set; }

        public int tongTienKho { get; set; }
        public int soLuongDapUng { get; set; }

    }
}
