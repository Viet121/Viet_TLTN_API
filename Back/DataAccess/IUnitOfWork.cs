using Back.Models;

namespace Back.DataAccess
{
    public interface IUnitOfWork
    {
        public IRepository<SanPham> SanPhamRepository { get; set; }
        public IRepository<UserForm> UserFormRepository { get; set; }
        public IRepository<GioHang> GioHangRepository { get; set; }
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

        Task SaveChangesAsync();

    }
}
