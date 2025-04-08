using Microsoft.EntityFrameworkCore;

namespace LinksApp.Models
{
    public class ApplicationContext(DbContextOptions<ApplicationContext> options) : DbContext(options)
    {
        public DbSet<Link> Links { get; set; }
    }
}
