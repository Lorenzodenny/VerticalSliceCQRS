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


var builder = WebApplication.CreateBuilder(args);

// Configura il logger
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Configura DbContext per Identity e per le altre entità
builder.Services.AddDbContext<eCommerceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configura Identity
builder.Services.AddDefaultIdentity<IdentityUser>()
    .AddEntityFrameworkStores<eCommerceDbContext>();


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

//// Validatore personalizzato per identity
//builder.Services.AddValidatorsFromAssemblyContaining<RegisterModelValidator>();


var app = builder.Build();

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Logger.LogInformation("Applicazione avviata correttamente."); 
app.Run();
