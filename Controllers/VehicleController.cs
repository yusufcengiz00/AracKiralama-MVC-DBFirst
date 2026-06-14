using CarRentalManagementSystem_DBFirst.Models;
using Microsoft.AspNetCore.Mvc;
using CarRentalManagementSystem_DBFirst.Models;

namespace CarRentalManagementSystem_DBFirst.Controllers
{
    public class VehicleController : Controller
    {

        private readonly AppDbContext _context;

        public VehicleController(AppDbContext dbContext)

        { _context = dbContext; }

        public IActionResult Index(string searchString)
        {
            var araclar = from u in _context.Vehicles
                          select u;

            if (!String.IsNullOrEmpty(searchString))
            {
                araclar = araclar.Where(s => s.PlateNumber.Contains(searchString));
            }

            ViewBag.CurrentFilter = searchString;

            return View(araclar.ToList());
        }
        [HttpGet]
        public IActionResult Create()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF Saldırılarına karşı güvenlik önlemi
        public IActionResult Create(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var arac = _context.Vehicles.Find(id);
            if (arac == null)
            {
                return NotFound();
            }
            return View(arac);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Vehicle vehicle)
        {
            _context.Vehicles.Update(vehicle);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var arac = _context.Vehicles.Find(id);
            if (arac == null)
            {
                return NotFound();
            }
            return View(arac);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var gercekArac = _context.Vehicles.Find(id);

            if (gercekArac != null)
            {
                _context.Vehicles.Remove(gercekArac);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

    }
}
