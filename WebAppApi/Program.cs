using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using WebAppApi.Database;
using WebAppApi.Shared;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configura il logger
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug); 

// Aggiunta del DbContext al sistema di iniezione delle dipendenze
builder.Services.AddDbContext<eCommerceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(s =>
{
    s.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAppApi", Version = "v1" });
});

var assembly = typeof(Program).Assembly;
builder.Services.AddValidatorsFromAssembly(assembly);
builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(assembly));

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

app.UseAuthorization();
app.MapControllers();

app.Logger.LogInformation("Applicazione avviata correttamente."); 
app.Run();
