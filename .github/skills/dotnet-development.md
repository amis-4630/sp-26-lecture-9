---
name: "C# / .NET Development"
description: "Patterns, conventions, and best practices for .NET backend development in this workspace. Covers models, DbContext, data seeding, validation, middleware, and Program.cs configuration."
---

# C# / .NET Development Skill

Use this skill when creating or modifying .NET backend code in this workspace.

---

## Project Structure

Follow the established folder layout:

```
Backend/
├── Controllers/       # HTTP endpoints (one per resource)
├── Models/           # EF Core entity classes
├── Dtos/             # Request/response Data Transfer Objects
├── Validators/       # FluentValidation AbstractValidator<T>
├── Mappings/         # AutoMapper Profile classes
├── Middleware/       # Global exception handling (IExceptionHandler)
├── Data/             # DbContext + seed data
└── Program.cs        # Service registration + middleware pipeline
```

---

## Models

- Simple POCO classes. No logic in model classes.
- Initialize collection navigation properties with empty lists: `public List<Payment> Payments { get; set; } = [];`
- Foreign key pattern: explicit FK property + nullable navigation property:
  ```csharp
  public int ApplicantId { get; set; }
  public Applicant? Applicant { get; set; }
  ```
- Use `[Column(TypeName = "decimal(12,2)")]` for monetary values.
- Use `[Required]` and `[MaxLength(n)]` data annotations on required fields.
- Default strings to `string.Empty`: `public string Notes { get; set; } = string.Empty;`
- Add XML doc comments above model classes.

---

## DbContext & Seeding

- One `DbContext` per project with `DbSet<T>` properties for each entity.
- Seed data in `OnModelCreating` using `modelBuilder.Entity<T>().HasData(...)`.
- Initialize on startup in `Program.cs`:
  ```csharp
  using (var scope = app.Services.CreateScope())
  {
      var context = scope.ServiceProvider.GetRequiredService<MyContext>();
      context.Database.EnsureCreated();
  }
  ```

---

## FluentValidation

- One validator class per model: `public class ApplicantValidator : AbstractValidator<Applicant>`.
- Define all rules in the constructor.
- Common rule patterns:
  ```csharp
  RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
  RuleFor(x => x.Amount).GreaterThan(0);
  RuleFor(x => x.Rating).InclusiveBetween(1, 5);
  ```
- Register all validators in `Program.cs`:
  ```csharp
  builder.Services.AddValidatorsFromAssemblyContaining<Program>();
  ```
- Use the `FluentValidationExtensions.AddToModelState()` extension to convert results to ModelState in controllers.

---

## AutoMapper

- One `Profile` class per mapping group in `Mappings/`.
- Register in `Program.cs`: `builder.Services.AddAutoMapper(typeof(Program));`
- Use `ForMember` for custom mappings, `AfterMap` for computed values.
- Map DTOs ↔ Models. Never expose raw EF models in responses when DTOs exist.

---

## Middleware & Error Handling

- Global exception handler implements `IExceptionHandler`.
- Map exceptions to HTTP status codes:
  - `KeyNotFoundException` → 404
  - `ArgumentException` / `InvalidOperationException` → 400
  - Unhandled → 500
- Return RFC 7807 Problem Details with `traceId` and `timestamp`.
- Include stack traces only in Development environment.
- Register in `Program.cs`:
  ```csharp
  builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
  builder.Services.AddProblemDetails();
  app.UseExceptionHandler();
  ```

---

## Program.cs Configuration Order

Follow this service registration and middleware order:

```csharp
// 1. CORS
builder.Services.AddCors(...);

// 2. Controllers + JSON options (IgnoreCycles for nav properties)
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// 3. Validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// 4. AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// 5. OpenAPI
builder.Services.AddOpenApi();

// 6. ProblemDetails + ExceptionHandler
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// 7. DbContext
builder.Services.AddDbContext<MyContext>(o => o.UseInMemoryDatabase("DbName"));

// --- Middleware pipeline ---
app.UseExceptionHandler();
app.UseCors();
app.MapControllers();
```

---

## C# Code Style

- File-scoped namespaces: `namespace MyApp.Controllers;`
- Modern C# 14 features when TFM allows: primary constructors, switch expressions, collection expressions, raw string literals.
- Nullable reference types enabled. Respect nullability annotations.
- Access modifiers: `private` by default, only widen when needed.
- Guard clauses: `ArgumentNullException.ThrowIfNull(x)`, `string.IsNullOrWhiteSpace(x)`.
- Async end-to-end—never sync-over-async.
- Comments explain **why**, not what. Add XML doc comments on public methods.

---

## .http Test Files

- Create a `*_Tests.http` file alongside the project for each controller.
- Include requests for all CRUD operations with example payloads.
- Use `@host` variable for the base URL.
