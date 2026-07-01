using CarRentalManagementSystem_DBFirst.Models;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Drawing;

namespace CarRentalManagementSystem_DBFirst.Controllers
{
    public class VehicleController : Controller
    {

        private readonly AppDbContext _context;

        public VehicleController(AppDbContext dbContext)

        { _context = dbContext; }

        [HttpGet]
        public IActionResult ExportToPdf()
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var products = _context.Vehicles
                .Select(v => new
                {
                    v.VehicleId,
                    v.Brand,
                    v.Model,
                    v.PlateNumber
                })
                .ToList();

            var pdfDocument = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header()
                        .Text("Araç Listesi Raporu")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingTop(1, Unit.Centimetre)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);
                                columns.RelativeColumn();
                                columns.ConstantColumn(100);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("ID").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Araç Bilgisi").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Plaka").Bold();
                            });

                            foreach (var item in products)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.VehicleId.ToString());
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text($"{(item.Brand ?? "")} {(item.Model ?? "")}");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.PlateNumber ?? "-");
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
            return File(pdfBytes, "application/pdf", $"Arac_Listesi_{DateTime.Now:yyyyMMdd}.pdf");
        }

        [HttpGet]
        public IActionResult ExportToExcel()
        {
            ExcelPackage.License.SetNonCommercialPersonal("Backend softito");

            var products = _context.Vehicles
                .Select(v => new
                {
                    v.VehicleId,
                    v.Brand,
                    v.Model,
                    v.PlateNumber
                })
                .ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Araç Listesi");

                worksheet.Cells[1, 1].Value = "Araç ID";
                worksheet.Cells[1, 2].Value = "Marka / Model";
                worksheet.Cells[1, 3].Value = "Plaka Numarası";

                using (var range = worksheet.Cells[1, 1, 1, 3])
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
                    worksheet.Cells[rowNumber, 1].Value = item.VehicleId;
                    worksheet.Cells[rowNumber, 2].Value = $"{(item.Brand ?? "")} {(item.Model ?? "")}";
                    worksheet.Cells[rowNumber, 3].Value = item.PlateNumber;

                    rowNumber++;
                }

                if (worksheet.Dimension != null)
                {
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                }

                var fileBytes = package.GetAsByteArray();
                string fileName = $"Arac_Listesi_{DateTime.Now:yyyyMMdd}.xlsx";

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        public IActionResult Index(string searchString)
        {
            var araclar = from u in _context.Vehicles
                          select u;

            if (!String.IsNullOrEmpty(searchString))
            {
                araclar = araclar.Where(s => s.PlateNumber.Contains(searchString));
            }

            ViewBag.CurrentFilter = searchString;

            return View(araclar.ToList());
        }
        [HttpGet]
        public IActionResult Create()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF güvenliği
        public IActionResult Create(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var arac = _context.Vehicles.Find(id);
            if (arac == null)
            {
                return NotFound();
            }
            return View(arac);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Vehicle vehicle)
        {
            _context.Vehicles.Update(vehicle);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var arac = _context.Vehicles.Find(id);
            if (arac == null)
            {
                return NotFound();
            }
            return View(arac);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var gercekArac = _context.Vehicles.Find(id);

            if (gercekArac != null)
            {
                _context.Vehicles.Remove(gercekArac);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

    }
}
