using CarRentalManagementSystem_DBFirst.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
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

        [HttpGet]
        public IActionResult ExportToPdf()
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var enYuksekKmLiarac = _context.Vehicles.OrderByDescending(v => v.Kilometer).FirstOrDefault();
            string r1 = enYuksekKmLiarac != null ? $"{enYuksekKmLiarac.Brand} {enYuksekKmLiarac.Model} ({enYuksekKmLiarac.Kilometer} KM)" : "Veri Yok";

            var enPopulerAracId = _context.Rentals.GroupBy(r => r.VehicleId).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
            var enPopulerArac = _context.Vehicles.FirstOrDefault(v => v.VehicleId == enPopulerAracId);
            string r2 = enPopulerArac != null ? $"{enPopulerArac.Brand} {enPopulerArac.Model} ({enPopulerArac.PlateNumber})" : "Veri Yok";

            var enSadikMusteriId = _context.Rentals.GroupBy(r => r.CustomerId).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
            var enSadikMusteri = _context.Customers.FirstOrDefault(c => c.CustomerId == enSadikMusteriId);
            string r3 = enSadikMusteri != null ? $"{enSadikMusteri.FirstName} {enSadikMusteri.LastName}" : "Veri Yok";

            var vipMusteriBilgisi = _context.Payments.Join(_context.Rentals, p => p.RentalId, r => r.RentalId, (p, r) => new { p, r }).GroupBy(x => x.r.CustomerId).Select(g => new { CustomerId = g.Key, ToplamTutar = g.Sum(x => x.p.Amount) }).OrderByDescending(x => x.ToplamTutar).FirstOrDefault();
            string r4 = "Veri Yok";
            if (vipMusteriBilgisi != null)
            {
                var vipMusteri = _context.Customers.FirstOrDefault(c => c.CustomerId == vipMusteriBilgisi.CustomerId);
                r4 = vipMusteri != null ? $"{vipMusteri.FirstName} {vipMusteri.LastName}" : "Veri Yok";
            }

            var tumKiralamalar = _context.Rentals.Where(r => r.StartDate != null && r.EndDate != null && r.TotalPrice != null).ToList();
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
                if (gunlukFiyatlarListesi.Any()) ortalamaGunlukFiyat = gunlukFiyatlarListesi.Average();
            }
            string r5 = string.Format("{0:C2}", ortalamaGunlukFiyat);

            var enUzunKiralama = _context.Rentals.Where(r => r.StartDate != null && r.EndDate != null).Join(_context.Vehicles, r => r.VehicleId, v => v.VehicleId, (r, v) => new { AraçMarka = v.Brand, AraçModel = v.Model, Gun = EF.Functions.DateDiffDay(r.StartDate, r.EndDate) }).OrderByDescending(x => x.Gun).FirstOrDefault();
            string r6 = enUzunKiralama != null ? $"{enUzunKiralama.AraçMarka} {enUzunKiralama.AraçModel} / {enUzunKiralama.Gun} Gün" : "Veri Yok";

            var kaybedilenTutar = _context.Payments.Where(p => p.PaymentStatus == false).Sum(p => (decimal?)p.Amount) ?? 0;
            string r7 = string.Format("{0:C2}", kaybedilenTutar);

            var musaitAracSayisi = _context.Vehicles.Count(v => v.Status == true);
            string r8 = $"{musaitAracSayisi} Adet";

            var pdfDocument = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header()
                        .Text("Filo Performans ve İstatistik Raporu")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingTop(1, Unit.Centimetre)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Analiz Metriği").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("İstatistik Sonucu").Bold();
                            });

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text("En Yüksek KM'li Araç");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(r1);

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text("En Çok Kiralanan Araç");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(r2);

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text("En Sadık Müşteri");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(r3);

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text("VIP Müşteri (En Çok Kazandıran)");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(r4);

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text("Ortalama Günlük Kiralama Bedeli");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(r5);

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text("En Uzun Kiralama Sözleşmesi");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(r6);

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text("İptal Edilen Toplam Tutar");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(r7);

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text("Müsait Durumdaki Araç Sayısı");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(r8);
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Sayfa ");
                            x.CurrentPageNumber();
                        });
                });
            });

            var pdfBytes = pdfDocument.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"Performans_Raporu_{DateTime.Now:yyyyMMdd}.pdf");
        }

        [HttpGet]
        public IActionResult ExportToExcel()
        {
            ExcelPackage.License.SetNonCommercialPersonal("Backend softito");

            var enYuksekKmLiarac = _context.Vehicles.OrderByDescending(v => v.Kilometer).FirstOrDefault();
            string r1 = enYuksekKmLiarac != null ? $"{enYuksekKmLiarac.Brand} {enYuksekKmLiarac.Model} ({enYuksekKmLiarac.Kilometer} KM)" : "Veri Yok";

            var enPopulerAracId = _context.Rentals.GroupBy(r => r.VehicleId).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
            var enPopulerArac = _context.Vehicles.FirstOrDefault(v => v.VehicleId == enPopulerAracId);
            string r2 = enPopulerArac != null ? $"{enPopulerArac.Brand} {enPopulerArac.Model} ({enPopulerArac.PlateNumber})" : "Veri Yok";

            var enSadikMusteriId = _context.Rentals.GroupBy(r => r.CustomerId).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
            var enSadikMusteri = _context.Customers.FirstOrDefault(c => c.CustomerId == enSadikMusteriId);
            string r3 = enSadikMusteri != null ? $"{enSadikMusteri.FirstName} {enSadikMusteri.LastName}" : "Veri Yok";

            var vipMusteriBilgisi = _context.Payments.Join(_context.Rentals, p => p.RentalId, r => r.RentalId, (p, r) => new { p, r }).GroupBy(x => x.r.CustomerId).Select(g => new { CustomerId = g.Key, ToplamTutar = g.Sum(x => x.p.Amount) }).OrderByDescending(x => x.ToplamTutar).FirstOrDefault();
            string r4 = "Veri Yok";
            if (vipMusteriBilgisi != null)
            {
                var vipMusteri = _context.Customers.FirstOrDefault(c => c.CustomerId == vipMusteriBilgisi.CustomerId);
                r4 = vipMusteri != null ? $"{vipMusteri.FirstName} {vipMusteri.LastName}" : "Veri Yok";
            }

            var tumKiralamalar = _context.Rentals.Where(r => r.StartDate != null && r.EndDate != null && r.TotalPrice != null).ToList();
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
                if (gunlukFiyatlarListesi.Any()) ortalamaGunlukFiyat = gunlukFiyatlarListesi.Average();
            }
            string r5 = string.Format("{0:C2}", ortalamaGunlukFiyat);

            var enUzunKiralama = _context.Rentals.Where(r => r.StartDate != null && r.EndDate != null).Join(_context.Vehicles, r => r.VehicleId, v => v.VehicleId, (r, v) => new { AraçMarka = v.Brand, AraçModel = v.Model, Gun = EF.Functions.DateDiffDay(r.StartDate, r.EndDate) }).OrderByDescending(x => x.Gun).FirstOrDefault();
            string r6 = enUzunKiralama != null ? $"{enUzunKiralama.AraçMarka} {enUzunKiralama.AraçModel} / {enUzunKiralama.Gun} Gün" : "Veri Yok";

            var kaybedilenTutar = _context.Payments.Where(p => p.PaymentStatus == false).Sum(p => (decimal?)p.Amount) ?? 0;
            string r7 = string.Format("{0:C2}", kaybedilenTutar);

            var musaitAracSayisi = _context.Vehicles.Count(v => v.Status == true);
            string r8 = $"{musaitAracSayisi} Adet";

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Performans Raporu");

                worksheet.Cells[1, 1].Value = "Analiz Metriği";
                worksheet.Cells[1, 2].Value = "İstatistik Sonucu";

                using (var range = worksheet.Cells[1, 1, 1, 2])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(41, 128, 185));
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                worksheet.Cells[2, 1].Value = "En Yüksek KM'li Araç"; worksheet.Cells[2, 2].Value = r1;
                worksheet.Cells[3, 1].Value = "En Çok Kiralanan Araç"; worksheet.Cells[3, 2].Value = r2;
                worksheet.Cells[4, 1].Value = "En Sadık Müşteri"; worksheet.Cells[4, 2].Value = r3;
                worksheet.Cells[5, 1].Value = "VIP Müşteri (En Çok Kazandıran)"; worksheet.Cells[5, 2].Value = r4;
                worksheet.Cells[6, 1].Value = "Ortalama Günlük Kiralama Bedeli"; worksheet.Cells[6, 2].Value = r5;
                worksheet.Cells[7, 1].Value = "En Uzun Kiralama Sözleşmesi"; worksheet.Cells[7, 2].Value = r6;
                worksheet.Cells[8, 1].Value = "İptal Edilen Toplam Tutar"; worksheet.Cells[8, 2].Value = r7;
                worksheet.Cells[9, 1].Value = "Müsait Durumdaki Araç Sayısı"; worksheet.Cells[9, 2].Value = r8;

                if (worksheet.Dimension != null)
                {
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                }

                var fileBytes = package.GetAsByteArray();
                string fileName = $"Performans_Raporu_{DateTime.Now:yyyyMMdd}.xlsx";

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
        public IActionResult Index()
        {
            // En yüksek KM'li araç
            var enYuksekKmLiarac = _context.Vehicles
                .OrderByDescending(v => v.Kilometer)
                .FirstOrDefault();
            ViewBag.EnYuksekKmArax = enYuksekKmLiarac != null ? $"{enYuksekKmLiarac.Brand} {enYuksekKmLiarac.Model} ({enYuksekKmLiarac.Kilometer} KM)" : "Veri Yok";


            // En popüler araç
            var enPopulerAracId = _context.Rentals
                .GroupBy(r => r.VehicleId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            var enPopulerArac = _context.Vehicles.FirstOrDefault(v => v.VehicleId == enPopulerAracId);
            ViewBag.EnPopulerArac = enPopulerArac != null ? $"{enPopulerArac.Brand} {enPopulerArac.Model} ({enPopulerArac.PlateNumber})" : "Veri Yok";


            // En sadık müşteri
            var enSadikMusteriId = _context.Rentals
                .GroupBy(r => r.CustomerId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            var enSadikMusteri = _context.Customers.FirstOrDefault(c => c.CustomerId == enSadikMusteriId);
            ViewBag.EnSadikMusteri = enSadikMusteri != null ? $"{enSadikMusteri.FirstName} {enSadikMusteri.LastName}" : "Veri Yok";


            // VIP müşteri
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


            // Ortalama günlük kiralama bedeli
            var tumKiralamalar = _context.Rentals
                .Where(r => r.StartDate != null && r.EndDate != null && r.TotalPrice != null)
                .ToList(); // Belleğe çek

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


            // En uzun kiralama
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


            // İptal edilen toplam ödeme
            var kaybedilenTutar = _context.Payments
                .Where(p => p.PaymentStatus == false)
                .Sum(p => (decimal?)p.Amount) ?? 0;
            ViewBag.KaybedilenTutar = string.Format("{0:C2}", kaybedilenTutar);


            // Müsait araç sayısı
            var musaitAracSayisi = _context.Vehicles.Count(v => v.Status == true);
            ViewBag.MusaitAracSayisi = $"{musaitAracSayisi} Adet";


            return View();
        }
    }
}