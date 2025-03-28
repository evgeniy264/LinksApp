
using Microsoft.EntityFrameworkCore;


namespace LinksApp.Models
{
    public class CheckDbTask : BackgroundService
    {
        private readonly IWebHostEnvironment _env;
        
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _period = TimeSpan.FromSeconds(3600);

        public CheckDbTask(IServiceProvider serviceProvider, IWebHostEnvironment env)
        {
            _serviceProvider = serviceProvider;
            _env = env;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var pathToKey = Path.Combine(_env.ContentRootPath, "secret");

            using PeriodicTimer timer = new PeriodicTimer(_period);
            using IServiceScope scope = _serviceProvider.CreateScope();
            await using ApplicationContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();


            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                List<Link>links = await dbContext.Links.Where(p => p.Date_of_expiration < DateOnly.FromDateTime(DateTime.Now)).ToListAsync();
                foreach (Link link in links) 
                {
                    if (link.File_name != null)
                    {
                        var decrypted_file_name = KeyStorage.DecryptData(Convert.FromBase64String(link.File_name), pathToKey);
                        
                        
                        var path = Path.Combine(_env.WebRootPath, "uploads", decrypted_file_name);
                        
                        File.Delete(path);
                    }
                }
                dbContext.Links.RemoveRange(links);
                await dbContext.SaveChangesAsync();
            }
           
        }

    }
}
