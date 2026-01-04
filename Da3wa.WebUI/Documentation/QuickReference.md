# Quick Reference Guide

## Request Flow Cheat Sheet

### Execution Order

```
1. GlobalExceptionHandlingMiddleware (try-catch wrapper)
2. HttpsRedirection
3. Static Files
4. Routing
5. Authentication
6. Authorization
7. ControllerActionFilter.OnActionExecuting
8. Model Binding
9. Action Execution
10. ExceptionHandlingFilter (if exception)
11. ControllerActionFilter.OnActionExecuted
12. Result Execution
13. Response (reverse middleware order)
```

## Exception Handling Quick Guide

### Exception Types → Actions

| Exception | Status | Action | Message |
|-----------|--------|--------|---------|
| `KeyNotFoundException` | 404 | Redirect to Index | Exception message |
| `InvalidOperationException` | 400 | Redirect/View | Exception message |
| `UnauthorizedAccessException` | 403 | Redirect to Home | "You do not have permission..." |
| `Exception` (generic) | 500 | Redirect to Index/Error | "An unexpected error occurred..." |

### AJAX Requests

- Detected by: `X-Requested-With: XMLHttpRequest` or `Accept: application/json`
- Response: JSON `{ "error": "message" }`
- Status code: Based on exception type

## Logging Quick Guide

### Log Levels (most to least severe)

1. **Critical** - System failures
2. **Error** - Exceptions and errors
3. **Warning** - Warnings
4. **Information** - General info
5. **Debug** - Debugging details
6. **Trace** - Very detailed

### Where We Log

- **ExceptionHandlingFilter**: Logs all exceptions with context
- **Services**: Can log operations (optional)
- **Middleware**: Logs unhandled exceptions

### Log Format

```
[Timestamp] [Level] Message
  Controller: AuthController
  Action: Create
  Exception: InvalidOperationException
  Message: User already exists
  StackTrace: ...
```

## TempData Flow

```
Exception → HttpContext.Items["ExceptionErrorMessage"]
    ↓
ControllerActionFilter.OnActionExecuted
    ↓
TempData["Error"] = message
    ↓
Redirect
    ↓
View reads TempData["Error"]
    ↓
Display to user
```

## File Locations

### Middleware
- `Da3wa.WebUI/Middleware/GlobalExceptionHandlingMiddleware.cs`

### Filters
- `Da3wa.WebUI/Filters/ExceptionHandlingFilter.cs` - Main exception handler
- `Da3wa.WebUI/Filters/ControllerActionFilter.cs` - TempData bridge
- `Da3wa.WebUI/Filters/HandleExceptionAttribute.cs` - Optional attribute

### Configuration
- `Da3wa.WebUI/DependencyInjection/ServiceCollectionExtensions.cs` - Filter registration
- `Da3wa.WebUI/Program.cs` - Middleware registration

## Common Patterns

### Throwing Exceptions in Services

```csharp
// Not found
throw new KeyNotFoundException("User not found");

// Business logic error
throw new InvalidOperationException("User already exists");

// Permission error
throw new UnauthorizedAccessException();
```

### Accessing TempData in Views

```razor
@if (TempData["Error"] != null)
{
    <div class="alert alert-danger">@TempData["Error"]</div>
}

@if (TempData["Success"] != null)
{
    <div class="alert alert-success">@TempData["Success"]</div>
}
```

### Logging in Services

```csharp
_logger.LogInformation("Creating user: {Email}", email);
_logger.LogWarning("User creation failed: {Reason}", reason);
_logger.LogError(exception, "Error creating user: {Email}", email);
```

## Troubleshooting

### Problem: Exception not caught
- ✅ Check filter is registered
- ✅ Check exception is thrown in action (not middleware)
- ✅ Check `context.ExceptionHandled = true`

### Problem: TempData not showing
- ✅ Check `ControllerActionFilter` is registered
- ✅ Check redirect is happening (TempData only works across redirects)
- ✅ Check view is reading `TempData["Error"]`

### Problem: No logs
- ✅ Check log level in `appsettings.json`
- ✅ Check logger is injected
- ✅ Check console/output window

## Key Differences

### Middleware vs Filters

| Middleware | Filters |
|------------|---------|
| Every request | MVC only |
| HttpContext | ActionContext |
| Early in pipeline | After routing |
| Cross-cutting | MVC-specific |

### Exception Handling Layers

1. **Action try-catch** (removed - handled by filter)
2. **ExceptionHandlingFilter** (catches action exceptions)
3. **GlobalExceptionHandlingMiddleware** (catches unhandled)

---

**For detailed information, see**: `RequestLifecycleAndExceptionHandling.md`

