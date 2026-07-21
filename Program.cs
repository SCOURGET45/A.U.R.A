using Aura.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAsistenciaService, AsistenciaService>();

var app = builder.Build();

app.MapGet("/", () => "OK");

app.Run();
