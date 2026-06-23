using ModuloEvento;
using Scalar.AspNetCore;
using Transversal.Cache;
using Transversal.Database;
using Transversal.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.EventosVivosDataBaseRegisterService(builder.Configuration);
builder.Services.AddCustomException();
builder.Services.CacheRegisterServices(builder.Configuration);

builder.Services.ModuloEventoRegisterServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.Services.EventosVivosDataBaseExecuteSeed();
app.UseCustomException();

app.UseAuthorization();

app.MapControllers();

app.Run();
