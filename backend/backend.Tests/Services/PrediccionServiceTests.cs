using backend.Data;
using backend.DTOs.Request;
using backend.Models;
using backend.Repositories.Interfaces;
using backend.Services.Implementations;
using backend.Services.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Tests.Services;

public class PrediccionServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IPoissonService> _poissonServiceMock = new();
    private readonly Mock<IDeepSeekService> _deepSeekServiceMock = new();
    private readonly Mock<IFootballApiService> _footballApiMock = new();
    private readonly Mock<IPrediccionRepository> _repositoryMock = new();   // ✅ NUEVO
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock = new(); // ✅ NUEVO
    private readonly Mock<ILogger<PrediccionService>> _loggerMock = new();

    public PrediccionServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
    }

    private PrediccionService CreateService() =>
        new(_context,
            _poissonServiceMock.Object,
            _deepSeekServiceMock.Object,
            _footballApiMock.Object,
            _repositoryMock.Object,       // ✅ NUEVO
            _scopeFactoryMock.Object,     // ✅ NUEVO
            _loggerMock.Object);

    [Fact]
    public async Task GenerarPrediccionAsync_ConDatosValidos_RetornaPrediccionCompleta()
    {
        // Arrange
        var request = new PartidoRequestDto
        {
            Local = "Barcelona",
            Visitante = "Real Madrid",
            LigaId = "140",
            Temporada = "2024"
        };

        SetupMocksBasicos();

        var service = CreateService();

        // Act
        var resultado = await service.GenerarPrediccionAsync(request);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Local.Should().Be("Barcelona");
        resultado.Visitante.Should().Be("Real Madrid");
        resultado.ProbabilidadLocal.Should().Be((decimal)0.45);
        resultado.ProbabilidadEmpate.Should().Be((decimal)0.30);
        resultado.ProbabilidadVisitante.Should().Be((decimal)0.25);
        resultado.Confianza.Should().Be((decimal)0.75);
    }

    [Fact]
    public async Task GenerarPrediccionAsync_ConLocalVacio_LanzaArgumentException()
    {
        // Arrange
        var request = new PartidoRequestDto
        {
            Local = "",
            Visitante = "Real Madrid",
            LigaId = "140",
            Temporada = "2024"
        };

        var service = CreateService();

        // Act
        Func<Task> act = () => service.GenerarPrediccionAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Local y Visitante son requeridos*");
    }

    [Fact]
    public async Task GenerarPrediccionAsync_ConEquipoMuyCorto_LanzaArgumentException()
    {
        // Arrange
        var request = new PartidoRequestDto
        {
            Local = "B",
            Visitante = "Real Madrid",
            LigaId = "140",
            Temporada = "2024"
        };

        var service = CreateService();

        // Act
        Func<Task> act = () => service.GenerarPrediccionAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*al menos 2 caracteres*");
    }

    [Fact]
    public async Task GenerarPrediccionAsync_GuardaEnBaseDeDatos()
    {
        // Arrange
        var request = new PartidoRequestDto
        {
            Local = "Barcelona",
            Visitante = "Real Madrid",
            LigaId = "140",
            Temporada = "2024"
        };

        SetupMocksBasicos();
        var service = CreateService();

        // Act
        await service.GenerarPrediccionAsync(request);

        // Assert
        var predicciones = await _context.Predicciones.ToListAsync();
        predicciones.Should().HaveCount(1);
        predicciones[0].Local.Should().Be("Barcelona");
    }

    [Fact]
    public async Task ObtenerPrediccionAsync_ConIdInexistente_LanzaArgumentException()
    {
        // Arrange
        var service = CreateService();

        // Act
        Func<Task> act = () => service.ObtenerPrediccionAsync(999);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*no encontrada*");
    }

    [Fact]
    public async Task ObtenerPrediccionAsync_ConIdExistente_RetornaPrediccion()
    {
        // Arrange
        var prediccion = new Prediccion
        {
            Local = "Barcelona",
            Visitante = "Real Madrid",
            GolesLocalPredichos = 2,
            GolesVisitantePredichos = 1,
            ProbabilidadLocal = (decimal)0.45,
            ProbabilidadEmpate = (decimal)0.30,
            ProbabilidadVisitante = (decimal)0.25,
            Confianza = (decimal)0.75,
            FechaPrediccion = DateTime.UtcNow
        };

        _context.Predicciones.Add(prediccion);
        await _context.SaveChangesAsync();

        var service = CreateService();

        // Act
        var resultado = await service.ObtenerPrediccionAsync(prediccion.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().Be(prediccion.Id);
        resultado.Local.Should().Be("Barcelona");
    }

    [Fact]
    public async Task ActualizarResultadosAsync_ConAcierto_AsignaPuntos()
    {
        // Arrange
        var prediccion = new Prediccion
        {
            Local = "Barcelona",
            Visitante = "Real Madrid",
            GolesLocalPredichos = 2,
            GolesVisitantePredichos = 1,
            ProbabilidadLocal = (decimal)0.45,
            ProbabilidadEmpate = (decimal)0.30,
            ProbabilidadVisitante = (decimal)0.25,
            Confianza = (decimal)0.75,
            FechaPrediccion = DateTime.UtcNow
        };

        _context.Predicciones.Add(prediccion);
        await _context.SaveChangesAsync();

        var service = CreateService();

        // Act
        await service.ActualizarResultadosAsync(prediccion.Id, 2, 1);

        // Assert
        var actualizada = await _context.Predicciones.FindAsync(prediccion.Id);
        actualizada!.EsAcertada.Should().BeTrue();
        actualizada.PuntosObtenidos.Should().BeGreaterThan(0);
    }

    private void SetupMocksBasicos()
    {
        _footballApiMock
            .Setup(x => x.GetEstadisticasEquipoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new EstadisticasEquipo
            {
                Equipo = "Test",
                PromedioGolesFavor = 1.8,
                PromedioGolesContra = 1.0,
                PartidosJugados = 10
            });

        _footballApiMock
            .Setup(x => x.GetPartidosHead2HeadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new List<PartidoHistorico>());

        _poissonServiceMock
            .Setup(x => x.CalcularProbabilidadesPartidoAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>()))
            .ReturnsAsync(((decimal)0.45, (decimal)0.30, (decimal)0.25));

        _poissonServiceMock
            .Setup(x => x.PredecirMarcadorAsync(It.IsAny<double>(), It.IsAny<double>()))
            .ReturnsAsync((2, 1));

        _poissonServiceMock
            .Setup(x => x.CalcularConfianzaAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(0.75);

        _deepSeekServiceMock
            .Setup(x => x.ObtenerPrediccionFutbolAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<EstadisticasEquipo>(),
                It.IsAny<EstadisticasEquipo>(),
                It.IsAny<decimal>(),
                It.IsAny<decimal>(),
                It.IsAny<decimal>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<List<PartidoHistorico>?>()))
            .ReturnsAsync("{\"explicacion\":\"Test\",\"factores_clave\":[],\"alertas\":[],\"recomendacion\":\"Victoria de Barcelona\"}");
    }

    public void Dispose() => _context.Dispose();
}