using Microsoft.EntityFrameworkCore;

namespace Back.Models
{
    public class DatabaseContext:DbContext
    {
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<UserForm> UserForms { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<Size> Sizes { get; set; }
        public DbSet<Kho> Khos { get; set; }
        public DbSet<GioHangWithProductInfo> GioHangWithProductInfos { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<CTHoaDon> CTHoaDons { get; set; }
        public DbSet<Code> Codes { get; set; }
        public DbSet<LSDuyet> LSDuyets { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<CMT> CMTs { get; set; }
        public DbSet<CMTS> CMTSs { get; set; }
        public DbSet<Test> Tests{ get; set; }
        public DbSet<Test2> Test2s { get; set; }
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Kho>()
                .HasKey(k => new { k.maSP, k.maSize });

            base.OnModelCreating(modelBuilder);
        }
    }
}
