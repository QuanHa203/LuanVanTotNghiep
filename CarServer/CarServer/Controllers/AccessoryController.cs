using CarServer.Databases;
using CarServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Crmf;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CarServer.Controllers
{
    public class AccessoryController : Controller
    {
        private readonly CarServerDbContext _context;
        private readonly string _uploadAccessoryImagePath;
        private readonly string accessoryImageFolder = "imgs/Accessories";

        public AccessoryController(CarServerDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _uploadAccessoryImagePath = Path.Combine(webHostEnvironment.WebRootPath, accessoryImageFolder);
        }

        [HttpGet]
        public async Task<IActionResult> Index() => View(await _context.Accessories.ToListAsync());

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(AccessoryCreateModel accessoryCreateModel)
        {
            if (!ModelState.IsValid)
                return View(accessoryCreateModel);

            Accessory accessory = new Accessory
            {
                Name = accessoryCreateModel.Name
            };

            _context.Accessories.Add(accessory);
            _context.SaveChanges();

            if (accessoryCreateModel.ImageFile != null)
            {
                string imageUrl = await SaveImageAsync(accessory.Id, accessoryCreateModel.ImageFile);
                accessory.ImageUrl = imageUrl;
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var accessory = await _context.Accessories.FindAsync(id);

            if (accessory == null)
                return BadRequest();

            if (accessory.ImageUrl != null)
            {
                var imgPath = Path.Combine(_uploadAccessoryImagePath, Path.GetFileName(accessory.ImageUrl));
                DeleteImage(imgPath);
            }

            _context.Accessories.Remove(accessory);
            await _context.SaveChangesAsync();
            return Ok();
        }

        private async Task<string> SaveImageAsync(int id, IFormFile imageFile)
        {
            if (!Directory.Exists(_uploadAccessoryImagePath))
                Directory.CreateDirectory(_uploadAccessoryImagePath);


            string fileName = id + Path.GetExtension(imageFile.FileName);
            string filePath = Path.Combine(_uploadAccessoryImagePath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
                await imageFile.CopyToAsync(stream);

            return new StringBuilder().Append('/').Append(accessoryImageFolder).Append('/').Append(fileName).ToString();
        }

        private void DeleteImage(string path)
        {
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }

        public class AccessoryCreateModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "{0} phải nhập")]
            [Display(Name = "Tên linh kiện")]
            public string Name { get; set; } = null!;

            [Display(Name = "Hình ảnh linh kiện")]
            public IFormFile? ImageFile { get; set; } = null!;
        }
    }
}
