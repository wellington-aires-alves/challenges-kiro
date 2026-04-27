using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.API;
using TaskFlow.API.Middleware;
using TaskFlow.Core.Interfaces;
using TaskFlow.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Configuração de cultura (pt-BR)
var supportedCultures = new[] { new CultureInfo("pt-BR") };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("pt-BR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// ── Repositórios em memória (Singleton — compartilhados por toda a vida da aplicação)
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddSingleton<ITaskRepository, InMemoryTaskRepository>();

// ── Serviços de negócio (Scoped — uma instância por requisição)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITaskService, TaskService>();

// ── Autenticação JWT Bearer
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "TaskFlow",

            ValidateAudience = true,
            ValidAudience = "TaskFlow",

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// ── Session support for storing username
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(3);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ── Controllers (API REST) e Razor Pages (frontend)
builder.Services.AddControllers();
builder.Services.AddRazorPages();

var app = builder.Build();

// ── Configuração de localização
app.UseRequestLocalization();

// ── Pipeline de middlewares
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();

// Torna a classe Program visível para WebApplicationFactory nos testes de integração
public partial class Program { }
