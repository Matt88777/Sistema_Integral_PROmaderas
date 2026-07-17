using ClosedXML.Excel;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Catalogos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using PdfContainer = QuestPDF.Infrastructure.IContainer;
using PdfDocument = QuestPDF.Fluent.Document;

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
            var movimientos = await _reportesRepositorio.ObtenerMovimientosInventario();

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

        private static bool EsBajoStock(InventarioExistenciaDTO e) => e.StockActual <= e.StockMinimo;

        private static string EstadoStock(InventarioExistenciaDTO e)
        {
            if (e.StockActual <= 0) return "Sin inventario";
            if (e.StockActual <= e.StockMinimo) return "Bajo mínimo";
            return "Disponible";
        }

        public async Task<byte[]> GenerarInventarioExcelAdmin()
        {
            var existencias = await _reportesRepositorio.ObtenerExistenciasInventario();
            var movimientos = await _reportesRepositorio.ObtenerMovimientosInventario();

            using var workbook = new XLWorkbook();

            var wsExistencias = workbook.Worksheets.Add("Existencias");
            wsExistencias.Cell(1, 1).Value = "Código"; wsExistencias.Cell(1, 2).Value = "Tipo de Tarima";
            wsExistencias.Cell(1, 3).Value = "Medida"; wsExistencias.Cell(1, 4).Value = "Entradas";
            wsExistencias.Cell(1, 5).Value = "Salidas"; wsExistencias.Cell(1, 6).Value = "Stock Actual";
            wsExistencias.Cell(1, 7).Value = "Stock Mínimo"; wsExistencias.Cell(1, 8).Value = "Estado";
            wsExistencias.Range(1, 1, 1, 8).Style.Font.SetBold();

            int row = 2;
            foreach (var e in existencias)
            {
                var bajoStock = EsBajoStock(e);

                wsExistencias.Cell(row, 1).Value = e.Codigo;
                wsExistencias.Cell(row, 2).Value = e.TipoTarima;
                wsExistencias.Cell(row, 3).Value = e.Medida;
                wsExistencias.Cell(row, 4).Value = e.Entradas;
                wsExistencias.Cell(row, 5).Value = e.Salidas;
                wsExistencias.Cell(row, 6).Value = e.StockActual;
                wsExistencias.Cell(row, 7).Value = e.StockMinimo;
                wsExistencias.Cell(row, 8).Value = EstadoStock(e);

                if (bajoStock)
                {
                    var rango = wsExistencias.Range(row, 1, row, 8);
                    rango.Style.Fill.BackgroundColor = XLColor.FromArgb(253, 216, 216);
                    rango.Style.Font.FontColor = XLColor.FromArgb(139, 0, 0);
                    wsExistencias.Cell(row, 8).Style.Font.SetBold();
                }
                row++;
            }
            wsExistencias.Columns().AdjustToContents();

            var totalBajoStock = existencias.Count(EsBajoStock);
            wsExistencias.Cell(row + 1, 1).Value = $"Productos con bajo stock: {totalBajoStock} de {existencias.Count}";
            wsExistencias.Cell(row + 1, 1).Style.Font.SetBold();

            var wsMovimientos = workbook.Worksheets.Add("Movimientos");
            wsMovimientos.Cell(1, 1).Value = "Fecha"; wsMovimientos.Cell(1, 2).Value = "Código";
            wsMovimientos.Cell(1, 3).Value = "Tipo de Tarima"; wsMovimientos.Cell(1, 4).Value = "Movimiento";
            wsMovimientos.Cell(1, 5).Value = "Cantidad"; wsMovimientos.Cell(1, 6).Value = "Saldo";
            wsMovimientos.Cell(1, 7).Value = "Motivo";
            wsMovimientos.Range(1, 1, 1, 7).Style.Font.SetBold();

            int rowMov = 2;
            foreach (var m in movimientos)
            {
                wsMovimientos.Cell(rowMov, 1).Value = m.FechaMovimiento.ToString("dd/MM/yyyy HH:mm");
                wsMovimientos.Cell(rowMov, 2).Value = m.Codigo;
                wsMovimientos.Cell(rowMov, 3).Value = m.TipoTarima;
                wsMovimientos.Cell(rowMov, 4).Value = m.TipoMovimiento;
                wsMovimientos.Cell(rowMov, 5).Value = m.Cantidad;
                wsMovimientos.Cell(rowMov, 6).Value = m.Saldo;
                wsMovimientos.Cell(rowMov, 7).Value = m.Motivo ?? "";
                rowMov++;
            }
            wsMovimientos.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> GenerarInventarioPdfAdmin()
        {
            var existencias = await _reportesRepositorio.ObtenerExistenciasInventario();
            var movimientos = await _reportesRepositorio.ObtenerMovimientosInventario();
            var totalBajoStock = existencias.Count(EsBajoStock);

            var document = PdfDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));
                    page.Header().Column(col =>
                    {
                        col.Item().Text("Reporte de Inventario — Existencias Actuales").FontSize(18).Bold();
                        col.Item().Text($"Productos con bajo stock: {totalBajoStock} de {existencias.Count}")
                            .FontSize(10).FontColor(Colors.Red.Darken2).Bold();
                    });

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(3);
                            c.RelativeColumn(2);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                        });
                        table.Header(h =>
                        {
                            h.Cell().Element(CellStyle).Text("Código").Bold();
                            h.Cell().Element(CellStyle).Text("Tipo de Tarima").Bold();
                            h.Cell().Element(CellStyle).Text("Medida").Bold();
                            h.Cell().Element(CellStyle).Text("Entradas").Bold();
                            h.Cell().Element(CellStyle).Text("Salidas").Bold();
                            h.Cell().Element(CellStyle).Text("Stock").Bold();
                            h.Cell().Element(CellStyle).Text("Mín.").Bold();
                            h.Cell().Element(CellStyle).Text("Estado").Bold();
                        });
                        foreach (var e in existencias)
                        {
                            var bajoStock = EsBajoStock(e);
                            var sinInventario = e.StockActual <= 0;
                            var colorFondo = sinInventario ? Colors.Red.Lighten4
                                : bajoStock ? Colors.Orange.Lighten4
                                : Colors.White;
                            PdfContainer EstiloFila(PdfContainer c) => CellStyle(c).Background(colorFondo);

                            table.Cell().Element(EstiloFila).Text(e.Codigo);
                            table.Cell().Element(EstiloFila).Text(e.TipoTarima);
                            table.Cell().Element(EstiloFila).Text(e.Medida);
                            table.Cell().Element(EstiloFila).Text(e.Entradas.ToString());
                            table.Cell().Element(EstiloFila).Text(e.Salidas.ToString());
                            table.Cell().Element(EstiloFila).Text(text =>
                            {
                                if (bajoStock)
                                    text.Span(e.StockActual.ToString()).Bold().FontColor(Colors.Red.Darken2);
                                else
                                    text.Span(e.StockActual.ToString());
                            });
                            table.Cell().Element(EstiloFila).Text(e.StockMinimo.ToString());
                            table.Cell().Element(EstiloFila).Text(EstadoStock(e));
                        }
                    });

                    page.Footer().AlignCenter().Text($"PROmaderas — {DateTime.Now:dd/MM/yyyy HH:mm}");
                });

                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(9));
                    page.Header().Text("Reporte de Inventario — Movimientos (Entradas y Salidas)").FontSize(16).Bold();

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                            c.RelativeColumn(3);
                            c.RelativeColumn(2);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1);
                        });
                        table.Header(h =>
                        {
                            h.Cell().Element(CellStyle).Text("Fecha").Bold();
                            h.Cell().Element(CellStyle).Text("Código").Bold();
                            h.Cell().Element(CellStyle).Text("Tipo de Tarima").Bold();
                            h.Cell().Element(CellStyle).Text("Movimiento").Bold();
                            h.Cell().Element(CellStyle).Text("Cant.").Bold();
                            h.Cell().Element(CellStyle).Text("Saldo").Bold();
                        });
                        foreach (var m in movimientos)
                        {
                            var esEntrada = TiposMovimientoInventario.TiposEntrada.Contains(m.TipoMovimiento);
                            PdfContainer EstiloFila(PdfContainer c) => CellStyle(c)
                                .Background(esEntrada ? Colors.Green.Lighten4 : Colors.Orange.Lighten4);

                            table.Cell().Element(EstiloFila).Text(m.FechaMovimiento.ToString("dd/MM/yyyy HH:mm"));
                            table.Cell().Element(EstiloFila).Text(m.Codigo);
                            table.Cell().Element(EstiloFila).Text(m.TipoTarima);
                            table.Cell().Element(EstiloFila).Text(m.TipoMovimiento);
                            table.Cell().Element(EstiloFila).Text(m.Cantidad.ToString());
                            table.Cell().Element(EstiloFila).Text(m.Saldo.ToString());
                        }
                    });

                    page.Footer().AlignCenter().Text($"PROmaderas — {DateTime.Now:dd/MM/yyyy HH:mm}");
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerarPlanillaExcelAdmin()
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

        public async Task<byte[]> GenerarPlanillaPdfAdmin()
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

        public async Task<byte[]> GenerarVentasExcel(string tipoPeriodo, DateTime fechaInicio, DateTime fechaFin)
        {
            var ventas = await _reportesRepositorio.ObtenerVentasPorPeriodo(tipoPeriodo, fechaInicio, fechaFin);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Ventas");
            ws.Cell(1, 1).Value = "Período";
            ws.Cell(1, 2).Value = "Pedidos";
            ws.Cell(1, 3).Value = "Monto Total";
            ws.Cell(1, 4).Value = "Fact. Emitidas";
            ws.Cell(1, 5).Value = "Fact. Pagadas";
            ws.Cell(1, 6).Value = "Fact. Pendientes";
            ws.Cell(1, 7).Value = "Fact. Anuladas";
            ws.Cell(1, 8).Value = "Sin Facturar";
            ws.Range(1, 1, 1, 8).Style.Font.SetBold();

            int row = 2;
            foreach (var v in ventas)
            {
                ws.Cell(row, 1).Value = v.Periodo;
                ws.Cell(row, 2).Value = v.CantidadPedidos;
                ws.Cell(row, 3).Value = v.MontoTotal;
                ws.Cell(row, 4).Value = v.FacturasEmitidas;
                ws.Cell(row, 5).Value = v.FacturasPagadas;
                ws.Cell(row, 6).Value = v.FacturasPendientes;
                ws.Cell(row, 7).Value = v.FacturasAnuladas;
                ws.Cell(row, 8).Value = v.PedidosSinFacturar;
                row++;
            }
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> GenerarVentasPdf(string tipoPeriodo, DateTime fechaInicio, DateTime fechaFin)
        {
            var ventas = await _reportesRepositorio.ObtenerVentasPorPeriodo(tipoPeriodo, fechaInicio, fechaFin);

            var document = PdfDocument.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4.Landscape());
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(9));
                    page.Header().Text($"Reporte de Ventas — {tipoPeriodo}").FontSize(16).Bold();

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3); c.RelativeColumn(1); c.RelativeColumn(2);
                            c.RelativeColumn(1); c.RelativeColumn(1); c.RelativeColumn(1);
                            c.RelativeColumn(1); c.RelativeColumn(1);
                        });
                        table.Header(h =>
                        {
                            h.Cell().Element(CellStyle).Text("Período").Bold();
                            h.Cell().Element(CellStyle).Text("Pedidos").Bold();
                            h.Cell().Element(CellStyle).Text("Monto Total").Bold();
                            h.Cell().Element(CellStyle).Text("Emitidas").Bold();
                            h.Cell().Element(CellStyle).Text("Pagadas").Bold();
                            h.Cell().Element(CellStyle).Text("Pendientes").Bold();
                            h.Cell().Element(CellStyle).Text("Anuladas").Bold();
                            h.Cell().Element(CellStyle).Text("Sin Fact.").Bold();
                        });
                        foreach (var v in ventas)
                        {
                            table.Cell().Element(CellStyle).Text(v.Periodo);
                            table.Cell().Element(CellStyle).Text(v.CantidadPedidos.ToString());
                            table.Cell().Element(CellStyle).Text($"₡{v.MontoTotal:N2}");
                            table.Cell().Element(CellStyle).Text(v.FacturasEmitidas.ToString());
                            table.Cell().Element(CellStyle).Text(v.FacturasPagadas.ToString());
                            table.Cell().Element(CellStyle).Text(v.FacturasPendientes.ToString());
                            table.Cell().Element(CellStyle).Text(v.FacturasAnuladas.ToString());
                            table.Cell().Element(CellStyle).Text(v.PedidosSinFacturar.ToString());
                        }
                    });

                    page.Footer().AlignCenter().Text($"PROmaderas — {DateTime.Now:dd/MM/yyyy HH:mm}");
                });
            });

            return document.GeneratePdf();
        }
    }
}