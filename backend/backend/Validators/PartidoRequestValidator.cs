using backend.DTOs.Request;
using FluentValidation;

namespace backend.Validators
{
    public class PartidoRequestValidator : AbstractValidator<PartidoRequestDto>
    {
        public PartidoRequestValidator()
        {
            RuleFor(x => x.Local)
                .NotEmpty()
                .WithMessage("El nombre del equipo local es obligatorio")
                .MinimumLength(2)
                .WithMessage("El nombre del equipo local debe tener al menos 2 caracteres");

            RuleFor(x => x.Visitante)
                .NotEmpty()
                .WithMessage("El nombre del equipo visitante es obligatorio")
                .MinimumLength(2)
                .WithMessage("El nombre del equipo visitante debe tener al menos 2 caracteres");

            RuleFor(x => x)
                .Must(x => x.Local != x.Visitante)
                .WithMessage("El equipo local y visitante no pueden ser el mismo");

            // ✅ Corregido: FechaHora es nullable
            RuleFor(x => x.FechaHora)
                .NotEmpty()
                .WithMessage("La fecha y hora del partido es obligatoria")
                .Must(fecha => fecha.HasValue && FechaEnFuturo(fecha.Value))
                .WithMessage("La fecha del partido debe ser en el futuro");

            RuleFor(x => x.Estadio)
                .MaximumLength(50)
                .WithMessage("El estadio no puede exceder los 50 caracteres");

            RuleFor(x => x.Estado)
                .MaximumLength(20)
                .WithMessage("El estado no puede exceder los 20 caracteres")
                .Must(EstadoValido)
                .When(x => !string.IsNullOrEmpty(x.Estado))
                .WithMessage("El estado debe ser: Programado, EnCurso, Finalizado, o Cancelado");

            RuleFor(x => x.GolesLocal)
                .GreaterThanOrEqualTo(0)
                .When(x => x.GolesLocal.HasValue)
                .WithMessage("Los goles del local no pueden ser negativos");

            RuleFor(x => x.GolesVisitante)
                .GreaterThanOrEqualTo(0)
                .When(x => x.GolesVisitante.HasValue)
                .WithMessage("Los goles del visitante no pueden ser negativos");
        }

        private static bool FechaEnFuturo(DateTime fecha)
        {
            return fecha > DateTime.UtcNow.AddMinutes(-5); // Permitir 5 minutos de tolerancia
        }

        private static bool EstadoValido(string estado)
        {
            var estadosValidos = new[] { "Programado", "EnCurso", "Finalizado", "Cancelado" };
            return estadosValidos.Contains(estado, StringComparer.OrdinalIgnoreCase);
        }
    }
}