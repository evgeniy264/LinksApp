using System.Diagnostics;

using LinksApp.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



namespace LinksApp.Controllers
{
    public class HomeController : Controller
    {
        
        ApplicationContext db;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<HomeController> _logger;
        
        public HomeController(ILogger<HomeController> logger, ApplicationContext context, IWebHostEnvironment env)
        {
            db = context;
            _logger = logger;
            _env = env;
        }

        
        [Route("")]
        [Route("/newLink")]
        public IActionResult Newlink()
        {
            return View();
        }

        [Route("link/{guid?}")]
        public async Task<IActionResult> Link(string guid)
        {
            if (guid != null)
            {
                Link? link = await db.Links.FirstOrDefaultAsync(p => p.Guid == guid);
                
                if (link != null)
                {
                    return View("link", link);
                }
                else
                {
                    await Task.Delay(5000);
                    return Content("—сылка не найдена");
                }
            }

            return NotFound();
        }

        [HttpPost]
        [Route("/create")]
        public async Task<IActionResult> Create(Link linkText, Link linkFile, IFormFile file)
        {
            var pathToKey = Path.Combine(_env.ContentRootPath, "secret");
            
            
            if(linkText.Text_content!= null && file == null)
            {
                linkText.Guid = Guid.NewGuid().ToString("N");
                
                linkText.Date_of_generation = DateOnly.FromDateTime(DateTime.Now);
                
                linkText.Date_of_expiration = linkText.Date_of_generation.AddDays(linkText.Days_of_active);
                
                if (!System.IO.File.Exists(@pathToKey))
                {
                    
                    KeyStorage.GenerateKey(pathToKey);
                    
                }
                
                linkText.Text_content = Convert.ToBase64String(KeyStorage.EncryptData2(linkText.Text_content, pathToKey));
                
                
                await db.Links.AddAsync(linkText);
                await db.SaveChangesAsync();

                
            }
            if (file != null && linkText.Text_content != null)
            {
                
                linkFile.Guid = Guid.NewGuid().ToString("N");
                linkFile.Text_content = null;
                
                linkFile.Date_of_generation = DateOnly.FromDateTime(DateTime.Now);
                
                linkFile.Date_of_expiration = linkFile.Date_of_generation.AddDays(linkFile.Days_of_active);
                
                
                var path = Path.Combine(_env.WebRootPath, "uploads",file.FileName);
                
                using (var stream = new FileStream(path, FileMode.Create))
                {
                     await file.CopyToAsync(stream);
                }
                if (!System.IO.File.Exists(@pathToKey))
                {
                    KeyStorage.GenerateKey(pathToKey);
                }
                linkFile.File_name = Path.GetFileName(path);
                
                linkFile.File_name = Convert.ToBase64String(KeyStorage.EncryptData2(linkFile.File_name, pathToKey));
                
                

                linkText.Guid = Guid.NewGuid().ToString("N");
                linkText.Date_of_generation = DateOnly.FromDateTime(DateTime.Now);
                linkText.Date_of_expiration = linkText.Date_of_generation.AddDays(linkText.Days_of_active);
                linkText.Text_content = Convert.ToBase64String(KeyStorage.EncryptData2(linkText.Text_content, pathToKey));
  
                await db.Links.AddRangeAsync(linkText, linkFile);
                await db.SaveChangesAsync();

            }
            TempData["LinkTextGuid"] = linkText.Guid;
            TempData["LinkFileGuid"] = linkFile.Guid;
            return PartialView("newLinkPartial");
        }

        [Route("get/{linkGuid?}")]
        public async Task<IActionResult> Get(string linkGuid)
        {
            var pathToKey = Path.Combine(_env.ContentRootPath, "secret");
           
            if (linkGuid != null)
            {
                Link? link = await db.Links.FirstOrDefaultAsync(p => p.Guid == linkGuid);
                if (link != null)
                {
                    if (link.File_name != null)
                    {
                        var decrypted_file_name = KeyStorage.DecryptData(Convert.FromBase64String(link.File_name), pathToKey);
                        var directory = Path.Combine(_env.WebRootPath, "uploads"); 
                        var path = Path.Combine(directory, decrypted_file_name);
                        
                        Response.OnStarting(async() =>
                        {
                            Response.Headers.ContentDisposition = $"attachment; filename=file{linkGuid}.png";
                            Response.StatusCode = 200;
                            await Response.SendFileAsync(path);
                            
                        });
                        
                        Response.OnCompleted(async () =>
                        {
                            db.Links.Remove(link);
                            System.IO.File.Delete(path);
                            await db.SaveChangesAsync();
                        });
                        
                    }
                    if (link.Text_content != null)
                    {
                        var decrypted_text_content = KeyStorage.DecryptData(Convert.FromBase64String(link.Text_content), pathToKey);
                        TempData["Decrypted_text_content"] = decrypted_text_content;
                        return PartialView("linkPartial");
                    }
                }
               }
                return NotFound();
            }
        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
