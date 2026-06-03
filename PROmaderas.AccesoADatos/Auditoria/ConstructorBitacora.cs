using System.Text.Json;
using PROmaderas.Abstracciones.Models;

namespace PROmaderas.AccesoADatos.Auditoria
{
    public static class ConstructorBitacora
    {
        public static BitacoraAuditoriaAD Construir(
            string tabla,
            int? idRegistro,
            ContextoAuditoria ctx,
            object valoresAnteriores,
            object valoresNuevos)
        {
            return new BitacoraAuditoriaAD
            {
                IdUsuario = null,
                TablaAfectada = tabla,
                IdRegistroAfectado = idRegistro,
                Accion = ctx.Accion,
                FechaAccion = DateTime.Now,
                DireccionIP = ctx.Ip,
                ValorAnterior = JsonSerializer.Serialize(valoresAnteriores),
                ValorNuevo = JsonSerializer.Serialize(new
                {
                    modificadoPor = new { ctx.Email, ctx.UsuarioIdentityId },
                    datos = valoresNuevos
                })
            };
        }
    }
}
