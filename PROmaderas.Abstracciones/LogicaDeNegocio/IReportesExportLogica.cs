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
    }
}