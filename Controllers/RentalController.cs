using CarRentalManagementSystem_DBFirst.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CarRentalManagementSystem_DBFirst.Controllers
{
    public class RentalController : Controller
    {

        private readonly AppDbContext _context;

        public RentalController(AppDbContext dbContext)

        { _context = dbContext; }

        [HttpGet]
        public IActionResult ExportToPdf()
        {
            // QuestPDF Lisansı
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            // Verileri anonim tipe eşle
            var products = _context.Rentals
                .Select(r => new
                {
                    r.RentalId,
                    Musteri = r.Customer != null ? (r.Customer.FirstName + " " + r.Customer.LastName) : "-",
                    Arac = r.Vehicle != null ? (r.Vehicle.Brand + " " + r.Vehicle.Model + " [" + r.Vehicle.PlateNumber + "]") : "-",
                    Baslangic = r.StartDate,
                    Bitis = r.EndDate,
                    ToplamTutar = r.TotalPrice
                })
                .ToList();

            var pdfDocument = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    // Üst bilgi
                    page.Header()
                        .PaddingBottom(0.5f, Unit.Centimetre)
                        .Text("Kiralama İşlemleri Raporu")
                        .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

                    // Tablo içeriği
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);  // ID
                            columns.RelativeColumn(1.5f); // Müşteri
                            columns.RelativeColumn(2);   // Araç Bilgisi
                            columns.RelativeColumn(1);   // Başlangıç Tarihi
                            columns.RelativeColumn(1);   // Bitiş Tarihi
                            columns.ConstantColumn(70);  // Toplam Tutar
                        });

                        // Tablo başlıkları
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("ID").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Müşteri").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Araç Bilgisi").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Başlangıç").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Bitiş").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Toplam Tutar").Bold();
                        });

                        // Veri döngüsü
                        foreach (var item in products)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.RentalId.ToString());
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Musteri);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Arac);

                            // Tarih kontrolleri
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Baslangic != null ? item.Baslangic.Value.ToString("dd.MM.yyyy") : "-");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Bitis != null ? item.Bitis.Value.ToString("dd.MM.yyyy") : "-");

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text($"{item.ToplamTutar} ₺");
                        }
                    });

                    // Alt bilgi
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
            return File(pdfBytes, "application/pdf", $"Kiralama_Listesi_{DateTime.Now:yyyyMMdd}.pdf");
        }

        [HttpGet]
        public IActionResult ExportToExcel()
        {
            ExcelPackage.License.SetNonCommercialPersonal("Backend softito");

            var products = _context.Rentals
                .Select(r => new
                {
                    r.RentalId,
                    Musteri = r.Customer != null ? (r.Customer.FirstName + " " + r.Customer.LastName) : "-",
                    Arac = r.Vehicle != null ? (r.Vehicle.Brand + " " + r.Vehicle.Model + " [" + r.Vehicle.PlateNumber + "]") : "-",
                    Baslangic = r.StartDate,
                    Bitis = r.EndDate,
                    ToplamTutar = r.TotalPrice
                })
                .ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Kiralama Listesi");

                // Tablo başlıkları
                worksheet.Cells[1, 1].Value = "Kiralama ID";
                worksheet.Cells[1, 2].Value = "Müşteri Adı Soyadı";
                worksheet.Cells[1, 3].Value = "Araç Bilgisi";
                worksheet.Cells[1, 4].Value = "Başlangıç Tarihi";
                worksheet.Cells[1, 5].Value = "Bitiş Tarihi";
                worksheet.Cells[1, 6].Value = "Toplam Tutar";

                // Başlığı biçimlendir
                using (var range = worksheet.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(41, 128, 185));
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                // Verileri yaz
                int rowNumber = 2;
                foreach (var item in products)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.RentalId;
                    worksheet.Cells[rowNumber, 2].Value = item.Musteri;
                    worksheet.Cells[rowNumber, 3].Value = item.Arac;

                    // Tarih kontrolleri
                    worksheet.Cells[rowNumber, 4].Value = item.Baslangic != null ? item.Baslangic.Value.ToString("dd.MM.yyyy") : "-";
                    worksheet.Cells[rowNumber, 5].Value = item.Bitis != null ? item.Bitis.Value.ToString("dd.MM.yyyy") : "-";

                    worksheet.Cells[rowNumber, 6].Value = item.ToplamTutar;

                    rowNumber++;
                }

                // Sütun genişliklerini ayarla
                if (worksheet.Dimension != null)
                {
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                }

                var fileBytes = package.GetAsByteArray();
                string fileName = $"Kiralama_Listesi_{DateTime.Now:yyyyMMdd}.xlsx";

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        public IActionResult Index(string searchString)
        {
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
            // Müşterileri listele
            ViewBag.CustomerId = _context.Customers
                .Select(c => new { c.CustomerId, FullName = c.FirstName + " " + c.LastName })
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.CustomerId.ToString(),
                    Text = c.FullName
                }).ToList();

            // Müsait araçları listele
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
        [ValidateAntiForgeryToken] // CSRF güvenliği
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
                return NotFound(); 
            }

            // Müşterileri yükle
            ViewBag.CustomerId = _context.Customers
                .Select(c => new { c.CustomerId, FullName = c.FirstName + " " + c.LastName })
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.CustomerId.ToString(),
                    Text = c.FullName,
                    Selected = (c.CustomerId == kiralama.CustomerId) // Seçili müşteri
                }).ToList();

            // Araçları yükle
            ViewBag.VehicleId = _context.Vehicles
                .Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = v.VehicleId.ToString(),
                    Text = v.Brand + " " + v.Model + " (" + v.PlateNumber + ") - " + v.DailyPrice + " ₺",
                    Selected = (v.VehicleId == kiralama.VehicleId) 
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
            // İlişkili tabloları yükle
            var kiralama = _context.Rentals
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .FirstOrDefault(r => r.RentalId == id);

            if (kiralama == null)
            {
                return NotFound();
            }

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
