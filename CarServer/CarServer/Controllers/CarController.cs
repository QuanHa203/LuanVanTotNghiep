using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CarServer.Databases;
using CarServer.Models;

namespace CarServer.Controllers
{
    public class CarController : Controller
    {
        private readonly CarServerDbContext _context;

        public CarController(CarServerDbContext context)
        {
            _context = context;
        }

        // GET: Car
        public async Task<IActionResult> Index()
        {
            var carServerDbContext = _context.Cars.Include(c => c.Esp32Camera).Include(c => c.Esp32Control);
            return View(await carServerDbContext.ToListAsync());
        }

        // GET: Car/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .Include(c => c.Esp32Camera)
                .Include(c => c.Esp32Control)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // GET: Car/Create
        public IActionResult Create()
        {

            return View();
        }

        // POST: Car/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid id)
        {
            if (ModelState.IsValid)
            {
                if (_context.Cars.Find(id) != null)
                    return View(id);

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
            return View(id);
        }

        // GET: Car/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars.FindAsync(id);
            if (car == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.Esp32Cameras, "Id", "Id", car.Id);
            ViewData["Id"] = new SelectList(_context.Esp32Controls, "Id", "Id", car.Id);
            return View(car);
        }

        // POST: Car/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id")] Car car)
        {
            if (id != car.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(car);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CarExists(car.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id"] = new SelectList(_context.Esp32Cameras, "Id", "Id", car.Id);
            ViewData["Id"] = new SelectList(_context.Esp32Controls, "Id", "Id", car.Id);
            return View(car);
        }

        // GET: Car/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var car = await _context.Cars
                .Include(c => c.Esp32Camera)
                .Include(c => c.Esp32Control)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (car == null)
            {
                return NotFound();
            }

            return View(car);
        }

        // POST: Car/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car != null)
            {
                _context.Cars.Remove(car);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CarExists(Guid id)
        {
            return _context.Cars.Any(e => e.Id == id);
        }
    }
}
