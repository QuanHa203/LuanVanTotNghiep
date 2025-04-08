using CarServer.Databases;
using CarServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarServer.Controllers;

public class CarController : Controller
{
    private readonly CarServerDbContext _context;

    public CarController(CarServerDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var carServerDbContext = _context.Cars.Include(c => c.Esp32Camera).Include(c => c.Esp32Control);
        return View(await carServerDbContext.ToListAsync());
    }


    public IActionResult Create()
        => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Guid id)
    {            
        if (ModelState.IsValid)
        {
            if (id == Guid.Empty)
            {
                ModelState.AddModelError("IdError", "Id này không hợp lệ");
                return View(id);
            }

            if (CarExists(id))
            {
                ModelState.AddModelError("IdError", "Id đã tồn tại");
                return View(id);
            }

            Esp32Control esp32Control = new Esp32Control
            {
                Id = id,
                IsOnline = false,
                LastSeen = DateTime.Now
            };
            Esp32Camera esp32Camera = new Esp32Camera
            {
                Id = id,
                IsOnline = false,
                LastSeen = DateTime.Now
            };

            Car car = new Car()
            {
                Id = id
            };

            _context.Add(esp32Control);
            _context.Add(esp32Camera);
            _context.Add(car);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        else
        {
            if (id == Guid.Empty)
            {
                ModelState.AddModelError("IdError", "Vui lòng điền Id");
                return View(id);
            }

            ModelState.AddModelError("IdError", "Id phải đúng định dạng");
            return View(id);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest();

        var esp32Control = await _context.Esp32Controls.FindAsync(id);
        var esp32Camera = await _context.Esp32Cameras.FindAsync(id);
        if (esp32Camera != null && esp32Control != null)
        {
            _context.Esp32Controls.Remove(esp32Control);
            _context.Esp32Cameras.Remove(esp32Camera);
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    private bool CarExists(Guid id)
    {
        return _context.Cars.Any(e => e.Id == id);
    }
}
