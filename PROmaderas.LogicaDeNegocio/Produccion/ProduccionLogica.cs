using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Catalogos;
using PROmaderas.Abstracciones.LogicaDeNegocio;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.LogicaDeNegocio.Produccion
{
    /// <summary>
    /// Lógica de negocio para el registro de producción semanal de tarimas.
    /// INV-HU-001 — Escenarios 1 (registro exitoso), 2 (campos obligatorios) y 3 (cantidad inválida).
    /// </summary>
    public class ProduccionLogica : IProduccionLogica
    {
        private readonly IProduccionRepositorio _repositorio;

        public ProduccionLogica(IProduccionRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Escenario 2: lanza ArgumentException si falta algún campo obligatorio.
        /// Escenario 3: lanza ArgumentException si la cantidad es menor o igual a 0.
        /// Escenario 1: persiste el movimiento y devuelve el registro creado.
        /// </remarks>
        public async Task<InventarioMovimientoAD> RegistrarProduccion(
            ProduccionSemanalDTO dto, int idUsuarioRegistro)
        {
            // — Escenario 2: validaciones de campos obligatorios —
            if (dto.IdTipoTarima <= 0)
                throw new ArgumentException("Debe seleccionar un tipo de tarima válido.");

            if (dto.FechaProduccion == default)
                throw new ArgumentException("La fecha de producción es requerida.");

            if (dto.FechaProduccion > DateTime.Today)
                throw new ArgumentException("La fecha de producción no puede ser futura.");

            // — Escenario 3: validación de cantidad —
            if (dto.Cantidad <= 0)
                throw new ArgumentException("La cantidad producida debe ser un número entero mayor a 0.");

            // Verificar que el tipo de tarima exista y esté activo
            var tipoExiste = await _repositorio.TipoTarimaExiste(dto.IdTipoTarima);
            if (!tipoExiste)
                throw new ArgumentException("El tipo de tarima seleccionado no existe o está inactivo.");

            // — Escenario 1: construir el movimiento de inventario —
            var movimiento = new InventarioMovimientoAD
            {
                IdTipoTarima = dto.IdTipoTarima,
                IdUsuarioRegistro = idUsuarioRegistro,
                TipoMovimiento = TiposMovimientoInventario.Entrada,
                Cantidad = dto.Cantidad,
                FechaMovimiento = dto.FechaProduccion,
                Motivo = string.IsNullOrWhiteSpace(dto.Motivo)
                    ? "Producción semanal"
                    : dto.Motivo.Trim()
            };

            return await _repositorio.RegistrarProduccion(movimiento);
        }

        /// <inheritdoc/>
        public async Task<(List<InventarioMovimientoAD> movimientos, int totalRegistros)> ObtenerHistorial(
            int pagina, int registrosPorPagina)
        {
            if (pagina < 1) pagina = 1;
            if (registrosPorPagina < 1) registrosPorPagina = 10;

            var movimientos = await _repositorio.ObtenerHistorialProduccion(pagina, registrosPorPagina);
            var totalRegistros = await _repositorio.ContarMovimientosProduccion();

            return (movimientos, totalRegistros);
        }

        /// <inheritdoc/>
        public async Task<int?> ObtenerIdUsuarioPorCorreo(string correo)
        {
            return await _repositorio.ObtenerIdUsuarioPorCorreo(correo);
        }
    }
}