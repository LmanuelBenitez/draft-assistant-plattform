using System.Text.Json.Serialization;

namespace backend.Models;

/// <summary>
/// Respuesta de la API de fútbol
/// </summary>
public class FootballApiResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("response")]
    public List<PartidoApi> Response { get; set; } = new();
}

public class PartidoApi
{
    [JsonPropertyName("fixture")]
    public PartidoInfo Fixture { get; set; } = new();

    [JsonPropertyName("teams")]
    public PartidoEquipos Teams { get; set; } = new();

    [JsonPropertyName("goals")]
    public PartidoGoles Goals { get; set; } = new();
}

public class PartidoInfo
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("status")]
    public EstatusInfo Status { get; set; } = new();
}

public class PartidoEquipos
{
    [JsonPropertyName("home")]
    public EquipoInfo Home { get; set; } = new();

    [JsonPropertyName("away")]
    public EquipoInfo Away { get; set; } = new();
}

public class EquipoInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class PartidoGoles
{
    [JsonPropertyName("home")]
    public int Home { get; set; }

    [JsonPropertyName("away")]
    public int Away { get; set; }
}

public class EstatusInfo
{
    [JsonPropertyName("long")]
    public string Long { get; set; } = string.Empty;

    [JsonPropertyName("short")]
    public string Short { get; set; } = string.Empty;
}

/// <summary>
/// Estadísticas agregadas de un equipo
/// </summary>
public class EstadisticasEquipo
{
    public string EquipoId { get; set; } = string.Empty;
    public string Equipo { get; set; } = string.Empty;
    public int PartidosJugados { get; set; }
    public double PromedioGolesFavor { get; set; }
    public double PromedioGolesContra { get; set; }
    public int Victorias { get; set; }
    public int Empates { get; set; }
    public int Derrotas { get; set; }
    public List<int> GolesPorPartido { get; set; } = new();
}

/// <summary>
/// Partido histórico para el repositorio local
/// </summary>
public class PartidoHistorico
{
    public int LocalId { get; set; }
    public int VisitanteId { get; set; }
    public string Local { get; set; } = string.Empty;
    public string Visitante { get; set; } = string.Empty;
    public int GolesLocal { get; set; }
    public int GolesVisitante { get; set; }
    public DateTime Fecha { get; set; }
}