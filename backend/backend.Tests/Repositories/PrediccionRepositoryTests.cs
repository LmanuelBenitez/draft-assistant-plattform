using backend.Data;
using backend.Models;
using backend.Repositories.Implementations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Tests.Repositories;

public class PrediccionRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly PrediccionRepository _repository;

    public PrediccionRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new PrediccionRepository(_context);
    }

    [Fact]
    public async Task AddAsync_GuardaPrediccionEnBD()
    {
        // Arrange
        var prediccion = new Prediccion
        {
            Local = "Barcelona",
            Visitante = "Real Madrid",
            GolesLocalPredichos = 2,
            GolesVisitantePredichos = 1,
            FechaPrediccion = DateTime.UtcNow
        };

        // Act
        var resultado = await _repository.AddAsync(prediccion);

        // Assert
        resultado.Id.Should().BeGreaterThan(0);
        var guardada = await _context.Predicciones.FindAsync(resultado.Id);
        guardada.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ConIdExistente_RetornaPrediccion()
    {
        // Arrange
        var prediccion = new Prediccion
        {
            Local = "Barcelona",
            Visitante = "Real Madrid",
            FechaPrediccion = DateTime.UtcNow
        };
        _context.Predicciones.Add(prediccion);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _repository.GetByIdAsync(prediccion.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Local.Should().Be("Barcelona");
    }

    [Fact]
    public async Task GetByIdAsync_ConIdInexistente_RetornaNull()
    {
        // Act
        var resultado = await _repository.GetByIdAsync(999);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GetByEquipoAsync_ConEquipoLocal_RetornaPredicciones()
    {
        // Arrange
        _context.Predicciones.AddRange(
            new Prediccion { Local = "Barcelona", Visitante = "Real Madrid", FechaPrediccion = DateTime.UtcNow },
            new Prediccion { Local = "Barcelona", Visitante = "Atletico", FechaPrediccion = DateTime.UtcNow },
            new Prediccion { Local = "Sevilla", Visitante = "Valencia", FechaPrediccion = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var resultados = new List<Prediccion>();
        await foreach (var p in _repository.GetByEquipoAsync("Barcelona"))
        {
            resultados.Add(p);
        }

        // Assert
        resultados.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteAsync_ConIdExistente_RetornaTrue()
    {
        // Arrange
        var prediccion = new Prediccion
        {
            Local = "Barcelona",
            Visitante = "Real Madrid",
            FechaPrediccion = DateTime.UtcNow
        };
        _context.Predicciones.Add(prediccion);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _repository.DeleteAsync(prediccion.Id);

        // Assert
        resultado.Should().BeTrue();
        var eliminada = await _context.Predicciones.FindAsync(prediccion.Id);
        eliminada.Should().BeNull();
    }

    public void Dispose() => _context.Dispose();
}