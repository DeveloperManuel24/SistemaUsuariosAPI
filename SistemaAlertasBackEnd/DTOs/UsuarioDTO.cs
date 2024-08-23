namespace SistemaAlertasBackEnd.DTOs
{
    public class UsuarioDTO
    {
        public string Id { get; set; }
        public string CorreoElectronico { get; set; }
        public string Password { get; set; }
        public string DerechosUsuario { get; set; }  // Para indicar si es un usuario general o un administrador.
        public DateTime FechaRegistro { get; set; } // Para la fecha de registro.
    }
}
