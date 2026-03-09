# Estado del Proyecto: Artemis Banking

## Decisiones Arquitectónicas (Viernes, 6 de marzo de 2026)

### 1. Estructura de Capas y Entidades
- **Domain/Entities**: Contiene las entidades de negocio puras.
  - `SavingsAccount`: Incluye `Id`, `AccountNumber` (9 dígitos), `Balance`, y `UserId` (string).
  - `Transaction`: Incluye `Id`, `Amount`, `CreatedAt`, `Type` (Enum), `UserId` y la relación con `SavingsAccount`.
- **Infrastructure/Identity**:
  - `AppUser`: Hereda de `IdentityUser`. Contiene campos extra (Cédula, Nombre, Apellido, etc.).
- **Infrastructure/Persistence**:
  - `ApplicationDbContext`: Punto de entrada para EF Core.

### 2. Relación Domain e Identity (Clean Architecture)
- Para evitar dependencias circulares, las entidades en `Domain` **no** tienen propiedades de navegación hacia `AppUser`.
- Se utiliza únicamente la propiedad `string UserId`.
- La relación se configura en la capa de **Infrastructure** mediante Fluent API.

### 3. Configuración de Base de Datos
- Se ha decidido utilizar clases separadas que implementen `IEntityTypeConfiguration<T>` para cada entidad.
- Ubicación: `Infrastructure/Persistence/Configurations/`.
- Se utilizará `builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly())` en el `OnModelCreating` del Context.

## Tareas Pendientes
1. [ ] Crear la clase `SavingsAccount` en `Domain/Entities`.
2. [ ] Crear el Enum `TransactionType` en `Domain/Enums`.
3. [ ] Finalizar la clase `Transaction` en `Domain/Entities`.
4. [ ] Implementar las configuraciones `IEntityTypeConfiguration` en `Infrastructure`.
5. [ ] Ejecutar migraciones iniciales.
