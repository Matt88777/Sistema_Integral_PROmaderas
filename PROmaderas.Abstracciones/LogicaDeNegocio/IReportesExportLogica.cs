namespace PROmaderas.Abstracciones.LogicaDeNegocio
{
    public interface IReportesExportLogica
    {
        Task<byte[]> GenerarFacturasExcel();
        Task<byte[]> GenerarFacturasPdf();
        Task<byte[]> GenerarInventarioExcel();
        Task<byte[]> GenerarInventarioPdf();
        Task<byte[]> GenerarPlanillaExcel();
        Task<byte[]> GenerarPlanillaPdf();
        Task<byte[]> GenerarVentasExcel(string tipoPeriodo, DateTime fechaInicio, DateTime fechaFin);
        Task<byte[]> GenerarVentasPdf(string tipoPeriodo, DateTime fechaInicio, DateTime fechaFin);
    }
}