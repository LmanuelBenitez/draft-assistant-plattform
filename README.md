# Draft Assistant Platform
App que predice resultados de partidos de fútbol usando Poisson + DeepSeek AI.

# ¿Qué hace?
Le metes dos equipos y una liga, y te devuelve:

. Probabilidades calculadas con Poisson
. Un análisis generado por IA (DeepSeek)
. Historial de predicciones guardadas

Los datos reales vienen de API-Football, y el texto lo genera DeepSeek.

STACK
# Frontend
. React 19 + TypeScript 6
. Vite 8 (build tool
. Tailwind CSS (estilos)
. Recharts (gráficos)
. React Hook Form (formularios)
. TanStack Query + Zustand (estado)
. Axios (HTTP)
. Nginx (producción)

# Backend
. ASP.NET Core 10 + C# 13
. Entity Framework Core 10
. SQLite (base de datos)
. FluentValidation
. Scalar + OpenAPI

# DevOps
. Docker + Docker Compose
. Docker Hub (registro)
. Nginx (proxy reverso)

# APIs externas
. API-Football v3 (datos de partidos)
. DeepSeek (análisis con IA)

# Backend
Repository - IPrediccionRepository, 
DI - Todo se inyecta en Program.cs, 
DTO - Separado de las entidades, 
Service Layer - PrediccionService orquesta todo, 
Strategy - IPoissonService, 
Options - IOptions<DeepSeekConfig>, 
Extension Methods	- ServiceExtensions, 
Primary Constructors - C# 12+

# Frontend
Container/Presentational - AppContent orquesta
Custom Hooks - usePrediccion
Controlled Components - React Hook Form
State	- Zustand + TanStack Query
Service Layer - api.ts
Barrel Exports - index.ts por carpeta
