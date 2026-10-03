# deploy.ps1
param(
    [string]$Tag = "latest",
    [string]$DockerUsername = $env:DOCKER_USERNAME
)

if (-not $DockerUsername) {
    Write-Host "   Error: DOCKER_USERNAME no está definido" -ForegroundColor Red
    Write-Host "   Uso: .\deploy.ps1 -Tag <sha> -DockerUsername <usuario>" -ForegroundColor Yellow
    exit 1
}

Write-Host "Deploying version $Tag" -ForegroundColor Cyan

# Actualizar backend
Write-Host "Actualizando backend..." -ForegroundColor Yellow
kubectl set image deployment/backend `
    backend=$DockerUsername/draft-backend:$Tag

# Actualizar frontend
Write-Host "Actualizando frontend..." -ForegroundColor Yellow
kubectl set image deployment/frontend `
    frontend=$DockerUsername/draft-frontend:$Tag

# Esperar rollout
Write-Host "Esperando rollout..." -ForegroundColor Yellow
kubectl rollout status deployment/backend --timeout=5m
kubectl rollout status deployment/frontend --timeout=5m

# Ver estado
Write-Host "✅ Deployment completado" -ForegroundColor Green
kubectl get pods