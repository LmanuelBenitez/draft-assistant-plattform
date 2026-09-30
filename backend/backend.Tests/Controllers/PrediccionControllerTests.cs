using backend.Controllers;
using backend.DTOs.Request;
using backend.DTOs.Response;
using backend.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Tests.Controllers;

public class PrediccionControllerTests
{
    private readonly Mock<IPrediccionService> _serviceMock = new();
    private readonly Mock<ILogger<PrediccionController>> _loggerMock = new();
    private readonly PrediccionController _controller;

    public PrediccionControllerTests()
    {
        _controller = new PrediccionController(_serviceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GenerarPrediccion_ConDatosValidos_RetornaOk()
    {
        // Arrange
        var request = new PartidoRequestDto
        {
            Local = "Barcelona",
            Visitante = "Real Madrid",
            LigaId = "140",
            Temporada = "2024"
        };

        var response = new PrediccionResponseDto
        {
            Id = 1,
            Local = "Barcelona",
            Visitante = "Real Madrid",
            ProbabilidadLocal = 0.45,
            ProbabilidadEmpate = 0.30,
            ProbabilidadVisitante = 0.25
        };

        _serviceMock
            .Setup(x => x.GenerarPrediccionAsync(It.IsAny<PartidoRequestDto>()))
            .ReturnsAsync(response);

        // Act
        var resultado = await _controller.GenerarPrediccion(request);

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)resultado;
        ok.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task ObtenerPrediccion_ConIdExistente_RetornaOk()
    {
        // Arrange
        var response = new PrediccionResponseDto
        {
            Id = 1,
            Local = "Barcelona",
            Visitante = "Real Madrid"
        };

        _serviceMock
            .Setup(x => x.ObtenerPrediccionAsync(1))
            .ReturnsAsync(response);

        // Act
        var resultado = await _controller.ObtenerPrediccion(1);

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
    }
}