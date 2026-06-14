using CarRentalManagementSystem_DBFirst.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalManagementSystem_DBFirst.Controllers
{
    public class RentalController : Controller
    {

        private readonly AppDbContext _context;

        public RentalController(AppDbContext dbContext)

        { _context = dbContext; }

        public IActionResult Index(string searchString)
        {
            // CRITICAL DÜZELTME: .Include(r => r.Customer) ve .Include(r => r.Vehicle) ekledik!
            var kiralama = _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                kiralama = kiralama.Where(s =>
                    s.Customer.FirstName.Contains(searchString) ||
                    s.Customer.LastName.Contains(searchString) ||
                    s.Vehicle.PlateNumber.Contains(searchString) ||
                    s.Vehicle.Model.Contains(searchString));
            }

            ViewBag.CurrentFilter = searchString;

            return View(kiralama.ToList());
        }
        [HttpGet]
        public IActionResult Create()
        {
            // Müşterileri "Ad Soyad" şeklinde birleştirerek SelectList'e gönderiyoruz
            ViewBag.CustomerId = _context.Customers
                .Select(c => new { c.CustomerId, FullName = c.FirstName + " " + c.LastName })
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.CustomerId.ToString(),
                    Text = c.FullName
                }).ToList();

            // Sadece "Müsait" (Status == true) olan araçları listeliyoruz
            ViewBag.VehicleId = _context.Vehicles
                .Where(v => v.Status == true)
                .Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = v.VehicleId.ToString(),
                    Text = v.Brand + " " + v.Model + " (" + v.PlateNumber + ") - " + v.DailyPrice + " ₺"
                }).ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF Saldırılarına karşı güvenlik önlemi
        public IActionResult Create(Rental rental)
        {
            _context.Rentals.Add(rental);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var kiralama = _context.Rentals.Find(id);
            if (kiralama == null)
            {
                return NotFound(); // Kiralama bulunamadıysa 404 hatası ver
            }

            // Müşteri listesini hazırlayıp ViewBag'e atıyoruz
            ViewBag.CustomerId = _context.Customers
                .Select(c => new { c.CustomerId, FullName = c.FirstName + " " + c.LastName })
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.CustomerId.ToString(),
                    Text = c.FullName,
                    Selected = (c.CustomerId == kiralama.CustomerId) // Mevcut müşteri seçili gelsin
                }).ToList();

            // Araç listesini hazırlayıp ViewBag'e atıyoruz
            ViewBag.VehicleId = _context.Vehicles
                .Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = v.VehicleId.ToString(),
                    Text = v.Brand + " " + v.Model + " (" + v.PlateNumber + ") - " + v.DailyPrice + " ₺",
                    Selected = (v.VehicleId == kiralama.VehicleId) // Mevcut araç seçili gelsin
                }).ToList();

            return View(kiralama);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Rental rental)
        {
            _context.Rentals.Update(rental);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            // Eager Loading kullanarak ilişkili Customer ve Vehicle tablolarını da çekiyoruz
            var kiralama = _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .FirstOrDefault(r => r.RentalId == id);

            if (kiralama == null)
            {
                return NotFound();
            }

            // Klasörün adının "Rental" olduğundan emin olarak View'ı tetikliyoruz
            return View(kiralama);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var kiralama = _context.Rentals.Find(id);

            if (kiralama != null)
            {
                _context.Rentals.Remove(kiralama);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

    }
}
