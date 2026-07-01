using CarRentalManagementSystem_DBFirst.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Project2_DBFirst.Controllers
{
    public class AdminController : Controller
    {

        private readonly AppDbContext _context;

        public AdminController(AppDbContext dbContext)

        { _context = dbContext; }

        public IActionResult Index()
        {
            // İstatistikleri yükle
            ViewBag.TotalVehicles = _context.Vehicles.Count();
            ViewBag.TotalCustomers = _context.Customers.Count();
            ViewBag.TotalRentals = _context.Rentals.Where(r => r.RentalStatus == "Aktif").Count();

            // Toplam geliri hesapla
            ViewBag.TotalRevenue = _context.Payments.Sum(p => (decimal?)p.Amount) ?? 0;
            return View();
        }
    }
}
