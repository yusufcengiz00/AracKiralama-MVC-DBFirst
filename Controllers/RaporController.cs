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
            // 1. Sorgu: En yüksek kilometreye sahip araç (AYNI KALDI)
            var enYuksekKmLiarac = _context.Vehicles
                .OrderByDescending(v => v.Kilometer)
                .FirstOrDefault();
            ViewBag.EnYuksekKmArax = enYuksekKmLiarac != null ? $"{enYuksekKmLiarac.Brand} {enYuksekKmLiarac.Model} ({enYuksekKmLiarac.Kilometer} KM)" : "Veri Yok";


            // 2. Sorgu: En çok kiralanan (En popüler) araç -> [GÜNCELLENDİ: LINQ JOIN]
            // Önce kiralama sayılarını bulup araç tablosuyla joinliyoruz
            var enPopulerArac = (from r in _context.Rentals
                                 group r by r.VehicleId into grup
                                 orderby grup.Count() descending
                                 join v in _context.Vehicles on grup.Key equals v.VehicleId
                                 select v).FirstOrDefault();

            ViewBag.EnPopulerArac = enPopulerArac != null ? $"{enPopulerArac.Brand} {enPopulerArac.Model} ({enPopulerArac.PlateNumber})" : "Veri Yok";


            // 3. Sorgu: En çok kiralama yapan (En sadık) müşteri -> [GÜNCELLENDİ: LINQ JOIN]
            // En çok kiralama yapan müşteri ID'sini bulup müşteri tablosuyla joinliyoruz
            var enSadikMusteri = (from r in _context.Rentals
                                  group r by r.CustomerId into grup
                                  orderby grup.Count() descending
                                  join c in _context.Customers on grup.Key equals c.CustomerId
                                  select c).FirstOrDefault();

            ViewBag.EnSadikMusteri = enSadikMusteri != null ? $"{enSadikMusteri.FirstName} {enSadikMusteri.LastName}" : "Veri Yok";


            // 4. Sorgu: Şirkete en çok para kazandıran VIP Müşteri -> [GÜNCELLENDİ: LINQ JOIN + GROUP BY]
            // Payments ve Rentals tablolarını JOIN'leyip ardından CustomerId'ye göre GROUP BY yapıyoruz
            var vipMusteriBilgisi = (from p in _context.Payments
                                     join r in _context.Rentals on p.RentalId equals r.RentalId
                                     group p by r.CustomerId into g
                                     orderby g.Sum(x => x.Amount) descending
                                     join c in _context.Customers on g.Key equals c.CustomerId
                                     select new
                                     {
                                         Musteri = c,
                                         ToplamKazanc = g.Sum(x => x.Amount)
                                     }).FirstOrDefault();

            ViewBag.VipMusteri = vipMusteriBilgisi != null ? $"{vipMusteriBilgisi.Musteri.FirstName} {vipMusteriBilgisi.Musteri.LastName}" : "Veri Yok";


            // 5. Sorgu: Sözleşmeler üzerinden GERÇEK ortalama günlük kiralama bedeli (AYNI KALDI)
            var tumKiralamalar = _context.Rentals
                .Where(r => r.StartDate != null && r.EndDate != null && r.TotalPrice != null)
                .ToList();

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


            // 6. Sorgu: Bugüne kadar yapılmış en uzun süreli kiralama sözleşmesi -> [GÜNCELLENDİ: LINQ JOIN]
            // Rentals ile Vehicles tablolarını joinleyerek Include kullanımından kurtulduk
            var enUzunKiralama = (from r in _context.Rentals
                                  where r.StartDate != null && r.EndDate != null
                                  join v in _context.Vehicles on r.VehicleId equals v.VehicleId
                                  orderby EF.Functions.DateDiffDay(r.StartDate, r.EndDate) descending
                                  select new
                                  {
                                      AraçMarka = v.Brand,
                                      AraçModel = v.Model,
                                      Gun = EF.Functions.DateDiffDay(r.StartDate, r.EndDate)
                                  }).FirstOrDefault();

            ViewBag.EnUzunKiralama = enUzunKiralama != null
                ? $"{enUzunKiralama.AraçMarka} {enUzunKiralama.AraçModel} / {enUzunKiralama.Gun} Gün"
                : "Veri Yok";


            // 7. Sorgu: İptal ya da iade edilen toplam ödeme miktarı (AYNI KALDI)
            var kaybedilenTutar = _context.Payments
                .Where(p => p.PaymentStatus == false)
                .Sum(p => (decimal?)p.Amount) ?? 0;
            ViewBag.KaybedilenTutar = string.Format("{0:C2}", kaybedilenTutar);


            // 8. Sorgu: Şu an filoda kiralık olmayıp boşta yatan (Müsait) araç sayısı (AYNI KALDI)
            var musaitAracSayisi = _context.Vehicles.Count(v => v.Status == true);
            ViewBag.MusaitAracSayisi = $"{musaitAracSayisi} Adet";

            return View();
        }
    }
}