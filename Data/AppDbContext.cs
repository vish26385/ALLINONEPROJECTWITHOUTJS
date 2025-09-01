using ALLINONEPROJECTWITHOUTJS.Models;
using Microsoft.EntityFrameworkCore;

namespace ALLINONEPROJECTWITHOUTJS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<ItemMaster> ItemMasters { get; set; }
        public DbSet<PartyMaster> PartyMasters { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<SalesMaster> SalesMaster { get; set; }
        public DbSet<SalesDetail> SalesDetails { get; set; }
    }
}
