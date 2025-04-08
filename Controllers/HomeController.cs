using System;
using System.Diagnostics;

using LinksApp.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



namespace LinksApp.Controllers
{
    public class HomeController(ApplicationContext context, IWebHostEnvironment env) : Controller
    {
        readonly ApplicationContext db = context;
        private readonly IWebHostEnvironment _env = env;

        [Route("")]
        [Route("/newLink")]
        public IActionResult Newlink()
        {
            return View();
        }

        [HttpGet]
        [Route("link/{guid?}")]
        public async Task<IActionResult> Link(Guid guid)
        {
            Link? link = await db.Links.FirstOrDefaultAsync(p => p.Guid == guid);
            if (link != null)
            {
                return View("link", link);
            }
            else
            {
                await Task.Delay(5000);
                return NotFound();
            }
        }

        [HttpPost]
        [Route("/create")]
        public async Task<IActionResult> Create(Link linkText, Link linkFile, IFormFile file)
        {
            var pathToKey = Path.Combine(_env.ContentRootPath, "secret");
            if (linkText.Text_content != null)
            {
                linkText.Date_of_generation = DateTime.UtcNow;
                linkText.Days_of_active_file = 0;
                if (!System.IO.File.Exists(@pathToKey))
                {
                    KeyStorage.GenerateKey(pathToKey);
                }
                linkText.Text_content = Convert.ToBase64String(KeyStorage.EncryptData2(linkText.Text_content, pathToKey));
                await db.Links.AddAsync(linkText);
                await db.SaveChangesAsync();
                TempData["LinkTextGuid"] = linkText.Guid;
            }
            if (file != null)
            {
                linkFile.Text_content = null;
                linkFile.Days_of_active_text = 0;
                linkFile.Date_of_generation = DateTime.UtcNow;
                var path = Path.Combine(_env.WebRootPath, "uploads", file.FileName);
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
                await db.Links.AddAsync(linkFile);
                await db.SaveChangesAsync();
                TempData["LinkFileGuid"] = linkFile.Guid;

            }
            return PartialView("newLinkPartial");
        }

        [HttpPost]
        [Route("link/{linkGuid?}")]
        public async Task<IActionResult> Get(Guid linkGuid)
        {
            var pathToKey = Path.Combine(_env.ContentRootPath, "secret");
            Link? link = await db.Links.FirstOrDefaultAsync(p => p.Guid == linkGuid);
            if (link != null)
            {
                if (link.File_name != null)
                {
                    var decrypted_file_name = KeyStorage.DecryptData(Convert.FromBase64String(link.File_name), pathToKey);
                    var directory = Path.Combine(_env.WebRootPath, "uploads");
                    var path = Path.Combine(directory, decrypted_file_name);
                    var fileExtension = Path.GetExtension(decrypted_file_name);
                    Response.OnStarting(async () =>
                    {
                        Response.Headers.ContentDisposition = $"attachment; filename={linkGuid}{fileExtension}";
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
                    Response.OnCompleted(async () =>
                        {
                            db.Links.Remove(link);
                            await db.SaveChangesAsync();
                        });
                    return PartialView("linkPartial");
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
