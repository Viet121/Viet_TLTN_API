using Back.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Back.DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext dbContext;
        public IRepository<SanPham> SanPhamRepository { get; set; }
        public IRepository<UserForm> UserFormRepository { get; set; }
        public  IRepository<GioHang> GioHangRepository { get; set; }
        public IRepository<Size> SizeRepository { get; set; }
        public IRepository<Kho> KhoRepository { get; set; }
        public IRepository<GioHangWithProductInfo> GioHangWithProductInfoRepository { get; set; }
        public IRepository<CTHoaDon> CTHoaDonRepository { get; set; }
        public IRepository<HoaDon> HoaDonRepository { get; set; }
        public IRepository<Code> CodeRepository { get; set; }
        public IRepository<LSDuyet> LSDuyetRepository { get; set; }
        public IRepository<Voucher> VoucherRepository { get; set; }
        public IRepository<Transaction> TransactionRepository { get; set; }
        public IRepository<Refund> RefundRepository { get; set; }
        public IRepository<CMT> CMTRepository { get; set; }
        public IRepository<CMTS> CMTSRepository { get; set; }
        public IRepository<Test> TestRepository { get; set; }
        public IRepository<Test2> Test2Repository { get; set; }
        public UnitOfWork(DbContext dbContext)
        {
            this.dbContext = dbContext;
            SanPhamRepository = new Repository<SanPham>(dbContext);
            UserFormRepository = new Repository<UserForm>(dbContext);
            GioHangRepository = new Repository<GioHang>(dbContext);
            SizeRepository = new Repository<Size>(dbContext);
            KhoRepository = new Repository<Kho>(dbContext);
            GioHangWithProductInfoRepository = new Repository<GioHangWithProductInfo>(dbContext);
            HoaDonRepository = new Repository<HoaDon>(dbContext);
            CTHoaDonRepository = new Repository<CTHoaDon>(dbContext);
            CodeRepository = new Repository<Code>(dbContext);
            LSDuyetRepository = new Repository<LSDuyet>(dbContext);
            VoucherRepository = new Repository<Voucher>(dbContext);
            TransactionRepository = new Repository<Transaction>(dbContext);
            RefundRepository = new Repository<Refund>(dbContext);
            CMTRepository = new Repository<CMT>(dbContext);
            CMTSRepository = new Repository<CMTS>(dbContext);
            TestRepository = new Repository<Test>(dbContext);
            Test2Repository = new Repository<Test2>(dbContext);
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
    }
}
