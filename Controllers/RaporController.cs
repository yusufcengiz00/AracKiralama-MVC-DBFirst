using CarRentalManagementSystem_DBFirst.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarRentalManagementSystem_DBFirst.Controllers
{
    public class RaporController : Controller
    {
        private readonly AppDbContext _context;

        public RaporController(AppDbContext dbContext)
        {
            _context = dbContext;
        }

        public IActionResult Index()
        {
            // 1. Sorgu: En yüksek kilometreye sahip araç
            var enYuksekKmLiarac = _context.Vehicles
                .OrderByDescending(v => v.Kilometer)
                .FirstOrDefault();
            ViewBag.EnYuksekKmArax = enYuksekKmLiarac != null ? $"{enYuksekKmLiarac.Brand} {enYuksekKmLiarac.Model} ({enYuksekKmLiarac.Kilometer} KM)" : "Veri Yok";


            // 2. Sorgu: En çok kiralanan (En popüler) araç
            var enPopulerAracId = _context.Rentals
                .GroupBy(r => r.VehicleId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            var enPopulerArac = _context.Vehicles.FirstOrDefault(v => v.VehicleId == enPopulerAracId);
            ViewBag.EnPopulerArac = enPopulerArac != null ? $"{enPopulerArac.Brand} {enPopulerArac.Model} ({enPopulerArac.PlateNumber})" : "Veri Yok";


            // 3. Sorgu: En çok kiralama yapan (En sadık) müşteri
            var enSadikMusteriId = _context.Rentals
                .GroupBy(r => r.CustomerId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            var enSadikMusteri = _context.Customers.FirstOrDefault(c => c.CustomerId == enSadikMusteriId);
            ViewBag.EnSadikMusteri = enSadikMusteri != null ? $"{enSadikMusteri.FirstName} {enSadikMusteri.LastName}" : "Veri Yok";


            // 4. Sorgu: Şirkete en çok para kazandıran VIP Müşteri
            var vipMusteriBilgisi = _context.Payments
                .Join(_context.Rentals,
                      p => p.RentalId,
                      r => r.RentalId,
                      (p, r) => new { p, r })
                .GroupBy(x => x.r.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    ToplamTutar = g.Sum(x => x.p.Amount)
                })
                .OrderByDescending(x => x.ToplamTutar)
                .FirstOrDefault();

            if (vipMusteriBilgisi != null)
            {
                var vipMusteri = _context.Customers.FirstOrDefault(c => c.CustomerId == vipMusteriBilgisi.CustomerId);
                ViewBag.VipMusteri = vipMusteri != null ? $"{vipMusteri.FirstName} {vipMusteri.LastName}" : "Veri Yok";
            }
            else
            {
                ViewBag.VipMusteri = "Veri Yok";
            }


            // 5. Sorgu: Sözleşmeler üzerinden GERÇEK ortalama günlük kiralama bedeli
            var tumKiralamalar = _context.Rentals
                .Where(r => r.StartDate != null && r.EndDate != null && r.TotalPrice != null)
                .ToList(); // Belleğe çekme (Client-side evaluation)

            decimal ortalamaGunlukFiyat = 0;

            if (tumKiralamalar.Any())
            {
                var gunlukFiyatlarListesi = new List<decimal>();

                foreach (var r in tumKiralamalar)
                {
                    int gunSayisi = r.EndDate.Value.DayNumber - r.StartDate.Value.DayNumber;

                    if (gunSayisi <= 0) gunSayisi = 1;

                    decimal gunlukBedel = (r.TotalPrice ?? 0) / gunSayisi;
                    gunlukFiyatlarListesi.Add(gunlukBedel);
                }

                if (gunlukFiyatlarListesi.Any())
                {
                    ortalamaGunlukFiyat = gunlukFiyatlarListesi.Average();
                }
            }

            ViewBag.OrtalamaDailyPrice = string.Format("{0:C2}", ortalamaGunlukFiyat);


            // 6. Sorgu: Bugüne kadar yapılmış en uzun süreli kiralama sözleşmesi
            var enUzunKiralama = _context.Rentals
                .Where(r => r.StartDate != null && r.EndDate != null)
                .Join(_context.Vehicles,
                      r => r.VehicleId,
                      v => v.VehicleId,
                      (r, v) => new
                      {
                          AraçMarka = v.Brand,
                          AraçModel = v.Model,
                          Gun = EF.Functions.DateDiffDay(r.StartDate, r.EndDate)
                      })
                .OrderByDescending(x => x.Gun)
                .FirstOrDefault();

            ViewBag.EnUzunKiralama = enUzunKiralama != null
                ? $"{enUzunKiralama.AraçMarka} {enUzunKiralama.AraçModel} / {enUzunKiralama.Gun} Gün"
                : "Veri Yok";


            // 7. Sorgu: İptal ya da iade edilen toplam ödeme miktarı
            var kaybedilenTutar = _context.Payments
                .Where(p => p.PaymentStatus == false)
                .Sum(p => (decimal?)p.Amount) ?? 0;
            ViewBag.KaybedilenTutar = string.Format("{0:C2}", kaybedilenTutar);


            // 8. Sorgu: Şu an filoda kiralık olmayıp boşta yatan (Müsait) araç sayısı
            var musaitAracSayisi = _context.Vehicles.Count(v => v.Status == true);
            ViewBag.MusaitAracSayisi = $"{musaitAracSayisi} Adet";


            return View();
        }
    }
}