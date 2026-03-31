---
name: "API Design"
description: "RESTful endpoint design conventions including controller structure, routing, DTOs, query parameters, validation flow, and response shapes used in this workspace."
---

# API Design Skill

Use this skill when designing, creating, or modifying API endpoints in this workspace.

---

## Controller Conventions

### Class Setup

```csharp
[ApiController]
[Route("api/[controller]")]
public class ResourcesController : ControllerBase
{
    private readonly MyContext _context;
    private readonly IValidator<Resource> _validator;

    public ResourcesController(MyContext context, IValidator<Resource> validator)
    {
        _context = context;
        _validator = validator;
    }
}
```

- One controller per resource.
- `[ApiController]` for automatic model binding and 400 responses.
- Constructor-inject `DbContext` and `IValidator<T>`.
- Add XML doc comments (`/// <summary>`) on every public action.

### Nested Sub-Resources

For child entities, use a parent-scoped route:

```csharp
[ApiController]
[Route("api/loanapplications/{loanApplicationId}/[controller]")]
public class PaymentsController : ControllerBase
```

---

## CRUD Endpoint Pattern

### GET (List)

```csharp
/// <summary>Get all resources with optional filters.</summary>
[HttpGet]
public async Task<ActionResult<IEnumerable<Resource>>> GetAll(
    [FromQuery] int? categoryId,
    [FromQuery] string? search)
{
    var query = _context.Resources
        .Include(r => r.Category)
        .AsQueryable();

    if (categoryId.HasValue)
        query = query.Where(r => r.CategoryId == categoryId.Value);

    if (!string.IsNullOrWhiteSpace(search))
        query = query.Where(r => r.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

    return Ok(await query.ToListAsync());
}
```

- Use nullable `[FromQuery]` parameters for optional filtering.
- Build the query incrementally with conditional `Where` clauses.
- Include navigation properties needed for the response.
- Return `Ok(list)` — status 200.

### GET (Single)

```csharp
/// <summary>Get a resource by ID.</summary>
[HttpGet("{id}")]
public async Task<ActionResult<Resource>> GetById(int id)
{
    var item = await _context.Resources
        .Include(r => r.Category)
        .Include(r => r.Children)
        .FirstOrDefaultAsync(r => r.Id == id);

    if (item == null)
        throw new KeyNotFoundException($"Resource with ID {id} not found");

    return Ok(item);
}
```

- Use `FirstOrDefaultAsync` with Includes for the detail view.
- Throw `KeyNotFoundException` for missing resources — the global exception handler maps it to 404.
- Include deeper navigation properties for detail endpoints.

### POST (Create)

```csharp
/// <summary>Create a new resource.</summary>
[HttpPost]
public async Task<ActionResult<Resource>> Create(Resource resource)
{
    var result = await _validator.ValidateAsync(resource);
    if (!result.IsValid)
    {
        result.AddToModelState(ModelState);
        return ValidationProblem(ModelState);
    }

    // Set server-controlled fields
    resource.Status = "Pending";
    resource.CreatedDate = DateTime.UtcNow;

    _context.Resources.Add(resource);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetById), new { id = resource.Id }, resource);
}
```

- Validate with FluentValidation first.
- On invalid: convert to ModelState, return `ValidationProblem()` (RFC 7807).
- Set server-controlled fields (status, dates, computed values) — never trust the client.
- Return `CreatedAtAction` (201) with Location header.

### PUT (Update)

```csharp
/// <summary>Update an existing resource.</summary>
[HttpPut("{id}")]
public async Task<ActionResult<Resource>> Update(int id, Resource updated)
{
    var existing = await _context.Resources.FindAsync(id);
    if (existing == null)
        throw new KeyNotFoundException($"Resource with ID {id} not found");

    var result = await _validator.ValidateAsync(updated);
    if (!result.IsValid)
    {
        result.AddToModelState(ModelState);
        return ValidationProblem(ModelState);
    }

    // Update only allowed fields
    existing.Name = updated.Name;
    existing.Amount = updated.Amount;
    // Don't update: Id, Status, CreatedDate (server-controlled)

    await _context.SaveChangesAsync();
    return Ok(existing);
}
```

- Fetch existing first, throw if not found.
- Validate the incoming payload.
- Explicitly map only user-editable fields — never overwrite server-controlled fields.
- Return `Ok(existing)` or `NoContent()`.

### DELETE

```csharp
/// <summary>Delete a resource by ID.</summary>
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(int id)
{
    var item = await _context.Resources.FindAsync(id);
    if (item == null)
        throw new KeyNotFoundException($"Resource with ID {id} not found");

    _context.Resources.Remove(item);
    await _context.SaveChangesAsync();

    return NoContent();
}
```

- Return `NoContent()` (204) on success.

---

## DTOs

### Naming Convention

| Purpose | Name Pattern | Example |
|---------|-------------|---------|
| POST body | `Create{Resource}Request` or `Add{Action}Request` | `AddToQueueRequest` |
| PUT body | `Update{Resource}Request` | `UpdateItemRequest` |
| Response | `{Resource}Response` | `ReviewItemResponse` |

- DTOs are simple POCOs — no logic, no annotations.
- Place in `Dtos/` folder. Group related DTOs in one file if small.
- Map between DTOs and Models using AutoMapper profiles.

---

## Validation Flow

1. Receive request body (model binding).
2. Call `_validator.ValidateAsync(entity)`.
3. If invalid → `result.AddToModelState(ModelState)` → `return ValidationProblem(ModelState)`.
4. If valid → proceed with business logic.

The `ValidationProblem()` response follows RFC 7807 format automatically.

---

## Error Response Format (RFC 7807)

All error responses use Problem Details:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Resource with ID 42 not found",
  "instance": "/api/resources/42",
  "traceId": "00-abc123...",
  "timestamp": "2026-03-31T12:00:00Z"
}
```

---

## Frontend ↔ API Contract

- TypeScript types in `data/` or `types/` must match DTO shapes exactly.
- Use camelCase in TypeScript, PascalCase in C# — System.Text.Json handles conversion.
- API base URL: `http://localhost:5000` (backend), frontend runs on `http://localhost:5173` (Vite).
- CORS is configured to allow the Vite dev server origin.

---

## .http Test Files

For every controller, create a `{Resource}_Tests.http` file:

```http
@host = http://localhost:5000

### Get all
GET {{host}}/api/resources

### Get by ID
GET {{host}}/api/resources/1

### Create
POST {{host}}/api/resources
Content-Type: application/json

{
  "name": "Test Resource",
  "amount": 1000
}

### Update
PUT {{host}}/api/resources/1
Content-Type: application/json

{
  "name": "Updated Resource",
  "amount": 2000
}

### Delete
DELETE {{host}}/api/resources/1
```
