using Asp.Versioning;
using EventosVivos.API.Middlewares;
using ModuloEvento;
using ModuloReporte;
using ModuloReserva;
using ModuloSecurity;
using ModuloTarea;
using Scalar.AspNetCore;
using Transversal.Cache;
using Transversal.Database;
using Transversal.Exceptions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new Microsoft.OpenApi.OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, Microsoft.OpenApi.IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["ApiKey"] = new Microsoft.OpenApi.OpenApiSecurityScheme
        {
            Type = Microsoft.OpenApi.SecuritySchemeType.ApiKey,
            Name = "X-Api-Key",
            In = Microsoft.OpenApi.ParameterLocation.Header,
            Description = "API Key requerida para acceder a los endpoints."
        };

        document.Security ??= [];
        document.Security.Add(new Microsoft.OpenApi.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.OpenApiSecuritySchemeReference("ApiKey"),
                new List<string>()
            }
        });

        return Task.CompletedTask;
    });
});

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
builder.Services.ModuloSecurityRegisterServices(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            var uri = new Uri(origin);
            return uri.Host == "localhost" ||
                   uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase);
        })
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Authentication = new ScalarAuthenticationOptions
        {
            PreferredSecurityScheme = "ApiKey"
        };
    });
}

app.Services.EventosVivosDataBaseExecuteSeed();
app.UseCustomException();

app.UseCors("AllowAngular");

app.UseMiddleware<ApiKeyMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
