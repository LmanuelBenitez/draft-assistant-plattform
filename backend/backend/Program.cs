using backend.Extensions;
using backend.Middleware;

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

// Configurar Swagger
builder.Services.AddSwaggerDocumentation();

// Configurar FluentValidation
builder.Services.AddFluentValidationServices();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Middleware de manejo de errores global
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Draft Assistant API V1");
    });
}

app.UseHttpsRedirection();

app.UseCors("DefaultPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
