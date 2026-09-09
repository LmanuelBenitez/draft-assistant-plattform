using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace backend.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado en la aplicación");
                await HandleExceptionAsync(context, ex);
            }
            finally
            {
                // Limpieza o logging adicional si es necesario
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var statusCode = HttpStatusCode.InternalServerError;
            var message = "Ocurrió un error interno en el servidor.";
            var details = "";

            switch (exception)
            {
                case ArgumentException argEx:
                    statusCode = HttpStatusCode.BadRequest;
                    message = "Solicitud inválida.";
                    details = argEx.Message;
                    break;

                case InvalidOperationException invOpEx:
                    statusCode = HttpStatusCode.BadRequest;
                    message = "Operación inválida.";
                    details = invOpEx.Message;
                    break;

                case UnauthorizedAccessException unauthEx:
                    statusCode = HttpStatusCode.Unauthorized;
                    message = "No autorizado.";
                    details = unauthEx.Message;
                    break;

                case KeyNotFoundException notFoundEx:
                    statusCode = HttpStatusCode.NotFound;
                    message = "Recurso no encontrado.";
                    details = notFoundEx.Message;
                    break;

                case DbUpdateException dbEx:
                    statusCode = HttpStatusCode.Conflict;
                    message = "Error al actualizar la base de datos.";
                    details = dbEx.InnerException?.Message ?? dbEx.Message;
                    break;

                case OperationCanceledException:
                    statusCode = HttpStatusCode.RequestTimeout;
                    message = "La operación fue cancelada.";
                    break;

                case HttpRequestException httpEx:
                    statusCode = HttpStatusCode.ServiceUnavailable;
                    message = "Error al comunicarse con un servicio externo.";
                    details = httpEx.Message;
                    break;

                default:
                    // Para errores no controlados, no mostrar detalles internos
                    break;
            }

            response.StatusCode = (int)statusCode;

            var errorResponse = new
            {
                statusCode = (int)statusCode,
                message,
                details = string.IsNullOrEmpty(details) ? null : details,
                timestamp = DateTime.UtcNow,
                traceId = context.TraceIdentifier
            };

            var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await response.WriteAsync(json);
        }
    }
}
