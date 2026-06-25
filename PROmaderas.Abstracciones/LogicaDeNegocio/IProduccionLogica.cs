using PROmaderas.Abstracciones.Models;

namespace PROmaderas.Abstracciones.LogicaDeNegocio
{
    /// <summary>
    /// Contrato de lógica de negocio para la producción semanal de tarimas.
    /// INV-HU-001.
    /// </summary>
    public interface IProduccionLogica
    {
        /// <summary>
        /// Registra la producción semanal: valida el DTO, crea el movimiento de entrada
        /// y actualiza el stock (a través de InventarioMovimiento).
        /// </summary>
        /// <param name="dto">Datos del formulario (tipo, cantidad, fecha).</param>
        /// <param name="idUsuarioRegistro">Id del operador autenticado.</param>
        /// <returns>El movimiento de inventario persistido.</returns>
        Task<InventarioMovimientoAD> RegistrarProduccion(ProduccionSemanalDTO dto, int idUsuarioRegistro);

        /// <summary>Historial de producciones registradas (para la vista Index).</summary>
        Task<(List<InventarioMovimientoAD> movimientos, int totalRegistros)> ObtenerHistorial(
            int pagina, int registrosPorPagina);

        /// <summary>Resuelve el Id entero del usuario por su correo (Identity).</summary>
        Task<int?> ObtenerIdUsuarioPorCorreo(string correo);
    }
}