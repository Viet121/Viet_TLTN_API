using Back.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Back.Helpers;
using System.Drawing;
using Newtonsoft.Json;
using System.Text;
using System.Net;

namespace Back.DataAccess
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, new()
    {
        private readonly DbContext context;


        public Repository(DbContext context)
        {
            this.context = context;
        }

        //get danh sach va ket hop ca get danh sach co dieu kien 
        public async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter = null)
        {
            IQueryable<TEntity> query = context.Set<TEntity>();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.ToListAsync();
        }

        // get danh sach va ket hop ca get danh sach co sap xep
        public async Task<IEnumerable<TEntity>> GetAsync2(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null)
        {
            IQueryable<TEntity> query = context.Set<TEntity>();

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query.ToListAsync();
        }
        //get san pham theo ma
        public async Task<TEntity> GetSingleAsync(object maSP)
        {
            return await context.Set<TEntity>().FindAsync(maSP);
        }
        // get 1 nhung khong phai khoa chinh
        public async Task<TEntity> GetSingleAsync2(Expression<Func<TEntity, bool>> filter)
        {
            return await context.Set<TEntity>().FirstOrDefaultAsync(filter);
        }
        //xoa
        public async Task DeleteAsync(object maSP)
        {
            var entity = await GetSingleAsync(maSP);
            context.Set<TEntity>().Remove(entity);
        }

        //sua
        public void Update(TEntity entity)
        {
            context.Set<TEntity>().Update(entity);
        }

        //get maSP cuoi
        public async Task<string> GetLastProductCode()
        {
            var result = await context.Set<SanPham>()
                .FromSqlInterpolated($@"
                    SELECT TOP 1 *
                    FROM SanPham
                    ORDER BY CAST(SUBSTRING(maSP, CHARINDEX('_', maSP) + 1, LEN(maSP)) AS INT) DESC
                ")
                .FirstOrDefaultAsync();

            return result?.maSP;
        }
        //get maHD cuoi
        public async Task<string> GetLastHDCode()
        {
            var lastProduct = await context.Set<HoaDon>()
                .FromSqlInterpolated($@"
                    SELECT TOP 1 *
                    FROM HoaDon
                    ORDER BY CAST(SUBSTRING(maHD, CHARINDEX('_', maHD) + 1, LEN(maHD)) AS INT) DESC
                ")
                .FirstOrDefaultAsync();
            return lastProduct?.maHD;
        }

        //Tao maSP tiep theo
        public string GenerateProductCode()
        {
            string lastProductCode = GetLastProductCode().Result;
            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastProductCode))
            {
                string[] parts = lastProductCode.Split('_');
                if (parts.Length == 2)
                {
                    if (int.TryParse(parts[1], out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }
            }
            return $"SP_{nextNumber}";
        }
        //Tao maHD tiep theo
        public string GenerateMaHDCode()
        {
            string lastProductCode = GetLastHDCode().Result;
            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastProductCode))
            {
                string[] parts = lastProductCode.Split('_');
                if (parts.Length == 2)
                {
                    // Thay đổi: Xử lý phần trước dấu "_"
                    if (int.TryParse(parts[1], out int lastNumber)) // Lấy số từ phần trước (giả sử mã bắt đầu bằng "HD")
                    {
                        nextNumber = lastNumber + 1;
                    }
                }
            }

            return $"H_{nextNumber}"; // Trả về định dạng mới với số trước dấu "_"
        }

        //Them san pham 
        public async Task InsertAsync(TEntity entity)
        {
            await context.Set<TEntity>().AddAsync(entity);
        }

        //get danh sach theo gia
        public async Task<IEnumerable<SanPham>> GetByPriceGreaterThanAsync(string condition)
        {
            IQueryable<SanPham> query = context.Set<SanPham>();

            switch (condition)
            {
                case "< 200":
                    query = query.Where(sp => sp.giaSP < 200);
                    break;
                case "> 300":
                    query = query.Where(sp => sp.giaSP > 300);
                    break;
                case "BETWEEN 200 AND 300":
                    query = query.Where(sp => sp.giaSP >= 200 && sp.giaSP <= 300);
                    break;
                default:
                    // Trường hợp không hợp lệ, trả về danh sách rỗng
                    return Enumerable.Empty<SanPham>();
            }

            return await query.ToListAsync();
        }

        //Ham loc cũ
        /*
        public async Task<IEnumerable<SanPham>> GetByPriceAndMaterialAsync(string[] trangThai = null, string[] kieuDang = null, string[] gia = null, string[] chatLieu = null)
        {
            IQueryable<SanPham> query = context.Set<SanPham>();
            //Loc trang thai
            if (trangThai != null && trangThai.Length > 0)
            {
                query = query.Where(sp => trangThai.Contains(sp.trangThai));
            }
            //Loc kieu dang
            if (kieuDang != null && kieuDang.Length > 0)
            {
                query = query.Where(sp => kieuDang.Contains(sp.kieuDang));
            }
            //Loc chat lieu
            if (chatLieu != null && chatLieu.Length > 0)
            {
                query = query.Where(sp => chatLieu.Contains(sp.chatLieu));
            }

            //Loc gia
            if (gia != null && gia.Length > 0)
            {
                // Xây dựng điều kiện tìm kiếm cho giá từ các điều kiện truyền vào
                foreach (var condition in gia)
                {
                    switch (condition)
                    {
                        case "< 200":
                            query = query.Where(sp => sp.giaSP < 200000);
                            break;
                        case "> 600":
                            query = query.Where(sp => sp.giaSP > 600000);
                            break;
                        case "BETWEEN 200 AND 299":
                            query = query.Where(sp => sp.giaSP >= 200000 && sp.giaSP <= 299000);
                            break;
                        case "BETWEEN 300 AND 399":
                            query = query.Where(sp => sp.giaSP >= 300000 && sp.giaSP <= 399000);
                            break;
                        case "BETWEEN 400 AND 499":
                            query = query.Where(sp => sp.giaSP >= 400000 && sp.giaSP <= 499000);
                            break;
                        case "BETWEEN 500 AND 599":
                            query = query.Where(sp => sp.giaSP >= 500000 && sp.giaSP <= 599000);
                            break;
                        default:
                            // Nếu có điều kiện không hợp lệ, bỏ qua nó
                            break;
                    }
                }
            }

            return await query.ToListAsync();
        }
        */

        //hàm lọc mơi
        public async Task<IEnumerable<SanPhamWithSoLuong>> GetByPriceAndMaterialAsync(string[] trangThai = null, string[] kieuDang = null, string[] gia = null, string[] chatLieu = null)
        {
            var query = from sanPham in context.Set<SanPham>()
                        join kho in context.Set<Kho>() on sanPham.maSP equals kho.maSP into gj
                        from subkho in gj.DefaultIfEmpty()
                        select new { SanPham = sanPham, SoLuong = subkho.soLuong };

            // Lọc trạng thái
            if (trangThai != null && trangThai.Length > 0)
            {
                query = query.Where(sp => trangThai.Contains(sp.SanPham.trangThai));
            }

            // Lọc kiểu dáng
            if (kieuDang != null && kieuDang.Length > 0)
            {
                query = query.Where(sp => kieuDang.Contains(sp.SanPham.kieuDang));
            }

            // Lọc chất liệu
            if (chatLieu != null && chatLieu.Length > 0)
            {
                query = query.Where(sp => chatLieu.Contains(sp.SanPham.chatLieu));
            }

            // Lọc giá
            if (gia != null && gia.Length > 0)
            {
                foreach (var condition in gia)
                {
                    switch (condition)
                    {
                        case "< 200":
                            query = query.Where(sp => sp.SanPham.giaSP < 200000);
                            break;
                        case "> 600":
                            query = query.Where(sp => sp.SanPham.giaSP > 600000);
                            break;
                        case "BETWEEN 200 AND 299":
                            query = query.Where(sp => sp.SanPham.giaSP >= 200000 && sp.SanPham.giaSP <= 299000);
                            break;
                        case "BETWEEN 300 AND 399":
                            query = query.Where(sp => sp.SanPham.giaSP >= 300000 && sp.SanPham.giaSP <= 399000);
                            break;
                        case "BETWEEN 400 AND 499":
                            query = query.Where(sp => sp.SanPham.giaSP >= 400000 && sp.SanPham.giaSP <= 499000);
                            break;
                        case "BETWEEN 500 AND 599":
                            query = query.Where(sp => sp.SanPham.giaSP >= 500000 && sp.SanPham.giaSP <= 599000);
                            break;
                        default:
                            // Nếu có điều kiện không hợp lệ, bỏ qua nó
                            break;
                    }
                }
            }

            var results = await query
                .GroupBy(x => new
                {
                    x.SanPham.maSP,
                    x.SanPham.tenSP,
                    x.SanPham.gioiTinh,
                    x.SanPham.trangThai,
                    x.SanPham.kieuDang,
                    x.SanPham.giaSP,
                    x.SanPham.chatLieu,
                    x.SanPham.mauSac,
                    x.SanPham.image_URL
                })
                .Select(grp => new SanPhamWithSoLuong
                {
                    maSP = grp.Key.maSP,
                    tenSP = grp.Key.tenSP,
                    gioiTinh = grp.Key.gioiTinh,
                    trangThai = grp.Key.trangThai,
                    kieuDang = grp.Key.kieuDang,
                    giaSP = grp.Key.giaSP,
                    chatLieu = grp.Key.chatLieu,
                    mauSac = grp.Key.mauSac,
                    image_URL = grp.Key.image_URL,
                    totalSoLuong = grp.Sum(x => x.SoLuong)
                })
                .ToListAsync();

            return results;
        }

        //ham loc co phan trang
        public async Task<(IEnumerable<SanPhamWithSoLuong>, int)> GetByPriceAndMaterialPagedAsync(
            string[] trangThai = null,string[] kieuDang = null,string[] gia = null,string[] chatLieu = null,int page = 1,int pageSize = 9)
            {
                if (page <= 0 || pageSize <= 0)
                    throw new ArgumentException("Page and PageSize must be greater than zero.");

                var query = from sanPham in context.Set<SanPham>()
                            join kho in context.Set<Kho>() on sanPham.maSP equals kho.maSP into gj
                            from subkho in gj.DefaultIfEmpty()
                            select new { SanPham = sanPham, SoLuong = subkho.soLuong };

                // Lọc trạng thái
                if (trangThai != null && trangThai.Length > 0)
                {
                    query = query.Where(sp => trangThai.Contains(sp.SanPham.trangThai));
                }

                // Lọc kiểu dáng
                if (kieuDang != null && kieuDang.Length > 0)
                {
                    query = query.Where(sp => kieuDang.Contains(sp.SanPham.kieuDang));
                }

                // Lọc chất liệu
                if (chatLieu != null && chatLieu.Length > 0)
                {
                    query = query.Where(sp => chatLieu.Contains(sp.SanPham.chatLieu));
                }

                // Lọc giá
                if (gia != null && gia.Length > 0)
                {
                    foreach (var condition in gia)
                    {
                        switch (condition)
                        {
                            case "< 200":
                                query = query.Where(sp => sp.SanPham.giaSP < 200000);
                                break;
                            case "> 600":
                                query = query.Where(sp => sp.SanPham.giaSP > 600000);
                                break;
                            case "BETWEEN 200 AND 299":
                                query = query.Where(sp => sp.SanPham.giaSP >= 200000 && sp.SanPham.giaSP <= 299000);
                                break;
                            case "BETWEEN 300 AND 399":
                                query = query.Where(sp => sp.SanPham.giaSP >= 300000 && sp.SanPham.giaSP <= 399000);
                                break;
                            case "BETWEEN 400 AND 499":
                                query = query.Where(sp => sp.SanPham.giaSP >= 400000 && sp.SanPham.giaSP <= 499000);
                                break;
                            case "BETWEEN 500 AND 599":
                                query = query.Where(sp => sp.SanPham.giaSP >= 500000 && sp.SanPham.giaSP <= 599000);
                                break;
                            default:
                                break;
                        }
                    }
                }

                // Tính tổng số bản ghi trước khi áp dụng phân trang
                int totalRecords = await query
                    .GroupBy(x => new
                    {
                        x.SanPham.maSP,
                        x.SanPham.tenSP,
                        x.SanPham.gioiTinh,
                        x.SanPham.trangThai,
                        x.SanPham.kieuDang,
                        x.SanPham.giaSP,
                        x.SanPham.chatLieu,
                        x.SanPham.mauSac,
                        x.SanPham.image_URL
                    })
                    .CountAsync();

                // Áp dụng phân trang
                var results = await query
                    .GroupBy(x => new
                    {
                        x.SanPham.maSP,
                        x.SanPham.tenSP,
                        x.SanPham.gioiTinh,
                        x.SanPham.trangThai,
                        x.SanPham.kieuDang,
                        x.SanPham.giaSP,
                        x.SanPham.chatLieu,
                        x.SanPham.mauSac,
                        x.SanPham.image_URL
                    })
                    .Select(grp => new SanPhamWithSoLuong
                    {
                        maSP = grp.Key.maSP,
                        tenSP = grp.Key.tenSP,
                        gioiTinh = grp.Key.gioiTinh,
                        trangThai = grp.Key.trangThai,
                        kieuDang = grp.Key.kieuDang,
                        giaSP = grp.Key.giaSP,
                        chatLieu = grp.Key.chatLieu,
                        mauSac = grp.Key.mauSac,
                        image_URL = grp.Key.image_URL,
                        totalSoLuong = grp.Sum(x => x.SoLuong)
                    })
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (results, totalRecords);
        }


        //9 sp tren 1 page
        public async Task<(IEnumerable<SanPham>, int)> GetSPsPagedAsync( int page, int pageSize)
        {
            IQueryable<SanPham> query = context.Set<SanPham>();

            int totalRecords = await query.CountAsync(); // Tổng số bản ghi
            var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (data, totalRecords);
        } 
        public async Task<UserForm> GetUserByEmailAsync(string email)
        {
            return await context.Set<UserForm>().FirstOrDefaultAsync(userForm => userForm.email == email);
        }

        public async Task<IEnumerable<int>> GetSizesByProductAsync(string maSP)
        {
            return await context.Set<Kho>()
                .Where(k => k.maSP == maSP && k.soLuong > 0)
                .Select(k => k.maSize)
                .ToListAsync();
        }
        public async Task<int?> GetQuantityByProductAndSizeAsync(string maSP, int maSize)
        {
            var kho = await context.Set<Kho>().FirstOrDefaultAsync(k => k.maSP == maSP && k.maSize == maSize);
            return kho?.soLuong;
        }
        public async Task<int> GetTotalQuantityByProductAsync(string maSP)
        {
            return await context.Set<Kho>().Where(k => k.maSP == maSP).SumAsync(k => k.soLuong);
        }
        public async Task<bool> ExistsAsync(int idUser, string maSP, int maSize)
        {
            return await context.Set<GioHang>()
                                .AnyAsync(gh => gh.idUser == idUser && gh.maSP == maSP && gh.maSize == maSize);
        }

        public async Task<int?> GetSoLuongAsync(int idUser, string maSP, int maSize)
        {
            var gioHang = await context.Set<GioHang>()
                                       .FirstOrDefaultAsync(gh => gh.idUser == idUser && gh.maSP == maSP && gh.maSize == maSize);
            return gioHang?.soLuong;
        }

        public async Task<int?> GetSoLuongAsyncKho(string maSP, int maSize)
        {
            var kho = await context.Set<Kho>()
                                       .FirstOrDefaultAsync(gh => gh.maSP == maSP && gh.maSize == maSize);
            return kho?.soLuong;
        }

        public async Task<bool> UpdateQuantityAsync(int idUser, string maSP, int maSize, int newQuantity)
        {
            try
            {
                var entity = await context.Set<TEntity>().Cast<Back.Models.GioHang>()
                    .FirstOrDefaultAsync(x => x.idUser == idUser && x.maSP == maSP && x.maSize == maSize);

                if (entity != null)
                {
                    entity.soLuong = newQuantity;
                    await context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                // Handle exception
                return false;
            }
        }

        public async Task<IEnumerable<GioHangWithProductInfo>> GetGioHangWithProductInfoByPartialUserId(int userId)
        {
            var gioHangsWithProductInfo = await context.Set<GioHang>()
                .Where(gh => gh.idUser == userId)
                .Join(
                    context.Set<SanPham>(),
                    gioHang => gioHang.maSP,
                    sanPham => sanPham.maSP,
                    (gioHang, sanPham) => new { GioHang = gioHang, SanPham = sanPham }
                )
                .Join(
                    context.Set<Kho>(),
                    combined => new { combined.GioHang.maSP, combined.GioHang.maSize },
                    kho => new { kho.maSP, kho.maSize },
                    (combined, kho) => new GioHangWithProductInfo
                    {
                        maGH = combined.GioHang.maGH,
                        idUser = combined.GioHang.idUser,
                        maSP = combined.GioHang.maSP,
                        soLuong = combined.GioHang.soLuong,
                        maSize = combined.GioHang.maSize,
                        tongTien = combined.GioHang.tongTien,
                        tenSP = combined.SanPham.tenSP,
                        kieuDang = combined.SanPham.kieuDang,
                        chatLieu = combined.SanPham.chatLieu,
                        image_URL = combined.SanPham.image_URL,
                        soLuongKho = kho.soLuong,
                        soLuongDapUng = kho.soLuong >= combined.GioHang.soLuong ? combined.GioHang.soLuong :
                                        (kho.soLuong != 0 && combined.GioHang.soLuong - kho.soLuong > 0 ? kho.soLuong :
                                        0),
                        tongTienKho = kho.soLuong >= combined.GioHang.soLuong ? combined.GioHang.tongTien * combined.GioHang.soLuong :
                                        (kho.soLuong != 0 && combined.GioHang.soLuong - kho.soLuong > 0 ? combined.GioHang.tongTien * kho.soLuong :
                                        0),
                        trangThai = kho.soLuong >= combined.GioHang.soLuong ? "Còn hàng" :
                                    (kho.soLuong != 0 && combined.GioHang.soLuong - kho.soLuong > 0 ? $"Chỉ còn {kho.soLuong} đôi" :
                                    "Hết hàng")
                    }
                )
                .ToListAsync();

            return gioHangsWithProductInfo;
        }


        public async Task<UserForm> GetByEmailAsync(string email)
        {
            return await context.Set<UserForm>().FirstOrDefaultAsync(u => u.email == email);
        }
        public int GetTotalQuantityByUserId(int userId)
        {
            return context.Set<GioHang>().Where(g => g.idUser == userId).Sum(g => g.soLuong);
        }

        // get all sp theo page
        public async Task<(IEnumerable<SanPhamWithSoLuong>, int)> GetSanPhamWithSoLuongPagedAsync(int page, int pageSize)
        {

            var query = from sanPham in context.Set<SanPham>()
                        join kho in context.Set<Kho>() on sanPham.maSP equals kho.maSP into gj
                        from subkho in gj.DefaultIfEmpty()
                        group subkho by new
                        {
                            sanPham.maSP,
                            sanPham.tenSP,
                            sanPham.gioiTinh,
                            sanPham.trangThai,
                            sanPham.kieuDang,
                            sanPham.giaSP,
                            sanPham.chatLieu,
                            sanPham.mauSac,
                            sanPham.image_URL
                        } into grp
                        select new SanPhamWithSoLuong
                        {
                            maSP = grp.Key.maSP,
                            tenSP = grp.Key.tenSP,
                            gioiTinh = grp.Key.gioiTinh,
                            trangThai = grp.Key.trangThai,
                            kieuDang = grp.Key.kieuDang,
                            giaSP = grp.Key.giaSP,
                            chatLieu = grp.Key.chatLieu,
                            mauSac = grp.Key.mauSac,
                            image_URL = grp.Key.image_URL,
                            totalSoLuong = grp.Sum(x => x.soLuong)
                        };

            // Tính tổng số bản ghi
            int totalRecords = await query.CountAsync();

            // Áp dụng phân trang
            var data = await query.Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();

            return (data, totalRecords);
        }


        public async Task<SanPhamWithSizeQuantity> GetSanPhamWithSizeQuantityAsync(string maSP)
        {
            var result = await (from sanPham in context.Set<SanPham>()
                                join kho in context.Set<Kho>() on sanPham.maSP equals kho.maSP into gj
                                from subkho in gj.DefaultIfEmpty()
                                where sanPham.maSP == maSP
                                group subkho by new
                                {
                                    sanPham.maSP,
                                    sanPham.tenSP,
                                    sanPham.gioiTinh,
                                    sanPham.trangThai,
                                    sanPham.kieuDang,
                                    sanPham.giaSP,
                                    sanPham.chatLieu,
                                    sanPham.mauSac,
                                    sanPham.image_URL
                                } into grp
                                select new SanPhamWithSizeQuantity
                                {
                                    maSP = grp.Key.maSP,
                                    tenSP = grp.Key.tenSP,
                                    gioiTinh = grp.Key.gioiTinh,
                                    trangThai = grp.Key.trangThai,
                                    kieuDang = grp.Key.kieuDang,
                                    giaSP = grp.Key.giaSP,
                                    chatLieu = grp.Key.chatLieu,
                                    mauSac = grp.Key.mauSac,
                                    image_URL = grp.Key.image_URL,
                                    soLuong38 = grp.Where(x => x.maSize == 38).Sum(x => x.soLuong),
                                    soLuong39 = grp.Where(x => x.maSize == 39).Sum(x => x.soLuong),
                                    soLuong40 = grp.Where(x => x.maSize == 40).Sum(x => x.soLuong),
                                    soLuong41 = grp.Where(x => x.maSize == 41).Sum(x => x.soLuong),
                                    soLuong42 = grp.Where(x => x.maSize == 42).Sum(x => x.soLuong),
                                    soLuong43 = grp.Where(x => x.maSize == 43).Sum(x => x.soLuong)
                                }).FirstOrDefaultAsync();

            return result;
        }

        public async Task<bool> DeleteKhoByMaSPAsync(string maSP)
        {
            try
            {
                var khoList = await context.Set<Kho>().Where(k => k.maSP == maSP).ToListAsync();
                if (khoList != null && khoList.Any())
                {
                    context.Set<Kho>().RemoveRange(khoList);
                    await context.SaveChangesAsync();
                    return true;
                }
                return false; // Trả về false nếu không có dữ liệu nào được xóa
            }
            catch (Exception)
            {
                // Xử lý lỗi nếu cần thiết
                throw;
            }
        }

        // get sp theo ten sp (tim kiem) co phan trang
        public async Task<(List<SanPhamWithSoLuong>, int)> GetSanPhamWithTotalQuantityByTenSPPagedAsync(string tenSP, int page, int pageSize)
        {

            // Query cơ sở dữ liệu
            var query = from sanPham in context.Set<SanPham>()
                        join kho in context.Set<Kho>() on sanPham.maSP equals kho.maSP into gj
                        from subkho in gj.DefaultIfEmpty()
                        where sanPham.tenSP.Contains(tenSP)
                        group subkho by new
                        {
                            sanPham.maSP,
                            sanPham.tenSP,
                            sanPham.gioiTinh,
                            sanPham.trangThai,
                            sanPham.kieuDang,
                            sanPham.giaSP,
                            sanPham.chatLieu,
                            sanPham.mauSac,
                            sanPham.image_URL
                        } into grp
                        select new SanPhamWithSoLuong
                        {
                            maSP = grp.Key.maSP,
                            tenSP = grp.Key.tenSP,
                            gioiTinh = grp.Key.gioiTinh,
                            trangThai = grp.Key.trangThai,
                            kieuDang = grp.Key.kieuDang,
                            giaSP = grp.Key.giaSP,
                            chatLieu = grp.Key.chatLieu,
                            mauSac = grp.Key.mauSac,
                            image_URL = grp.Key.image_URL,
                            totalSoLuong = grp.Sum(x => x.soLuong)
                        };

            // Tính tổng số bản ghi
            int totalRecords = await query.CountAsync();

            // Áp dụng phân trang
            var data = await query.Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();

            return (data, totalRecords);
        }

        // get sp theo gioTinh sp co phan trang
        public async Task<(List<SanPhamWithSoLuong>, int)> GetSanPhamWithTotalQuantityByGioiTinhSPPagedAsync(string gioiTinh, int page, int pageSize)
        {

            // Query cơ sở dữ liệu
            var query = from sanPham in context.Set<SanPham>()
                        join kho in context.Set<Kho>() on sanPham.maSP equals kho.maSP into gj
                        from subkho in gj.DefaultIfEmpty()
                        where sanPham.gioiTinh.Contains(gioiTinh)
                        group subkho by new
                        {
                            sanPham.maSP,
                            sanPham.tenSP,
                            sanPham.gioiTinh,
                            sanPham.trangThai,
                            sanPham.kieuDang,
                            sanPham.giaSP,
                            sanPham.chatLieu,
                            sanPham.mauSac,
                            sanPham.image_URL
                        } into grp
                        select new SanPhamWithSoLuong
                        {
                            maSP = grp.Key.maSP,
                            tenSP = grp.Key.tenSP,
                            gioiTinh = grp.Key.gioiTinh,
                            trangThai = grp.Key.trangThai,
                            kieuDang = grp.Key.kieuDang,
                            giaSP = grp.Key.giaSP,
                            chatLieu = grp.Key.chatLieu,
                            mauSac = grp.Key.mauSac,
                            image_URL = grp.Key.image_URL,
                            totalSoLuong = grp.Sum(x => x.soLuong)
                        };

            // Tính tổng số bản ghi
            int totalRecords = await query.CountAsync();

            // Áp dụng phân trang
            var data = await query.Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();

            return (data, totalRecords);
        }
        // get sp theo sale-off sp co phan trang
        public async Task<(List<SanPhamWithSoLuong>, int)> GetSanPhamWithTotalQuantityByTrangThaiSPPagedAsync(string trangThai, int page, int pageSize)
        {

            // Query cơ sở dữ liệu
            var query = from sanPham in context.Set<SanPham>()
                        join kho in context.Set<Kho>() on sanPham.maSP equals kho.maSP into gj
                        from subkho in gj.DefaultIfEmpty()
                        where sanPham.trangThai.Contains(trangThai)
                        group subkho by new
                        {
                            sanPham.maSP,
                            sanPham.tenSP,
                            sanPham.gioiTinh,
                            sanPham.trangThai,
                            sanPham.kieuDang,
                            sanPham.giaSP,
                            sanPham.chatLieu,
                            sanPham.mauSac,
                            sanPham.image_URL
                        } into grp
                        select new SanPhamWithSoLuong
                        {
                            maSP = grp.Key.maSP,
                            tenSP = grp.Key.tenSP,
                            gioiTinh = grp.Key.gioiTinh,
                            trangThai = grp.Key.trangThai,
                            kieuDang = grp.Key.kieuDang,
                            giaSP = grp.Key.giaSP,
                            chatLieu = grp.Key.chatLieu,
                            mauSac = grp.Key.mauSac,
                            image_URL = grp.Key.image_URL,
                            totalSoLuong = grp.Sum(x => x.soLuong)
                        };

            // Tính tổng số bản ghi
            int totalRecords = await query.CountAsync();

            // Áp dụng phân trang
            var data = await query.Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();

            return (data, totalRecords);
        }
        public async Task<int> GetTotalSoLuongByMaSPAsyncCTHD(string maSP)
        {
            // Lấy DbSet của CTHoaDon
            IQueryable<CTHoaDon> query = context.Set<CTHoaDon>();

            // Lọc dữ liệu theo maSP và tính tổng soLuong
            var totalSoLuong = await query
                                    .Where(ct => ct.maSP == maSP)
                                    .SumAsync(ct => ct.soLuong);

            return totalSoLuong;
        }

        public async Task<Code> GetSingleAsyncEmail(string email)
        {
            return await context.Set<Code>().FirstOrDefaultAsync(c => c.email == email);
        }

        public async Task<bool> SendEmailAsync(string email, string code)
        {
            try
            {
                var message = new MimeKit.MimeMessage();
                message.From.Add(new MimeKit.MailboxAddress("ShoesTLTN", "duongthihuyen.10051980@gmail.com"));
                message.To.Add(new MimeKit.MailboxAddress("", email));
                message.Subject = "Your Verification Code";

                message.Body = new MimeKit.TextPart("html")
                {
                    Text = $@"
                        <html>
                            <body>
                                <div style='height:32px'></div>
                                <div style='font-family: Arial, sans-serif;line-height: 1.6;color: 0000;margin: 0 auto;border: 1px solid #ddd;border-radius: 8px;padding: 20px;background-color: #f9f9f9;padding-bottom:20px;max-width:516px;min-width:220px;'>
                                    <div style='font-family:'Google Sans',Roboto,RobotoDraft,Helvetica,Arial,sans-serif;border-bottom:thin solid #dadce0;color:rgba(0,0,0,0.87);line-height:32px;padding-bottom:24px;text-align:center;word-break:break-word'>
                                        <div style='font-size:24px'>Verify Your Email </div>
                                    </div>
                                    <div style='font-family:Roboto-Regular,Helvetica,Arial,sans-serif;font-size:14px;color:rgba(0,0,0,0.87);line-height:20px;padding-top:20px;text-align:left'>Thank you for using our service! 
                                        <br><br>Use this code to complete account setup on our website:<br>
                                        <div style='text-align:center;font-size:36px;margin-top:20px;line-height:44px'>{code}</div>
                                        <br><br>If you didn't request this code, you can safely ignore this email.
                                    </div>
                                </div>
                                <div style='text-align:left'>
                                    <div style='font-family:Roboto-Regular,Helvetica,Arial,sans-serif;color:rgba(0,0,0,0.54);font-size:11px;line-height:18px;padding-top:12px;text-align:center'>
                                        <div>You received this email to let you know about important changes to your account and services on our website.</div>
                                        <div style='direction:ltr'>© 2024 Viet Shoes. <a class='m_2407751674457431810afal' style='font-family:Roboto-Regular,Helvetica,Arial,sans-serif;color:rgba(0,0,0,0.54);font-size:11px;line-height:18px;padding-top:12px;text-align:center'>All rights reserved.</a></div>
                                    </div>
                                </div>
                            </body>
                        </html>
                    "
                };


                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    // Kết nối với SMTP server của bạn
                    await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync("duongthihuyen.10051980@gmail.com", "phus mxok qiud uxmp");

                    // Gửi email
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<TopSellingProductDto>> GetTopSellingProductsAsync(int top)
        {
            return await context.Set<CTHoaDon>()
                .Join(context.Set<SanPham>(),
                      cthd => cthd.maSP,
                      sp => sp.maSP,
                      (cthd, sp) => new { sp.tenSP, cthd.soLuong })
                .GroupBy(x => x.tenSP)
                .Select(g => new TopSellingProductDto
                {
                    tenSanPham = g.Key,
                    tongSoLuongBan = g.Sum(x => x.soLuong)
                })
                .OrderByDescending(x => x.tongSoLuongBan)
                .Take(top)
                .ToListAsync();
        }

        //thong ke 12t
        public async Task<List<int>> GetTongTienByYearAsync(int year)
        {
            // Khởi tạo danh sách với 12 số 0 (tương ứng với 12 tháng)
            var result = Enumerable.Repeat(0, 12).ToList();

            // Truy vấn dữ liệu từ cơ sở dữ liệu
            var data = await context.Set<HoaDon>()
                .Where(hd => hd.thoiGian.Year == year)
                .GroupBy(hd => hd.thoiGian.Month)
                .Select(g => new { Month = g.Key, Total = g.Sum(hd => hd.tongTien) })
                .ToListAsync();

            // Cập nhật danh sách kết quả với dữ liệu từ cơ sở dữ liệu
            foreach (var item in data)
            {
                result[item.Month - 1] = item.Total; // Trừ 1 để khớp với chỉ số 0-based
            }

            return result;
        }
        //danh sach Hoa Don cho ben admin de duyet va gui hang
        public async Task<IEnumerable<HoaDon>> GetByTinhTrangHD(int tinhTrang, Func<IQueryable<HoaDon>, IOrderedQueryable<HoaDon>> orderBy = null)
        {
            IQueryable<HoaDon> query = context.Set<HoaDon>().Where(hd => hd.tinhTrang == tinhTrang);

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query.ToListAsync();
        }
        //phan trang cho hoa don
        public async Task<(IEnumerable<HoaDon>, int)> GetPagedByTinhTrangAsync(int tinhTrang, int page, int pageSize, Func<IQueryable<HoaDon>, IOrderedQueryable<HoaDon>> orderBy = null)
        {
            IQueryable<HoaDon> query = context.Set<HoaDon>().Where(hd => hd.tinhTrang == tinhTrang);

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            int totalRecords = await query.CountAsync(); // Tổng số bản ghi
            var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (data, totalRecords);
        }
        //phan trang cho hoa don
        public async Task<(IEnumerable<HoaDon>, int)> GetPagedByIdAsync(int id, int page, int pageSize, Func<IQueryable<HoaDon>, IOrderedQueryable<HoaDon>> orderBy = null)
        {
            IQueryable<HoaDon> query = context.Set<HoaDon>().Where(hd => hd.idUser == id);

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            int totalRecords = await query.CountAsync(); // Tổng số bản ghi
            var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (data, totalRecords);
        }

        public async Task<IEnumerable<CTHoaDonWithSanPhamDTO>> GetCTHoaDonWithSanPhamByPartialMaHDAsync(string partialMaHD)
        {
            var result = await (from cthd in context.Set<CTHoaDon>()
                                join sp in context.Set<SanPham>() on cthd.maSP equals sp.maSP
                                where EF.Functions.Like(cthd.maHD, $"%{partialMaHD}%")
                                select new CTHoaDonWithSanPhamDTO
                                {
                                    maCTHD = cthd.maCTHD,
                                    maHD = cthd.maHD,
                                    maSP = cthd.maSP,
                                    soLuong = cthd.soLuong,
                                    giaSP = cthd.giaSP,
                                    maSize = cthd.maSize,
                                    giaTong = cthd.giaTong,
                                    tenSP = sp.tenSP,
                                    kieuDang = sp.kieuDang,
                                    chatLieu = sp.chatLieu,
                                    image_URL = sp.image_URL
                                }).ToListAsync();

            return result;
        }
        public async Task<bool> UpdateTinhTrangAsync(string maHD, int tinhTrang)
        {
            // Tìm hóa đơn cần sửa
            var hoaDon = await context.Set<HoaDon>().FirstOrDefaultAsync(h => h.maHD == maHD);

            if (hoaDon == null)
            {
                return false; // Không tìm thấy hóa đơn
            }

            // Cập nhật tình trạng
            hoaDon.tinhTrang = tinhTrang;

            // Lưu thay đổi
            await context.SaveChangesAsync();
            return true; // Cập nhật thành công
        }
        public async Task<bool> UpdateTamTinhAsync(string maHD, int tamTinh)
        {
            // Tìm hóa đơn cần sửa
            var hoaDon = await context.Set<HoaDon>().FirstOrDefaultAsync(h => h.maHD == maHD);
            var check = await context.Set<Transaction>().FirstOrDefaultAsync(z => z.orderId == maHD && z.status == "Success");

            if (hoaDon == null)
            {
                return false; // Không tìm thấy hóa đơn
            }
            if(check == null)
            {
                return false;
            }

            // Cập nhật tamTinh
            hoaDon.tamTinh = tamTinh;

            // Lưu thay đổi
            await context.SaveChangesAsync();
            return true; // Cập nhật thành công
        }
        public async Task<(IEnumerable<HoaDon>, int)> GetPagedByTinhTrangAndAdminAsync(int tinhTrang, int idAdmin, int page, int pageSize, Func<IQueryable<HoaDon>, IOrderedQueryable<HoaDon>> orderBy = null)
        {
            // Lọc danh sách hóa đơn dựa vào tinhTrang và idAdmin trong bảng LSDuyet
            var query = context.Set<HoaDon>()
                .Join(
                    context.Set<LSDuyet>(),
                    hoaDon => hoaDon.maHD,
                    lsDuyet => lsDuyet.maHD,
                    (hoaDon, lsDuyet) => new { hoaDon, lsDuyet }
                )
                .Where(joined => joined.hoaDon.tinhTrang == tinhTrang && joined.lsDuyet.idAdmin == idAdmin)
                .Select(joined => joined.hoaDon);

            // Sắp xếp nếu có orderBy
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            // Tổng số bản ghi
            int totalRecords = await query.CountAsync();

            // Lấy dữ liệu phân trang
            var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (data, totalRecords);
        }
        public async Task ImportNVDataAsync(List<UserForm> nvData)
        {

            foreach (var nvModel in nvData)
            {
                var nv = new UserForm
                {
                    id = nvModel.id,
                    name = nvModel.name,
                    email = nvModel.email,
                    password = PasswordHasher.HashPassword(nvModel.password),
                    user_type = nvModel.user_type,
                };

                await context.Set<UserForm>().AddAsync(nv);
                await context.SaveChangesAsync();
            }

        }
        public async Task<(IEnumerable<UserForm>, int)> GetPagedEmployee(string user_type, int page, int pageSize, Func<IQueryable<UserForm>, IOrderedQueryable<UserForm>> orderBy = null)
        {
            IQueryable<UserForm> query = context.Set<UserForm>().Where(u => u.user_type == user_type);

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            int totalRecords = await query.CountAsync(); // Tổng số bản ghi
            var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (data, totalRecords);
        }

        //phan trang all voucher, (neu ko de TEntity la voucher co the dung chung nhung chi dung chung cho get all data du lieu rieng bang do)
        public async Task<(IEnumerable<TEntity>, int)> GetPageAll(int page, int pageSize)
        {
            IQueryable<TEntity> query = context.Set<TEntity>();
            int totalRecords = await query.CountAsync(); // Tổng số bản ghi
            var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (data, totalRecords);
        }

        public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
        {
            context.Set<Transaction>().Add(transaction);
            await context.SaveChangesAsync();
            return transaction;
        }
        public async Task<Refund> CreateRefundAsync(Refund refund)
        {
            context.Set<Refund>().Add(refund);
            await context.SaveChangesAsync();
            return refund;
        }

        public async Task<string> RefundTransactionAsync(RefundRequest request)
        {
            // 1. Lấy thông tin giao dịch
            var transaction = await context.Set<Transaction>().FindAsync(request.transactionId);
            if (transaction == null)
                throw new Exception("Giao dịch không tồn tại");

            if (transaction.status != "Success")
                throw new Exception("Chỉ có thể hoàn tiền giao dịch thành công");

            // Load configuration
            string vnp_Api = "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction";
            string vnp_TmnCode = "61PX58QM";
            string vnp_HashSecret = "TSTJ49OGEXRTDFUGHZ2A9FVOC1HOGR7T";

            // Generate required parameters
            var vnp_RequestId = DateTime.Now.Ticks.ToString();
            var vnp_Version = "2.1.0";
            var vnp_Command = "refund";
            var vnp_TransactionType = "02";
            var vnp_Amount = ((int)(transaction.amount * 100)).ToString();
            var vnp_TxnRef = (transaction.transactionId).ToString();
            var vnp_OrderInfo = $"Hoan tien don hang {transaction.orderId}";
            var vnp_TransactionNo = transaction.vnPayTransactionId;
            var vnp_TransactionDate = transaction.payDate;
            var vnp_CreateDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            var vnp_CreateBy = request.name;
            var vnp_IpAddr = "127.0.0.1";

            // Create sign data
            var signData = $"{vnp_RequestId}|{vnp_Version}|{vnp_Command}|{vnp_TmnCode}|{vnp_TransactionType}|{vnp_TxnRef}|{vnp_Amount}|{vnp_TransactionNo}|{vnp_TransactionDate}|{vnp_CreateBy}|{vnp_CreateDate}|{vnp_IpAddr}|{vnp_OrderInfo}";
            var vnp_SecureHash = VNPayLibrary.HmacSHA512(vnp_HashSecret, signData);

            // Prepare request data
            var rfData = new
            {
                vnp_RequestId,
                vnp_Version,
                vnp_Command,
                vnp_TmnCode,
                vnp_TransactionType,
                vnp_TxnRef,
                vnp_Amount,
                vnp_OrderInfo,
                vnp_TransactionNo,
                vnp_TransactionDate,
                vnp_CreateBy,
                vnp_CreateDate,
                vnp_IpAddr,
                vnp_SecureHash
            };

            var jsonData = JsonConvert.SerializeObject(rfData);

            // Send HTTP POST request
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(vnp_Api);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(jsonData);
            }

            // Get response
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                return result;
            }
        }

        public async Task<(IEnumerable<CMT>, int)> GetPagedCmt(string maSP, int page, int pageSize, Func<IQueryable<CMT>, IOrderedQueryable<CMT>> orderBy = null)
        {
            IQueryable<CMT> query = context.Set<CMT>().Where(hd => hd.maSP == maSP);

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            int totalRecords = await query.CountAsync(); // Tổng số bản ghi
            var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (data, totalRecords);
        }

        public async Task<(IEnumerable<object>, int)> GetPagedCmtWithReplyCount(
            string maSP,
            int page,
            int pageSize,
            Func<IQueryable<CMT>, IOrderedQueryable<CMT>> orderBy = null)
        {
            IQueryable<CMT> query = context.Set<CMT>().Where(hd => hd.maSP == maSP);

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            int totalRecords = await query.CountAsync(); // Tổng số bản ghi

            // Lấy dữ liệu phân trang và tính số lượng phản hồi
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(cmt => new
                {
                    cmt.id,
                    cmt.maSP,
                    cmt.name,
                    cmt.noiDung,
                    cmt.thoiGian,
                    ReplyCount = context.Set<CMTS>().Count(cmts => cmts.idCmt == cmt.id) // Đếm số phản hồi
                })
                .ToListAsync();

            return (data, totalRecords);
        }
        public async Task<IEnumerable<object>> GetAsyncCountReplies(string maSP, Func<IQueryable<CMT>, IOrderedQueryable<CMT>> orderBy = null)
        {
            IQueryable<CMT> query = context.Set<CMT>().Where(hd => hd.maSP == maSP);

            if (orderBy != null)
            {
                query = orderBy(query);
            }
            var data = await query
                .Select(cmt => new
                {
                    cmt.id,
                    cmt.maSP,
                    cmt.name,
                    cmt.noiDung,
                    cmt.thoiGian,
                    ReplyCount = context.Set<CMTS>().Count(cmts => cmts.idCmt == cmt.id) // Đếm số phản hồi
                }).ToListAsync();

            return data;
        }


    }
}
