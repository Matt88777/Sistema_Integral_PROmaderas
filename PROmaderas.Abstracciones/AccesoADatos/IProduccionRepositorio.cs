using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.AccesoADatos
{
    /// <summary>
    /// Contrato de acceso a datos para la producción semanal de tarimas.
    /// INV-HU-001.
    /// </summary>
    public interface IProduccionRepositorio
    {
        /// <summary>Inserta un movimiento de tipo Entrada en InventarioMovimiento.</summary>
        Task<InventarioMovimientoAD> RegistrarProduccion(InventarioMovimientoAD movimiento);

        /// <summary>Lista los movimientos de entrada (producción) más recientes, para el historial.</summary>
        Task<List<InventarioMovimientoAD>> ObtenerHistorialProduccion(int pagina, int registrosPorPagina);

        /// <summary>Devuelve el total de registros del historial.</summary>
        Task<int> ContarMovimientosProduccion();

        /// <summary>Verifica que el tipo de tarima exista y esté activo.</summary>
        Task<bool> TipoTarimaExiste(int idTipoTarima);

        /// <summary>Devuelve el Id entero del usuario cuyo correo coincida (tabla Usuario).</summary>
        Task<int?> ObtenerIdUsuarioPorCorreo(string correo);
    }
}
