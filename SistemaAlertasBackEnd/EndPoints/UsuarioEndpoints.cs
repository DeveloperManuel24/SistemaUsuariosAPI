using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaAlertasBackEnd.DTOs;
using SistemaAlertasBackEnd.Filtros;
using SistemaAlertasBackEnd.Servicios;
using SistemaAlertasBackEnd.Utilidades;
using SistemaAlertasBackEnd.Validaciones;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SistemaAlertasBackEnd.EndPoints
{
    public static class UsuariosEndPoints
    {
        public static RouteGroupBuilder MapUsuarios(this RouteGroupBuilder group)
        {
            group.MapPost("/registrar", Registrar)
                .AddEndpointFilter<FiltroValidaciones<UsuarioCrearDTO>>();

            group.MapPost("/login", Login)
                .AddEndpointFilter<FiltroValidaciones<UsuarioCrearDTO>>();

            group.MapGet("/usuarios", ObtenerUsuarios)
                .RequireAuthorization("esadmin");

            group.MapGet("/usuarios/{id}", ObtenerUsuarioPorId)
                .RequireAuthorization("esadmin");

            group.MapPut("/usuarios/{id}", ActualizarUsuario)
                .AddEndpointFilter<FiltroValidaciones<ActualizarUsuarioDTO>>()
                .RequireAuthorization("esadmin");

            group.MapDelete("/usuarios/{id}", EliminarUsuario)
                .RequireAuthorization("esadmin");

            group.MapPost("/asignar-rol", AsignarRol)
                .AddEndpointFilter<FiltroValidaciones<EditarClaimDTO>>()
                .RequireAuthorization("esadmin");

            group.MapPost("/remover-rol-admin", RemoverRolAdmin)
                .AddEndpointFilter<FiltroValidaciones<EditarClaimDTO>>()
                .RequireAuthorization("esadmin");

            return group;
        }

        static async Task<Results<Ok<RespuestaAutenticacionDTO>, BadRequest<IEnumerable<IdentityError>>>> Registrar(
            UsuarioCrearDTO usuarioCrearDTO,
            [FromServices] UserManager<IdentityUser> userManager,
            [FromServices] ServicioEmail servicioEmail,
            [FromServices] IConfiguration configuration)
        {
            if (usuarioCrearDTO == null)
            {
                throw new ArgumentNullException(nameof(usuarioCrearDTO), "El objeto usuarioCrearDTO es null");
            }

            var usuario = new IdentityUser
            {
                UserName = usuarioCrearDTO.CorreoElectronico,
                Email = usuarioCrearDTO.CorreoElectronico
            };

            var resultado = await userManager.CreateAsync(usuario, usuarioCrearDTO.Password);

            if (resultado.Succeeded)
            {
                // Enviar correo con las credenciales
                await servicioEmail.SendCredentialsEmailAsync(usuario.Email, usuario.UserName, usuarioCrearDTO.Password);

                // Generar el token JWT
                var token = await ConstruirToken(usuarioCrearDTO, configuration, userManager);

                // Devolver el token junto con la respuesta de autenticación
                return TypedResults.Ok(token);
            }

            return TypedResults.BadRequest(resultado.Errors);
        }

        static async Task<IResult> ObtenerUsuarios([FromServices] UserManager<IdentityUser> userManager)
        {
            var usuarios = await userManager.Users.ToListAsync();

            if (usuarios == null || !usuarios.Any())
            {
                return Results.Ok(Enumerable.Empty<UsuarioDTO>());
            }

            var usuariosDTO = new List<UsuarioDTO>();

            foreach (var usuario in usuarios)
            {
                bool esAdmin = await userManager.IsInRoleAsync(usuario, "esadmin");

                usuariosDTO.Add(new UsuarioDTO
                {
                    Id = usuario.Id,
                    CorreoElectronico = usuario.Email,
                    Password = usuario.UserName,
                    FechaRegistro = usuario.LockoutEnd?.DateTime ?? DateTime.UtcNow,
                    DerechosUsuario = esAdmin ? "Administrador" : "Usuario General"
                });
            }

            return Results.Ok(usuariosDTO);
        }

        static async Task<Results<Ok<UsuarioDTO>, NotFound>> ObtenerUsuarioPorId(string id, [FromServices] UserManager<IdentityUser> userManager)
        {
            var usuario = await userManager.FindByIdAsync(id);

            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            bool esAdmin = await userManager.IsInRoleAsync(usuario, "esadmin");

            var usuarioDTO = new UsuarioDTO
            {
                Id = usuario.Id,
                CorreoElectronico = usuario.Email,
                Password = usuario.UserName,
                FechaRegistro = usuario.LockoutEnd?.DateTime ?? DateTime.UtcNow,
                DerechosUsuario = esAdmin ? "Administrador" : "Usuario General"
            };

            return TypedResults.Ok(usuarioDTO);
        }

        static async Task<Results<Ok, NotFound>> ActualizarUsuario(
            string id,
            ActualizarUsuarioDTO actualizarUsuarioDTO,
            [FromServices] UserManager<IdentityUser> userManager,
            [FromServices] ServicioEmail servicioEmail)
        {
            var usuario = await userManager.FindByIdAsync(id);

            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            usuario.Email = actualizarUsuarioDTO.CorreoElectronico;
            usuario.UserName = actualizarUsuarioDTO.CorreoElectronico;

            var hashedPassword = userManager.PasswordHasher.HashPassword(usuario, actualizarUsuarioDTO.Password);
            usuario.PasswordHash = hashedPassword;

            var resultado = await userManager.UpdateAsync(usuario);

            if (resultado.Succeeded)
            {
                await servicioEmail.SendEmailUpdatedNotificationAsync(usuario.Email, actualizarUsuarioDTO.Password, usuario.UserName);
                return TypedResults.Ok();
            }

            return TypedResults.NotFound();
        }

        static async Task<Results<NoContent, NotFound>> EliminarUsuario(string id, [FromServices] UserManager<IdentityUser> userManager)
        {
            var usuario = await userManager.FindByIdAsync(id);

            if (usuario is null)
            {
                return TypedResults.NotFound();
            }

            var resultado = await userManager.DeleteAsync(usuario);

            if (resultado.Succeeded)
            {
                return TypedResults.NoContent();
            }

            return TypedResults.NotFound();
        }

        static async Task<Results<NoContent, NotFound, BadRequest<IEnumerable<IdentityError>>>> AsignarRol(
            EditarClaimDTO editarClaimDTO,
            [FromServices] UserManager<IdentityUser> userManager,
            [FromServices] RoleManager<IdentityRole> roleManager)
        {
            var usuario = await userManager.FindByEmailAsync(editarClaimDTO.CorreoElectronico);

            if (usuario == null)
            {
                return TypedResults.NotFound();
            }

            if (!await roleManager.RoleExistsAsync(editarClaimDTO.TipoClaim))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(editarClaimDTO.TipoClaim));
                if (!roleResult.Succeeded)
                {
                    return TypedResults.BadRequest(roleResult.Errors);
                }
            }

            var roleAssignmentResult = await userManager.AddToRoleAsync(usuario, editarClaimDTO.TipoClaim);
            if (!roleAssignmentResult.Succeeded)
            {
                return TypedResults.BadRequest(roleAssignmentResult.Errors);
            }

            return TypedResults.NoContent();
        }

        static async Task<Results<NoContent, NotFound, BadRequest<string>>> RemoverRolAdmin(
            EditarClaimDTO editarClaimDTO,
            [FromServices] UserManager<IdentityUser> userManager)
        {
            var usuario = await userManager.FindByEmailAsync(editarClaimDTO.CorreoElectronico);

            if (usuario == null)
            {
                return TypedResults.NotFound();
            }

            if (await userManager.IsInRoleAsync(usuario, "esadmin"))
            {
                var roleRemovalResult = await userManager.RemoveFromRoleAsync(usuario, "esadmin");
                if (!roleRemovalResult.Succeeded)
                {
                    return TypedResults.BadRequest("Error al intentar remover el rol de administrador.");
                }
            }
            else
            {
                return TypedResults.BadRequest("El usuario no tiene el rol de administrador.");
            }

            return TypedResults.NoContent();
        }

        static async Task<Results<Ok<RespuestaAutenticacionDTO>, BadRequest<string>>> Login(
            UsuarioCrearDTO usuarioCrearDTO,
            [FromServices] UserManager<IdentityUser> userManager,
            [FromServices] SignInManager<IdentityUser> signInManager,
            [FromServices] IConfiguration configuration)
        {
            var usuario = await userManager.FindByEmailAsync(usuarioCrearDTO.CorreoElectronico);

            if (usuario == null)
            {
                return TypedResults.BadRequest("Login incorrecto");
            }

            var resultado = await signInManager.CheckPasswordSignInAsync(usuario, usuarioCrearDTO.Password, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                var respuestaAutenticacion = await ConstruirToken(usuarioCrearDTO, configuration, userManager);
                return TypedResults.Ok(respuestaAutenticacion);
            }
            else
            {
                return TypedResults.BadRequest("Login incorrecto");
            }
        }

        private async static Task<RespuestaAutenticacionDTO> ConstruirToken(
            UsuarioCrearDTO usuarioCrearDTO,
            IConfiguration configuration,
            UserManager<IdentityUser> userManager)
        {
            var claims = new List<Claim>
            {
                new Claim("email", usuarioCrearDTO.CorreoElectronico),
                new Claim("lo que yo quiera", "cualquier otro valor")
            };

            var usuario = await userManager.FindByNameAsync(usuarioCrearDTO.CorreoElectronico);
            var claimsDB = await userManager.GetClaimsAsync(usuario!);

            if (await userManager.IsInRoleAsync(usuario, "esadmin"))
            {
                claims.Add(new Claim("esadmin", "true"));
            }

            claims.AddRange(claimsDB);

            var llave = Llaves.ObtenerLlave(configuration);
            var creds = new SigningCredentials(llave.First(), SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddYears(1);

            var tokenDeSeguridad = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiracion,
                signingCredentials: creds);

            var token = new JwtSecurityTokenHandler().WriteToken(tokenDeSeguridad);

            return new RespuestaAutenticacionDTO
            {
                Token = token,
                Expiracion = expiracion
            };
        }
    }
}
