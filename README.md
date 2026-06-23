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
├─────────────────────────────────────────────────────────────────────┤
│                      SHARED (Transversal)                           │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────────────────┐  │
│  │  Database    │  │    Cache     │  │      Exceptions          │  │
│  │  EF Core     │  │  FusionCache │  │  Global Exception Handler│  │
│  │  SQL Server  │  │  Redis       │  │  Custom Business Errors  │  │
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
├── 1 docker-compose (API + SQL Server + Redis)
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
| **Containerización** | Docker Compose con volúmenes persistentes para SQL Server y Redis |

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
    .AddSqlServer(connectionString)
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
| **ORM** | Entity Framework Core | 10.0.9 |
| **Base de Datos** | SQL Server | 2022 |
| **Cache Distribuido** | Redis + FusionCache | Redis latest / FusionCache 2.6.0 |
| **Backplane** | StackExchange.Redis Backplane | 2.6.0 |
| **Validación** | FluentValidation | 12.1.1 |
| **Background Jobs** | Quartz.NET | 3.14.0 |
| **Reportes Excel** | ClosedXML | 0.105.0 |
| **API Docs** | OpenAPI + Scalar | - |
| **API Versioning** | Asp.Versioning | 10.0.0 |
| **Testing** | xUnit + Moq + FluentAssertions + EF Core InMemory | - |
| **Contenedores** | Docker + Docker Compose | - |

---

## 🗄️ Base de Datos — SQL Server

Se utiliza **Entity Framework Core** con **Code-First Migrations** y **Data Seeding** automático al iniciar la aplicación.

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

### ModuloReserva
- **Crear Reserva**: Validación de disponibilidad, límites por proximidad temporal (24h/1h), límites por precio (>$100 = máx 10 entradas)
- **Confirmar/Cancelar Reserva**: Strategy Pattern para manejar transiciones de estado con reglas de penalización (cancelación <48h = pérdida)

### ModuloReporte
- **Generar Reportes Excel**: Strategy Pattern con 5 tipos de reporte:
  - Estado del Evento
  - Total Entradas Vendidas
  - Total Entradas Disponibles
  - Porcentaje de Ocupación
  - Total de Ingresos

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
| **Docker-ready** | La infraestructura ya corre en contenedores (SQL Server + Redis + API) |
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
  SqlServer           → mcr.microsoft.com/mssql/server:2022-latest
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

### Stack de Testing
- **xUnit**: Framework de testing
- **Moq**: Mocking de dependencias
- **FluentAssertions**: Assertions legibles
- **EF Core InMemory**: Base de datos en memoria para tests de integración

---

## 📡 API Endpoints

### v1/Evento
| Método | Ruta | Descripción |
|--------|------|-------------|
| `POST` | `/api/v1/Evento` | Crear evento |
| `GET` | `/api/v1/Evento` | Buscar eventos (filtros opcionales) |

### v1/Reservas
| Método | Ruta | Descripción |
|--------|------|-------------|
| `POST` | `/api/v1/Reservas` | Crear reserva |
| `POST` | `/api/v1/Reservas/Payment` | Confirmar/Cancelar reserva |

### v1/Reporte
| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/v1/Reporte` | Descargar reporte Excel |

---

## ⚙️ Configuración

### Connection Strings requeridos
```json
{
  "ConnectionStrings": {
    "SqlServerConnection": "Server=...",
    "RedisConnection": "localhost:6379"
  }
}
```

---

## 📄 Licencia

Proyecto privado — Todos los derechos reservados.
