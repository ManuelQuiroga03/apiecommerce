# 🛒 ApiEcommerce - RESTful Web API (.NET 10)

API RESTful empresarial para la gestión de comercio electrónico (E-commerce), desarrollada con **.NET 10**, **ASP.NET Core Web API**, **Entity Framework Core 10** y **SQL Server 2022**.

Diseñada bajo patrones de arquitectura limpia, seguridad con JWT y roles en Identity, documentación interactiva con Swagger OpenAPI v2 y gestión optimizada de archivos estáticos.

---

## 🚀 Características Principales

- **Framework & Runtime**: Desarrollado en .NET 10.0 (C# 13).
- **Persistencia de Datos**: Entity Framework Core 10 con SQL Server.
- **Autenticación & Autorización**:
  - JWT (JSON Web Tokens) con esquema Bearer HTTP.
  - ASP.NET Core Identity con gestión de roles (`Admin` y `User`).
  - Hasheo seguro de credenciales con `PasswordHasher<ApplicationUser>` / BCrypt.
- **Mapeo de Objetos**: AutoMapper para desacoplamiento estricto de DTOs y Entidades.
- **Paginación Integrada**: Respuesta enriquecida con metadatos (`pageNumber`, `pageSize`, `totalItems`, `totalPages`, `hasPreviousPage`, `hasNextPage`).
- **Manejo de Archivos**: Subida y gestión de imágenes para productos con `IWebHostEnvironment`, generación de URLs dinámicas y limpieza automática de archivos huérfanos.
- **Database Seeding**: Sembrado de base de datos automatizado e idempotente con catálogo maestro de productos y usuarios (`DbInitializer`).
- **Documentación Interactiva**: Swagger / OpenAPI 3 con soporte para autorización Bearer integrada.
- **Versionamiento de API**: Configurado con `Asp.Versioning` para soporte multiversión (`v1`, `v2`).
- **CORS & Response Caching**: Políticas de CORS y almacenamiento en caché para respuestas optimizadas.

---

## 🛠️ Stack Tecnológico

| Componente | Tecnología | Versión |
| :--- | :--- | :--- |
| **Backend** | .NET / C# | 10.0 |
| **ORM** | Entity Framework Core | 10.0 |
| **Base de Datos** | Microsoft SQL Server (Container) | 2022 |
| **Seguridad** | JWT Bearer + ASP.NET Core Identity | 10.0 |
| **Documentación** | Swashbuckle / Microsoft.OpenApi | 2.x |
| **Mapeador** | AutoMapper | 16.2.0 |

---

## 📂 Estructura del Proyecto

```text
ApiEcommerce/
├── Constants/                  # Políticas CORS y constantes del sistema
├── Controllers/                # Controladores de la API (Categories, Products, Users)
├── Data/                       # ApplicationDbContext y DbInitializer (Seed)
├── Mapping/                    # Perfiles de AutoMapper (Category, Product, User)
├── Migrations/                 # Migraciones de Entity Framework Core
├── Models/                     # Entidades del Dominio y DTOs
│   ├── Dtos/                   # Data Transfer Objects
│   ├── ApplicationUser.cs      # Extensión de IdentityUser
│   ├── Category.cs             # Modelo de Categorías
│   └── Product.cs              # Modelo de Productos
├── Repository/                 # Patrón Repositorio (IRepository / Implementaciones)
├── Properties/                 # launchSettings.json y configuración de inicio
├── appsettings.json            # Cadenas de conexión y configuraciones de JWT
└── Program.cs                  # Configuración de servicios y pipeline HTTP
```

---

## 👥 Usuarios y Credenciales Sembradas (Seeding)

Al iniciar la aplicación por primera vez, el sistema inicializa automáticamente los siguientes usuarios de prueba:

| Rol | Correo | Contraseña | Propósito |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@techstore.com` | `Admin123!` | Administración total del catálogo, creación y edición |
| **User** | `cliente@techstore.com` | `User123!` | Usuario cliente para catálogo y compras |

---

## ⚡ Ejecución Rápida

### Requisitos Previos:
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Instancia de SQL Server 2022 o contenedor en Docker/Podman

### Pasos:
1. Clonar el repositorio:
   ```bash
   git clone https://github.com/ManuelQuiroga03/apiecommerce.git
   ```
2. Configurar la cadena de conexión en `appsettings.json`.
3. Aplicar migraciones:
   ```bash
   dotnet ef database update
   ```
4. Ejecutar la API:
   ```bash
   dotnet run --project ApiEcommerce
   ```
5. Abrir Swagger UI en el navegador:
   ```text
   https://localhost:7038/swagger
   ```

---

## 🌿 Ramas del Repositorio

- `main`: Versión base y estable del Backend API.
- `v1_Manu`: Versión extendida con filtros avanzados, resiliencia y posterior integración con frontend (Angular / Blazor).

---

## 📄 Licencia

Este proyecto fue desarrollado para fines académicos y de portafolio profesional.
