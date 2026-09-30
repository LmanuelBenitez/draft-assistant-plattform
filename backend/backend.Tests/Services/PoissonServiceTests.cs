using backend.Services.Implementations;
using FluentAssertions;
using Xunit;

namespace backend.Tests.Services;

public class PoissonServiceTests
{
    private readonly PoissonService _service = new();

    [Fact]
    public async Task CalcularProbabilidadesPartidoAsync_ConMediasValidas_SumaEsUno()
    {
        // Arrange
        var mediaLocal = 1.8;
        var mediaVisitante = 1.2;

        // Act
        var resultado = await _service.CalcularProbabilidadesPartidoAsync(mediaLocal, mediaVisitante);

        // Assert
        var suma = resultado.Local + resultado.Empate + resultado.Visitante;
        suma.Should().BeApproximately((decimal)1.0, (decimal)0.01);
    }

    [Fact]
    public async Task CalcularProbabilidadesPartidoAsync_ConLocalFavorito_FavoreceAlLocal()
    {
        // Arrange
        var mediaLocal = 3.0;
        var mediaVisitante = 0.5;

        // Act
        var resultado = await _service.CalcularProbabilidadesPartidoAsync(mediaLocal, mediaVisitante);

        // Assert
        resultado.Local.Should().BeGreaterThan(resultado.Visitante);
    }

    [Fact]
    public async Task CalcularProbabilidadesPartidoAsync_ConMediasIguales_ProbabilidadesSimilares()
    {
        // Arrange
        var media = 1.5;

        // Act
        var resultado = await _service.CalcularProbabilidadesPartidoAsync(media, media);

        // Assert
        resultado.Local.Should().BeApproximately(resultado.Visitante, (decimal)0.01);
    }

    [Fact]
    public async Task PredecirMarcadorAsync_ConMediasValidas_RetornaMarcadorNoNegativo()
    {
        // Arrange
        var mediaLocal = 1.5;
        var mediaVisitante = 1.0;

        // Act
        var resultado = await _service.PredecirMarcadorAsync(mediaLocal, mediaVisitante);

        // Assert
        resultado.GolesLocal.Should().BeGreaterThanOrEqualTo(0);
        resultado.GolesVisitante.Should().BeGreaterThanOrEqualTo(0);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0.5, 0.5)]
    [InlineData(2.0, 1.5)]
    [InlineData(3.0, 0.5)]
    public async Task CalcularConfianzaAsync_ConMediasValidas_RetornaEntreCeroYUno(double mediaLocal, double mediaVisitante)
    {
        // Act
        var resultado = await _service.CalcularConfianzaAsync(mediaLocal, mediaVisitante, 1, 1);

        // Assert
        resultado.Should().BeInRange(0, 1);
    }
}