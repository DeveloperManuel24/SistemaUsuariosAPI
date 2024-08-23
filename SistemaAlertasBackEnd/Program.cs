using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaAlertasBackEnd;
using SistemaAlertasBackEnd.EndPoints;
using SistemaAlertasBackEnd.Servicios;
using SistemaAlertasBackEnd.Utilidades;

var builder = WebApplication.CreateBuilder(args);

// Configurar servicios en el contenedor de dependencias
builder.Services.AddControllers();

// Configuración de Entity Framework y base de datos
builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
    opciones.UseSqlServer("name=DefaultConnection"));

// Configuración de Identity
builder.Services.AddIdentityCore<IdentityUser>()
     .AddRoles<IdentityRole>() // Agrega el manejo de roles
     .AddEntityFrameworkStores<ApplicationDbContext>()
     .AddDefaultTokenProviders();

builder.Services.AddScoped<UserManager<IdentityUser>>();
builder.Services.AddScoped<SignInManager<IdentityUser>>();

// Configuración de CORS
var origenesPermitidos = builder.Configuration.GetValue<string>("origenespermitidos")!;
builder.Services.AddCors(opciones =>
{
    opciones.AddDefaultPolicy(configuracion =>
    {
        configuracion.WithOrigins(origenesPermitidos).AllowAnyHeader().AllowAnyMethod();
    });

    opciones.AddPolicy("libre", configuracion =>
    {
        configuracion.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// Configuración de Autenticación y JWT
builder.Services.AddAuthentication().AddJwtBearer(opciones =>
{
    opciones.MapInboundClaims = false; // Necesario si quieres obtener el email del HttpContext
    opciones.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = Llaves.ObtenerLlave(builder.Configuration).First(), // Para una sola llave generada por nosotros mismos (lo mejor)
        ClockSkew = TimeSpan.Zero
    };
});

// Creación de Roles y Políticas de Autorización
builder.Services.AddAuthorization(opciones =>
{
    opciones.AddPolicy("esadmin", politica => politica.RequireClaim("esadmin"));
});

// Configuración de Output Cache
builder.Services.AddOutputCache();

// Configuración de Swagger para la documentación de la API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Inyección de dependencias para servicios personalizados
builder.Services.AddTransient<IServicioUsuarios, ServicioUsuarios>();
builder.Services.AddTransient<ServicioEmail>();

// Otros servicios
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(typeof(Program)); // Configuración AutoMapper
builder.Services.AddProblemDetails(); // Para el manejo de errores en caso de que una pantalla no se encuentre

// Registro de los validadores de FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Configuración del pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(); // Configuración de CORS

app.UseAuthentication(); // Autenticación antes de autorización
app.UseMiddleware<CustomAuthorizationMiddleware>();
app.UseAuthorization(); // Autorización después de autenticación

// Configuración de los EndPoints
app.MapGroup("/usuarios").MapUsuarios();

app.Run();
