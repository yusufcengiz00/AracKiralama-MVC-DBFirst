using CarRentalManagementSystem_DBFirst.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace CarRentalManagementSystem_DBFirst.Controllers
{
    public class İstatistikController : Controller
    {
        private readonly AppDbContext _context;

        public İstatistikController(AppDbContext dbContext)
        {
            _context = dbContext;
        }

        public IActionResult Index()
        {
            // Son 10 ödeme ve ilişkiler
            var sonOdemeler = _context.Payments
                .Include(p => p.Rental)
                    .ThenInclude(r => r.Customer)
                .Include(p => p.Rental)
                    .ThenInclude(r => r.Vehicle)
                .Where(p => p.Amount != null)
                .OrderBy(p => p.PaymentDate) // Kronolojik
                .Take(10) // Son 10
                .ToList();

            // Grafik etiketleri
            var etiketlerListesi = sonOdemeler.Select(p =>
                p.Rental != null && p.Rental.Customer != null && p.Rental.Vehicle != null
                ? $"'{p.Rental.Customer.FirstName} {p.Rental.Customer.LastName[0]}. ({p.Rental.Vehicle.Brand})'"
                : "'Bilinmeyen Ödeme'"
            ).ToArray();

            // Ödeme tutarları
            var tutarlarListesi = sonOdemeler.Select(p => p.Amount ?? 0).ToArray();

            // Boş veri koruması
            if (etiketlerListesi.Length == 0)
            {
                etiketlerListesi = new string[] { "'Kayıt Yok'" };
                tutarlarListesi = new decimal[] { 0 };
            }

            ViewBag.GrafikEtiketler = string.Join(",", etiketlerListesi);
            ViewBag.GrafikTutarlar = string.Join(",", tutarlarListesi);

            return View();
        }
    }
}