using CarRentalManagementSystem_DBFirst.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarRentalManagementSystem_DBFirst.Controllers
{
    public class PaymentController : Controller
    {
        private readonly AppDbContext _context;

        public PaymentController(AppDbContext dbContext)
        {
            _context = dbContext;
        }

        public IActionResult Index(string searchString)
        {
            // Ödemeleri çekerken bağlı olduğu Rental (Kiralama) ve onun da bağlı olduğu Müşteri/Araç bilgilerini de çekiyoruz
            var odemeler = _context.Payments
                .Include(p => p.Rental)
                    .ThenInclude(r => r.Customer)
                .Include(p => p.Rental)
                    .ThenInclude(r => r.Vehicle)
                .AsQueryable();

            // Eğer arama kutusuna müşteri adı, soyadı veya plaka yazıldıysa filtrele
            if (!String.IsNullOrEmpty(searchString))
            {
                odemeler = odemeler.Where(s =>
                    s.Rental.Customer.FirstName.Contains(searchString) ||
                    s.Rental.Customer.LastName.Contains(searchString) ||
                    s.Rental.Vehicle.PlateNumber.Contains(searchString));
            }

            ViewBag.CurrentFilter = searchString;

            return View(odemeler.ToList());
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Ödemenin hangi kiralamaya ait olduğunu seçtirmek için aktif kiralamaları listeliyoruz
            ViewBag.RentalId = _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .Select(r => new SelectListItem
                {
                    Value = r.RentalId.ToString(),
                    Text = $"Sözleşme #{r.RentalId} - {r.Customer.FirstName} {r.Customer.LastName} ({r.Vehicle.PlateNumber}) - Toplam: {r.TotalPrice} ₺"
                }).ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Payment payment)
        {
            if (ModelState.IsValid)
            {
                _context.Payments.Add(payment);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            // Eğer validasyon hatası varsa listeyi tekrar doldurup sayfaya geri gönderiyoruz
            ViewBag.RentalId = _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .Select(r => new SelectListItem
                {
                    Value = r.RentalId.ToString(),
                    Text = $"Sözleşme #{r.RentalId} - {r.Customer.FirstName} {r.Customer.LastName} ({r.Vehicle.PlateNumber})"
                }).ToList();

            return View(payment);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var odeme = _context.Payments.Find(id);
            if (odeme == null)
            {
                return NotFound();
            }

            ViewBag.RentalId = _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .Select(r => new SelectListItem
                {
                    Value = r.RentalId.ToString(),
                    Text = $"Sözleşme #{r.RentalId} - {r.Customer.FirstName} {r.Customer.LastName} ({r.Vehicle.PlateNumber})",
                    Selected = (r.RentalId == odeme.RentalId)
                }).ToList();

            return View(odeme);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Payment payment)
        {
            if (ModelState.IsValid)
            {
                _context.Payments.Update(payment);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.RentalId = _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .Select(r => new SelectListItem
                {
                    Value = r.RentalId.ToString(),
                    Text = $"Sözleşme #{r.RentalId} - {r.Customer.FirstName} {r.Customer.LastName} ({r.Vehicle.PlateNumber})"
                }).ToList();

            return View(payment);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            // Silme ekranında detaylı bilgi göstermek için tüm ilişkileri bağlıyoruz
            var odeme = _context.Payments
                .Include(p => p.Rental)
                    .ThenInclude(r => r.Customer)
                .Include(p => p.Rental)
                    .ThenInclude(r => r.Vehicle)
                .FirstOrDefault(p => p.PaymentId == id);

            if (odeme == null)
            {
                return NotFound();
            }

            return View(odeme);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var odeme = _context.Payments.Find(id);
            if (odeme != null)
            {
                _context.Payments.Remove(odeme);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}