namespace backend.Models;

/// <summary>
/// Respuesta de la API de fútbol
/// </summary>
public class FootballApiResponse
{
    public string Status { get; set; } = string.Empty;
    public List<PartidoApi> Response { get; set; } = new();
}

public class PartidoApi
{
    public PartidoInfo Fixture { get; set; } = new();
    public PartidoEquipos Teams { get; set; } = new();
    public PartidoGoles Goals { get; set; } = new();
}

public class PartidoInfo
{
    public DateTime Date { get; set; }
    public int Status { get; set; }  // 0 = pendiente, 1 = finalizado, etc.
}

public class PartidoEquipos
{
    public EquipoInfo Home { get; set; } = new();
    public EquipoInfo Away { get; set; } = new();
}

public class EquipoInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class PartidoGoles
{
    public int Home { get; set; }
    public int Away { get; set; }
}

/// <summary>
/// Estadísticas agregadas de un equipo
/// </summary>
public class EstadisticasEquipo
{
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
    public string Local { get; set; } = string.Empty;
    public string Visitante { get; set; } = string.Empty;
    public int GolesLocal { get; set; }
    public int GolesVisitante { get; set; }
    public DateTime Fecha { get; set; }
}