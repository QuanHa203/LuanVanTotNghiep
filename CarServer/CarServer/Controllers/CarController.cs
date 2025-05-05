using CarServer.Models;
using CarServer.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CarServer.Controllers;

public class CarController : Controller
{
    private readonly IGenericRepository<Car> _carRepository;
    private readonly IGenericRepository<Esp32Camera> _esp32CameraRepository;
    private readonly IGenericRepository<Esp32Control> _esp32ControlRepository;
    private readonly IGenericRepository<CarAccessory> _carAccessoryRepository;
    private readonly IGenericRepository<Accessory> _accessoryRepository;

    private readonly string _uploadCarImagePath;
    private readonly string carImageFolder = "imgs/Cars";

    public CarController(IWebHostEnvironment webHostEnvironment, IGenericRepository<Car> carRepository, IGenericRepository<Esp32Camera> esp32CameraRepository, IGenericRepository<Esp32Control> esp32ControlRepository, IGenericRepository<CarAccessory> carAccessoryRepository, IGenericRepository<Accessory> accessoryRepository)
    {
        _uploadCarImagePath = Path.Combine(webHostEnvironment.WebRootPath, carImageFolder);
        _carRepository = carRepository;
        _esp32CameraRepository = esp32CameraRepository;
        _esp32ControlRepository = esp32ControlRepository;
        _carAccessoryRepository = carAccessoryRepository;
        _accessoryRepository = accessoryRepository;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        var carServerDbContext = _carRepository.GetDbSet().Include(c => c.Esp32Camera).Include(c => c.Esp32Control);

        return View(await carServerDbContext.ToListAsync());
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        List<Accessory> accessories = await _accessoryRepository.GetAllAsync();
        CarModel carModel = new CarModel();

        foreach (var a in accessories)
        {
            var carAccessoryModel = new CarModel.CarAccessoryModel
            {
                IdAccessory = a.Id,
                Name = a.Name,
                IsSelect = false
            };
            carModel.Accessories.Add(carAccessoryModel);
        }
        return View(carModel);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(Guid id)
    {
        if (Guid.Empty == id)
            return NotFound();

        Car? car = await _carRepository.GetDbSet().Include(c => c.CarAccessories).ThenInclude(cA => cA.Accessory).FirstOrDefaultAsync(c => c.Id == id);

        if (car == null)
            return NotFound();

        var accessories = await _accessoryRepository.GetAllAsync();

        
        CarModel carModel = new CarModel()
        {
            Id = car.Id.ToString(),
            Name = car.Name,
            Description = car.Description,
            OldImageUrl = car.ImageUrl
        };

        foreach (var accessory in accessories)
        {
            CarModel.CarAccessoryModel carAccessoryModel = new()
            {
                IdAccessory = accessory.Id,
                Name = accessory.Name
            };

            CarAccessory? carAccessory = car.CarAccessories.FirstOrDefault(cA => cA.IdAccessory == accessory.Id);
            if (carAccessory == null)
                carAccessoryModel.IsSelect = false;
            else
            {
                carAccessoryModel.IsSelect = true;
                carAccessoryModel.Quantity = carAccessory.Quantity;
            }

            carModel.Accessories.Add(carAccessoryModel);
        }


        return View(carModel);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Detail(Guid id)
    {
        if (Guid.Empty == id)
            return NotFound();

        Car? car = await _carRepository.GetDbSet().Include(c => c.CarAccessories).ThenInclude(cA => cA.Accessory).FirstOrDefaultAsync(c => c.Id == id);
        if (car == null)
            return NotFound();

        return View(car);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CarModel carModel)
    {
        if (!ModelState.IsValid)
            return View(carModel);

        if (!Guid.TryParse(carModel.Id, out Guid guid) || guid == Guid.Empty)
        {
            ModelState.AddModelError(nameof(carModel.Id), "Định dạng Guid không hợp lệ.");
            return View(carModel);
        }

        bool isCarExist = await _carRepository.IsExistAsync(c => c.Id == guid);

        if (isCarExist)
        {
            ModelState.AddModelError(nameof(carModel.Id), "Guid đã tồn tại");
            return View(carModel);
        }

        Esp32Control esp32Control = new Esp32Control
        {
            Id = guid,
            IsOnline = false,
            LastSeen = DateTime.Now
        };
        Esp32Camera esp32Camera = new Esp32Camera
        {
            Id = guid,
            IsOnline = false,
            LastSeen = DateTime.Now
        };

        Car car = new Car()
        {
            Id = guid,
            Name = carModel.Name,
            Description = carModel.Description
        };

        if (carModel.ImageFile != null)
        {
            string imageUrl = await SaveImage(guid, carModel.ImageFile);
            car.ImageUrl = imageUrl;
        }

        await _esp32ControlRepository.AddAsync(esp32Control);
        await _esp32CameraRepository.AddAsync(esp32Camera);
        await _carRepository.AddAsync(car);

        foreach (var item in carModel.Accessories)
        {
            if (!item.IsSelect)
                continue;

            CarAccessory carAccessory = new CarAccessory
            {
                IdAccessory = item.IdAccessory,
                IdCar = guid,
                Quantity = item.Quantity,
            };
            await _carAccessoryRepository.AddAsync(carAccessory);
        }

        await _carRepository.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(CarModel carModel)
    {
        if (!ModelState.IsValid)
            return View(carModel);

        if (!Guid.TryParse(carModel.Id, out Guid guid) || guid == Guid.Empty)
            return View(carModel);

        Car? car = await _carRepository.GetByIdAsync(guid);

        if (car == null)
            return NotFound();

        car.Name = carModel.Name;
        car.Description = carModel.Description;        

        // Save and update new Image
        if (carModel.ImageFile != null)
        {
            // Delete old Image
            if (carModel.OldImageUrl != null)
            {
                var imgPath = Path.Combine(_uploadCarImagePath, Path.GetFileName(carModel.OldImageUrl));
                DeleteImage(imgPath);
            }

            string imageUrl = await SaveImage(guid, carModel.ImageFile);
            car.ImageUrl = imageUrl;
        }
        var selectedAccessories = carModel.Accessories;

        car.CarAccessories.Clear();

        // Delete data in CarAccessory table
        await _carAccessoryRepository.GetDbSet().Where(cA => cA.IdCar == car.Id).ForEachAsync(cA => _carAccessoryRepository.Delete(cA));

        foreach (var item in carModel.Accessories)
        {
            if (!item.IsSelect)
                continue;

            CarAccessory carAccessory = new CarAccessory
            {
                IdAccessory = item.IdAccessory,
                IdCar = guid,
                Quantity = item.Quantity,
            };            
            await _carAccessoryRepository.AddAsync(carAccessory);
        }

        _carRepository.Update(car);
        await _carRepository.SaveChangesAsync();



        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest();

        var car = await _carRepository.GetByIdAsync(id);
        var esp32Control = await _esp32ControlRepository.GetByIdAsync(id);
        var esp32Camera = await _esp32CameraRepository.GetByIdAsync(id);

        if (car != null && esp32Camera != null && esp32Control != null)
        {
            if (car.ImageUrl != null)
            {
                var imgPath = Path.Combine(_uploadCarImagePath, Path.GetFileName(car.ImageUrl));
                DeleteImage(imgPath);
            }

            _esp32ControlRepository.Delete(esp32Control);
            _esp32CameraRepository.Delete(esp32Camera);
        }


        await _carRepository.SaveChangesAsync();
        return Ok();
    }

    private async Task<string> SaveImage(Guid guid, IFormFile imageFile)
    {
        if (!Directory.Exists(_uploadCarImagePath))
            Directory.CreateDirectory(_uploadCarImagePath);


        string fileName = guid.ToString() + Path.GetExtension(imageFile.FileName);
        string filePath = Path.Combine(_uploadCarImagePath, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
            await imageFile.CopyToAsync(stream);

        return new StringBuilder().Append('/').Append(carImageFolder).Append('/').Append(fileName).ToString();
    }

    private void DeleteImage(string path)
    {
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Operation,Viewer")]
    public async Task<IActionResult> GetCarById(Guid carId)
    {
        if (Guid.Empty == carId)
            return NotFound();

        Car? car = await _carRepository.GetByIdAsync(carId);
        if (car == null)
            return NotFound();
        
        return Ok(car);

    }

    public class CarModel
    {
        [Required(ErrorMessage = "{0} phải nhập")]
        [Display(Name = "Guid")]
        public string Id { get; set; } = null!;

        [Required(ErrorMessage = "{0} phải nhập")]
        [Display(Name = "Tên xe")]
        public string Name { get; set; } = null!;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Chọn hình ảnh")]
        public IFormFile? ImageFile { get; set; } = null!;

        public string? OldImageUrl { get; set; }

        public List<CarAccessoryModel> Accessories { get; set; } = new();

        public class CarAccessoryModel
        {
            public int IdAccessory { get; set; }
            public string Name { get; set; } = null!;
            public int Quantity { get; set; }
            public bool IsSelect { get; set; }
        }
    }

}
