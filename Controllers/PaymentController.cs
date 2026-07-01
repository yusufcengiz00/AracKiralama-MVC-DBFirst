using CarRentalManagementSystem_DBFirst.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CarRentalManagementSystem_DBFirst.Controllers
{
    public class PaymentController : Controller
    {
        private readonly AppDbContext _context;

        public PaymentController(AppDbContext dbContext)
        {
            _context = dbContext;
        }
        [HttpGet]
        public IActionResult ExportToPdf()
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var products = _context.Payments
                .Select(p => new
                {
                    p.PaymentId,
                    Musteri = p.Rental != null && p.Rental.Customer != null ? (p.Rental.Customer.FirstName + " " + p.Rental.Customer.LastName) : "-",
                    Arac = p.Rental != null && p.Rental.Vehicle != null ? (p.Rental.Vehicle.Brand + " " + p.Rental.Vehicle.Model + " [" + p.Rental.Vehicle.PlateNumber + "]") : "-",
                    OdemeTarihi = p.PaymentDate,
                    Miktar = p.Amount,
                    OdemeYontemi = p.PaymentMethod ?? "-"
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

                    page.Header()
                        .PaddingBottom(0.5f, Unit.Centimetre)
                        .Text("Ödeme Listesi Raporu")
                        .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.ConstantColumn(70);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("ID").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Müşteri").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Araç Bilgisi").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Ödeme Tarihi").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Tutar").Bold();
                            header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Yöntem").Bold();
                        });

                        foreach (var item in products)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.PaymentId.ToString());
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Musteri);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Arac);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.OdemeTarihi != null ? item.OdemeTarihi.Value.ToString("dd.MM.yyyy") : "-");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text($"{item.Miktar} ₺");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.OdemeYontemi);
                        }
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
            return File(pdfBytes, "application/pdf", $"Odeme_Listesi_{DateTime.Now:yyyyMMdd}.pdf");
        }

        [HttpGet]
        public IActionResult ExportToExcel()
        {
            ExcelPackage.License.SetNonCommercialPersonal("Backend softito");

            var products = _context.Payments
                .Select(p => new
                {
                    p.PaymentId,
                    Musteri = p.Rental != null && p.Rental.Customer != null ? (p.Rental.Customer.FirstName + " " + p.Rental.Customer.LastName) : "-",
                    Arac = p.Rental != null && p.Rental.Vehicle != null ? (p.Rental.Vehicle.Brand + " " + p.Rental.Vehicle.Model + " [" + p.Rental.Vehicle.PlateNumber + "]") : "-",
                    OdemeTarihi = p.PaymentDate,
                    Miktar = p.Amount,
                    OdemeYontemi = p.PaymentMethod ?? "-"
                })
                .ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Ödeme Listesi");

                worksheet.Cells[1, 1].Value = "Ödeme ID";
                worksheet.Cells[1, 2].Value = "Müşteri Adı Soyadı";
                worksheet.Cells[1, 3].Value = "Araç Bilgisi";
                worksheet.Cells[1, 4].Value = "Ödeme Tarihi";
                worksheet.Cells[1, 5].Value = "Ödeme Tutarı";
                worksheet.Cells[1, 6].Value = "Ödeme Yöntemi";

                using (var range = worksheet.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(41, 128, 185));
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                int rowNumber = 2;
                foreach (var item in products)
                {
                    worksheet.Cells[rowNumber, 1].Value = item.PaymentId;
                    worksheet.Cells[rowNumber, 2].Value = item.Musteri;
                    worksheet.Cells[rowNumber, 3].Value = item.Arac;
                    worksheet.Cells[rowNumber, 4].Value = item.OdemeTarihi != null ? item.OdemeTarihi.Value.ToString("dd.MM.yyyy") : "-";
                    worksheet.Cells[rowNumber, 5].Value = item.Miktar;
                    worksheet.Cells[rowNumber, 6].Value = item.OdemeYontemi;

                    rowNumber++;
                }

                if (worksheet.Dimension != null)
                {
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                }

                var fileBytes = package.GetAsByteArray();
                string fileName = $"Odeme_Listesi_{DateTime.Now:yyyyMMdd}.xlsx";

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
        public IActionResult Index(string searchString)
        {
            // Ödemeleri ve ilişkileri yükle
            var odemeler = _context.Payments
                .Include(p => p.Rental)
                    .ThenInclude(r => r.Customer)
                .Include(p => p.Rental)
                    .ThenInclude(r => r.Vehicle)
                .AsQueryable();

            // Arama filtresi
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
            // Aktif kiralamaları listele
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

            // Hata durumunda listeyi yeniden doldur
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
            // Silme detayı ve ilişkiler
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