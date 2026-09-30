using backend.DTOs.Request;
using backend.Validators;
using FluentAssertions;
using Xunit;

namespace backend.Tests.Validators;

public class PartidoRequestValidatorTests
{
    private readonly PartidoRequestValidator _validator = new();

    [Fact]
    public void Validar_ConDatosValidos_RetornaExito()
    {
        // Arrange
        var request = new PartidoRequestDto
        {
            Local = "Barcelona",
            Visitante = "Real Madrid",
            LigaId = "140",
            Temporada = "2024"
        };

        // Act
        var resultado = _validator.Validate(request);

        // Assert
        resultado.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Real Madrid")]
    [InlineData("Barcelona", "")]
    [InlineData("B", "Real Madrid")]
    [InlineData("Barcelona", "R")]
    public void Validar_ConDatosInvalidos_RetornaError(string local, string visitante)
    {
        // Arrange
        var request = new PartidoRequestDto
        {
            Local = local,
            Visitante = visitante,
            LigaId = "140",
            Temporada = "2024"
        };

        // Act
        var resultado = _validator.Validate(request);

        // Assert
        resultado.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validar_ConEquiposIguales_RetornaError()
    {
        // Arrange
        var request = new PartidoRequestDto
        {
            Local = "Barcelona",
            Visitante = "Barcelona",
            LigaId = "140",
            Temporada = "2024"
        };

        // Act
        var resultado = _validator.Validate(request);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.ErrorMessage.Contains("no pueden ser el mismo"));
    }

    [Theory]
    [InlineData("2024")]
    [InlineData("2023")]
    [InlineData("2022")]
    public void Validar_ConTemporadaValida_RetornaExito(string temporada)
    {
        // Arrange
        var request = new PartidoRequestDto
        {
            Local = "Barcelona",
            Visitante = "Real Madrid",
            LigaId = "140",
            Temporada = temporada
        };

        // Act
        var resultado = _validator.Validate(request);

        // Assert
        resultado.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("24")]
    [InlineData("20244")]
    [InlineData("abc")]
    [InlineData("")]
    public void Validar_ConTemporadaInvalida_RetornaError(string temporada)
    {
        // Arrange
        var request = new PartidoRequestDto
        {
            Local = "Barcelona",
            Visitante = "Real Madrid",
            LigaId = "140",
            Temporada = temporada
        };

        // Act
        var resultado = _validator.Validate(request);

        // Assert
        resultado.IsValid.Should().BeFalse();
    }
}