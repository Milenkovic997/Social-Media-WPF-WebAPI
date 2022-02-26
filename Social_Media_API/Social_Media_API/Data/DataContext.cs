using Microsoft.EntityFrameworkCore;

namespace Social_Media_API.Data
{
    public class DataContext : DbContext
    {
        // cd .\Social_Media_API
        // dotnet ef migrations add CreateInitial
        // dotnet ef database update
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Users> Users { get; set; }
        public DbSet<Following> Following { get; set; }
        public DbSet<Messages> Messages { get; set; }
        public DbSet<Posts> Posts { get; set; }
    }
}
