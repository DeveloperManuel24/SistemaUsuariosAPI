using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaAlertasBackEnd.Servicios
{
    public class ServicioUsuarios : IServicioUsuarios
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<IdentityUser> userManager;

        public ServicioUsuarios(IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
        }

        public async Task<IdentityUser?> ObtenerUsuario()
        {
            var emailClaim = httpContextAccessor.HttpContext!
                .User.Claims.Where(x => x.Type == "email").FirstOrDefault();

            if (emailClaim is null)
            {
                return null;
            }

            var email = emailClaim.Value;
            return await userManager.FindByEmailAsync(email);
        }

        public async Task<List<IdentityUser>> ObtenerTodosLosUsuarios()
        {
            return await userManager.Users.ToListAsync();
        }
    }
}
