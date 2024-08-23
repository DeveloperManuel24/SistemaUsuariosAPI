using FluentValidation;
using SistemaAlertasBackEnd.DTOs;
using SistemaAlertasBackEnd.Validaciones;

public class CredencialesUsuarioDTOValidador : AbstractValidator<UsuarioCrearDTO>
{
    public CredencialesUsuarioDTOValidador()
    {
        RuleFor(x => x.CorreoElectronico)
             .MaximumLength(256).WithMessage(Utilidades.MaximumLengthMensaje)
             .EmailAddress().WithMessage(Utilidades.EmailMensaje)
             .NotEmpty().WithMessage(Utilidades.CampoRequeridoMensaje);
             

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(Utilidades.CampoRequeridoMensaje);

       
       

        // Log para verificar si la validación se ejecuta
        RuleFor(x => x.CorreoElectronico).Must(x =>
        {
            Console.WriteLine($"Validando longitud: {x.Length} caracteres");
            return x.Length <= 256;
        }).WithMessage("El correo electrónico supera la longitud máxima permitida de 256 caracteres.");
    }
}
