using FluentValidation;
using SistemaAlertasBackEnd.DTOs;

namespace SistemaAlertasBackEnd.Filtros
{
    public class FiltroValidaciones<T> : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            // Log para verificar si el filtro se ejecuta
            Console.WriteLine("FiltroValidaciones<T> se está ejecutando.");

            var validador = context.HttpContext.RequestServices.GetService<IValidator<T>>();

            if (validador is null)
            {
                Console.WriteLine("No se encontró un validador para el tipo especificado.");
                return await next(context);
            }

            var insumoAValidar = context.Arguments.OfType<T>().FirstOrDefault();

            if (insumoAValidar is null)
            {
                Console.WriteLine("No se pudo encontrar la entidad a validar.");
                return TypedResults.Problem("No pudo ser encontrada la entidad a validar");
            }

            var resultadoValidacion = await validador.ValidateAsync(insumoAValidar);

            if (!resultadoValidacion.IsValid)
            {
                Console.WriteLine("La validación falló. Errores encontrados:");
                foreach (var error in resultadoValidacion.Errors)
                {
                    Console.WriteLine($"- {error.ErrorMessage}");
                }
                return TypedResults.ValidationProblem(resultadoValidacion.ToDictionary());
            }

            // Log para confirmar que la validación fue exitosa
            Console.WriteLine("La validación fue exitosa.");
            return await next(context);
        }
    }
}
