using backend.Extensions;
using backend.Middleware;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configurar servicios de base de datos
builder.Services.AddDatabaseServices(builder.Configuration);

// Configurar servicios de aplicación
builder.Services.AddApplicationServices(builder.Configuration);

// Configurar servicios de Football API
builder.Services.AddFootballApiService(builder.Configuration);

// Configurar CORS
builder.Services.AddCorsPolicies(builder.Configuration);


//Configurar OpenAPI
builder.Services.AddOpenApi();

// Configurar FluentValidation
builder.Services.AddFluentValidationServices();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Middleware de manejo de errores global
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors("DefaultPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
