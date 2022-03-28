using Microsoft.EntityFrameworkCore;

namespace TelegramBot
{
    public class TelegramDbContext : DbContext
    {
        public DbSet<Coordinates> Coordinates { get; set; }
        public TelegramDbContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseNpgsql("connStr");

    }
}
