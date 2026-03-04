# 🏦 Artemis Banking — Guía de Trabajo en Equipo

> **Proyecto:** Mini Proyecto Final — Internet Banking (Web API)
> **Stack:** ASP.NET Core 8/9 · Entity Framework Core (Code First) · ASP.NET Identity · JWT · AutoMapper · Swagger
> **Arquitectura obligatoria:** ONION Architecture

---

## 📐 Arquitectura ONION — Estructura de la Solución

Toda la solución **debe seguir estrictamente la arquitectura ONION**. Cualquier desviación se considera una falla grave. La solución debe estar dividida en los siguientes proyectos/capas:

```
ArtemisB ank ing.sln
│
├── ArtemisB anking.Core/                  ← Capa de Dominio (centro)
│   ├── Entities/                          # Entidades del dominio (sin dependencias externas)
│   ├── Enums/
│   └── Interfaces/
│       ├── Repositories/                  # Contratos de repositorios genéricos y específicos
│       └── Services/                      # Contratos de servicios de negocio
│
├── ArtemisB anking.Application/           ← Capa de Aplicación
│   ├── DTOs/                              # Data Transfer Objects (request y response)
│   ├── Services/                          # Implementación de los servicios de negocio
│   └── Mappings/                          # Perfiles de AutoMapper
│
├── ArtemisB anking.Infrastructure/        ← Capa de Infraestructura
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs        # DbContext con Identity
│   │   ├── Migrations/
│   │   └── Repositories/                  # Implementación de repositorios
│   ├── Identity/                          # Configuración de ASP.NET Identity + JWT
│   └── Services/                          # Servicios externos (correo, Hangfire, etc.)
│
└── ArtemisB anking.WebApi/                ← Capa de Presentación (API)
    ├── Controllers/
    ├── Extensions/                        # Registro de servicios (DI), Swagger, JWT
    └── Program.cs
```

### Reglas de dependencia (ONION)

```
WebApi  →  Application  →  Core
Infrastructure  →  Application  →  Core

❌ Core NO puede referenciar a ninguna capa externa.
❌ Application NO puede referenciar a Infrastructure ni a WebApi.
✅ Solo la capa WebApi y Infrastructure conocen los detalles de implementación.
```

---

## 🌿 Convención de Ramas (Git Flow Simplificado)

```
main              ← rama de producción, NUNCA se pushea directo
develop           ← rama de integración del equipo
feature/<nombre>  ← ramas de desarrollo individual
```

### Flujo de trabajo

1. Siempre parte desde `develop` para crear tu rama:
   ```bash
   git checkout develop
   git pull origin develop
   git checkout -b feature/nombre-del-modulo
   ```

2. Trabaja en tu rama. Cuando termines, abre un **Pull Request** hacia `develop`.

3. Nunca hagas merge directamente a `main` sin revisión del equipo.

---

## ✅ Convención de Commits

Todos los commits deben seguir el estándar **Conventional Commits**:

```
<tipo>(<alcance>): <descripción corta en presente>
```

### Tipos permitidos

| Tipo | Cuándo usarlo |
|---|---|
| `feat` | Nueva funcionalidad |
| `fix` | Corrección de bug |
| `refactor` | Refactorización sin cambiar comportamiento |
| `chore` | Configuración, dependencias, scaffolding |
| `docs` | Documentación |
| `test` | Agregar o modificar pruebas |

### Ejemplos válidos

```bash
feat(auth): implementar endpoint de login con JWT
feat(users): agregar endpoint para crear usuario cliente con cuenta de ahorro
fix(loans): corregir cálculo de cuota en tabla de amortización francesa
chore(infra): configurar DbContext y migraciones iniciales
refactor(credit-card): mover lógica de cancelación al servicio de aplicación
docs(readme): agregar instrucciones de configuración del proyecto
```

### Ejemplos inválidos ❌

```bash
git commit -m "arregle cosas"
git commit -m "cambios varios"
git commit -m "WIP"
```

---

## 📦 Estructura interna de cada módulo

Cada módulo que se desarrolle debe seguir este patrón sin excepciones:

### 1. Entidad en `Core/Entities/`
```csharp
// Core/Entities/Loan.cs
public class Loan
{
    public string Id { get; set; }           // 9 dígitos únicos
    public string ClientId { get; set; }
    public decimal Amount { get; set; }
    public decimal InterestRate { get; set; }
    public int TermMonths { get; set; }
    public LoanStatus Status { get; set; }   // enum en Core/Enums/
    // Propiedades de navegación...
}
```

### 2. Interfaz de Repositorio en `Core/Interfaces/Repositories/`
```csharp
// Core/Interfaces/Repositories/ILoanRepository.cs
public interface ILoanRepository : IGenericRepository<Loan>
{
    Task<Loan?> GetByIdWithAmortizationAsync(string loanId);
    Task<bool> ClientHasActiveLoanAsync(string clientId);
    Task<IEnumerable<Loan>> GetAllPaginatedAsync(int page, int pageSize, string? status, string? cedula);
}
```

### 3. Interfaz de Servicio en `Core/Interfaces/Services/`
```csharp
// Core/Interfaces/Services/ILoanService.cs
public interface ILoanService
{
    Task<LoanListResponseDto> GetAllAsync(int page, int pageSize, string? status, string? cedula);
    Task<LoanDetailResponseDto> GetByIdAsync(string loanId);
    Task AssignLoanAsync(AssignLoanRequestDto dto, string adminId);
    Task UpdateInterestRateAsync(string loanId, decimal newRate);
}
```

### 4. DTOs en `Application/DTOs/`
```csharp
// Application/DTOs/Loans/AssignLoanRequestDto.cs
public class AssignLoanRequestDto
{
    [Required] public string ClienteId { get; set; }
    [Required, Range(1, double.MaxValue)] public decimal Monto { get; set; }
    [Required, Range(0.01, 100)] public decimal InteresAnual { get; set; }
    [Required] public int PlazoMeses { get; set; }   // 6,12,18...60
}
```

### 5. Servicio en `Application/Services/`
```csharp
// Application/Services/LoanService.cs
public class LoanService : ILoanService
{
    private readonly ILoanRepository _loanRepository;
    private readonly ISavingsAccountRepository _savingsRepo;
    private readonly IMapper _mapper;
    // Inyección por constructor, NUNCA new()
}
```

### 6. Repositorio en `Infrastructure/Persistence/Repositories/`
```csharp
// Infrastructure/Persistence/Repositories/LoanRepository.cs
public class LoanRepository : GenericRepository<Loan>, ILoanRepository
{
    public LoanRepository(ApplicationDbContext context) : base(context) { }
    // Solo consultas a la base de datos aquí, sin lógica de negocio
}
```

### 7. Controlador en `WebApi/Controllers/`
```csharp
// WebApi/Controllers/LoanController.cs
[ApiController]
[Route("api/loan")]
[Authorize(Roles = "Administrador")]
public class LoanController : ControllerBase
{
    private readonly ILoanService _loanService;
    // Solo delega al servicio, sin lógica de negocio aquí
}
```

---

## 👥 Distribución de Tareas

---

### 🧑‍💻 Persona 1 — Infraestructura Base + Autenticación + Usuarios

**Responsabilidad:** Construir los cimientos del proyecto que el resto del equipo usará.

#### Fase 1 — Setup (hacerlo primero, antes que los demás empiecen)

- [ ] Crear la solución con los 4 proyectos (.Core, .Application, .Infrastructure, .WebApi)
- [ ] Configurar referencias entre proyectos respetando ONION
- [ ] Instalar paquetes NuGet necesarios:
  - `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
  - `Microsoft.EntityFrameworkCore.SqlServer`
  - `Microsoft.AspNetCore.Authentication.JwtBearer`
  - `AutoMapper.Extensions.Microsoft.DependencyInjection`
  - `Swashbuckle.AspNetCore`
- [ ] Crear `ApplicationDbContext` con Identity en Infrastructure
- [ ] Crear las entidades base: `ApplicationUser`, `SavingsAccount`, `Transaction`
- [ ] Configurar seeding: roles (Administrador, Comercio), usuarios por defecto
- [ ] Crear migración inicial y verificar que la BD se crea correctamente
- [ ] Crear repositorio genérico `IGenericRepository<T>` y su implementación
- [ ] Configurar extensiones de DI en WebApi (servicios, Identity, JWT, Swagger, AutoMapper)

#### Fase 2 — Módulo de Autenticación

- [ ] **`POST /account/login`** — Autenticación con JWT
- [ ] **`POST /account/confirm`** — Confirmación de cuenta vía token
- [ ] **`POST /account/get-reset-token`** — Generar token de reseteo (inactiva usuario + envía correo con token en cuerpo)
- [ ] **`POST /account/reset-password`** — Cambiar contraseña con token
- [ ] Configurar servicio de envío de correo (SMTP o SendGrid)

#### Fase 3 — Módulo de Gestión de Usuarios

- [ ] **`GET /api/users`** — Listado paginado (excluye rol comercio, filtro por rol)
- [ ] **`GET /api/users/commerce`** — Listado paginado solo usuarios comercio
- [ ] **`POST /api/users`** — Crear usuario (admin/cajero/cliente). Si es cliente: crear cuenta principal de 9 dígitos. Enviar correo con token en cuerpo.
- [ ] **`POST /api/users/commerce/{commerceId}`** — Crear usuario de comercio (un solo usuario por comercio). Crear cuenta principal.
- [ ] **`PUT /api/users/{id}`** — Actualizar usuario. Si es cliente: sumar montoAdicional al saldo de cuenta principal.
- [ ] **`PATCH /api/users/{id}/status`** — Activar/inactivar (admin no puede modificar su propio estado)
- [ ] **`GET /api/users/{id}`** — Detalle de usuario con cuenta principal

**Ramas sugeridas:**
```
feature/setup-solucion-onion
feature/auth-module
feature/users-module
```

---

### 🧑‍💻 Persona 2 — Préstamos + Tarjetas de Crédito + Cuentas de Ahorro

**Responsabilidad:** Los tres módulos financieros principales del administrador.

> ⚠️ Espera que Persona 1 termine la Fase 1 (setup) antes de comenzar.

#### Módulo de Gestión de Préstamos

- [ ] Crear entidades: `Loan`, `AmortizationEntry` en Core
- [ ] Implementar repositorio, servicio y controlador
- [ ] **`GET /api/loan`** — Listado paginado con filtros por estado y cédula
- [ ] **`POST /api/loan`** — Asignar préstamo:
  - Validar que el cliente no tenga préstamo activo
  - Calcular cuota con fórmula francesa: `C = P * r(1+r)^n / ((1+r)^n - 1)`
  - Generar tabla de amortización (fechas desde el mes siguiente)
  - Evaluar riesgo (comparar deuda vs. promedio del sistema) → retornar `409` si es alto riesgo
  - Acreditar monto a cuenta principal del cliente
  - Enviar correo de aprobación al cliente
- [ ] **`GET /api/loan/{id}`** — Detalle con tabla de amortización completa
- [ ] **`PATCH /api/loan/{id}/rate`** — Editar tasa de interés:
  - Solo recalcular cuotas **futuras** (las pagadas o vencidas no se tocan)
  - Enviar correo al cliente con nueva tasa y nueva cuota

#### Módulo de Gestión de Tarjetas de Crédito

- [ ] Crear entidades: `CreditCard`, `CardConsumption` en Core
- [ ] Implementar repositorio, servicio y controlador
- [ ] **`GET /api/credit-card`** — Listado paginado con filtros por cédula y estado
- [ ] **`POST /api/credit-card`** — Asignar tarjeta:
  - Número único de 16 dígitos
  - Fecha expiración: fecha actual + 3 años (formato MM/AA)
  - CVC de 3 dígitos cifrado con SHA-256
- [ ] **`GET /api/credit-card/{id}`** — Ver consumos (con texto "AVANCE" si aplica)
- [ ] **`PATCH /api/credit-card/{id}/limit`** — Editar límite (no puede ser menor a la deuda actual). Enviar correo al cliente.
- [ ] **`PATCH /api/credit-card/{id}/cancel`** — Cancelar tarjeta (solo si deuda = 0)

#### Módulo de Gestión de Cuentas de Ahorro

- [ ] **`GET /api/savings-account`** — Listado paginado con filtros por cédula, estado y tipo
- [ ] **`POST /api/savings-account`** — Asignar cuenta secundaria:
  - Número único de 9 dígitos (no repetido en todo el sistema)
  - Estado activo, tipo secundaria
- [ ] **`GET /api/savings-account/{accountNumber}/transactions`** — Historial de transacciones con todos los campos (fecha, monto, tipo DÉBITO/CRÉDITO, beneficiario, origen, estado)

**Ramas sugeridas:**
```
feature/loans-module
feature/credit-card-module
feature/savings-account-module
```

---

### 🧑‍💻 Persona 3 — Comercios + Procesador de Pagos (Hermes Pay)

**Responsabilidad:** El módulo de comercios y el motor de pagos con tarjeta.

> ⚠️ Espera que Persona 1 termine la Fase 1 (setup) antes de comenzar.

#### Módulo de Gestión de Comercios

- [ ] Crear entidad `Commerce` en Core
- [ ] Implementar repositorio, servicio y controlador
- [ ] **`GET /api/commerce`** — Listado paginado (si no se pasan parámetros, devuelve todos los activos)
- [ ] **`GET /api/commerce/{id}`** — Detalle de comercio
- [ ] **`POST /api/commerce`** — Crear nuevo comercio (name, descripcion, logo)
- [ ] **`PUT /api/commerce/{id}`** — Actualizar comercio existente
- [ ] **`PATCH /api/commerce/{id}`** — Cambiar estado:
  - Al **desactivar**: inactivar todos los usuarios asociados al comercio
  - Al **reactivar**: los usuarios permanecen inactivos (deben resetear contraseña para activarse)

#### Módulo: Procesador de Pago — Hermes Pay

- [ ] Implementar servicio y controlador para `/pay/`
- [ ] Configurar acceso dual: rol **Administrador** o rol **Comercio**
- [ ] **`GET /pay/get-transactions/{commerceId}`** — Historial de transacciones paginado:
  - Si el token es de rol **comercio**: ignorar el `commerceId` de la URL y usar el ID del token
  - Si el token es de rol **administrador**: usar el `commerceId` de la URL
- [ ] **`POST /pay/process-payment/{commerceId}`** — Procesar pago con tarjeta:
  - Validaciones obligatorias:
    - Todos los campos requeridos (`cardNumber`, `monthExpirationCard`, `yearExpirationCard`, `CVC`, `transactionAmount`)
    - Tarjeta existe, está activa y no está vencida
    - CVC válido (comparar hash SHA-256)
    - Comercio existe y está activo
    - Monto no excede el límite disponible de la tarjeta (límite - deuda actual)
  - Si pasa validaciones:
    - Acreditar monto a cuenta principal del comercio
    - Registrar consumo en la tarjeta con estado "APROBADO", nombre del comercio, fecha/hora
    - Enviar correo al **cliente** con asunto `"Consumo realizado con la tarjeta [XXXX]"`
    - Enviar correo al **comercio** con asunto `"Pago recibido a través de tarjeta [XXXX]"`
  - Si falla alguna validación → registrar consumo con estado "RECHAZADO", retornar `400`

**Ramas sugeridas:**
```
feature/commerce-module
feature/hermes-pay-module
```

---

## 🔁 Proceso de Integración (Pull Requests)

Antes de abrir un PR hacia `develop`, verifica:

- [ ] El código compila sin errores (`dotnet build`)
- [ ] Las migraciones están actualizadas si agregaste/modificaste entidades
- [ ] Swagger documenta correctamente los endpoints nuevos
- [ ] Los códigos HTTP de respuesta coinciden con la especificación
- [ ] No hay lógica de negocio en el controlador
- [ ] No hay consultas a la BD en el controlador o el servicio directamente (solo a través del repositorio)
- [ ] El repositorio no contiene lógica de negocio (solo consultas)

---

## 🗂️ Tabla de Endpoints por Persona

| Módulo | Endpoint | Persona |
|---|---|---|
| Auth | `POST /account/login` | 1 |
| Auth | `POST /account/confirm` | 1 |
| Auth | `POST /account/get-reset-token` | 1 |
| Auth | `POST /account/reset-password` | 1 |
| Usuarios | `GET /api/users` | 1 |
| Usuarios | `GET /api/users/commerce` | 1 |
| Usuarios | `POST /api/users` | 1 |
| Usuarios | `POST /api/users/commerce/{id}` | 1 |
| Usuarios | `PUT /api/users/{id}` | 1 |
| Usuarios | `PATCH /api/users/{id}/status` | 1 |
| Usuarios | `GET /api/users/{id}` | 1 |
| Préstamos | `GET /api/loan` | 2 |
| Préstamos | `POST /api/loan` | 2 |
| Préstamos | `GET /api/loan/{id}` | 2 |
| Préstamos | `PATCH /api/loan/{id}/rate` | 2 |
| Tarjetas | `GET /api/credit-card` | 2 |
| Tarjetas | `POST /api/credit-card` | 2 |
| Tarjetas | `GET /api/credit-card/{id}` | 2 |
| Tarjetas | `PATCH /api/credit-card/{id}/limit` | 2 |
| Tarjetas | `PATCH /api/credit-card/{id}/cancel` | 2 |
| Cuentas | `GET /api/savings-account` | 2 |
| Cuentas | `POST /api/savings-account` | 2 |
| Cuentas | `GET /api/savings-account/{num}/transactions` | 2 |
| Comercios | `GET /api/commerce` | 3 |
| Comercios | `GET /api/commerce/{id}` | 3 |
| Comercios | `POST /api/commerce` | 3 |
| Comercios | `PUT /api/commerce/{id}` | 3 |
| Comercios | `PATCH /api/commerce/{id}` | 3 |
| Hermes Pay | `GET /pay/get-transactions/{id}` | 3 |
| Hermes Pay | `POST /pay/process-payment/{id}` | 3 |

---

## ⚙️ Checklist de Requerimientos Técnicos Globales

Estos aplican a **todos**:

- [ ] Arquitectura ONION correctamente implementada (sin saltarse capas)
- [ ] Entity Framework Code First con migraciones
- [ ] Repositorio genérico (`IGenericRepository<T>`) + repositorios específicos
- [ ] Servicios genéricos y de dominio en capa Application
- [ ] ASP.NET Identity para gestión de usuarios
- [ ] JWT para autenticación en todos los endpoints (menos `/account/login`)
- [ ] AutoMapper para mapeo Entidad ↔ DTO
- [ ] Swagger configurado y funcional
- [ ] Todos los montos en `decimal` (no `float` ni `double`)
- [ ] Correos enviados con token en el cuerpo (nunca como enlace en el API)
- [ ] Respuestas HTTP correctas: `200`, `201`, `204`, `400`, `401`, `403`, `404`, `409`
