using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using WebAppApi.Database;
using WebAppApi.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using WebAppApi.Database.Interface;
using WebAppApi.Identity.Validation;
using Hangfire;
using FluentEmail.Core;
using FluentEmail.Smtp;
using FluentEmail.Razor;
using System.Net.Mail;
using System.Net;
using WebAppApi.BackGroundJob;
using WebAppApi.Identity.Entities;



var builder = WebApplication.CreateBuilder(args);

// Configura il logger
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Configura DbContext per Identity e per le altre entità
builder.Services.AddDbContext<eCommerceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrazione identity
builder.Services.AddDefaultIdentity<ApplicationUser>()
    .AddEntityFrameworkStores<eCommerceDbContext>();



// Configura il database per Hangfire
builder.Services.AddHangfire(x => x.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();


// Registro il servizio per l'uso della mail SMTP
builder.Services.AddFluentEmail(builder.Configuration["Email:FromEmail"])
    .AddRazorRenderer()
    .AddSmtpSender(new SmtpClient(builder.Configuration["Email:Smtp:Host"])
    {
        Port = int.Parse(builder.Configuration["Email:Smtp:Port"]),
        Credentials = new NetworkCredential(
            builder.Configuration["Email:Smtp:User"],
            builder.Configuration["Email:Smtp:Password"]),
        EnableSsl = true,
        UseDefaultCredentials = false
    });


// UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configura Swagger per supportare l'autenticazione JWT
builder.Services.AddSwaggerGen(s =>
{
    s.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAppApi", Version = "v1" });

    // Aggiunge la definizione dello schema di sicurezza
    s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Inserisci il token JWT nel formato: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Aggiunge il requisito di sicurezza globale a tutte le operazioni
    s.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Registra e configura JwtOptions
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

// Configura l'autenticazione JWT
var jwtOptions = new JwtOptions();
jwtOptions.Configure(builder.Configuration);

// Configura l'autenticazione JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = jwtOptions.SecurityKey
        };
    });


var assembly = typeof(Program).Assembly;
builder.Services.AddValidatorsFromAssembly(assembly);
builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));

// Validazione custom per identity ( registrazione )
builder.Services.AddValidatorsFromAssemblyContaining<RegisterModelValidator>();

// Supponendo che EmailService sia la tua classe che implementa l'interfaccia IEmailService
builder.Services.AddScoped<IEmailService, EmailService>();


var app = builder.Build();

// Abilita la dashboard di Hangfire per monitorare i job
app.UseHangfireDashboard();

// Mappa tutte le rotte presenti in Shared MapRoute
app.MapEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAppApi v1");
    });
}

// Dashboard di hangfire per seguire i job
app.UseHangfireDashboard();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Logger.LogInformation("Applicazione avviata correttamente."); 
app.Run();
