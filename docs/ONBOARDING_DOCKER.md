# 🐳 Onboarding Docker — Device Activity Monitor (DAM)

> Guía completa para que cualquier integrante nuevo del equipo levante el stack completo (API + Frontend) en Docker, entienda el flujo de construcción de imágenes y resuelva los problemas conocidos.

---

## 📑 Tabla de contenidos

1. [Arquitectura en Docker](#1-arquitectura-en-docker)
2. [Prerrequisitos](#2-prerrequisitos)
3. [Inicio rápido (docker compose)](#3-inicio-rápido-docker-compose)
4. [Verificación paso a paso](#4-verificación-paso-a-paso)
5. [Credenciales de prueba](#5-credenciales-de-prueba)
6. [Configuración](#6-configuración)
7. [Flujo completo de construcción de imágenes](#7-flujo-completo-de-construcción-de-imágenes)
8. [Comandos de referencia](#8-comandos-de-referencia)
9. [Troubleshooting (problemas reales resueltos)](#9-troubleshooting-problemas-reales-resueltos)
10. [Mapa de archivos Docker (canónicos vs experimentales)](#10-mapa-de-archivos-docker)
11. [Notas para producción](#11-notas-para-producción)
12. [Preguntas frecuentes de onboarding](#12-preguntas-frecuentes-de-onboarding)

---

## 1. Arquitectura en Docker

```
                            ┌─────────────────────────────┐
                            │      NAVEADOR (host)        │
                            │   http://localhost:3000     │
                            └──────────────┬──────────────┘
                                           │
                    ┌──────────────────────┼──────────────────────┐
                    │ SPA estática         │  llamadas HTTP/AJAX  │
                    │ (serve -s dist)      │  axios → baseURL     │
                    ▼                      ▼                      │
        ┌───────────────────────┐   ┌───────────────────────────┐ │
        │  dam-frontend (:3000) │   │     dam-api (:8081)       │ │
        │  node:22-alpine       │   │  .NET 8 (aspnet:8.0)      │ │
        │  imagen:              │   │  imagen: dam-api-fixed    │ │
        │  dam-frontend-react   │   │  FastEndpoints + EF Core  │ │
        └───────────────────────┘   └─────────────┬─────────────┘ │
                                                  │               │
                                                  ▼               │
                                     ┌─────────────────────────┐  │
                                     │  volumen: dam-data      │  │
                                     │  /app/data              │  │
                                     │  ├─ DAM_System.db (SQLite)│
                                     │  └─ Logs/               │  │
                                     └─────────────────────────┘  │
                                                                  │
   ⚠️ El navegador llama a la API en http://localhost:8081/api ◄──┘
      (la URL se hornea en el bundle con VITE_API_URL en el build)
```

| Servicio | Puerto host | Puerto contenedor | Imagen | Dockerfile canónico |
|----------|-------------|-------------------|--------|---------------------|
| API (.NET 8) | 8081 | 8080 | `dam-api-fixed` | `Dockerfile.api.fixed` |
| Frontend (React) | 3000 | 3000 | `dam-frontend-react` | `DAM.Frontend.React/Dockerfile.npm` |

> **Nota:** el contenedor `agrimpex-greenmail` (puertos 3025/3110/3143/3993/8080) es un **harness de pruebas de correo** independiente (GreenMail). **No** forma parte del stack de DAM y no debe confundirse con la API.

---

## 2. Prerrequisitos

| Requisito | Versión | Notas |
|-----------|---------|-------|
| Docker Engine | 24+ | `docker --version` |
| Docker Compose | v2+ | `docker compose version` (plugin, no el standalone `docker-compose`) |
| RAM libre | ≥ 4 GB | El build de .NET + Node requiere memoria |
| Disco | ≥ 3 GB libres | Imágenes base (.NET SDK ~1.8 GB, Node, caches) |

### 2.1 Red corporativa / proxy (entornos detrás de firewall)

Si estás en una red corporativa con proxy, se necesitan **dos** configuraciones:

**a) Proxy para que Docker descargue imágenes base** (`/etc/docker/daemon.json`):

```json
{
  "registry-mirrors": [
    "https://docker.1ms.run",
    "https://docker.xuanyuan.me",
    "https://dockerproxy.net"
  ]
}
```

Luego reiniciar Docker: `sudo systemctl restart docker`.

**b) Proxy para npm dentro del build del frontend:** ya está encapsulado en `DAM.Frontend.React/Dockerfile.npm` (variables `HTTP_PROXY`/`HTTPS_PROXY`). Si cambias de red, actualiza esas líneas o elimínalas si no hay proxy.

> **NuGet (build de la API):** el `dotnet restore` del `Dockerfile.api.fixed` sale directo a nuget.org. Si está bloqueado, agrega un `NuGet.config` con el proxy o configura las variables `HTTP_PROXY`/`HTTPS_PROXY` en la fase de build del Dockerfile (igual que el frontend).

### 2.2 Archivos de configuración incluidos en el repo

| Archivo | Propósito |
|---------|-----------|
| `docker-compose.yml` | Orquestación de API + Frontend (raíz) |
| `.env.example` | Plantilla de variables para compose |
| `.dockerignore` (raíz) | Excluye `node_modules`, `.git`, etc. del contexto de build de la API |
| `DAM.Frontend.React/.dockerignore` | Excluye `node_modules` del host (¡crítico! ver §9) |
| `DAM.Frontend.React/.npmrc` | Config de npm (registry, timeouts, retries) para el proxy |
| `appsettings.container.json` | Config de la API en contenedor (SQLite en `/app/data`, JWT, CORS) |

---

## 3. Inicio rápido (docker compose)

```bash
# 1. Clonar el repositorio
git clone https://github.com/potlitel/DeviceActivityMonitor.git
cd DeviceActivityMonitor

# 2. (Opcional) Crear .env para sobrescribir VITE_API_URL
cp .env.example .env

# 3. Levantar TODO (construye ambas imágenes si no existen)
docker compose up -d --build

# 4. Ver estado
docker compose ps
```

> ⏱️ **Primer build:** ~10-15 min (descarga imágenes base + restore NuGet + npm install + compilados). Los builds posteriores usan cache y tardan segundos.

> ⚠️ **Puertos ocupados:** si ya corren contenedores manuales en 8081/3000 (ej. `dam-api-test`, `dam-frontend-react`), detenlos primero:
> ```bash
> docker rm -f dam-api-test dam-frontend-react
> ```

---

## 4. Verificación paso a paso

```bash
# 4.1. Ambos contenedores arriba
docker compose ps
#   dam-api      Up ...   0.0.0.0:8081->8080/tcp
#   dam-frontend Up ...   0.0.0.0:3000->3000/tcp

# 4.2. Health de la API
curl http://localhost:8081/api/health/ping
#   → 200 OK

# 4.3. Login (obtener token JWT)
curl -s -X POST http://localhost:8081/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@dam.com","password":"Admin123!"}'
#   → {"success":true,"data":{"token":"eyJ..."}}

# 4.4. Swagger UI de la API (FastEndpoints)
#   Abrir en navegador: http://localhost:8081/swagger

# 4.5. Frontend React
#   Abrir en navegador: http://localhost:3000
#   Login con admin@dam.com / Admin123!

# 4.6. Logs en vivo
docker compose logs -f dam-api
docker compose logs -f dam-frontend
```

---

## 5. Credenciales de prueba

La API siembra usuarios automáticamente **solo en entorno `Development`** (ver `DAM.Infrastructure/Persistence/DbInitializer.cs`). El `docker-compose.yml` ya configura `ASPNETCORE_ENVIRONMENT: Development` para que funcione out-of-the-box.

| Email | Contraseña | Rol | Uso |
|-------|------------|-----|-----|
| `admin@dam.com` | `Admin123!` | Manager | Acceso total (auditoría, facturación, gestión) |
| `manager@dam.com` | `Manager123!` | Manager | Acceso total |
| `worker@dam.com` | `Worker123!` | Manager (sic) | Usuario de prueba adicional |

> ⚠️ **Estas credenciales son de desarrollo.** El JWT secreto también está en `appsettings.container.json` (clave débil de ejemplo). **Nunca** usar en producción (ver §11).

**Roles de la API** (`DAM.Core/Entities/ApplicationUser.cs`): `Manager` (acceso total) y `Worker` (lectura de actividades). Los endpoints de auditoría, facturación, presence y eventos requieren rol `Manager`.

---

## 6. Configuración

### 6.1 URL de la API para el frontend (`VITE_API_URL`)

El frontend React usa axios con `baseURL = VITE_API_URL` (`src/lib/api.ts`). **Vite hornea esta variable en el bundle durante el build**, por eso es un `ARG` del Dockerfile y un `build.arg` de compose.

Regla de oro: **la URL debe ser alcanzable desde el navegador**, no desde el contenedor.

| Escenario | `VITE_API_URL` |
|-----------|----------------|
| Dev local (compose) | `http://localhost:8081/api` (default) |
| Servidor remoto (navegador en otra máquina) | `http://<IP-servidor>:8081/api` |
| Misma red Docker con nginx proxy (avanzado) | `/api` + proxy inverso |

Cambiarla implica **reconstruir** la imagen del frontend (`docker compose up -d --build dam-frontend`).

### 6.2 Base de datos SQLite (volumen `dam-data`)

- La BD vive en el volumen Docker `dam-data` → `/app/data/DAM_System.db`.
- Los logs de la API van a `/app/data/Logs/`.
- **Resetear la BD** (vuelve a sembrar usuarios):
  ```bash
  docker compose down -v   # -v borra el volumen
  docker compose up -d
  ```

### 6.3 Puertos

Si 8081 o 3000 están ocupados, edita `docker-compose.yml`:

```yaml
    ports:
      - "9090:8080"   # API ahora en http://localhost:9090
```

Y ajusta `VITE_API_URL` en `.env` en consecuencia.

---

## 7. Flujo completo de construcción de imágenes

Este es el flujo real que se ejecutó para dejar el stack funcionando. Útil si necesitas reconstruir manualmente o debugguear.

### 7.1 Imagen de la API (`dam-api-fixed`)

```bash
# Desde la RAÍZ del repo (el contexto de build es la raíz)
docker build -f Dockerfile.api.fixed -t dam-api-fixed .

# Ejecutar (la BD se monta en /app/data para persistencia)
docker run -d -p 8081:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -v dam-data:/app/data \
  --name dam-api dam-api-fixed
```

**¿Por qué `Dockerfile.api.fixed` y no `Dockerfile.api`?**
- `Dockerfile.api` (raíz) restaura y compila **toda la solución** (incluye `DAM.Host.WindowsService` y el Blazor `DAM.Frontend`) → lento y propenso a fallar en Linux.
- `Dockerfile.api.fixed` compila **solo** `DAM.Api` + `DAM.Core` + `DAM.Infrastructure` (multi-stage: sdk:8.0 → aspnet:8.0), copia `appsettings.container.json` como `appsettings.json`, expone 8080. Es el canónico.

**Qué hace la API al arrancar** (`DAM.Api/Program.cs` + `ApplicationBuilderExtensions.cs`):
1. `MigrateAsync()` → aplica migraciones EF Core (crea `DAM_System.db` si no existe).
2. `DbInitializer.SeedAsync()` → si `ASPNETCORE_ENVIRONMENT=Development` y no hay usuarios, crea admin/manager/worker.
3. FastEndpoints + Swagger + CORS abierto (`DAMPolicy` con `AllowAnyOrigin`).

### 7.2 Imagen del frontend (`dam-frontend-react`)

```bash
# Desde DAM.Frontend.React/
docker build -f Dockerfile.npm \
  --build-arg VITE_API_URL=http://localhost:8081/api \
  -t dam-frontend-react .

# Ejecutar
docker run -d -p 3000:3000 --name dam-frontend dam-frontend-react
```

**Pipeline del `Dockerfile.npm` (multi-stage):**
1. `node:22-alpine` + proxy env → `npm install` (instala **devDependencies**: typescript, vite, plugins).
2. `ARG VITE_API_URL` → `ENV VITE_API_URL` (Vite lo lee del entorno).
3. `npm run build` = `tsc -b && vite build` (type-check + bundle con **rollup** → `dist/`).
4. Fase runner: `node:22-alpine` + `serve` → sirve `dist/` en puerto 3000 (SPA: `serve -s` reescribe rutas a `index.html`).

**Decisiones clave (y por qué):**
- **npm y no yarn:** yarn 1.x se colgaba en "Resolving packages..." detrás del proxy corporativo; npm avanzaba y completaba (con `.npmrc` de timeouts/retries).
- **vite 7.3.7 (rollup) y no vite 8 (rolldown):** sin lockfile, `^8.0.3` resolvió a vite 8.3.5 + rolldown 1.2.15, que tiene un bug (`builtin:vite-wasm-fallback` no existe en el enum de rolldown) y problemas de native bindings musl en alpine. vite 7 es rollup-based y estable.
- **Sin `package-lock.json` (estado actual):** el lockfile anterior se eliminó porque fijaba vite 8 (roto). El build funciona sin él. Para hacer builds reproducibles, regenerarlo con: `npm install --package-lock-only --legacy-peer-deps` (hay conflictos de peer deps entre `eslint@^10` y `@tanstack/eslint-plugin-query` / `typescript-eslint`).
- **`NODE_ENV` no se setea en build:** poner `NODE_ENV=production` en la fase de build hace que `npm install` omita devDependencies (typescript, vite) → `tsc: not found`. Ver §9.

---

## 8. Comandos de referencia

```bash
# --- Levantar / bajar ---
docker compose up -d --build        # build + up (detached)
docker compose up -d                # up sin rebuild (si las imágenes existen)
docker compose down                 # detener y remover contenedores (datos persisten)
docker compose down -v              # + borrar volumen (resetea la BD)
docker compose down --rmi all       # + borrar imágenes

# --- Observabilidad ---
docker compose ps
docker compose logs -f [dam-api|dam-frontend]
docker compose logs --tail=100 dam-api
docker compose exec dam-api sh      # shell dentro del contenedor API
docker compose exec dam-frontend sh # shell dentro del contenedor frontend

# --- Rebuild de un solo servicio ---
docker compose up -d --build dam-frontend
docker compose up -d --build dam-api

# --- Limpieza de espacio ---
docker image prune -a               # borra imágenes huérfanas
docker builder prune                # limpia cache de buildkit
docker system df                    # ver uso de disco
```

---

## 9. Troubleshooting (problemas reales resueltos)

Todos estos ya están resueltos en el repo, pero se documentan porque **vuelven a aparecer** si se modifican los Dockerfiles o se actualizan dependencias.

| # | Síntoma | Causa raíz | Solución aplicada |
|---|---------|------------|-------------------|
| 1 | `yarn install` se cuelga en "Resolving packages..." (10+ min) | yarn 1.x + proxy corporativo lento | Migrar a **npm** con `.npmrc` (timeouts/retries) |
| 2 | `error TS2339: Property 'pageNumber' does not exist on type '{}'` | Rutas sin `validateSearch`; `route.useSearch()` no tipado | Agregar `validateSearch` con **Zod** en las rutas (audit, invoices, presence, system-events, activities) |
| 3 | `Unknown option --check` en vite build | Vite 8 quitó `--no-check` de la CLI | Build script: `tsc -b && vite build` (sin `--no-check`) |
| 4 | `value "builtin:vite-wasm-fallback" does not match any variant of enum BindingBuiltinPluginName` | vite 8.3.5 + rolldown 1.2.15 incompatibles (drift por `^` sin lockfile) | Pin `vite: 7.3.7` (rollup-based) |
| 5 | `Cannot find native binding @rolldown/binding-linux-x64-musl` | `COPY . .` copiaba el `node_modules` **parcial del host** al contenedor | Crear `.dockerignore` en `DAM.Frontend.React` excluyendo `node_modules` |
| 6 | `sh: tsc: not found` | `ENV NODE_ENV=production` en fase de build → npm omite devDependencies | Quitar `NODE_ENV=production` de la fase de build (solo runner si acaso) |
| 7 | Login falla con "Credenciales inválidas" en BD fresca | `DbInitializer` solo siembra si `ASPNETCORE_ENVIRONMENT=Development` | compose setea `ASPNETCORE_ENVIRONMENT: Development` |
| 8 | `EACCES` instalando `node_modules` en el host | `node_modules` con dueño `root` de builds previas | No instalar en el host; **todo el build ocurre dentro de Docker**. Si hace falta: `sudo chown -R $USER:$USER DAM.Frontend.React` |
| 9 | `docker build` "context canceled" / se cancela | Build muy largo (>10 min) + timeout de herramienta | Lanzar con `setsid ... &` en background, o usar compose (no tiene timeout) |
| 10 | Imagen base no descarga | Sin mirrors de registry en red corporativa | `/etc/docker/daemon.json` con `registry-mirrors` (ver §2.1) |

**Regla de oro:** ante cualquier fallo de build, revisar primero (1) proxy/daemon.json, (2) `.dockerignore`, (3) `node_modules` del host no contaminen, (4) versiones pinadas.

---

## 10. Mapa de archivos Docker

### Canónicos (usar estos)

| Archivo | Estado | Notas |
|---------|--------|-------|
| `docker-compose.yml` | ✅ canónico | Orquesta API + Frontend |
| `Dockerfile.api.fixed` | ✅ canónico | API multi-stage .NET 8 (lean) |
| `DAM.Frontend.React/Dockerfile.npm` | ✅ canónico | Frontend multi-stage node 22 + npm + serve |
| `DAM.Frontend.React/.dockerignore` | ✅ canónico | Excluye `node_modules` (crítico, §9 #5) |
| `DAM.Frontend.React/.npmrc` | ✅ canónico | npm por proxy |
| `appsettings.container.json` | ✅ canónico | Config de la API en contenedor |
| `.dockerignore` (raíz) | ✅ canónico | Contexto liviano para build de la API |

### Experimentales / históricos (no usar; de sesiones de debugging)

`Dockerfile.api`, `Dockerfile.api.minimal`, `DAM.Api/Dockerfile.simple`, `DAM.Frontend.React/Dockerfile` (Blazor-era), `Dockerfile.simple`, `Dockerfile.node-serve`, `Dockerfile.optimized`, `Dockerfile.with-proxy`, `Dockerfile.production`, `Dockerfile.dev`, `Dockerfile.frontend.nginx`.

> 💡 Cuando el onboarding esté consolidado, se recomienda **borrar los Dockerfiles experimentales** y renombrar `Dockerfile.api.fixed` → `Dockerfile.api` y `Dockerfile.npm` → `Dockerfile` para simplificar.

### Otros directorios

- `DAM.Frontend/` → frontend **Blazor** (original). El activo para Docker es `DAM.Frontend.React/`.
- `simple-frontend/` → artefacto temporal de diagnóstico (HTML estático). Puede borrarse.
- `DAM.Host.WindowsService/` → worker de Windows (WMI). **No contenerizable** en Linux; se despliega vía `Create-Release.ps1` (ver README principal).

---

## 11. Notas para producción

Lo documentado aquí es para **desarrollo/onboarding**. Para producción:

1. **No usar `ASPNETCORE_ENVIRONMENT: Development`** (siembra usuarios de prueba). Proveer un mecanismo de seed seguro o migración de datos reales.
2. **Cambiar el JWT secreto** (`appsettings.container.json` → `JwtSettings:Secret`). Mejor aún: inyectar por variable de entorno (`JwtSettings__Secret`) o secrets manager.
3. **CORS:** la API usa `AllowAnyOrigin()` (`ServiceCollectionExtensions.cs`). Restringir a los orígenes del frontend en producción.
4. **HTTPS:** terminar TLS en un reverse proxy (nginx/traefik) delante del frontend y/o API.
5. **`VITE_API_URL`:** apuntar al dominio público de la API.
6. **Persistencia:** el volumen `dam-data` debe respaldarse (SQLite). Considerar migrar a SQL Server/Postgres para producción (`DAM.Infrastructure` soporta EF Core).
7. **Imágenes base:** pinear digests y escanear con `docker scout`/Trivy.
8. **Regenerar `package-lock.json`** del frontend para builds reproducibles: `npm install --package-lock-only --legacy-peer-deps` (conflictos de peer deps entre `eslint@^10` y `@tanstack/eslint-plugin-query`/`typescript-eslint`). El lockfile anterior se eliminó porque fijaba vite 8 (roto).

---

## 12. Preguntas frecuentes de onboarding

**¿Puedo correr el frontend sin Docker?**
Sí, pero el build de `node_modules` en el host tiene problemas de permisos (§9 #8). Si igual quieres: `npm install` (con `.npmrc` del repo) + `npm run dev` con `VITE_API_URL` en `.env.local`. La ruta recomendada es Docker.

**¿El frontend y la API deben estar en la misma red Docker?**
Para el *build* no. Para *runtime*, el navegador llama directamente a la API (axios), no el contenedor frontend. La red `dam-network` existe por si se agrega un nginx proxy `/api` en el futuro.

**¿Cómo veo la BD SQLite?**
```bash
docker compose exec dam-api ls -la /app/data
# o desde el host, si usas un bind mount en lugar de volumen nombrado
```

**¿Se pierde la BD al reconstruir imágenes?**
No. La BD está en el volumen `dam-data`, independiente de las imágenes. Solo se pierde con `docker compose down -v`.

**¿Por qué el login devuelve 401 con "Credenciales inválidas"?**
Casi siempre es que la BD está vacía (entorno Production sin seed). Ver §9 #7.

**¿Puedo cambiar la versión de Node/.NET de las imágenes base?**
Sí, pero la app está probada con `node:22-alpine` (frontend) y `sdk:8.0/aspnet:8.0` (API, `net8.0`). Cambiar versiones puede requerir ajustes (ej: Node 20 con Vite 7 también funciona).

---

*Documentación generada del flujo real de construcción del stack DAM. Ante dudas, revisar los logs: `docker compose logs -f`.*
