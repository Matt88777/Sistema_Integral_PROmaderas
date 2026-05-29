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

        // Sprint 2: tabla Usuario (IdVendedor en OrdenCompra)
        public DbSet<UsuarioAD> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CategoriaAD y PlanillaAD no existen en la BD nueva.
            modelBuilder.Ignore<CategoriaAD>();
            modelBuilder.Ignore<PlanillaAD>();

            // ProductoAD -> TipoTarima
            modelBuilder.Entity<ProductoAD>(e =>
            {
                e.ToTable("TipoTarima");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("IdTipoTarima");
                e.Property(x => x.Nombre).HasColumnName("Nombre");
                e.Property(x => x.Precio).HasColumnName("PrecioUnitario");
                e.Property(x => x.Activo).HasColumnName("Estado");
                e.Ignore(x => x.CategoriaId);
                e.Ignore(x => x.ImpuestoPorc);
                e.Ignore(x => x.Stock);
                e.Ignore(x => x.ImagenUrl);
                e.Ignore(x => x.Categoria);
            });

            // ClienteAD -> Cliente
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

            // PedidoAD -> OrdenCompra  (Sprint 2: agrega NumeroOrden, IdVendedor, Observacion, Activa)
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
                // UsuarioId es [NotMapped] — ignorado automáticamente
            });

            // PedidoDetalleAD -> OrdenCompraDetalle
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

            // FacturacionAD -> Factura
            modelBuilder.Entity<FacturacionAD>(e =>
            {
                e.ToTable("Factura");
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("IdFactura");
                e.Property(x => x.PedidoId).HasColumnName("IdOrdenCompra");
                e.Property(x => x.ClienteId).HasColumnName("IdCliente");
                e.Property(x => x.Fecha).HasColumnName("FechaEmision");
                e.Property(x => x.Subtotal).HasColumnName("Subtotal");
                e.Property(x => x.Impuestos).HasColumnName("Impuesto");
                e.Property(x => x.Total).HasColumnName("Total");
                e.Property(x => x.Estado).HasColumnName("Estado");
                e.Ignore(x => x.MetodoPago);
            });

            // EmpleadoAD -> Empleado
            modelBuilder.Entity<EmpleadoAD>(e =>
            {
                e.ToTable("Empleado", "dbo");
                e.HasKey(x => x.IdEmpleado);
                e.Ignore(x => x.Puesto);
            });

            // UsuarioAD -> Usuario  (Sprint 2: para resolver IdVendedor)
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
        }
    }
}
