using Microsoft.EntityFrameworkCore;
using library_management_system_backend.Models;

namespace library_management_system_backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books => Set<Book>();
        public DbSet<User> Users => Set<User>();
    }
}
