using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PROmaderas.UI.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.ExceptionHandled)
                return;

            // Log the exception
            _logger.LogError(context.Exception, 
                "Excepción no manejada en {Controller}.{Action}: {Message}",
                context.RouteData.Values["controller"],
                context.RouteData.Values["action"], 
                context.Exception.Message);

            // Don't handle in development - let developer exception page show
            if (context.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
                return;

            // Set status code
            context.HttpContext.Response.StatusCode = 500;

            // Mark as handled
            context.ExceptionHandled = true;

            // Redirect to error page
            context.Result = new RedirectToActionResult("Error500", "Error", null);
        }
    }
}