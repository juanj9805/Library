---
date: 2026-04-25
tags: [docker, deployment, mysql, ef-core, csharp, networking, debugging, production]
project: M5-bookstore
status: resolved
duration: ~2h
---

# Bugfix: HTTP 500 al desplegar app .NET en VPS

> **Síntoma reportado:** `bookstore.barrera.andrescortes.dev` devuelve HTTP 500.
>
> **Causa real:** cinco bugs encadenados, cada uno tapando al siguiente.

---

## 1. Stack del despliegue

- **VPS** Linux con Nginx + Certbot como reverse proxy.
- **App** ASP.NET Core (`net10.0`, MVC), EF Core con Pomelo MySQL.
- **DB** contenedor `juansql` (MySQL 9.6.0).
- **Routing** subdominio `bookstore.barrera.andrescortes.dev` → Nginx → contenedor app puerto 8010 → 8080 interno.

---

## 2. Metodología de diagnóstico (la parte importante)

### Regla #0 — Distinguir 500 de 502 antes de tocar nada

| Código | Capa que falla | Dónde mirar |
|---|---|---|
| **502 Bad Gateway** | Nginx no alcanza la app | config Nginx, contenedor caído, puerto incorrecto |
| **500 Internal Server** | La app responde pero lanza excepción | logs del contenedor |

**Tratar ambos como "el deploy no funciona" es el error #1.** Son problemas en capas distintas.

### Regla #1 — Leer el log antes de cambiar código

```bash
docker ps                                    # nombre del contenedor
docker logs --tail=200 <nombre>              # excepción completa
```

No "fix" por intuición. El log te dice exactamente qué excepción se lanza, dónde, y con qué mensaje. Buscar la línea con la excepción más interna (la `MySqlException` real, no el wrapper de ASP.NET).

### Regla #2 — Verificar capa por capa

Cuando el log dice "no puedo conectar a MySQL", hay 6 capas posibles que pueden fallar. Verificar en orden:

```bash
# 1. ¿El contenedor MySQL existe y está vivo?
docker ps | grep mysql
docker exec <db> mysqladmin -uroot -p<password> ping

# 2. ¿En qué red Docker viven ambos?
docker inspect <app> --format '{{range $k,$v := .NetworkSettings.Networks}}{{$k}} {{end}}'
docker inspect <db>  --format '{{range $k,$v := .NetworkSettings.Networks}}{{$k}} {{end}}'

# 3. ¿La app puede resolver el nombre del DB?
docker exec <app> getent hosts <db_name>

# 4. ¿El puerto está abierto desde la red Docker?
docker run --rm --network <shared_net> busybox nc -zv <db_name> 3306

# 5. ¿Qué connection string usa REALMENTE el contenedor (no el repo)?
docker exec <app> cat /app/appsettings.json

# 6. ¿La DB existe? ¿Las tablas existen?
docker exec <db> mysql -uroot -p<password> -e "SHOW DATABASES;"
docker exec <db> mysql -uroot -p<password> -e "USE <dbname>; SHOW TABLES;"
```

---

## 3. Los cinco bugs encadenados

| # | Bug | Cómo se manifestó | Capa |
|---|---|---|---|
| 1 | App container en red `bridge` default, DB en red user-defined `bookstore-net` | `getent hosts juansql` → "DNS NO RESUELVE" | Red Docker |
| 2 | Connection string usaba `Port=3390` (puerto host-mapped), no `3306` (interno) | TCP fallaba aunque DNS funcionara | Configuración |
| 3 | `appsettings.json` en VPS editado a mano sin commit → `git pull` bloqueado | "Your local changes would be overwritten" | Git workflow |
| 4 | Dos repos GitHub diferentes para el mismo proyecto | Push iba a uno, deploy pulleaba del otro | Infra git |
| 5 | DB `bookStore` existía pero migraciones nunca aplicadas | `Table 'bookStore.users' doesn't exist` | EF Core |

Cada bug tapaba al siguiente. Resolverlos requirió **iterar diagnóstico → fix → re-verificar**, no aplicar un parche grande.

---

## 4. Conceptos técnicos críticos (los que tenía que saber)

### 4.1 — Docker `bridge` default vs user-defined

```
bridge (default)           bridge user-defined
=======================    =========================
DNS por nombre: NO         DNS por nombre: SÍ
Existe siempre             Se crea con docker network create
Compartido por todos       Solo containers que la unan
```

**Regla:** nunca correr stack multi-contenedor en `bridge` default. Crear red user-defined explícita.

### 4.2 — Container port vs host port

Cuando Docker mapea `-p 3390:3306`:
- **Desde el host** (la VPS): `localhost:3390` ✓
- **Desde otro contenedor en la misma red Docker**: `<container_name>:3306` ✓
- **Desde otro contenedor pero usando 3390**: ✗ — ese puerto solo existe en el host

Confundir los dos es uno de los errores más comunes en Docker networking.

### 4.3 — `Database.Migrate()` al arranque

```csharp
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MysqlDbcontext>();
    db.Database.Migrate();
}
```

**Pro:** todo deploy auto-migra. Reproducible.
**Contra:** acopla ciclo de vida app↔DB. Si una migración falla, la app no arranca. Para producción crítica, separar a step de CI/CD.

### 4.4 — Errores wrapper de MySqlConnector

`MySqlException: Unable to connect to any of the specified MySQL hosts` es **un mensaje genérico** que envuelve:
- Falla de DNS.
- Falla de TCP.
- Falla de auth.
- Falla de TLS.
- Falla del handshake del protocolo MySQL.

**No tomarlo literal.** El error real puede no estar en el wrapper. Verificar capa por capa con herramientas externas (`nc`, `mysql client` desde otro contenedor).

---

## 5. Solución paso a paso (el orden importa)

### Paso 1 — Sincronizar código local con producción

```bash
# Local (Windows)
git status
git log --oneline -5
git push origin main
```

Antes de cualquier deploy: **confirmar que el remote tiene el commit que crees**.

### Paso 2 — Sincronizar VPS con repo

```bash
# VPS
cd /var/www/juan-barrera/Library
git stash                           # si hay cambios locales no commiteados
git pull origin main
grep -i port appsettings.json       # verificar que el archivo en disco refleja lo esperado
```

### Paso 3 — Conectar el contenedor a la red Docker correcta

Si los contenedores no comparten red user-defined, **no se ven**. Asegurar que la app vive en la misma red que la DB:

```bash
# Verificar redes existentes
docker network ls

# El contenedor de DB debe estar en una red user-defined
docker inspect juansql --format '{{json .NetworkSettings.Networks}}'

# Lanzar la app explícitamente en esa red (NO usar bridge default)
docker run -d \
  --name jb-bookstore-container \
  --network bookstore-net \
  -p 8010:8080 \
  --restart unless-stopped \
  jb-bookstore-img
```

### Paso 4 — Connection string usando el puerto interno

En `appsettings.json`:

```json
"ConnectionStrings": {
  "mysqlConntection": "Server=juansql;Port=3306;Database=bookStore;User=root;Password=password;"
}
```

- `Server` = nombre del contenedor MySQL (no IP, no `localhost`).
- `Port` = puerto interno de MySQL (3306), NO el puerto mapeado al host.

### Paso 5 — Auto-migrar al arrancar

En `Program.cs`, después de `var app = builder.Build();`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MysqlDbcontext>();
    db.Database.Migrate();
}
```

### Paso 6 — Rebuild SIN cache, redeploy

```bash
docker stop jb-bookstore-container
docker rm jb-bookstore-container
docker build --no-cache -t jb-bookstore-img .
docker run -d \
  --name jb-bookstore-container \
  --network bookstore-net \
  -p 8010:8080 \
  --restart unless-stopped \
  jb-bookstore-img
```

`--no-cache` previene que Docker reutilice un layer viejo con el archivo anterior.

### Paso 7 — VERIFICACIÓN OBLIGATORIA (la regla #1 que olvidé)

```bash
# El contenedor tiene el archivo nuevo?
docker exec jb-bookstore-container cat /app/appsettings.json | grep -i port

# Las migraciones se aplicaron?
docker exec juansql mysql -uroot -ppassword -e "USE bookStore; SHOW TABLES;"

# La app responde?
docker logs --tail=80 jb-bookstore-container
curl -i http://localhost:8010/

# Y desde fuera?
curl -i https://bookstore.barrera.andrescortes.dev/
```

Esperar `200 OK` en los dos curl. Esperar las tablas (`users`, `books`, `loans`, `loanBooks`, `__EFMigrationsHistory`). Esperar líneas `Applying migration '...'` en el log.

---

## 6. Errores de proceso (los míos)

### 6.1 — No verificar estado del contenedor tras rebuild

Hice cambios en `appsettings.json`, hicimos `docker build`, y NO verificamos con `docker exec ... cat /app/appsettings.json` que el archivo cambió. Resultado: 3 vueltas adivinando entre TLS, bind-address, auth plugin, cuando el bug era simplemente "el contenedor sigue con el archivo viejo".

**Lección:** después de cualquier `docker build`, el primer comando es `docker exec <container> cat <archivo_modificado>`.

### 6.2 — Asumir que git push y git pull alcanzaban su destino

Pusheé local sin verificar el remote. La VPS pulleaba de un repo distinto (renombrado/duplicado). Resultado: cambio en repo A, deploy desde repo B, parecía que nada se actualizaba.

**Lección:** `git remote -v` en ambas máquinas antes del primer deploy.

### 6.3 — No diagnosticar por qué `git checkout` falló

Cuando `git checkout -- appsettings.json` no descartó los cambios, propuse `git stash` como solución sin entender la causa. Funcionó, pero la causa real (probablemente line endings CRLF/LF entre Windows y Linux, o encoding) sigue sin diagnosticar.

**Lección:** entender el problema antes de aplicar el bypass. `git status` y `git diff` siempre primero.

---

## 7. Anti-patterns identificados (para no repetir)

| Anti-pattern | Síntoma | Solución |
|---|---|---|
| Editar archivos en producción sin commit | `git pull` falla con "local changes" | Workflow: SIEMPRE commit, en cualquier máquina |
| Lanzar contenedores con `docker run` manual | Estado del deploy solo en mi cabeza | `docker-compose.yml` versionado en el repo |
| Connection string commiteada con password en plano | Credenciales filtradas en git history | Variable de entorno + `.env` gitignored |
| Usar `:latest` como tag | "¿qué versión está corriendo?" | Tag por SHA del commit: `app:abc1234` |
| Múltiples contenedores MySQL en la VPS, uno por proyecto | 4 servicios redundantes, cada uno con config propia | Un solo MySQL con databases separadas y users dedicados |
| `obj/` y `bin/` commiteados al repo | Diffs de ruido cada compilación | `git rm -r --cached`, añadir a `.gitignore` |
| `app.UseHttpsRedirection()` detrás de Nginx | Loops 307, requests duplicados | Eliminar o condicionar a `IsDevelopment()` |

---

## 8. Comandos de diagnóstico Docker (cheat sheet)

```bash
# Identidad y estado
docker ps                                       # contenedores corriendo
docker ps -a                                    # incluyendo apagados
docker logs --tail=N -f <container>             # logs (seguimiento)
docker inspect <container>                      # info completa (JSON)

# Red Docker
docker network ls                               # redes existentes
docker network inspect <net>                    # quién está en la red
docker inspect <c> --format '{{range $k,$v := .NetworkSettings.Networks}}{{$k}} {{end}}'

# Conectividad entre contenedores
docker exec <c> getent hosts <other>            # DNS interno
docker run --rm --network <net> busybox nc -zv <host> <port>   # TCP test

# Puertos
docker port <container>                         # mapeo puerto host:contenedor

# Inspeccionar el contenido del contenedor
docker exec <c> cat /app/appsettings.json
docker exec <c> env

# Conectar/desconectar de redes en runtime
docker network connect <net> <container>
docker network disconnect <net> <container>
```

---

## 9. Cómo hubiera evitado todo esto desde el inicio

Si el proyecto hubiera tenido **desde el día uno** un `docker-compose.yml` así:

```yaml
services:
  app:
    build: .
    ports:
      - "8010:8080"
    depends_on:
      - db
    environment:
      ConnectionStrings__mysqlConnection: "Server=db;Port=3306;Database=bookStore;User=appuser;Password=${DB_PASSWORD}"
    networks: [backend]
    restart: unless-stopped
  db:
    image: mysql:9.6
    environment:
      MYSQL_ROOT_PASSWORD: ${DB_ROOT_PASSWORD}
      MYSQL_DATABASE: bookStore
      MYSQL_USER: appuser
      MYSQL_PASSWORD: ${DB_PASSWORD}
    volumes:
      - db_data:/var/lib/mysql
    networks: [backend]
    restart: unless-stopped
networks:
  backend:
volumes:
  db_data:
```

...los 5 bugs no hubieran existido:

- **Bug 1** (red Docker): `networks: [backend]` declarativo en ambos servicios.
- **Bug 2** (puerto): `Server=db;Port=3306` viene del compose, sin confusión host/container.
- **Bug 3** (config drift): edits van al repo, deploy es `docker compose up`. No hay edición a mano.
- **Bug 4** (repos duplicados): un solo lugar de verdad, el repo del compose.
- **Bug 5** (migraciones): `Database.Migrate()` en startup + DB persistente en volumen.

**Conclusión operacional:** la inversión inicial de 30 minutos en infraestructura declarativa ahorra horas de debugging recurrente.

---

## 10. Pendientes

- [ ] Rotar password de root MySQL (lo viejo está en git history).
- [ ] Mover connection string a variable de entorno.
- [ ] Crear usuario MySQL dedicado (no usar root).
- [ ] Crear `docker-compose.yml` versionado.
- [ ] Quitar `app.UseHttpsRedirection()` cuando hay Nginx + Certbot.
- [ ] Limpiar `obj/` y `bin/` del git history.
- [ ] Consolidar los 4 contenedores MySQL a uno con DBs separadas.
- [ ] Configurar volumen persistente para Data Protection keys.
- [ ] Definir `ENV ASPNETCORE_URLS` explícito en Dockerfile.
- [ ] Healthcheck en Dockerfile.
- [ ] Migrar de `:latest` a tags por SHA en deploy.
