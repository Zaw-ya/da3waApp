# Exception Handling Solutions

This project implements multiple layers of exception handling to eliminate duplicate try-catch blocks in controllers.

## Solutions Implemented

### 1. **Global Exception Handling Middleware** (Recommended for API/Unhandled Exceptions)
**File:** `Middleware/GlobalExceptionHandlingMiddleware.cs`

- Catches all unhandled exceptions in the request pipeline
- Best for: API endpoints, unhandled exceptions
- Automatically registered in `Program.cs`
- Returns JSON responses for API calls

### 2. **Exception Handling Filter** (Recommended for Controllers)
**File:** `Filters/ExceptionHandlingFilter.cs`

- Registered globally for all controller actions
- Handles exceptions with proper TempData support
- Automatically redirects or returns views with error messages
- Handles different exception types:
  - `KeyNotFoundException` → 404 with error message
  - `InvalidOperationException` → 400 with error message (preserves model for POST)
  - `UnauthorizedAccessException` → 403 redirect
  - Generic exceptions → 500 with generic message

### 3. **HandleException Attribute** (Optional - For Selective Use)
**File:** `Filters/HandleExceptionAttribute.cs`

- Can be applied to specific actions if needed
- Same functionality as the global filter but selective
- Usage: `[HandleException]` on action methods

## Benefits

✅ **No more duplicate try-catch blocks** - All exceptions handled automatically
✅ **Consistent error handling** - Same behavior across all controllers
✅ **Automatic logging** - All exceptions logged with context
✅ **TempData support** - Error messages automatically set
✅ **Model preservation** - POST requests keep model state on errors
✅ **ViewBag setup** - Helper methods handle ViewBag for Create/Edit actions

## Usage Example

### Before (with try-catch):
```csharp
public async Task<IActionResult> Index()
{
    try
    {
        var users = await _authService.GetAllUsersAsync();
        return View(users);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error loading users");
        TempData["Error"] = "An error occurred while loading users.";
        return View(new List<UserDto>());
    }
}
```

### After (no try-catch needed):
```csharp
public async Task<IActionResult> Index()
{
    var users = await _authService.GetAllUsersAsync();
    return View(users);
}
```

The filter automatically:
- Catches any exception
- Logs it with context
- Sets TempData["Error"]
- Redirects to Index action

## Customization

### To customize error messages:
Edit `ExceptionHandlingFilter.cs` methods:
- `HandleKeyNotFoundException`
- `HandleInvalidOperationException`
- `HandleUnauthorizedException`
- `HandleGenericException`

### To disable for specific actions:
Use `[SkipExceptionHandling]` attribute (if you create one) or handle exceptions manually in that action.

## Best Practices

1. **Use the global filter** for most controller actions
2. **Throw specific exceptions** from services:
   - `KeyNotFoundException` for not found
   - `InvalidOperationException` for business logic errors
   - `UnauthorizedAccessException` for permission issues
3. **Let the filter handle** - Don't add try-catch unless you need special handling
4. **Use helper methods** for ViewBag setup (like `SetupViewBagForCreate`)

