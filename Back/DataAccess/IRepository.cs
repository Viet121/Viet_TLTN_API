using Back.Models;
using System.Linq.Expressions;

namespace Back.DataAccess
{
    public interface IRepository<TEntity> where TEntity : class, new()
    {
        //get danh sach, co hoac ko dk 
        Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter = null);
        //get danh sach co sap xep
        Task<IEnumerable<TEntity>> GetAsync2(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null);
        //get danh sach co phan trang
        Task<(IEnumerable<TEntity>, int)> GetPageAll(int page, int pageSize);
        string GenerateProductCode();
        string GenerateMaHDCode();
        Task InsertAsync(TEntity entity);
        Task<IEnumerable<SanPham>> GetByPriceGreaterThanAsync(string condition);
        Task<TEntity> GetSingleAsync(object maSP);
        Task DeleteAsync(object maSP);
        void Update(TEntity entity);
        Task<IEnumerable<SanPhamWithSoLuong>> GetByPriceAndMaterialAsync(string[] trangThai = null, string[] kieuDang = null, string[] gia = null, string[] chatLieu = null);
        Task<(IEnumerable<SanPhamWithSoLuong>, int)> GetByPriceAndMaterialPagedAsync(
            string[] trangThai = null, string[] kieuDang = null, string[] gia = null, string[] chatLieu = null, int page = 1, int pageSize = 9);
        Task<(IEnumerable<SanPham>, int)> GetSPsPagedAsync(int page, int pageSize);
        Task<UserForm> GetUserByEmailAsync(string email);
        Task<IEnumerable<int>> GetSizesByProductAsync(string maSP);
        Task<int?> GetQuantityByProductAndSizeAsync(string maSP, int maSize);
        Task<int> GetTotalQuantityByProductAsync(string maSP);
        Task<bool> ExistsAsync(int idUser, string maSP, int maSize);
        Task<int?> GetSoLuongAsync(int idUser, string maSP, int maSize);
        Task<bool> UpdateQuantityAsync(int idUser, string maSP, int maSize, int newQuantity);
        Task<IEnumerable<GioHangWithProductInfo>> GetGioHangWithProductInfoByPartialUserId(int userId);
        Task<UserForm> GetByEmailAsync(string email);
        int GetTotalQuantityByUserId(int userId);
        Task<int?> GetSoLuongAsyncKho(string maSP, int maSize);
        //Task<IEnumerable<SanPhamWithSoLuong>> GetSanPhamWithSoLuongAsync();
        Task<(IEnumerable<SanPhamWithSoLuong>, int)> GetSanPhamWithSoLuongPagedAsync(int page, int pageSize);
        Task<SanPhamWithSizeQuantity> GetSanPhamWithSizeQuantityAsync(string maSP);
        Task<bool> DeleteKhoByMaSPAsync(string maSP);
        Task<(List<SanPhamWithSoLuong>, int)> GetSanPhamWithTotalQuantityByTenSPPagedAsync(string tenSP, int page, int pageSize);
        Task<(List<SanPhamWithSoLuong>, int)> GetSanPhamWithTotalQuantityByGioiTinhSPPagedAsync(string gioiTinh, int page, int pageSize);
        Task<(List<SanPhamWithSoLuong>, int)> GetSanPhamWithTotalQuantityByTrangThaiSPPagedAsync(string trangThai, int page, int pageSize);
        Task<int> GetTotalSoLuongByMaSPAsyncCTHD(string maSP);
        Task<Code> GetSingleAsyncEmail(string email);
        Task<bool> SendEmailAsync(string email, string code);
        Task<TEntity> GetSingleAsync2(Expression<Func<TEntity, bool>> filter);
        Task<List<TopSellingProductDto>> GetTopSellingProductsAsync(int top);
        Task<List<int>> GetTongTienByYearAsync(int year);
        Task<IEnumerable<HoaDon>> GetByTinhTrangHD(int tinhTrang, Func<IQueryable<HoaDon>, IOrderedQueryable<HoaDon>> orderBy = null);
        Task<(IEnumerable<HoaDon>, int)> GetPagedByTinhTrangAsync(int tinhTrang,int page,int pageSize,Func<IQueryable<HoaDon>, IOrderedQueryable<HoaDon>> orderBy = null);
        Task<(IEnumerable<HoaDon>, int)> GetPagedByIdAsync(int id, int page, int pageSize, Func<IQueryable<HoaDon>, IOrderedQueryable<HoaDon>> orderBy = null);
        Task<IEnumerable<CTHoaDonWithSanPhamDTO>> GetCTHoaDonWithSanPhamByPartialMaHDAsync(string partialMaHD);
        Task<bool> UpdateTinhTrangAsync(string maHD, int tinhTrang);
        Task<(IEnumerable<HoaDon>, int)> GetPagedByTinhTrangAndAdminAsync(int tinhTrang, int idAdmin, int page, int pageSize, Func<IQueryable<HoaDon>, IOrderedQueryable<HoaDon>> orderBy = null);
        Task ImportNVDataAsync(List<UserForm> nvData);
        Task<(IEnumerable<UserForm>, int)> GetPagedEmployee(string user_type, int page, int pageSize, Func<IQueryable<UserForm>, IOrderedQueryable<UserForm>> orderBy = null);

        Task<Transaction> CreateTransactionAsync(Transaction transaction);
        Task<bool> UpdateTamTinhAsync(string maHD, int tamTinh);
        Task<Refund> CreateRefundAsync(Refund refund);
        Task<string> RefundTransactionAsync(RefundRequest request);
        Task<(IEnumerable<CMT>, int)> GetPagedCmt(string maSP, int page, int pageSize, Func<IQueryable<CMT>, IOrderedQueryable<CMT>> orderBy = null);
        Task<(IEnumerable<object>, int)> GetPagedCmtWithReplyCount(
            string maSP,
            int page,
            int pageSize,
            Func<IQueryable<CMT>, IOrderedQueryable<CMT>> orderBy = null);
        Task<IEnumerable<object>> GetAsyncCountReplies(string maSP, Func<IQueryable<CMT>, IOrderedQueryable<CMT>> orderBy = null);
    }
}
