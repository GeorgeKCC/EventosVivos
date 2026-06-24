# EventosVivos

Plataforma backend para la gestión de eventos culturales, conferencias y talleres. Permite crear eventos, gestionar reservas, generar reportes en Excel y automatizar el ciclo de vida de los eventos.

---

## 📐 Arquitectura

### Modular Monolith + Vertical Slice Architecture

El backend implementa una **arquitectura modular monolítica** combinada con **Vertical Slice Architecture**, donde cada funcionalidad del negocio está encapsulada en un módulo autónomo que contiene todas las capas necesarias (desde el contrato hasta la implementación) en un solo corte vertical.

```
┌─────────────────────────────────────────────────────────────────────┐
│                        API (Entry Point)                             │
│              ASP.NET Core Web API - .NET 10                          │
│         Versionado (URL Segment + Header X-Api-Version)             │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌────────┐ │
│  │ModuloEvento  │  │ModuloReserva │  │ModuloReporte │  │Modulo  │ │
│  │              │  │              │  │              │  │Tarea   │ │
│  │ Contract     │  │ Contract     │  │ Contract     │  │        │ │
│  │ ├─Interfaces │  │ ├─Interfaces │  │ ├─Interfaces │  │ Jobs   │ │
│  │ ├─DTOs       │  │ ├─DTOs       │  │ ├─DTOs/Enums │  │        │ │
│  │              │  │              │  │              │  │        │ │
│  │ Implementation│ │ Implementation│ │ Implementation│ │        │ │
│  │ ├─UseCases   │  │ ├─UseCases   │  │ ├─UseCases   │  │        │ │
│  │ ├─Mappers    │  │ ├─Mappers    │  │ ├─Strategies │  │        │ │
│  │ ├─Validators │  │ ├─Strategies │  │              │  │        │ │
│  │              │  │ ├─Validators │  │              │  │        │ │
│  └──────────────┘  └──────────────┘  └──────────────┘  └────────┘ │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │                     ModuloSecurity                            │  │
│  │  Contract: ILoginUseCase, ILogoutUseCase, ITokenBlacklist    │  │
│  │  Implementation: JWT Auth, Token Blacklist, Validators       │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │                     Middlewares                               │  │
│  │  ApiKeyMiddleware: Validación X-Api-Key en cada request      │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│                      SHARED (Transversal)                           │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────────────────┐  │
│  │  Database    │  │    Cache     │  │      Exceptions          │  │
│  │  EF Core     │  │  FusionCache │  │  Global Exception Handler│  │
│  │  PostgreSQL  │  │  Redis       │  │  Custom Business Errors  │  │
│  │  Migrations  │  │  Backplane   │  │                          │  │
│  │  Seed Data   │  │  Invalidation│  │                          │  │
│  └─────────────┘  └──────────────┘  └──────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

### Principios de Diseño

| Principio | Implementación |
|-----------|---------------|
| **Vertical Slice** | Cada feature (crear evento, crear reserva, generar reporte) es un UseCase autocontenido que encapsula validación, lógica de negocio y persistencia |
| **Separation of Concerns** | Contract (interfaces + DTOs) separado de Implementation (lógica concreta) |
| **Dependency Inversion** | Los módulos exponen interfaces públicas; las implementaciones son `internal` |
| **Strategy Pattern** | Reportes y cambios de estado de reservas usan estrategias intercambiables |
| **Single Responsibility** | Un UseCase = una acción de negocio |
| **Open/Closed** | Nuevos tipos de reporte o estados de reserva se agregan sin modificar código existente |

### Patrón Use Case

Cada operación de negocio se modela como un **Use Case** con una interfaz pública (`IXxxUseCase`) y una implementación interna:

```
Contract/
├── ICrearEventoUseCase.cs          ← Interfaz pública
├── Dtos/
│   ├── RequestCrearEvento.cs       ← Input (record inmutable)
│   └── ResponseBuscarEvento.cs     ← Output (record inmutable)

Implementation/
├── ImplementationUseCase/
│   └── CrearEventoUseCase.cs       ← Lógica de negocio (internal)
├── Mappers/
│   └── EventosMappers.cs           ← Extension methods de mapeo
└── ModuloEventoService.cs          ← Registro DI del módulo
```

---

## 🧠 Justificación Técnica — ¿Por qué Modular Monolith?

### El Problema de Elegir Arquitectura en un Negocio en Crecimiento

Cuando un negocio está en etapa de crecimiento, la decisión arquitectónica más crítica no es "¿cuál es la mejor arquitectura?" sino **"¿cuál es la arquitectura correcta para ESTE momento y que no me bloquee en el futuro?"**

### Alternativas Evaluadas

| Arquitectura | Ventaja | Desventaja para este contexto |
|-------------|---------|-------------------------------|
| **Monolito tradicional (N-Layers)** | Simple de implementar | El acoplamiento entre capas crece exponencialmente. Refactorizar a microservicios requiere reescribir desde cero. El código tiende al "Big Ball of Mud" |
| **Microservicios desde el inicio** | Escalabilidad independiente | Complejidad operacional prematura (service mesh, event bus, distributed tracing, eventual consistency). El equipo necesita resolver problemas de infraestructura antes de entregar valor al negocio. [Martin Fowler: "Monolith First"](https://martinfowler.com/bliki/MonolithFirst.html) |
| **Modular Monolith** ✅ | Boundaries claros con simplicidad operacional | Requiere disciplina para no romper los límites entre módulos |

### ¿Por qué Modular Monolith es óptimo para EventosVivos HOY?

#### 1. Velocidad de entrega > Escalabilidad prematura
El negocio está en crecimiento. La prioridad es **iterar rápido**, validar features y responder al mercado. Un monolito modular permite:
- Deploy en un solo artefacto (1 contenedor Docker)
- Debugging local sin orquestar 4+ servicios
- Un solo pipeline CI/CD
- Transacciones ACID entre módulos sin sagas ni eventual consistency

#### 2. Costo operacional mínimo
```
Microservicios desde el inicio:
├── 4+ repositorios o proyectos independientes
├── 4+ pipelines CI/CD
├── Service discovery / API Gateway
├── Distributed tracing (Jaeger/Zipkin)
├── Event Bus (RabbitMQ/Azure Service Bus)
├── Health checks inter-servicio
├── Retry policies + Circuit breakers inter-servicio
├── 4+ bases de datos + migraciones independientes
└── Kubernetes / Container orchestration

Monolito Modular (actual):
├── 1 repositorio
├── 1 pipeline CI/CD
├── 1 docker-compose (API + PostgreSQL + Redis)
├── Transacciones locales
└── Deploy simple
```

**Ahorro estimado**: Un equipo pequeño puede gastar 40-60% del tiempo en infraestructura de microservicios en lugar de entregar valor al negocio.

#### 3. Los boundaries ya están definidos (el refactoring costoso ya está hecho)
El error más común de un monolito tradicional es que cuando finalmente necesitas microservicios, **no puedes separar los módulos** porque todo está entrelazado. En EventosVivos esto YA está resuelto:

```csharp
// ❌ Monolito tradicional — acoplamiento directo
public class ReservaService
{
    private readonly EventoRepository _eventoRepo;  // dependencia directa
    private readonly EmailService _emailService;     // cross-cutting mezclado
}

// ✅ EventosVivos — módulos aislados
// ModuloReserva NO referencia a ModuloEvento directamente
// Cada módulo solo conoce el DbContext compartido y sus propias entidades
internal class CrearReservaUserCase(EventosVivosDbContext context, ...) { }
```

#### 4. Preparado para escalar sin reescribir

| Señal de negocio | Acción técnica | Esfuerzo |
|-----------------|----------------|----------|
| El equipo crece a 3+ squads | Extraer módulo a repo independiente | Bajo — los boundaries ya existen |
| Un módulo necesita escalar independientemente | Extraer a microservicio con su propia DB | Medio — solo cambiar DI registration + agregar HTTP/gRPC client |
| Picos de tráfico en reservas | Escalar horizontalmente el monolito (stateless + Redis) | Bajo — ya es stateless con cache distribuido |
| El job de tareas consume muchos recursos | Extraer `ModuloTarea` a un Worker Service separado | Bajo — ya es un módulo independiente |

#### 5. Decisión respaldada por la industria

> *"If you can't build a well-structured monolith, what makes you think microservices is the answer?"* — Simon Brown

> *"Start with a monolith and extract microservices when you have a clear reason."* — Sam Newman (Building Microservices)

> *"A modular monolith is a valid architectural end state, not just a stepping stone."* — Kamil Grzybek

### Indicadores para Migrar a Microservicios

La migración a microservicios se justifica cuando aparecen **uno o más** de estos indicadores reales:

| # | Indicador | Métrica |
|---|-----------|---------|
| 1 | **Escalamiento independiente** | Un módulo necesita 10x más instancias que otro |
| 2 | **Equipos autónomos** | 3+ equipos trabajan en el mismo deploy y se bloquean mutuamente |
| 3 | **Ciclos de release diferentes** | Reservas necesita deploys diarios, Reportes es quincenal |
| 4 | **Requisitos tecnológicos diferentes** | Un módulo necesita Python/ML, otro necesita real-time con SignalR |
| 5 | **Fault isolation** | Un bug en Reportes no debe tumbar la creación de Reservas |
| 6 | **Compliance/Seguridad** | Datos de pago deben estar en un servicio PCI-DSS aislado |

**Ninguno de estos indicadores existe HOY en EventosVivos**, por lo tanto microservicios sería complejidad accidental sin beneficio real.

---

## 🔬 Análisis Técnico — Fortalezas y Mejoras Recomendadas

### ✅ Fortalezas actuales de la arquitectura

| Aspecto | Detalle |
|---------|---------|
| **Encapsulación fuerte** | Implementaciones `internal` + `InternalsVisibleTo` solo para tests |
| **Contratos explícitos** | Proyectos `Contract` separados permiten que un futuro API Gateway solo referencie las interfaces |
| **Records inmutables** | DTOs como `record` garantizan inmutabilidad y reducen bugs de estado compartido |
| **Cache inteligente** | FusionCache con L1 (memory) + L2 (Redis) + Backplane da la mejor latencia posible sin stale data |
| **Validación centralizada** | FluentValidation con AbstractValidator desacopla reglas del UseCase |
| **Containerización** | Docker Compose con volúmenes persistentes para PostgreSQL y Redis |

### 🔧 Mejoras recomendadas para la siguiente iteración

#### 1. Bounded Context por módulo con DbContext separado (preparación para DB-per-service)
```csharp
// Actual: DbContext compartido
public class EventosVivosDbContext : DbContext { /* todas las entidades */ }

// Recomendado: DbContext por módulo apuntando al mismo schema inicialmente
public class EventoDbContext : DbContext { public DbSet<Evento> Eventos { get; set; } }
public class ReservaDbContext : DbContext { public DbSet<Reserva> Reservas { get; set; } }
```
**Beneficio**: Cuando migres a microservicios, cada servicio ya tendrá su DbContext listo para apuntar a su propia DB.

#### 2. Comunicación entre módulos via Mediator/Events internos
```csharp
// Actual: ModuloReserva accede directamente a la tabla Eventos para validar
var evento = await dbContext.Eventos.FirstOrDefaultAsync(e => e.Id == request.EventoId);

// Recomendado: Solicitar datos via interfaz del módulo dueño
var evento = await eventoQueryService.GetByIdAsync(request.EventoId);
```
**Beneficio**: Cuando extraigas a microservicios, solo cambias la implementación del `IEventoQueryService` de "query local" a "HTTP client".

#### 3. Agregar Health Checks
```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString)
    .AddRedis(redisConnection);
```
**Beneficio**: Monitoreo de dependencias externas, útil tanto en monolito como en microservicios. Compatible con Docker health checks y orquestadores.

#### 4. Structured Logging con correlación
```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t.AddAspNetCoreInstrumentation())
    .WithMetrics(m => m.AddAspNetCoreInstrumentation());
```
**Beneficio**: Trazabilidad end-to-end que es esencial cuando eventualmente tengas servicios distribuidos.

#### 5. Integration Tests con `WebApplicationFactory`
```csharp
public class EventoIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    // Tests que ejercitan el pipeline completo: HTTP → Controller → UseCase → DB
}
```
**Beneficio**: Mayor confianza en que el sistema funciona end-to-end, especialmente al refactorizar módulos.

#### 6. API Rate Limiting por módulo
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("reservas", opt => { opt.PermitLimit = 100; });
});
```
**Beneficio**: Protección contra abuso sin necesitar un API Gateway externo. Cada módulo puede tener sus propios límites según la criticidad del negocio.

---

## 🗂️ Estructura del Proyecto

```
D:\EventosVivos\Backend\
│
├── API/
│   └── EventosVivos.API/            ← Host ASP.NET Core (Controllers, Program.cs)
│       ├── Controllers/
│       │   ├── AuthController.cs     ← Login / Logout
│       │   ├── EventoController.cs
│       │   ├── ReservasController.cs
│       │   └── ReporteController.cs
│       └── Middlewares/
│           └── ApiKeyMiddleware.cs   ← Validación X-Api-Key por header
│
├── MODULES/
│   ├── Evento/
│   │   ├── Contract/                ← Interfaces + DTOs públicos
│   │   └── Implementation/          ← UseCases, Mappers, Validators
│   │
│   ├── Reserva/
│   │   ├── Contract/                ← Interfaces + DTOs públicos
│   │   └── Implementation/          ← UseCases, Mappers, Strategies, Validators
│   │
│   ├── Reporte/
│   │   ├── Contract/                ← Interfaces + DTOs + Enums públicos
│   │   └── Implementation/          ← UseCases, Strategies (Excel generation)
│   │
│   ├── Security/
│   │   ├── Contract/                ← ILoginUseCase, ILogoutUseCase, ITokenBlacklistService
│   │   └── Implementation/          ← JWT Auth, Token Blacklist, Validators
│   │
│   └── Tarea/
│       ├── ModuloTarea/             ← Background Jobs (Quartz.NET)
│       └── ModuloTareaTest/         ← Tests del módulo
│
├── SHARED/
│   └── Transversal/                 ← Cross-cutting concerns
│       ├── Database/                ← DbContext, Entities, Enums, Migrations, Seed
│       ├── Cache/                   ← Redis + FusionCache + Attributes
│       └── Exceptions/              ← Global handler + Custom exceptions
│
├── docker-compose.yml               ← Orquestación de contenedores
└── docker-compose.override.yml      ← Variables de entorno para desarrollo
```

---

## 🛠️ Stack Tecnológico

| Capa | Tecnología | Versión |
|------|-----------|---------|
| **Runtime** | .NET | 10 |
| **Framework** | ASP.NET Core Web API | 10 |
| **ORM** | Entity Framework Core + Npgsql | 10.0.4 |
| **Base de Datos** | PostgreSQL | 17 (Alpine) |
| **Cache Distribuido** | Redis + FusionCache | Redis latest / FusionCache 2.6.0 |
| **Backplane** | StackExchange.Redis Backplane | 2.6.0 |
| **Autenticación** | JWT Bearer (Microsoft.AspNetCore.Authentication.JwtBearer) | 10.0.9 |
| **Validación** | FluentValidation | 12.1.1 |
| **Background Jobs** | Quartz.NET | 3.14.0 |
| **Reportes Excel** | ClosedXML | 0.105.0 |
| **API Docs** | OpenAPI + Scalar | - |
| **API Versioning** | Asp.Versioning | 10.0.0 |
| **Testing** | xUnit + Moq + FluentAssertions + EF Core InMemory | - |
| **Contenedores** | Docker + Docker Compose | - |
| **Despliegue** | Railway | - |

---

## 🗄️ Base de Datos — PostgreSQL

Se utiliza **Entity Framework Core** con **Npgsql**, **Code-First Migrations** y **Data Seeding** automático al iniciar la aplicación.

### Modelo de Datos

```
┌─────────────┐       ┌─────────────┐       ┌──────────────┐
│  Venue      │       │   Evento    │       │   Reserva    │
├─────────────┤       ├─────────────┤       ├──────────────┤
│ Id          │◄──────│ VenueId     │       │ Id           │
│ Nombre      │       │ Id          │◄──────│ EventoId     │
│ Capacidad   │       │ Titulo      │       │ Cantidad     │
│ Ciudad      │       │ Descripcion │       │ NombreCompra │
└─────────────┘       │ Capacidad   │       │ EmailCompra  │
                      │ InicioEvento│       │ EstadoReserva│
┌─────────────┐       │ FinEvento   │       │ FechaCancela │
│ TipoEvento  │       │ Precio      │       │ EsPerdida    │
├─────────────┤       │ TipoEventoId│       └──────────────┘
│ Id          │◄──────│ EstadoId    │
│ Nombre      │       └─────────────┘       ┌──────────────┐
└─────────────┘              │              │EstadoReserva │
                             │              ├──────────────┤
┌─────────────┐              │              │ Id           │
│EstadoEvento │◄─────────────┘              │ Nombre       │
├─────────────┤                             └──────────────┘
│ Id          │
│ Nombre      │
└─────────────┘
```

### Características
- **Migraciones automáticas**: `context.Database.MigrateAsync()` al iniciar
- **Seed Data**: Tipos de evento, Venues, Estados de reserva y Estados de evento se crean automáticamente
- **AsNoTracking**: Queries de lectura optimizadas sin tracking de cambios

---

## ⚡ Cache — Redis + FusionCache

Se implementa un sistema de **cache distribuido multi-nivel** usando **FusionCache** con **Redis** como backplane y almacenamiento distribuido.

### Arquitectura del Cache

```
┌────────────────────────────────────────────┐
│              API Request                    │
├────────────────────────────────────────────┤
│  [CacheableAttribute]     →  Cache HIT?    │
│                               ├─ YES → Return cached response
│                               └─ NO  → Execute action → Store in cache
│                                                                        │
│  [InvalidateCacheAttribute] → On success → Increment version tag      │
│                                          → Invalidate related keys     │
├────────────────────────────────────────────┤
│         FusionCache (L1: Memory + L2: Redis)                          │
│         Backplane: Redis Pub/Sub (sync entre instancias)              │
├────────────────────────────────────────────┤
│                Redis Server                 │
└────────────────────────────────────────────┘
```

### Estrategia de Invalidación
- **Tag-based versioning**: Cada recurso tiene un tag versionado (ej: `Evento`, `Reserva`)
- Al mutar datos, se incrementa la versión del tag, invalidando todas las keys asociadas
- Soporta **múltiples instancias** sincronizadas via Redis Backplane (Pub/Sub)

### Configuración de Resiliencia
- `AbortOnConnectFail = false`: No falla si Redis no está disponible inicialmente
- `ConnectRetry = 3`: Reintentos de conexión
- `ExponentialRetry(5000)`: Backoff exponencial desde 5 segundos
- `KeepAlive = 180`: Mantiene conexión activa

---

## 📦 Módulos de Negocio

### ModuloEvento
- **Crear Evento**: Validación de campos, capacidad vs venue, solapamiento de horarios, restricción nocturna en fines de semana
- **Buscar Eventos**: Filtrado dinámico por tipo, venue, estado, título y rango de fechas
- **Obtener Venues**: Lista de venues disponibles
- **Obtener Tipos de Evento**: Lista de tipos de evento disponibles

### ModuloReserva
- **Crear Reserva**: Validación de disponibilidad, límites por proximidad temporal (24h/1h), límites por precio (>$100 = máx 10 entradas)
- **Confirmar/Cancelar Reserva**: Strategy Pattern para manejar transiciones de estado con reglas de penalización (cancelación <48h = pérdida)
- **Obtener Reservas**: Lista de todas las reservas registradas
- **Código de Reserva**: Generación aleatoria con formato `EV-{6 dígitos}`

### ModuloReporte
- **Generar Reportes Excel**: Strategy Pattern con 5 tipos de reporte:
  - Estado del Evento
  - Total Entradas Vendidas
  - Total Entradas Disponibles
  - Porcentaje de Ocupación
  - Total de Ingresos

### ModuloSecurity
- **Login**: Autenticación con usuario y contraseña, retorna token JWT con claims (nombre, rol)
- **Logout**: Invalidación de token JWT mediante blacklist en memoria
- **Token Blacklist**: Servicio singleton que gestiona tokens revocados
- **Validación de tokens**: En cada request autenticada se verifica que el token no esté revocado
- **Seed por defecto**: Usuario `admin` / Password `admin` / Rol `Admin`

### ModuloTarea
- **CompletarEventosJob**: Job programado (Quartz.NET, cron cada 5 min) que marca como "Completado" los eventos cuya fecha/hora de fin ya pasó

---

## 🚀 Estrategia de Escalabilidad — De Monolito Modular a Microservicios

La arquitectura fue diseñada **intencionalmente** para facilitar una transición incremental a microservicios cuando el negocio lo requiera. No es una cuestión de "si" sino de "cuándo" y el código ya está preparado.

### Evolución Progresiva (3 fases)

```
Fase 1 (ACTUAL)              Fase 2 (Crecimiento)           Fase 3 (Escala)
─────────────────           ──────────────────────         ─────────────────────

┌─────────────────┐         ┌─────────────────────┐       ┌───────┐  ┌───────┐
│  Modular        │         │  Modular Monolith   │       │Evento │  │Reserva│
│  Monolith       │    →    │  + Worker Services  │  →    │Service│  │Service│
│  (1 deploy)     │         │  (2-3 deploys)      │       └───────┘  └───────┘
│                 │         │                     │       ┌───────┐  ┌───────┐
│  Shared DB      │         │  Shared DB +        │       │Reporte│  │ Tarea │
│  + Redis        │         │  Module-specific    │       │Service│  │Worker │
└─────────────────┘         │  Redis namespaces   │       └───────┘  └───────┘
                            └─────────────────────┘       DB per service + Bus

Equipo: 1-3 devs            Equipo: 3-6 devs              Equipo: 6+ devs
Tráfico: < 1K RPM           Tráfico: 1K-10K RPM           Tráfico: > 10K RPM
```

### Fase Actual: Modular Monolith
```
┌─────────────────────────────────────┐
│         Single Deployment Unit       │
│  ┌───────┐ ┌───────┐ ┌───────┐    │
│  │Evento │ │Reserva│ │Reporte│    │
│  └───────┘ └───────┘ └───────┘    │
│         Shared DB + Cache           │
└─────────────────────────────────────┘
```

### Fase Futura: Microservicios
```
┌──────────┐   ┌──────────┐   ┌──────────┐   ┌──────────┐
│ Evento   │   │ Reserva  │   │ Reporte  │   │  Tarea   │
│ Service  │   │ Service  │   │ Service  │   │ Service  │
│          │   │          │   │          │   │          │
│ Own DB   │   │ Own DB   │   │ Own DB   │   │ Own DB   │
└──────────┘   └──────────┘   └──────────┘   └──────────┘
      │              │              │              │
      └──────────────┴──────────────┴──────────────┘
                    Event Bus / API Gateway
```

### Factores que habilitan la migración

| Factor | Cómo se implementa |
|--------|-------------------|
| **Bajo acoplamiento entre módulos** | Cada módulo se comunica solo via interfaces públicas (Contract). No hay referencias cruzadas entre implementaciones |
| **Encapsulación** | Clases de implementación son `internal`, solo las interfaces y DTOs son públicos |
| **Independencia de datos** | Cada módulo opera sobre un subconjunto de entidades. Separar en DBs independientes requiere mínimo refactoring |
| **Registro independiente** | Cada módulo tiene su propio `RegisterServices()`, migrable a un `Program.cs` independiente |
| **Docker-ready** | La infraestructura ya corre en contenedores (PostgreSQL + Redis + API) |
| **Stateless API** | Cache distribuido en Redis permite escalar horizontalmente sin estado en memoria local |
| **Background Jobs aislados** | `ModuloTarea` ya es un módulo separado, extraíble a un Worker Service independiente |

### Camino de Migración Recomendado

```
¿Necesito microservicios?
│
├─ ¿Un módulo necesita escalar 10x más que otros?
│   └─ SÍ → Extraer ese módulo a servicio independiente
│
├─ ¿Tengo 3+ equipos bloqueándose en deploys?
│   └─ SÍ → Separar ownership por módulo, deploy independiente
│
├─ ¿Un módulo necesita tecnología diferente?
│   └─ SÍ → Extraer como microservicio con tech stack propio
│
└─ ¿Ninguno aplica?
    └─ Mantener Modular Monolith → Escalar horizontalmente con más instancias
```

**Pasos concretos cuando la señal aparezca:**
1. **Strangler Fig Pattern**: Extraer módulos uno por uno empezando por los más independientes (`ModuloReporte`, `ModuloTarea`)
2. **Database per Service**: Separar el DbContext compartido en contextos aislados por módulo
3. **Event-Driven Communication**: Reemplazar llamadas directas por eventos (Azure Service Bus / RabbitMQ)
4. **API Gateway**: Agregar un gateway (YARP / Azure API Management) para enrutar al servicio correcto
5. **Observabilidad distribuida**: OpenTelemetry + Distributed Tracing para correlacionar requests entre servicios

---

## 🐳 Docker

La aplicación se ejecuta completamente en contenedores:

```yaml
services:
  eventosvivos.api    → ASP.NET Core API (.NET 10)
  PostgreSql          → postgres:17-alpine
  Redis               → redis:latest
```

### Ejecutar localmente
```bash
docker-compose up -d
```

La API estará disponible en `http://localhost:8080` con documentación interactiva en Scalar.

---

## 🧪 Testing

Cada módulo tiene su proyecto de tests asociado con la convención `Modulo*Test`:

| Proyecto | Módulo |
|----------|--------|
| `ModuloEventoTest` | Evento |
| `ModuloReservaTest` | Reserva |
| `ModuloReporteTest` | Reporte |
| `ModuloTareaTest` | Tarea |
| `ModuloSecurityTest` | Security |

### Stack de Testing
- **xUnit**: Framework de testing
- **Moq**: Mocking de dependencias
- **FluentAssertions**: Assertions legibles
- **EF Core InMemory**: Base de datos en memoria para tests de integración

---

## 📡 API Endpoints

> ⚠️ Todos los endpoints (excepto Login y Scalar) requieren el header `X-Api-Key` con el valor configurado en `appsettings.json`.

### v1/Auth
| Método | Ruta | Descripción | Autenticación |
|--------|------|-------------|---------------|
| `POST` | `/api/v1/Auth/login` | Iniciar sesión (retorna JWT) | Pública |
| `POST` | `/api/v1/Auth/logout` | Cerrar sesión (invalida JWT) | Bearer Token |

### v1/Evento
| Método | Ruta | Descripción | Autenticación |
|--------|------|-------------|---------------|
| `POST` | `/api/v1/Evento` | Crear evento | API Key |
| `GET` | `/api/v1/Evento` | Buscar eventos (filtros opcionales) | API Key |
| `GET` | `/api/v1/Evento/GetAllVenue` | Obtener todos los venues | API Key |
| `GET` | `/api/v1/Evento/GetAllTipoEvento` | Obtener todos los tipos de evento | API Key |

### v1/Reservas
| Método | Ruta | Descripción | Autenticación |
|--------|------|-------------|---------------|
| `POST` | `/api/v1/Reservas` | Crear reserva | API Key |
| `POST` | `/api/v1/Reservas/Payment` | Confirmar/Cancelar reserva | API Key |
| `GET` | `/api/v1/Reservas` | Obtener todas las reservas | API Key |

### v1/Reporte
| Método | Ruta | Descripción | Autenticación |
|--------|------|-------------|---------------|
| `GET` | `/api/v1/Reporte` | Descargar reporte Excel | API Key |

---

## ⚙️ Configuración

### Connection Strings requeridos
```json
{
  "ConnectionStrings": {
    "PostgreSqlConnection": "Host=...;Port=5432;Database=EventosVivos;Username=postgres;Password=...",
    "RedisConnection": "localhost:6379"
  }
}
```

### JWT
```json
{
  "Jwt": {
    "Key": "EventosVivos_SuperSecretKey_2024_MinLength32Chars!",
    "Issuer": "EventosVivos.API",
    "Audience": "EventosVivos.Client"
  }
}
```

### API Key
```json
{
  "ApiKey": "EV-2024-SecureKey-a1b2c3d4e5f6"
}
```

---

## 🔐 Seguridad

La API implementa dos capas de seguridad:

### 1. API Key (Middleware)
Todas las peticiones (excepto `/Auth/login`, `/scalar` y `/openapi`) requieren el header:
```
X-Api-Key: EV-2024-SecureKey-a1b2c3d4e5f6
```

| Escenario | Resultado |
|-----------|-----------|
| Sin header `X-Api-Key` | `401 Unauthorized` |
| Key incorrecta | `403 Forbidden` |
| Key correcta | Request procesada |

### 2. JWT Bearer Token
Para endpoints protegidos con `[Authorize]` (como logout), se requiere el header:
```
Authorization: Bearer {token}
```

### Flujo de autenticación
```
1. POST /api/v1/Auth/login   → { "username": "admin", "password": "admin" }
2. Respuesta                  → { "token": "eyJ...", "username": "admin", "rol": "Admin" }
3. Usar token en requests     → Authorization: Bearer eyJ...
4. POST /api/v1/Auth/logout   → Invalida el token
```

### CORS
Configurado para permitir peticiones desde Angular:
- Origen permitido: `http://localhost:4200`
- Métodos: Todos
- Headers: Todos

---

## 📖 Manual de Uso — Guía Paso a Paso

### Prerrequisitos

| Herramienta | Descarga | Descripción |
|------------|----------|-------------|
| **Docker Desktop** | [docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop/) | Para ejecutar los contenedores (API, PostgreSQL, Redis) |
| **Git** | [git-scm.com](https://git-scm.com/) | Para clonar el repositorio |

> 💡 **Nota**: No necesitas instalar .NET SDK, PostgreSQL ni Redis en tu máquina. Todo se ejecuta dentro de contenedores Docker.

### Paso 1: Clonar el repositorio

```bash
git clone https://github.com/GeorgeKCC/EventosVivos.git
cd EventosVivos/Backend
```

### Paso 2: Iniciar Docker Desktop

Abre **Docker Desktop** y espera a que el motor Docker esté en estado **Running** (ícono verde en la barra de tareas).

### Paso 3: Levantar los contenedores

```bash
docker-compose up -d
```

Esto levantará 3 contenedores:
| Contenedor | Puerto | Descripción |
|-----------|--------|-------------|
| `eventosvivos.api` | `8080` | API .NET 10 |
| `PostgreSql` | `5432` | PostgreSQL 17 |
| `Redis` | `6379` | Redis (cache) |

### Paso 4: Verificar que los contenedores están corriendo

```bash
docker-compose ps
```

Todos deben mostrar estado `Up` o `running`.

### Paso 5: Acceder a la API

| URL | Descripción |
|-----|-------------|
| `http://localhost:8080/scalar/v1` | Documentación interactiva (Scalar UI) |
| `http://localhost:8080/api/v1/Auth/login` | Endpoint de login |

### Paso 6: Autenticarse en Scalar

1. Abre `http://localhost:8080/scalar/v1` en tu navegador
2. Haz clic en el botón **Authenticate**
3. Ingresa la API Key: `EV-2024-SecureKey-a1b2c3d4e5f6`
4. Ahora puedes probar todos los endpoints desde la interfaz

### Paso 7: Probar el Login

Envía un `POST` a `/api/v1/Auth/login` con el body:
```json
{
  "username": "admin",
  "password": "admin"
}
```

Respuesta:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "username": "admin",
  "rol": "Admin"
}
```

### Paso 8: Usar los endpoints

Con la API Key configurada en Scalar, puedes probar:

1. **Crear un evento**: `POST /api/v1/Evento`
2. **Buscar eventos**: `GET /api/v1/Evento`
3. **Crear una reserva**: `POST /api/v1/Reservas`
4. **Confirmar reserva**: `POST /api/v1/Reservas/Payment`
5. **Descargar reporte**: `GET /api/v1/Reporte`

### Detener los contenedores

```bash
docker-compose down
```

Para detener y **eliminar los datos** (volúmenes):
```bash
docker-compose down -v
```

---

### Uso desde Angular

Para consumir la API desde Angular, agrega un interceptor que envíe la API Key en cada petición:

```typescript
// api-key.interceptor.ts
import { HttpInterceptorFn } from '@angular/common/http';

export const apiKeyInterceptor: HttpInterceptorFn = (req, next) => {
  const apiReq = req.clone({
    setHeaders: { 'X-Api-Key': 'EV-2024-SecureKey-a1b2c3d4e5f6' }
  });
  return next(apiReq);
};
```

Registrarlo en `app.config.ts`:
```typescript
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { apiKeyInterceptor } from './api-key.interceptor';

export const appConfig = {
  providers: [
    provideHttpClient(withInterceptors([apiKeyInterceptor])),
  ]
};
```

---

### Despliegue en Railway (Producción)

La API está desplegada en Railway:

| Configuración | Valor |
|---|---|
| **URL Producción** | `https://eventosvivos-production-1267.up.railway.app` |
| **Scalar UI** | `https://eventosvivos-production-1267.up.railway.app/scalar/v1` |
| **Root Directory** | `Backend` |
| **Dockerfile Path** | `API/EventosVivos.API/Dockerfile` |
| **Watch Paths** | `/Backend/**` |

---

## �️ Cliente — Angular UI

### Arquitectura Modular

El frontend está construido con **Angular 21** usando **standalone components** y una arquitectura modular con **lazy loading** por feature.

```
Cliente/src/app/
│
├── app.config.ts              ← Configuración global (router, HTTP, interceptors)
├── app.routes.ts              ← Rutas principales con lazy loading
├── app.ts / app.html          ← Componente raíz
│
├── core/                      ← Servicios singleton e interceptores
│   └── interceptors/
│       └── api.interceptor.ts ← Agrega X-Api-Key y Bearer Token a cada request
│
├── features/                  ← Módulos de negocio (lazy loaded)
│   ├── login/                 ← Autenticación
│   ├── buscar-evento/         ← Búsqueda y listado de eventos
│   ├── crear-evento/          ← Creación de eventos
│   ├── crear-reserva/         ← Creación de reservas
│   ├── pagar-reserva/         ← Confirmación/cancelación de reservas
│   ├── buscar-reserva/        ← Búsqueda de reservas
│   └── reportes/              ← Generación de reportes
│
├── shared/                    ← Componentes, pipes y directivas reutilizables
│   └── components/
│
└── layout/                    ← Componentes de estructura visual
    └── navbar/                ← Barra de navegación
```

### Estructura de cada Feature

Cada feature es autocontenido con sus propios modelos, servicios y rutas:

```
features/buscar-evento/
├── buscar-evento.ts           ← Componente standalone
├── buscar-evento.html         ← Template
├── buscar-evento.scss         ← Estilos
├── buscar-evento.routes.ts    ← Rutas del feature
├── buscar-evento.spec.ts      ← Tests
├── models/
│   └── evento.model.ts        ← Interfaces/tipos del dominio
└── services/
    └── evento.service.ts      ← Servicio HTTP del feature
```

### Routing (Lazy Loading)

Todas las rutas cargan sus módulos de forma diferida para optimizar el bundle inicial:

```typescript
export const routes: Routes = [
  { path: '', redirectTo: 'buscar-evento', pathMatch: 'full' },
  { path: 'login', loadChildren: () => import('./features/login/login.routes') },
  { path: 'buscar-evento', loadChildren: () => import('./features/buscar-evento/buscar-evento.routes') },
  { path: 'crear-evento', loadChildren: () => import('./features/crear-evento/crear-evento.routes') },
  { path: 'crear-reserva', loadChildren: () => import('./features/crear-reserva/crear-reserva.routes') },
  { path: 'pagar-reserva', loadChildren: () => import('./features/pagar-reserva/pagar-reserva.routes') },
  { path: 'buscar-reserva', loadChildren: () => import('./features/buscar-reserva/buscar-reserva.routes') },
  { path: 'reportes', loadChildren: () => import('./features/reportes/reportes.routes') },
];
```

### Levantar la UI en local

#### Prerrequisitos

| Herramienta | Versión | Descarga |
|------------|---------|----------|
| **Node.js** | 22+ | [nodejs.org](https://nodejs.org/) |
| **npm** | 11+ | Incluido con Node.js |

#### Pasos

```bash
# 1. Clonar el repositorio (si no lo has hecho)
git clone https://github.com/GeorgeKCC/EventosVivos.git
cd EventosVivos/Cliente

# 2. Instalar dependencias
npm install

# 3. Levantar el servidor de desarrollo
ng serve
# o alternativamente:
npm start
```

La aplicación estará disponible en `http://localhost:4200`.

> **Nota**: Asegúrate de tener el backend corriendo (`docker-compose up -d` en la carpeta `Backend/`) para que la UI pueda consumir la API.

### Stack del Cliente

| Tecnología | Versión |
|-----------|---------|
| **Angular** | 21 |
| **TypeScript** | 5.9 |
| **RxJS** | 7.8 |
| **SCSS** | - |
| **Vitest** | 4.0 |
| **Node.js** | 22+ |

### Despliegue en Vercel (Producción)

El frontend se despliega automáticamente en **Vercel** al hacer push a `main` con cambios en `Cliente/**`.

---

## �📄 Licencia

Proyecto privado — Todos los derechos reservados.
