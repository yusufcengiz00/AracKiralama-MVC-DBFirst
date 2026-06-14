using CarRentalManagementSystem_DBFirst.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var enPopulerArac = _context.Vehicles.Find(enPopulerAracId);
            ViewBag.EnPopulerArac = enPopulerArac != null ? $"{enPopulerArac.Brand} {enPopulerArac.Model} ({enPopulerArac.PlateNumber})" : "Veri Yok";

            // 3. Sorgu: En çok kiralama yapan (En sadık) müşteri
            var enCokKiralayanMusteriId = _context.Rentals
                .GroupBy(r => r.CustomerId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
            var enSadikMusteri = _context.Customers.Find(enCokKiralayanMusteriId);
            ViewBag.EnSadikMusteri = enSadikMusteri != null ? $"{enSadikMusteri.FirstName} {enSadikMusteri.LastName}" : "Veri Yok";

            // 4. Sorgu: Şirkete en çok para kazandıran VIP Müşteri
            var vipMusteriId = _context.Payments
                .Include(p => p.Rental)
                .GroupBy(p => p.Rental.CustomerId)
                .OrderByDescending(g => g.Sum(p => p.Amount))
                .Select(g => g.Key)
                .FirstOrDefault();
            var vipMusteri = _context.Customers.Find(vipMusteriId);
            ViewBag.VipMusteri = vipMusteri != null ? $"{vipMusteri.FirstName} {vipMusteri.LastName}" : "Veri Yok";

            // 5. Sorgu: Sözleşmeler üzerinden GERÇEK ortalama günlük kiralama bedeli
            var tumKiralamalar = _context.Rentals
                .Where(r => r.StartDate != null && r.EndDate != null && r.TotalPrice != null)
                .ToList(); // Verileri belleğe (RAM) çekiyoruz

            decimal ortalamaGunlukFiyat = 0;

            if (tumKiralamalar.Any())
            {
                var gunlukFiyatlarListesi = new List<decimal>();

                foreach (var r in tumKiralamalar)
                {
                    // DateOnly için gün farkını DayNumber özelliklerini çıkararak buluyoruz
                    int gunSayisi = r.EndDate.Value.DayNumber - r.StartDate.Value.DayNumber;

                    // Eğer gün sayısı 0 veya negatifse (aynı gün teslim), sıfıra bölünme hatasını engellemek için 1 gün sayıyoruz
                    if (gunSayisi <= 0) gunSayisi = 1;

                    // Sözleşme tutarını bulunan gün sayısına bölüyoruz
                    decimal gunlukBedel = (r.TotalPrice ?? 0) / gunSayisi;
                    gunlukFiyatlarListesi.Add(gunlukBedel);
                }

                if (gunlukFiyatlarListesi.Any())
                {
                    // Tüm günlük fiyatların ortalamasını alıyoruz
                    ortalamaGunlukFiyat = gunlukFiyatlarListesi.Average();
                }
            }

            ViewBag.OrtalamaDailyPrice = string.Format("{0:C2}", ortalamaGunlukFiyat);

            // 6. Sorgu: Bugüne kadar yapılmış en uzun süreli kiralama sözleşmesi (Gün olarak)
            var enUzunKiralama = _context.Rentals
                .Include(r => r.Vehicle) // Araç bilgisini de çekiyoruz
                .Where(r => r.StartDate != null && r.EndDate != null) // Boş olan tarihleri eliyoruz
                .OrderByDescending(r => EF.Functions.DateDiffDay(r.StartDate, r.EndDate)) // SQL düzeyinde gün farkı hesabı
                .Select(r => new
                {
                    Rental = r,
                    Gun = EF.Functions.DateDiffDay(r.StartDate, r.EndDate)
                })
                .FirstOrDefault();

            ViewBag.EnUzunKiralama = enUzunKiralama != null && enUzunKiralama.Rental.Vehicle != null
                ? $"{enUzunKiralama.Rental.Vehicle.Brand} {enUzunKiralama.Rental.Vehicle.Model} / {enUzunKiralama.Gun} Gün"
                : "Veri Yok";

            // 7. Sorgu: İptal ya da iade edilen toplam ödeme miktarı (Kayıp kazanç)
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