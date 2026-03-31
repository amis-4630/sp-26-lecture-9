---
name: "DevOps / CI-CD"
description: "GitHub Actions workflows, Docker multi-stage builds, and Azure deployment (azd, Bicep, App Service) conventions for this workspace."
---

# DevOps / CI-CD Skill

Use this skill when creating CI/CD pipelines, Docker configurations, or Azure deployment infrastructure.

---

## GitHub Actions Workflows

### Workflow Location

All workflows go in `.github/workflows/`. Name files descriptively: `ci.yml`, `deploy-backend.yml`, `deploy-frontend.yml`.

### Build Workflow Template (.NET + React)

```yaml
name: CI

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

permissions:
  contents: read

jobs:
  build-backend:
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: ./buckeye-lending/backend/Buckeye.Lending.Api
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore --configuration Release

      - name: Test
        run: dotnet test --no-build --configuration Release --verbosity normal

  build-frontend:
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: ./buckeye-lending/frontend
    steps:
      - uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '22'
          cache: 'npm'
          cache-dependency-path: './buckeye-lending/frontend/package-lock.json'

      - name: Install dependencies
        run: npm ci

      - name: Lint
        run: npm run lint

      - name: Build
        run: npm run build
```

### Key Principles

- **Pin action versions** to full SHA or major version tags (`@v4`).
- **Use `permissions`** at job level with least privilege.
- **Cache dependencies**: `actions/setup-dotnet` caches NuGet by default; use `cache: 'npm'` for Node.
- **Use `npm ci`** (not `npm install`) in CI for reproducible builds.
- **Set `working-directory`** in `defaults.run` when the project isn't at repo root.
- **Separate jobs** for backend and frontend — they run in parallel.

---

## Docker

### Multi-Stage Dockerfile (.NET)

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Run as non-root
RUN adduser --disabled-password --gecos '' appuser
USER appuser

EXPOSE 8080
ENTRYPOINT ["dotnet", "Buckeye.Lending.Api.dll"]
```

### Multi-Stage Dockerfile (React/Vite)

```dockerfile
# Build stage
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . ./
RUN npm run build

# Serve stage
FROM nginx:alpine AS runtime
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
```

### .dockerignore

```
bin/
obj/
node_modules/
.git/
*.md
.env
```

### Docker Principles

- Multi-stage builds: SDK/Node for build, runtime-only for final image.
- Copy lock files first, restore/install, then copy source (layer caching).
- Run as non-root user in production.
- Use Alpine variants for smaller images.
- Never copy `.env`, secrets, or `.git` into images.

---

## Azure Deployment

### Azure Developer CLI (azd)

Use `azd` for provisioning and deployment. Project structure:

```
azure.yaml          # azd project definition
infra/
├── main.bicep      # Orchestrator
├── main.parameters.json
├── app/
│   └── api.bicep   # App Service for backend
└── core/
    └── monitor.bicep
```

### azure.yaml

```yaml
name: buckeye-lending
services:
  api:
    host: appservice
    language: dotnet
    project: ./buckeye-lending/backend/Buckeye.Lending.Api
  web:
    host: appservice
    language: js
    project: ./buckeye-lending/frontend
```

### Bicep Conventions

- Use parameter files for environment-specific values.
- Tag all resources: `environment`, `project`, `managedBy`.
- Use managed identities — never connection strings with passwords.
- Enable diagnostic logging to Log Analytics.
- Use deployment slots for zero-downtime production deploys.

### App Service Configuration

```bicep
resource appService 'Microsoft.Web/sites@2024-04-01' = {
  name: appServiceName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v10.0'
      alwaysOn: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}
```

### Deployment Commands

```bash
# Initialize azd project
azd init

# Provision infrastructure
azd provision

# Deploy application
azd deploy

# Provision + deploy in one step
azd up

# Tear down
azd down
```

### Secrets Management

- **GitHub Secrets** for CI/CD credentials (`AZURE_CREDENTIALS`, `AZURE_SUBSCRIPTION_ID`).
- **Azure Key Vault** for application secrets.
- **Never** store secrets in `appsettings.json`, environment files, or source code.
- Use `azd env set` for non-secret environment configuration.

---

## Environment Configuration

### Development

| Service | URL |
|---------|-----|
| Backend API | `http://localhost:5000` |
| Frontend (Vite) | `http://localhost:5173` |
| Swagger UI | `http://localhost:5000/swagger` |

### Production

- HTTPS enforced (`httpsOnly: true`).
- CORS restricted to the frontend domain.
- Swagger UI disabled.
- `ASPNETCORE_ENVIRONMENT=Production`.
