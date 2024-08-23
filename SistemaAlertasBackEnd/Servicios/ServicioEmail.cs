using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace SistemaAlertasBackEnd.Servicios
{
    public class ServicioEmail : IEmailSender
    {
        private readonly IConfiguration configuration;

        public ServicioEmail(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var fromEmail = configuration.GetValue<string>("CONFIGURACIONES_GMAIL:EMAIL");
            var password = configuration.GetValue<string>("CONFIGURACIONES_GMAIL:PASSWORD");
            var host = configuration.GetValue<string>("CONFIGURACIONES_GMAIL:HOST");
            var puerto = configuration.GetValue<int>("CONFIGURACIONES_GMAIL:PUERTO");

            // Agregar los logs para verificar los valores
            Console.WriteLine($"Email: {fromEmail}, Password: {password}, Host: {host}, Puerto: {puerto}");

            if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(host))
            {
                throw new InvalidOperationException("Gmail configuration is not set correctly in the configuration files.");
            }

            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email), "The recipient email address cannot be null or empty.");
            }

            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress("Guatemaltecos por la nutrición", fromEmail));
            mensaje.To.Add(new MailboxAddress("", email));
            mensaje.Subject = subject;

            mensaje.Body = new TextPart("html")
            {
                Text = htmlMessage
            };

            await EnviarEmail(mensaje, fromEmail, password, host, puerto);
        }

        public async Task SendEmailUpdatedNotificationAsync(string email,string password, string userName)
        {
            var subject = "Guatemaltecos por la nutrición - Actualización de Correo Electrónico";
            var htmlMessage = $@"
                <p>Hola: {userName},</p>
                <p>Queremos informarte que tu correo electrónico ha sido actualizado exitosamente en nuestra plataforma.</p>
                <p><strong>Nuevo Correo Electrónico:</strong> {email}</p>
                <p><strong>Nueva Contraseña:</strong> {password}</p>
                <p>Si no solicitaste esta actualización, por favor contáctanos de inmediato.</p>
                <p>Saludos,</p>
                <p>El equipo de Guatemaltecos por la nutrición</p>";

            await SendEmailAsync(email, subject, htmlMessage);
        }



        public async Task SendCredentialsEmailAsync(string email, string userName, string password)
        {
            var subject = "Guatemaltecos por la nutrición - Credenciales de Acceso";
            var htmlMessage = $@"
                <p>Hola: {userName},</p>
                <p>Has sido registrado en nuestra plataforma. A continuación, te proporcionamos tus credenciales de acceso:</p>
                <p><strong>Correo Electrónico:</strong> {email}</p>
                <p><strong>Contraseña:</strong> {password}</p>
                <p><strong>Advertencia:</strong> Por favor, mantén estas credenciales en un lugar seguro y no las compartas con nadie. La contraseña es una información sensible.</p>
                <p>Saludos,</p>
                <p>El equipo de Guatemaltecos por la nutrición</p>";

            await SendEmailAsync(email, subject, htmlMessage);
        }

        private async Task EnviarEmail(MimeMessage mensaje, string fromEmail, string password, string host, int puerto)
        {
            using (var cliente = new SmtpClient())
            {
                try
                {
                    await cliente.ConnectAsync(host, puerto, true);
                    await cliente.AuthenticateAsync(fromEmail, password);
                    await cliente.SendAsync(mensaje);
                    await cliente.DisconnectAsync(true);
                    Console.WriteLine("Email enviado exitosamente.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al enviar el email: {ex.Message}");
                    throw;
                }
            }
        }
    }
}
