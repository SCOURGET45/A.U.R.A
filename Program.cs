using Aura.Data;
using Aura.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuraDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionAura")));

builder.Services.AddScoped<IAsistenciaService, AsistenciaService>();

var app = builder.Build();

app.MapGet("/", () => "OK");

app.Run();
