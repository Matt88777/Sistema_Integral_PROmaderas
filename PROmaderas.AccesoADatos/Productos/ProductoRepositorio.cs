using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.AccesoADatos;
using PROmaderas.Abstracciones.Catalogos;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.AccesoADatos.Productos
{
    // Sprint 0 PROMADERAS: ProductoAD se mapea a la tabla TipoTarima de la BD
    // nueva. Las propiedades CategoriaId, Stock, ImpuestoPorc, ImagenUrl y la
    // navegación Categoria son [Ignore] en EF Core, así que no se usan
    // .Include(p => p.Categoria) ni filtros por CategoriaId aquí.
    public class ProductoRepositorio : IProductoRepositorio
    {
        private readonly Contexto _contexto;
        private readonly ICategoriaRepositorio _categoriaRepositorio;

        public ProductoRepositorio(Contexto contexto, ICategoriaRepositorio categoriaRepositorio)
        {
            _contexto = contexto;
            _categoriaRepositorio = categoriaRepositorio;
        }

        public async Task<List<ProductoAD>> ObtenerTodos()
        {
            var productos = await _contexto.Productos
                .Where(p => p.Activo)
                .ToListAsync();

            await AplicarStockActualAsync(productos);

            return productos;
        }

        public async Task<ProductoAD?> ObtenerPorId(int id)
        {
            var producto = await _contexto.Productos
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto != null)
            {
                producto.Stock = await CalcularStockActualAsync(producto.Id);
            }

            return producto;
        }

        public Task<CategoriaAD?> ObtenerCategoriaPorId(int id)
        {
            return _categoriaRepositorio.ObtenerPorId(id);
        }

        public Task<List<CategoriaAD>> ObtenerCategorias()
        {
            return _categoriaRepositorio.ObtenerTodas();
        }

        public async Task<ProductoAD> Crear(ProductoAD producto)
        {
            _contexto.Productos.Add(producto);
            await _contexto.SaveChangesAsync();
            return producto;
        }

        public async Task<ProductoAD> Actualizar(ProductoAD producto)
        {
            _contexto.Productos.Update(producto);
            await _contexto.SaveChangesAsync();
            return producto;
        }

        public async Task<bool> ExisteDuplicado(string codigo, string nombre)
        {
            codigo = codigo.Trim().ToLower();
            nombre = nombre.Trim().ToLower();

            return await _contexto.Productos
                .AnyAsync(p =>
                    p.Codigo.ToLower() == codigo ||
                    p.Nombre.ToLower() == nombre);
        }

        public async Task<bool> Eliminar(int id)
        {
            var producto = await ObtenerPorId(id);
            if (producto == null) return false;

            producto.Activo = false;
            await Actualizar(producto);
            return true;
        }

        public async Task<bool> Existe(int id)
        {
            return await _contexto.Productos.AnyAsync(p => p.Id == id);
        }

        public async Task<List<ProductoAD>> BuscarPorNombre(string nombre)
        {
            var productos = await _contexto.Productos
                .Where(p => p.Activo && p.Nombre.Contains(nombre))
                .ToListAsync();

            await AplicarStockActualAsync(productos);

            return productos;
        }

        public async Task<List<ProductoAD>> FiltrarPorCategoria(int? categoriaId)
        {
            _ = categoriaId;

            var productos = await _contexto.Productos
                .Where(p => p.Activo)
                .ToListAsync();

            await AplicarStockActualAsync(productos);

            return productos;
        }

        private async Task<int> CalcularStockActualAsync(int idTipoTarima)
        {
            var tiposEntrada = TiposMovimientoInventario.TiposEntrada.ToList();
            var tiposSalida = TiposMovimientoInventario.TiposSalida.ToList();

            var entradas = await _contexto.InventarioMovimientos
                .Where(m => m.IdTipoTarima == idTipoTarima && tiposEntrada.Contains(m.TipoMovimiento))
                .SumAsync(m => (int?)m.Cantidad) ?? 0;

            var salidas = await _contexto.InventarioMovimientos
                .Where(m => m.IdTipoTarima == idTipoTarima && tiposSalida.Contains(m.TipoMovimiento))
                .SumAsync(m => (int?)m.Cantidad) ?? 0;

            return entradas - salidas;
        }

        private async Task AplicarStockActualAsync(List<ProductoAD> productos)
        {
            foreach (var producto in productos)
            {
                producto.Stock = await CalcularStockActualAsync(producto.Id);
            }
        }

        public async Task<(List<ProductoAD> productos, int totalRegistros)> ObtenerPaginado(
            int pagina,
            int registrosPorPagina,
            string? filtroNombre,
            int? categoriaId)
        {
            // Sprint 0: categoriaId se ignora porque la BD no tiene Categoria.
            _ = categoriaId;

            // INV-HU-004: se muestran activos e inactivos para que el Administrador
            // pueda reactivar tipos de tarima sin eliminar su historial.
            var query = _contexto.Productos.AsQueryable();

            if (!string.IsNullOrEmpty(filtroNombre))
            {
                query = query.Where(p => p.Nombre.Contains(filtroNombre));
            }

            var totalRegistros = await query.CountAsync();

            var productos = await query
                .OrderBy(p => p.Nombre)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

            await AplicarStockActualAsync(productos);

            return (productos, totalRegistros);
        }

        public async Task<List<InventarioExistenciaDTO>> ObtenerExistenciasActuales(int? idTipoTarima)
        {
            var tiposEntrada = TiposMovimientoInventario.TiposEntrada.ToList();
            var tiposSalida = TiposMovimientoInventario.TiposSalida.ToList();

            var productosQuery = _contexto.Productos
                .Where(p => p.Activo);

            if (idTipoTarima.HasValue)
            {
                productosQuery = productosQuery.Where(p => p.Id == idTipoTarima.Value);
            }

            var productos = await productosQuery
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            var idsProductos = productos.Select(p => p.Id).ToList();

            var movimientosResumen = await _contexto.InventarioMovimientos
                .Where(m => idsProductos.Contains(m.IdTipoTarima))
                .GroupBy(m => m.IdTipoTarima)
                .Select(g => new
                {
                    IdTipoTarima = g.Key,
                    Entradas = g.Where(m => tiposEntrada.Contains(m.TipoMovimiento))
                                .Sum(m => (int?)m.Cantidad) ?? 0,
                    Salidas = g.Where(m => tiposSalida.Contains(m.TipoMovimiento))
                               .Sum(m => (int?)m.Cantidad) ?? 0
                })
                .ToListAsync();

            var resultado = productos
                .Select(p =>
                {
                    var resumen = movimientosResumen.FirstOrDefault(m => m.IdTipoTarima == p.Id);
                    var entradas = resumen?.Entradas ?? 0;
                    var salidas = resumen?.Salidas ?? 0;

                    return new InventarioExistenciaDTO
                    {
                        IdTipoTarima = p.Id,
                        Codigo = p.Codigo,
                        TipoTarima = p.Nombre,
                        Medida = p.Medida,
                        Entradas = entradas,
                        Salidas = salidas,
                        StockActual = entradas - salidas,
                        StockMinimo = p.StockMinimo
                    };
                })
                .Where(x => x.Entradas > 0 || x.Salidas > 0 || x.StockActual != 0)
                .ToList();

            return resultado;
        }

        public async Task<List<InventarioMovimientoDTO>> ObtenerHistorialMovimientos(int? idTipoTarima)
        {
            var tiposEntrada = TiposMovimientoInventario.TiposEntrada.ToList();
            var tiposSalida = TiposMovimientoInventario.TiposSalida.ToList();

            var query =
                from movimiento in _contexto.InventarioMovimientos
                join producto in _contexto.Productos on movimiento.IdTipoTarima equals producto.Id
                select new
                {
                    movimiento.IdMovimiento,
                    movimiento.IdTipoTarima,
                    producto.Codigo,
                    TipoTarima = producto.Nombre,
                    producto.Medida,
                    movimiento.TipoMovimiento,
                    movimiento.Cantidad,
                    movimiento.FechaMovimiento,
                    movimiento.Motivo,
                    movimiento.IdProduccion,
                    movimiento.IdOrdenCompra
                };

            if (idTipoTarima.HasValue)
            {
                query = query.Where(m => m.IdTipoTarima == idTipoTarima.Value);
            }

            var movimientosBase = await query
                .OrderBy(m => m.IdTipoTarima)
                .ThenBy(m => m.FechaMovimiento)
                .ThenBy(m => m.IdMovimiento)
                .ToListAsync();

            var resultado = new List<InventarioMovimientoDTO>();

            foreach (var grupo in movimientosBase.GroupBy(m => m.IdTipoTarima))
            {
                var saldo = 0;

                foreach (var movimiento in grupo)
                {
                    if (tiposEntrada.Contains(movimiento.TipoMovimiento))
                    {
                        saldo += movimiento.Cantidad;
                    }
                    else if (tiposSalida.Contains(movimiento.TipoMovimiento))
                    {
                        saldo -= movimiento.Cantidad;
                    }

                    resultado.Add(new InventarioMovimientoDTO
                    {
                        IdMovimiento = movimiento.IdMovimiento,
                        IdTipoTarima = movimiento.IdTipoTarima,
                        Codigo = movimiento.Codigo,
                        TipoTarima = movimiento.TipoTarima,
                        Medida = movimiento.Medida,
                        TipoMovimiento = movimiento.TipoMovimiento,
                        Cantidad = movimiento.Cantidad,
                        Saldo = saldo,
                        FechaMovimiento = movimiento.FechaMovimiento,
                        Motivo = movimiento.Motivo,
                        IdProduccion = movimiento.IdProduccion,
                        IdOrdenCompra = movimiento.IdOrdenCompra
                    });
                }
            }

            return resultado
                .OrderByDescending(m => m.FechaMovimiento)
                .ThenByDescending(m => m.IdMovimiento)
                .ToList();
        }


        public async Task<ProductoAD> CambiarEstadoTipoTarima(int id)
        {
            var producto = await _contexto.Productos
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null)
                throw new ArgumentException("El tipo de tarima seleccionado no existe.");

            // INV-HU-004: no se eliminan registros ni movimientos de inventario.
            // Solo se alterna el campo Estado/Activo de TipoTarima para conservar
            // la trazabilidad histórica en InventarioMovimiento.
            producto.Activo = !producto.Activo;
            await _contexto.SaveChangesAsync();

            producto.Stock = await CalcularStockActualAsync(producto.Id);
            return producto;
        }

        public async Task RegistrarAjusteInventario(AjusteInventarioDTO ajuste)
        {
            var productoExiste = await _contexto.Productos
                .AnyAsync(p => p.Id == ajuste.IdTipoTarima && p.Activo);

            if (!productoExiste)
                throw new ArgumentException("El tipo de tarima seleccionado no existe o está inactivo.");

            var idUsuario = await ObtenerIdUsuarioPorCorreoAsync(ajuste.CorreoUsuarioRegistro) ?? 1;

            var tipoMovimiento = ajuste.TipoAjuste == TiposMovimientoInventario.AjusteSalida
                ? TiposMovimientoInventario.AjusteSalida
                : TiposMovimientoInventario.AjusteEntrada;

            var motivoConAutorizacion = $"Motivo: {ajuste.Motivo.Trim()} | Autorización: {ajuste.Autorizacion.Trim()}";

            var movimiento = new InventarioMovimientoAD
            {
                IdTipoTarima = ajuste.IdTipoTarima,
                IdUsuarioRegistro = idUsuario,
                TipoMovimiento = tipoMovimiento,
                Cantidad = ajuste.Cantidad,
                FechaMovimiento = DateTime.Now,
                Motivo = motivoConAutorizacion
            };

            _contexto.InventarioMovimientos.Add(movimiento);
            await _contexto.SaveChangesAsync();
        }

        private async Task<int?> ObtenerIdUsuarioPorCorreoAsync(string? correo)
        {
            if (!string.IsNullOrWhiteSpace(correo))
            {
                var idUsuario = await _contexto.Usuarios
                    .Where(u => u.Correo == correo)
                    .Select(u => (int?)u.IdUsuario)
                    .FirstOrDefaultAsync();

                if (idUsuario.HasValue)
                    return idUsuario;
            }

            return await _contexto.Usuarios
                .OrderBy(u => u.IdUsuario)
                .Select(u => (int?)u.IdUsuario)
                .FirstOrDefaultAsync();
        }

    }
}
