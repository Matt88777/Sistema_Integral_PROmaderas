using System.Linq;

namespace PROmaderas.Abstracciones.Models
{
    // REP-HU-002: resultado combinado del reporte de inventario.
    // Escenario 1: Existencias. Escenario 2: Movimientos. Escenario 3: bajo stock (TotalBajoStock).
    public class ReporteInventarioResultadoDTO
    {
        public List<InventarioExistenciaDTO> Existencias { get; set; } = new();
        public List<InventarioMovimientoDTO> Movimientos { get; set; } = new();

        // Escenario 1: hay productos registrados con existencias.
        public bool HayExistencias => Existencias.Any();

        // Escenario 2: hay entradas y/o salidas registradas.
        public bool HayMovimientos => Movimientos.Any();

        // Escenario 3: cantidad de productos críticos (stock actual <= stock mínimo).
        public int TotalBajoStock => Existencias.Count(e => e.StockActual <= e.StockMinimo);
        public bool HayBajoStock => TotalBajoStock > 0;

        public int TotalProductos => Existencias.Count;
    }
}