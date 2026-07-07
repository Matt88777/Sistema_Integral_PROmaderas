using Microsoft.EntityFrameworkCore;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.AccesoADatos
{
    public class Contexto : DbContext
    {
        public Contexto(DbContextOptions<Contexto> options) : base(options)
        {
        }

        public DbSet<ProductoAD> Productos { get; set; }
        public DbSet<ClienteAD> Clientes { get; set; }
        public DbSet<PedidoAD> Pedidos { get; set; }
        public DbSet<PedidoDetalleAD> PedidoDetalles { get; set; }
        public DbSet<FacturacionAD> Facturaciones { get; set; }
        public DbSet<EmpleadoAD> Empleados { get; set; }
        public DbSet<InventarioMovimientoAD> InventarioMovimientos { get; set; }
        public DbSet<UsuarioAD> Usuarios { get; set; }
        public DbSet<BitacoraAuditoriaAD> Bitacoras { get; set; }
        public DbSet<PuestoAD> Puestos { get; set; }
        public DbSet<PagoFacturaAD> PagosFactura { get; set; }
        public DbSet<PlanillaPeriodoAD> PlanillaPeriodos { get; set; }
        public DbSet<PlanillaDetalleFinancieroAD> PlanillaDetallesFinancieros { get; set; }
        public DbSet<SalarioHistorialAD> SalarioHistoriales { get; set; }
        public DbSet<LicenciaAD> Licencias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Ignore<CategoriaAD>();
            modelBuilder.Ignore<PlanillaAD>();

            modelBuilder.Entity<ProductoAD>(e =>
            {
                e.ToTable("TipoTarima");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("IdTipoTarima");
                e.Property(x => x.Codigo).HasColumnName("Codigo");
                e.Property(x => x.Nombre).HasColumnName("Nombre");
                e.Property(x => x.Medida).HasColumnName("Medida");
                e.Property(x => x.Descripcion).HasColumnName("Descripcion");
                e.Property(x => x.Precio).HasColumnName("PrecioUnitario");
                e.Property(x => x.StockMinimo).HasColumnName("StockMinimo");
                e.Property(x => x.Activo).HasColumnName("Estado");
                e.Property(x => x.FechaCreacion).HasColumnName("FechaCreacion");
                e.Ignore(x => x.CategoriaId);
                e.Ignore(x => x.ImpuestoPorc);
                e.Ignore(x => x.Stock);
                e.Ignore(x => x.ImagenUrl);
                e.Ignore(x => x.Categoria);
            });

            modelBuilder.Entity<ClienteAD>(e =>
            {
                e.ToTable("Cliente");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("IdCliente");
                e.Property(x => x.Nombre).HasColumnName("NombreCliente");
                e.Property(x => x.Cedula).HasColumnName("CedulaJuridica");
                e.Property(x => x.Correo).HasColumnName("Correo");
                e.Property(x => x.Telefono).HasColumnName("Telefono");
                e.Property(x => x.Direccion).HasColumnName("Direccion");
                e.Property(x => x.CondicionPago).HasColumnName("CondicionPago");
                e.Property(x => x.Exonerado).HasColumnName("Exonerado");
                e.Property(x => x.PorcentajeExoneracion).HasColumnName("PorcentajeExoneracion");
                e.Property(x => x.Estado).HasColumnName("Estado");
                e.Property(x => x.FechaCreacion).HasColumnName("FechaCreacion");
                e.Ignore(x => x.UsuarioIdentityId);
            });

            modelBuilder.Entity<PedidoAD>(e =>
            {
                e.ToTable("OrdenCompra");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("IdOrdenCompra");
                e.Property(x => x.NumeroOrden).HasColumnName("NumeroOrden");
                e.Property(x => x.ClienteId).HasColumnName("IdCliente");
                e.Property(x => x.VendedorId).HasColumnName("IdVendedor");
                e.Property(x => x.Fecha).HasColumnName("FechaOrden");
                e.Property(x => x.Observacion).HasColumnName("Observacion");
                e.Property(x => x.Subtotal).HasColumnName("Subtotal");
                e.Property(x => x.Impuestos).HasColumnName("Impuesto");
                e.Property(x => x.Total).HasColumnName("Total");
                e.Property(x => x.Estado).HasColumnName("Estado");
                e.Property(x => x.Activa).HasColumnName("Activa");
            });

            modelBuilder.Entity<PedidoDetalleAD>(e =>
            {
                e.ToTable("OrdenCompraDetalle");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("IdOrdenCompraDetalle");
                e.Property(x => x.PedidoId).HasColumnName("IdOrdenCompra");
                e.Property(x => x.ProductoId).HasColumnName("IdTipoTarima");
                e.Property(x => x.Cantidad).HasColumnName("Cantidad");
                e.Property(x => x.PrecioUnit).HasColumnName("PrecioUnitario");
                e.Property(x => x.TotalLinea).HasColumnName("Subtotal");
                e.Ignore(x => x.Descuento);
                e.Ignore(x => x.ImpuestoPorc);
            });

            modelBuilder.Entity<InventarioMovimientoAD>(e =>
            {
                e.ToTable("InventarioMovimiento");
                e.HasKey(x => x.IdMovimiento);
                e.Property(x => x.IdMovimiento).HasColumnName("IdMovimiento");
                e.Property(x => x.IdTipoTarima).HasColumnName("IdTipoTarima");
                e.Property(x => x.IdUsuarioRegistro).HasColumnName("IdUsuarioRegistro");
                e.Property(x => x.TipoMovimiento).HasColumnName("TipoMovimiento");
                e.Property(x => x.Cantidad).HasColumnName("Cantidad");
                e.Property(x => x.FechaMovimiento).HasColumnName("FechaMovimiento");
                e.Property(x => x.Motivo).HasColumnName("Motivo");
                e.Property(x => x.IdProduccion).HasColumnName("IdProduccion");
                e.Property(x => x.IdOrdenCompra).HasColumnName("IdOrdenCompra");
                e.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.IdTipoTarima);
                e.HasOne(x => x.OrdenCompra).WithMany().HasForeignKey(x => x.IdOrdenCompra);
            });

            modelBuilder.Entity<FacturacionAD>(e =>
            {
                e.ToTable("Factura");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("IdFactura");
                e.Property(x => x.NumeroFactura).HasColumnName("NumeroFactura");
                e.Property(x => x.PedidoId).HasColumnName("IdOrdenCompra");
                e.Property(x => x.ClienteId).HasColumnName("IdCliente");
                e.Property(x => x.Fecha).HasColumnName("FechaEmision");
                e.Property(x => x.Subtotal).HasColumnName("Subtotal");
                e.Property(x => x.Exoneracion).HasColumnName("Exoneracion");
                e.Property(x => x.Impuestos).HasColumnName("Impuesto");
                e.Property(x => x.Total).HasColumnName("Total");
                e.Property(x => x.Estado).HasColumnName("Estado");
                e.Property(x => x.Activa).HasColumnName("Activa");
                e.Property(x => x.IdUsuarioEmisor).HasColumnName("IdUsuarioEmisor");
                e.Property(x => x.SaldoPendiente).HasColumnName("SaldoPendiente");
                e.Ignore(x => x.MetodoPago);
            });

            modelBuilder.Entity<EmpleadoAD>(e =>
            {
                e.ToTable("Empleado", "dbo");
                e.HasKey(x => x.IdEmpleado);
                e.Ignore(x => x.Puesto);
            });

            modelBuilder.Entity<UsuarioAD>(e =>
            {
                e.ToTable("Usuario");
                e.HasKey(x => x.IdUsuario);
                e.Property(x => x.IdUsuario).HasColumnName("IdUsuario");
                e.Property(x => x.IdEmpleado).HasColumnName("IdEmpleado");
                e.Property(x => x.IdRol).HasColumnName("IdRol");
                e.Property(x => x.NombreUsuario).HasColumnName("NombreUsuario");
                e.Property(x => x.Correo).HasColumnName("Correo");
                e.Property(x => x.Estado).HasColumnName("Estado");
            });

            modelBuilder.Entity<BitacoraAuditoriaAD>(e =>
            {
                e.ToTable("BitacoraAuditoria");
                e.HasKey(x => x.IdBitacora);
                e.Property(x => x.IdBitacora).HasColumnName("IdBitacora");
                e.Property(x => x.IdUsuario).HasColumnName("IdUsuario");
                e.Property(x => x.TablaAfectada).HasColumnName("TablaAfectada");
                e.Property(x => x.IdRegistroAfectado).HasColumnName("IdRegistroAfectado");
                e.Property(x => x.Accion).HasColumnName("Accion");
                e.Property(x => x.ValorAnterior).HasColumnName("ValorAnterior");
                e.Property(x => x.ValorNuevo).HasColumnName("ValorNuevo");
                e.Property(x => x.FechaAccion).HasColumnName("FechaAccion");
                e.Property(x => x.DireccionIP).HasColumnName("DireccionIP");
            });

            modelBuilder.Entity<PagoFacturaAD>(e =>
            {
                e.ToTable("PagoFactura");
                e.HasKey(x => x.IdPagoFactura);
                e.Property(x => x.IdPagoFactura).HasColumnName("IdPagoFactura");
                e.Property(x => x.IdFactura).HasColumnName("IdFactura");
                e.Property(x => x.FechaPago).HasColumnName("FechaPago");
                e.Property(x => x.Monto).HasColumnName("Monto");
                e.Property(x => x.FormaPago).HasColumnName("FormaPago");
                e.Property(x => x.Referencia).HasColumnName("Referencia");
                e.Property(x => x.IdUsuarioRegistro).HasColumnName("IdUsuarioRegistro");
            });

            modelBuilder.Entity<PlanillaPeriodoAD>(e =>
            {
                e.ToTable("PlanillaPeriodo");
                e.HasKey(x => x.IdPlanillaPeriodo);
                e.Property(x => x.IdPlanillaPeriodo).HasColumnName("IdPlanillaPeriodo");
                e.Property(x => x.FechaInicio).HasColumnName("FechaInicio");
                e.Property(x => x.FechaFin).HasColumnName("FechaFin");
                e.Property(x => x.TipoPeriodo).HasColumnName("TipoPeriodo");
                e.Property(x => x.Estado).HasColumnName("Estado");
                e.Property(x => x.FechaCreacion).HasColumnName("FechaCreacion");
                e.Property(x => x.IdUsuarioCreacion).HasColumnName("IdUsuarioCreacion");
            });

            modelBuilder.Entity<PlanillaDetalleFinancieroAD>(e =>
            {
                e.ToTable("PlanillaDetalle");
                e.HasKey(x => x.IdPlanillaDetalle);
                e.Property(x => x.IdPlanillaDetalle).HasColumnName("IdPlanillaDetalle");
                e.Property(x => x.IdPlanillaPeriodo).HasColumnName("IdPlanillaPeriodo");
                e.Property(x => x.IdEmpleado).HasColumnName("IdEmpleado");
                e.Property(x => x.SalarioNeto).HasColumnName("SalarioNeto");
                e.HasOne(x => x.Periodo).WithMany(x => x.Detalles).HasForeignKey(x => x.IdPlanillaPeriodo);
            });

            modelBuilder.Entity<PuestoAD>(e =>
            {
                e.ToTable("Puesto");
                e.HasKey(x => x.IdPuesto);
                e.Property(x => x.IdPuesto).HasColumnName("IdPuesto");
                e.Property(x => x.NombrePuesto).HasColumnName("NombrePuesto");
                e.Property(x => x.IdDepartamento).HasColumnName("IdDepartamento");
                e.Property(x => x.Estado).HasColumnName("Estado");
            });

            modelBuilder.Entity<SalarioHistorialAD>(e =>
            {
                e.ToTable("SalarioHistorial");
                e.HasKey(x => x.IdHistorial);
                e.HasOne(x => x.Empleado).WithMany().HasForeignKey(x => x.IdEmpleado);
            });

            
            modelBuilder.Entity<LicenciaAD>(e =>
            {
                e.ToTable("Licencia");
                e.HasKey(x => x.IdLicencia);
                e.HasOne(x => x.Empleado).WithMany().HasForeignKey(x => x.IdEmpleado);
            });
        }  
    }      
}