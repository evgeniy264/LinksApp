using Microsoft.EntityFrameworkCore;

namespace LinksApp.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Link> Links { get; set; } = null!;
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) 
        {
           
        }
    }
}
