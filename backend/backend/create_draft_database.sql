-- ============================================
-- CREAR BASE DE DATOS (SQLite)
-- ============================================
-- SQLite usa archivos, así que "crear" la base de datos
-- es simplemente crear el archivo .db
-- Esto se hace al conectarse a la base de datos

-- ============================================
-- TABLA: Partidos
-- ============================================
CREATE TABLE IF NOT EXISTS Partidos (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Local TEXT NOT NULL,
    Visitante TEXT NOT NULL,
    FechaHora TEXT NOT NULL,  -- ISO8601: 'YYYY-MM-DD HH:MM:SS'
    Estadio TEXT NULL,
    Competicion TEXT NULL,
    GolesLocal INTEGER NULL,
    GolesVisitante INTEGER NULL,
    Finalizado INTEGER NOT NULL DEFAULT 0,  -- 0 = false, 1 = true
    Estado TEXT NULL DEFAULT 'Programado'
);

-- Índices de Partidos
CREATE INDEX IF NOT EXISTS IX_Partidos_FechaHora ON Partidos(FechaHora);
CREATE INDEX IF NOT EXISTS IX_Partidos_Estado ON Partidos(Estado);
CREATE INDEX IF NOT EXISTS IX_Partidos_FechaHora_Estado ON Partidos(FechaHora, Estado);

-- ============================================
-- TABLA: Predicciones
-- ============================================
CREATE TABLE IF NOT EXISTS Predicciones (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Local TEXT NOT NULL,
    Visitante TEXT NOT NULL,
    LigaIdLocal INTEGER NULL,
    LigaIdVisitante INTEGER NULL,
    Temporada TEXT NULL,
    Competicion TEXT NULL,
    Estadio TEXT NULL,
    Bajas TEXT NULL,
    Contexto TEXT NULL,
    GolesLocalPredichos INTEGER NOT NULL,
    GolesVisitantePredichos INTEGER NOT NULL,
    ProbabilidadLocal REAL NOT NULL,
    ProbabilidadEmpate REAL NOT NULL,
    ProbabilidadVisitante REAL NOT NULL,
    Confianza REAL NOT NULL,
    PromedioGolesLocal REAL NOT NULL,
    PromedioGolesVisitante REAL NOT NULL,
    AnalisisDeepSeek TEXT NULL,
    FechaPrediccion TEXT NOT NULL DEFAULT (datetime('now')),
    EsAcertada INTEGER NOT NULL DEFAULT 0,
    PuntosObtenidos INTEGER NULL,
    GolesRealesLocal INTEGER NULL,
    GolesRealesVisitante INTEGER NULL
);

-- Índices de Predicciones
CREATE INDEX IF NOT EXISTS IX_Predicciones_FechaPrediccion ON Predicciones(FechaPrediccion);
CREATE INDEX IF NOT EXISTS IX_Predicciones_EsAcertada ON Predicciones(EsAcertada);
CREATE INDEX IF NOT EXISTS IX_Predicciones_Local ON Predicciones(Local);
CREATE INDEX IF NOT EXISTS IX_Predicciones_Visitante ON Predicciones(Visitante);