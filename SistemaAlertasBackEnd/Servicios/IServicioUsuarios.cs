using Microsoft.AspNetCore.Identity;

namespace SistemaAlertasBackEnd.Servicios
{
    public interface IServicioUsuarios
    {
        Task<IdentityUser?> ObtenerUsuario();
    }
}