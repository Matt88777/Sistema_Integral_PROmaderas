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

        // Sprint 0 PROMADERAS:
        // - CategoriaAD: la BD nueva no tiene tabla Categoria. El controller
        //   de productos recibe una lista dummy en memoria. DbSet removido.
        // - PlanillaAD: la BD nueva usa PlanillaPeriodo + PlanillaDetalle (modelo
        //   relacional incompatible con el plano actual). DbSet removido y el
        //   PlanillaController muestra "En construcción".

        protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// CategoriaAD y PlanillaAD no existen como tabla en la BD nueva.
			// Hay que ignorarlas a nivel de modelo (no solo el DbSet) porque la
			// navegación inversa `CategoriaAD.Productos` haría que EF Core las
			// descubra por convención y cree una FK shadow `CategoriaADId` en
			// `TipoTarima`, que produce: SqlException "Invalid column name
			// 'CategoriaADId'".
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
				e.Ignore(x => x.UsuarioIdentityId);
			});

			// PedidoAD -> OrdenCompra
			// La BD requiere NumeroOrden e IdVendedor (NOT NULL) que el modelo no
			// tiene -> solo lectura/listado en Sprint 0; el Create del controller
			// queda en construcción.
			modelBuilder.Entity<PedidoAD>(e =>
			{
				e.ToTable("OrdenCompra");
				e.HasKey(x => x.Id);
				e.Property(x => x.Id).HasColumnName("IdOrdenCompra");
				e.Property(x => x.ClienteId).HasColumnName("IdCliente");
				e.Property(x => x.Fecha).HasColumnName("FechaOrden");
				e.Property(x => x.Subtotal).HasColumnName("Subtotal");
				e.Property(x => x.Impuestos).HasColumnName("Impuesto");
				e.Property(x => x.Total).HasColumnName("Total");
				e.Property(x => x.Estado).HasColumnName("Estado");
				// UsuarioId (string Identity) != IdVendedor (INT Usuario).
				e.Ignore(x => x.UsuarioId);
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
			// La BD requiere NumeroFactura, IdUsuarioEmisor, SaldoPendiente (NOT
			// NULL) que el modelo no tiene -> solo lectura en Sprint 0.
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
                e.Ignore(x => x.Puesto); // solo Puesto se ignora
            });

        }
    }
}
