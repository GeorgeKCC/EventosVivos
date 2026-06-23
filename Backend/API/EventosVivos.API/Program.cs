using Asp.Versioning;
using ModuloEvento;
using ModuloReporte;
using ModuloReserva;
using ModuloTarea;
using Scalar.AspNetCore;
using Transversal.Cache;
using Transversal.Database;
using Transversal.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.EventosVivosDataBaseRegisterService(builder.Configuration);
builder.Services.AddCustomException();
builder.Services.CacheRegisterServices(builder.Configuration);

builder.Services.ModuloEventoRegisterServices();
builder.Services.ModuloReservaRegisterServices();
builder.Services.ModuloReporteRegisterService();
builder.Services.ModuloTareaRegisterServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.Services.EventosVivosDataBaseExecuteSeed();
app.UseCustomException();

app.UseCors("AllowAngular");

app.UseAuthorization();

app.MapControllers();

app.Run();
