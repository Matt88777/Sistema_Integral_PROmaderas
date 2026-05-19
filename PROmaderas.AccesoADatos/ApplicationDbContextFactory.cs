using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PROmaderas.AccesoADatos
{
	public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
	{
		public ApplicationDbContext CreateDbContext(string[] args)
		{
			var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
			var connectionString = "Server=localhost;Database=PROmaderasDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

			optionsBuilder.UseSqlServer(connectionString);
			return new ApplicationDbContext(optionsBuilder.Options);
		}
	}
}
