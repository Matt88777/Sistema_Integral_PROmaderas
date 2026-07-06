using ClosedXML.Excel;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using PdfDocument = QuestPDF.Fluent.Document;
using PdfContainer = QuestPDF.Infrastructure.IContainer;

namespace PROmaderas.LogicaDeNegocio.Reportes
{
    public class ReportesExportLogica : IReportesExportLogica
    {
        private readonly IReportesRepositorio _reportesRepositorio;

        public ReportesExportLogica(IReportesRepositorio reportesRepositorio)
        {
            _reportesRepositorio = reportesRepositorio;
        }

        private static PdfContainer CellStyle(PdfContainer c) =>
            c.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5);

        public async Task<byte[]> GenerarFacturasExcel()
        {
            var facturas = await _reportesRepositorio.ObtenerFacturas();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Facturacion");
            ws.Cell(1, 1).Value = "N° Factura"; ws.Cell(1, 2).Value = "Cliente";
            ws.Cell(1, 3).Value = "Fecha"; ws.Cell(1, 4).Value = "Subtotal";
            ws.Cell(1, 5).Value = "Impuestos"; ws.Cell(1, 6).Value = "Total";
            ws.Cell(1, 7).Value = "Estado"; ws.Cell(1, 8).Value = "Saldo Pendiente";

            int row = 2;
            foreach (var f in facturas)
            {
                ws.Cell(row, 1).Value = f.NumeroFactura;
                ws.Cell(row, 2).Value = f.Cliente?.Nombre ?? "";
                ws.Cell(row, 3).Value = f.Fecha.ToString("dd/MM/yyyy");
                ws.Cell(row, 4).Value = f.Subtotal;
                ws.Cell(row, 5).Value = f.Impuestos;
                ws.Cell(row, 6).Value = f.Total;
                ws.Cell(row, 7).Value = f.Estado;
                ws.Cell(row, 8).Value = f.SaldoPendiente;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> GenerarFacturasPdf()
        {
            var facturas = await _reportesRepositorio.ObtenerFacturas();

            var document = PdfDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));
                    page.Header().Text("Reporte de Facturación").FontSize(18).Bold();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(3);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                        });
                        table.Header(h =>
                        {
                            h.Cell().Element(CellStyle).Text("N° Factura").Bold();
                            h.Cell().Element(CellStyle).Text("Cliente").Bold();
                            h.Cell().Element(CellStyle).Text("Fecha").Bold();
                            h.Cell().Element(CellStyle).Text("Total").Bold();
                            h.Cell().Element(CellStyle).Text("Estado").Bold();
                            h.Cell().Element(CellStyle).Text("Saldo").Bold();
                        });
                        foreach (var f in facturas)
                        {
                            table.Cell().Element(CellStyle).Text(f.NumeroFactura);
                            table.Cell().Element(CellStyle).Text(f.Cliente?.Nombre ?? "");
                            table.Cell().Element(CellStyle).Text(f.Fecha.ToString("dd/MM/yyyy"));
                            table.Cell().Element(CellStyle).Text($"₡{f.Total:N2}");
                            table.Cell().Element(CellStyle).Text(f.Estado);
                            table.Cell().Element(CellStyle).Text($"₡{f.SaldoPendiente:N2}");
                        }
                    });

                    page.Footer().AlignCenter().Text($"PROmaderas — {DateTime.Now:dd/MM/yyyy HH:mm}");
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerarInventarioExcel()
        {
            var productos = await _reportesRepositorio.ObtenerProductos();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Inventario");
            ws.Cell(1, 1).Value = "Código"; ws.Cell(1, 2).Value = "Nombre";
            ws.Cell(1, 3).Value = "Categoría ID"; ws.Cell(1, 4).Value = "Stock";
            ws.Cell(1, 5).Value = "Stock Mínimo"; ws.Cell(1, 6).Value = "Precio";

            int row = 2;
            foreach (var p in productos)
            {
                ws.Cell(row, 1).Value = p.Codigo;
                ws.Cell(row, 2).Value = p.Nombre;
                ws.Cell(row, 3).Value = p.CategoriaId;
                ws.Cell(row, 4).Value = p.Stock;
                ws.Cell(row, 5).Value = p.StockMinimo;
                ws.Cell(row, 6).Value = p.Precio;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> GenerarInventarioPdf()
        {
            var productos = await _reportesRepositorio.ObtenerProductos();

            var document = PdfDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));
                    page.Header().Text("Reporte de Inventario").FontSize(18).Bold();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(3);
                            c.RelativeColumn(2);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                        });
                        table.Header(h =>
                        {
                            h.Cell().Element(CellStyle).Text("Código").Bold();
                            h.Cell().Element(CellStyle).Text("Nombre").Bold();
                            h.Cell().Element(CellStyle).Text("Cat. ID").Bold();
                            h.Cell().Element(CellStyle).Text("Stock").Bold();
                            h.Cell().Element(CellStyle).Text("Mín.").Bold();
                            h.Cell().Element(CellStyle).Text("Precio").Bold();
                        });
                        foreach (var p in productos)
                        {
                            table.Cell().Element(CellStyle).Text(p.Codigo);
                            table.Cell().Element(CellStyle).Text(p.Nombre);
                            table.Cell().Element(CellStyle).Text(p.CategoriaId.ToString());
                            table.Cell().Element(CellStyle).Text(p.Stock.ToString());
                            table.Cell().Element(CellStyle).Text(p.StockMinimo.ToString());
                            table.Cell().Element(CellStyle).Text($"₡{p.Precio:N2}");
                        }
                    });

                    page.Footer().AlignCenter().Text($"PROmaderas — {DateTime.Now:dd/MM/yyyy HH:mm}");
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerarPlanillaExcel()
        {
            var detalles = await _reportesRepositorio.ObtenerPlanillaDetalles();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Planilla");
            ws.Cell(1, 1).Value = "Empleado"; ws.Cell(1, 2).Value = "Período";
            ws.Cell(1, 3).Value = "Salario Base"; ws.Cell(1, 4).Value = "Horas Extra";
            ws.Cell(1, 5).Value = "Salario Bruto"; ws.Cell(1, 6).Value = "Deducciones";
            ws.Cell(1, 7).Value = "Salario Neto";

            int row = 2;
            foreach (var d in detalles)
            {
                ws.Cell(row, 1).Value = d.Empleado?.Nombre ?? "";
                ws.Cell(row, 2).Value = $"{d.Periodo?.FechaInicio:dd/MM/yyyy} - {d.Periodo?.FechaFin:dd/MM/yyyy}";
                ws.Cell(row, 3).Value = d.SalarioBase;
                ws.Cell(row, 4).Value = d.MontoHorasExtra;
                ws.Cell(row, 5).Value = d.SalarioBruto;
                ws.Cell(row, 6).Value = d.TotalDeducciones;
                ws.Cell(row, 7).Value = d.SalarioNeto;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> GenerarPlanillaPdf()
        {
            var detalles = await _reportesRepositorio.ObtenerPlanillaDetalles();

            var document = PdfDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(9));
                    page.Header().Text("Reporte de Planilla").FontSize(18).Bold();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);
                            c.RelativeColumn(3);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                        });
                        table.Header(h =>
                        {
                            h.Cell().Element(CellStyle).Text("Empleado").Bold();
                            h.Cell().Element(CellStyle).Text("Período").Bold();
                            h.Cell().Element(CellStyle).Text("Bruto").Bold();
                            h.Cell().Element(CellStyle).Text("Deducciones").Bold();
                            h.Cell().Element(CellStyle).Text("Neto").Bold();
                        });
                        foreach (var d in detalles)
                        {
                            table.Cell().Element(CellStyle).Text(d.Empleado?.Nombre ?? "");
                            table.Cell().Element(CellStyle).Text($"{d.Periodo?.FechaInicio:dd/MM/yyyy} - {d.Periodo?.FechaFin:dd/MM/yyyy}");
                            table.Cell().Element(CellStyle).Text($"₡{d.SalarioBruto:N2}");
                            table.Cell().Element(CellStyle).Text($"₡{d.TotalDeducciones:N2}");
                            table.Cell().Element(CellStyle).Text($"₡{d.SalarioNeto:N2}");
                        }
                    });

                    page.Footer().AlignCenter().Text($"PROmaderas — {DateTime.Now:dd/MM/yyyy HH:mm}");
                });
            });

            return document.GeneratePdf();
        }
    }
}