using PROmaderas.Abstracciones.Models;
using Microsoft.EntityFrameworkCore;

namespace PROmaderas.AccesoADatos
{
	// Sprint 0 PROMADERAS: el seed anterior (Pedidos360) creaba tablas
	// Categoria/Producto y poblaba datos de un restaurante. La BD nueva
	// PROmaderasDB_NEW viene poblada desde el script SQL (roles, departamentos,
	// puestos, empleado admin y tipos de tarima base), así que estos métodos
	// quedan como stubs y ya no se invocan desde Program.cs. El cuerpo original
	// se conserva en el historial de Git por si se necesita rescatar algún
	// fragmento al re-implementar Categoria/Producto sobre el modelo nuevo.
	public static class DbSeeder
	{
		public static void EnsureClienteIdentitySchema(Contexto contexto)
		{
			// Antes agregaba la columna Cliente.UsuarioIdentityId. PROMADERAS
			// no vincula clientes a usuarios de Identity en Sprint 0.
			_ = contexto;
		}

		public static void EnsureCategoriaSchema(Contexto contexto)
		{
			// La BD nueva no tiene tabla Categoria.
			_ = contexto;
		}

		public static void EnsureProductoSchema(Contexto contexto)
		{
			// La BD nueva usa TipoTarima y el campo Activo se llama Estado.
			_ = contexto;
		}

		public static void SeedCategorias(Contexto contexto)
		{
			// Sin Categoria en la BD nueva. CategoriaRepositorio devuelve una
			// lista dummy en memoria para que las vistas no revienten.
			_ = contexto;
		}

		public static void SeedClientes(Contexto contexto)
		{
			// Los clientes se cargan manualmente desde la UI. La BD nueva
			// trae la tabla Cliente vacía.
			_ = contexto;
		}

		public static void SeedProductos(Contexto contexto)
		{
			// TipoTarima ya viene con 3 productos seed desde PROmaderasDB_NEW.sql.
			_ = contexto;
			_ = new ProductoAD(); // referencia simbólica para mantener el using
		}
	}
}
