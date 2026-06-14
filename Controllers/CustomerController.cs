using CarRentalManagementSystem_DBFirst.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalManagementSystem_DBFirst.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AppDbContext _context;

        public CustomerController(AppDbContext dbContext)
        {
            _context = dbContext;
        }

        // 1. MÜŞTERİ LİSTELEME & ARAMA (INDEX)
        public IActionResult Index(string searchString)
        {
            // Veritabanındaki müşterileri sorgu olarak hazırlıyoruz
            var musteriler = from c in _context.Customers
                             select c;

            // Eğer arama kutusuna isim veya soyisim yazılmışsa filtrele
            if (!string.IsNullOrEmpty(searchString))
            {
                musteriler = musteriler.Where(s =>
                    s.FirstName.Contains(searchString) ||
                    s.LastName.Contains(searchString));
            }

            // Arama kelimesini kutuda çakılı kalsın diye ViewBag ile gönderiyoruz
            ViewBag.CurrentFilter = searchString;

            return View(musteriler.ToList());
        }

        // 2. YENİ MÜŞTERİ EKLEME SAYFASI (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // 3. YENİ MÜŞTERİ EKLEME İŞLEMİ (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                _context.Customers.Add(customer);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // 4. MÜŞTERİ DÜZENLEME SAYFASI (GET)
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var customer = _context.Customers.Find(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // 5. MÜŞTERİ DÜZENLEME İŞLEMİ (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                _context.Customers.Update(customer);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // 6. MÜŞTERI SİLME ONAY SAYFASI (GET)
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var customer = _context.Customers.Find(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // 7. GERÇEK SİLME İŞLEMİ (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var customer = _context.Customers.Find(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}