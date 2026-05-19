using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using PROmaderas.Models;

namespace PROmaderas.UI.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            var errorViewModel = new ErrorViewModel
            {
                RequestId = requestId
            };
            
            Response.StatusCode = 500;
            return View(errorViewModel);
        }

        [Route("Error/404")]
        public IActionResult Error404()
        {
            Response.StatusCode = 404;
            ViewBag.ErrorCode = "404";
            ViewBag.ErrorMessage = "La página que buscas no existe";
            ViewBag.ErrorDescription = "La URL solicitada no pudo ser encontrada en el servidor. Verifica la dirección e inténtalo de nuevo.";
            return View();
        }

        [Route("Error/500")]
        public IActionResult Error500()
        {
            Response.StatusCode = 500;
            ViewBag.ErrorCode = "500";
            ViewBag.ErrorMessage = "Error interno del servidor";
            ViewBag.ErrorDescription = "Ha ocurrido un error inesperado en el servidor. Nuestro equipo técnico ha sido notificado.";
            return View();
        }

        [Route("Error/{statusCode}")]
        public IActionResult HandleErrorCode(int statusCode)
        {
            var codeTitles = new Dictionary<int, string>
            {
                [400] = "Solicitud Incorrecta",
                [401] = "No Autorizado", 
                [403] = "Acceso Prohibido",
                [404] = "Página No Encontrada",
                [500] = "Error Interno del Servidor",
                [503] = "Servicio No Disponible"
            };

            Response.StatusCode = statusCode;
            ViewBag.ErrorCode = statusCode.ToString();
            ViewBag.ErrorMessage = codeTitles.ContainsKey(statusCode) 
                ? codeTitles[statusCode] 
                : $"Error {statusCode}";

            return statusCode switch
            {
                404 => View("Error404"),
                500 => View("Error500"),
                _ => View("Index", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier })
            };
        }
    }
}