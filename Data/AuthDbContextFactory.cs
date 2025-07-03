using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LMS.Data
{
    public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
    {
        public AuthDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
            // Use the same hardcoded connection string
            optionsBuilder.UseNpgsql("Host=localhost;Database=LMS;Username=postgres;Password=admin");
            return new AuthDbContext(optionsBuilder.Options);
        }
    }
}