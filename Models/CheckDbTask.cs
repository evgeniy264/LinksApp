
using Microsoft.EntityFrameworkCore;


namespace LinksApp.Models
{
    public class CheckDbTask(IServiceProvider serviceProvider, IWebHostEnvironment env) : BackgroundService
    {
        private readonly IWebHostEnvironment _env = env;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly TimeSpan _period = TimeSpan.FromSeconds(3600);
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var pathToKey = Path.Combine(_env.ContentRootPath, "secret");
            using PeriodicTimer timer = new(_period);
            using IServiceScope scope = _serviceProvider.CreateScope();
            await using ApplicationContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                List<Link> links = await dbContext.Links.Where(p => (p.Date_of_generation.AddDays(p.Days_of_active_text) < DateTime.UtcNow && p.Days_of_active_file == 0) || (p.Date_of_generation.AddDays(p.Days_of_active_file) < DateTime.UtcNow && p.Days_of_active_text == 0)).ToListAsync(cancellationToken: stoppingToken);
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
                await dbContext.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
