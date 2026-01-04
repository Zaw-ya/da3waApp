# Request Lifecycle, Middleware, Filters, and Exception Handling

## Table of Contents
1. [Overview](#overview)
2. [Request Lifecycle in ASP.NET Core](#request-lifecycle-in-aspnet-core)
3. [Middleware Pipeline](#middleware-pipeline)
4. [Filter Pipeline](#filter-pipeline)
5. [Exception Handling Flow](#exception-handling-flow)
6. [Logging System](#logging-system)
7. [Complete Request Flow Example](#complete-request-flow-example)
8. [Configuration](#configuration)

---

## Overview

This document explains how requests flow through the application, how middleware and filters work, how exceptions are handled, and how logging is implemented throughout the entire lifecycle.

### Key Components

1. **Middleware**: Components that form a pipeline to handle requests and responses
2. **Filters**: Components that run at specific points in the MVC action execution pipeline
3. **Exception Handling**: Global exception handling using filters and middleware
4. **Logging**: Structured logging throughout the application

---

## Request Lifecycle in ASP.NET Core

### High-Level Flow

```
HTTP Request
    ↓
Middleware Pipeline (in order)
    ↓
Routing Middleware
    ↓
Authentication Middleware
    ↓
Authorization Middleware
    ↓
MVC Pipeline
    ↓
Action Filters (Before Action)
    ↓
Controller Action Execution
    ↓
Action Filters (After Action)
    ↓
Result Filters
    ↓
View Rendering (if applicable)
    ↓
Response Filters
    ↓
Middleware Pipeline (reverse order)
    ↓
HTTP Response
```

---

## Middleware Pipeline

### What is Middleware?

Middleware are components that are assembled into an application pipeline to handle requests and responses. Each middleware component can:
- Choose whether to pass the request to the next component
- Perform work before and after the next component in the pipeline

### Middleware in Our Project

#### 1. GlobalExceptionHandlingMiddleware
**Location**: `Da3wa.WebUI/Middleware/GlobalExceptionHandlingMiddleware.cs`

**Purpose**: Catches unhandled exceptions that weren't caught by filters

**Execution Order**: Early in the pipeline (registered first)

**What it does**:
- Wraps the entire request pipeline in a try-catch
- Catches any exception that bubbles up to the middleware level
- Logs the exception
- Returns appropriate response (JSON for API, redirect for MVC)

**Code Flow**:
```csharp
Request comes in
    ↓
try {
    await _next(context);  // Continue to next middleware
} catch (Exception ex) {
    Log exception
    Handle exception (JSON or redirect)
}
```

**When it runs**:
- Only if an exception reaches the middleware level
- Usually after filters have tried to handle it
- Acts as a safety net

#### 2. HttpsRedirection Middleware
**Purpose**: Redirects HTTP requests to HTTPS

#### 3. Static Files Middleware
**Purpose**: Serves static files (CSS, JS, images)

#### 4. Routing Middleware
**Purpose**: Matches incoming requests to routes

#### 5. Authentication Middleware
**Purpose**: Authenticates the user (checks cookies, tokens, etc.)

#### 6. Authorization Middleware
**Purpose**: Authorizes the user (checks if user has permission)

### Middleware Execution Order

The order in `Program.cs` is critical:

```csharp
1. GlobalExceptionHandlingMiddleware  // First - catches everything
2. UseExceptionHandler (if not dev)    // Alternative exception handler
3. UseHsts                             // Security headers
4. UseHttpsRedirection                 // Force HTTPS
5. UseRouting                          // Route matching
6. UseAuthentication                  // Check who you are
7. UseAuthorization                   // Check what you can do
8. MapControllers                     // Execute controller actions
9. MapRazorPages                      // Execute Razor pages
```

**Important**: Middleware executes in the order registered, but the response flows back in reverse order.

---

## Filter Pipeline

### What are Filters?

Filters allow you to run code before or after specific stages in the MVC action execution pipeline. They run in a specific order and can short-circuit the pipeline.

### Filter Types and Execution Order

```
1. Authorization Filters (IAsyncAuthorizationFilter)
   - Run first
   - Can short-circuit pipeline
   - Example: [Authorize] attribute

2. Resource Filters (IAsyncResourceFilter)
   - Run after authorization
   - Can cache responses
   - Run around model binding

3. Action Filters (IAsyncActionFilter)
   - Run before and after action execution
   - Can modify action arguments
   - Can modify action results

4. Exception Filters (IAsyncExceptionFilter)
   - Run only if exception occurs
   - Can handle exceptions
   - Run before result filters

5. Result Filters (IAsyncResultFilter)
   - Run before and after result execution
   - Can modify the result

6. AlwaysRunResult Filters
   - Always run, even if exception occurred
```

### Filters in Our Project

#### 1. ExceptionHandlingFilter
**Type**: `IAsyncExceptionFilter`
**Location**: `Da3wa.WebUI/Filters/ExceptionHandlingFilter.cs`

**Purpose**: Handles all exceptions in controller actions

**When it runs**:
- Only when an exception is thrown
- Before result filters
- After action execution

**What it does**:
1. Logs the exception with context (controller, action, message)
2. Marks exception as handled
3. Stores error message in `HttpContext.Items`
4. Returns appropriate result based on exception type:
   - `KeyNotFoundException` → 404 redirect
   - `InvalidOperationException` → 400 redirect (or view for POST)
   - `UnauthorizedAccessException` → 403 redirect
   - Generic exceptions → 500 redirect

**Code Flow**:
```csharp
Exception thrown in action
    ↓
OnExceptionAsync called
    ↓
Log exception
    ↓
Store error in HttpContext.Items
    ↓
Determine result based on exception type
    ↓
Set context.Result
    ↓
Mark exception as handled
```

#### 2. ControllerActionFilter
**Type**: `IActionFilter`
**Location**: `Da3wa.WebUI/Filters/ControllerActionFilter.cs`

**Purpose**: Bridges exception filter and TempData

**When it runs**:
- `OnActionExecuting`: Before action executes
- `OnActionExecuted`: After action executes (even if exception occurred)

**What it does**:

**OnActionExecuting**:
```csharp
1. Check if controller is a Controller instance
2. Store controller in HttpContext.Items["__Controller"]
3. This makes controller available to exception filter
```

**OnActionExecuted**:
```csharp
1. Check if error message exists in HttpContext.Items
2. Get controller from HttpContext.Items
3. Transfer error message to TempData["Error"]
4. Now views can display the error
```

**Why we need it**:
- `ExceptionContext` doesn't have direct access to `Controller`
- We use `HttpContext.Items` as a bridge
- This filter transfers data between exception handling and TempData

#### 3. HandleExceptionAttribute (Optional)
**Type**: `ActionFilterAttribute` (implements `IActionFilter`)
**Location**: `Da3wa.WebUI/Filters/HandleExceptionAttribute.cs`

**Purpose**: Alternative to global filter (can be applied per action)

**Note**: Not currently used since we have global filter, but available if needed.

---

## Exception Handling Flow

### Complete Exception Handling Flow

```
1. Action executes
   ↓
2. Exception thrown
   ↓
3. ExceptionHandlingFilter.OnExceptionAsync called
   ├─ Log exception
   ├─ Store error in HttpContext.Items["ExceptionErrorMessage"]
   ├─ Determine result (redirect/view)
   └─ Set context.Result
   ↓
4. ControllerActionFilter.OnActionExecuted called
   ├─ Get error from HttpContext.Items
   ├─ Get controller from HttpContext.Items
   └─ Set TempData["Error"] = error message
   ↓
5. Result executed (redirect or view)
   ↓
6. View renders (if view result)
   ├─ Reads TempData["Error"]
   └─ Displays error message
   ↓
7. Response sent to client
```

### Exception Types and Handling

| Exception Type | Status Code | Action | User Sees |
|---------------|-------------|--------|-----------|
| `KeyNotFoundException` | 404 | Redirect to Index | Error message in TempData |
| `InvalidOperationException` | 400 | Redirect to Index (or return view for POST) | Error message in TempData |
| `UnauthorizedAccessException` | 403 | Redirect to Home/Index | "You do not have permission..." |
| Generic `Exception` | 500 | Redirect to Index or Error page | "An unexpected error occurred..." |

### AJAX Request Handling

For AJAX requests (detected by `X-Requested-With` header or `Accept: application/json`):
- Returns JSON response instead of redirect
- Format: `{ "error": "error message" }`
- Appropriate HTTP status code set

---

## Logging System

### Logging Levels

ASP.NET Core uses the following log levels (from most to least severe):

1. **Critical**: Critical failures requiring immediate attention
2. **Error**: Errors and exceptions that are handled
3. **Warning**: Warning messages for unusual or unexpected events
4. **Information**: General informational messages
5. **Debug**: Detailed messages for debugging
6. **Trace**: Very detailed messages (most verbose)

### Logging in Our Project

#### 1. Exception Logging

**Location**: `ExceptionHandlingFilter.OnExceptionAsync`

**What is logged**:
```csharp
_logger.LogError(exception,
    "Exception in {Controller}.{Action}: {Message}",
    controllerName,
    actionName,
    exception.Message);
```

**Log Format**:
```
[Error] Exception in AuthController.Create: User with ID 123 not found.
```

**Information captured**:
- Exception type and message
- Controller name
- Action name
- Full stack trace (in exception object)
- Timestamp (automatic)
- Request ID (automatic)

#### 2. Service Layer Logging

**Example**: `AuthService`

Services can log operations:
```csharp
_logger.LogInformation("Creating user: {Email}", createUserDto.Email);
_logger.LogWarning("User creation failed: {Reason}", reason);
```

#### 3. Middleware Logging

**Location**: `GlobalExceptionHandlingMiddleware`

**What is logged**:
```csharp
_logger.LogError(ex, "An unhandled exception occurred. Request Path: {Path}", 
    context.Request.Path);
```

**When**: Only if exception reaches middleware level (rare, since filters usually catch it)

### Logging Configuration

Logging is configured in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**Log Levels**:
- `Default`: Minimum level for all loggers
- `Microsoft.AspNetCore`: Level for ASP.NET Core framework logs

**In Development**: Usually set to `Information` or `Debug`
**In Production**: Usually set to `Warning` or `Error`

---

## Complete Request Flow Example

### Example: Creating a User (POST /Auth/Create)

#### Step-by-Step Flow

```
1. HTTP Request Arrives
   Method: POST
   Path: /Auth/Create
   Body: { "Email": "user@example.com", ... }
   ↓

2. GlobalExceptionHandlingMiddleware
   - Wraps pipeline in try-catch
   - Calls next middleware
   ↓

3. HttpsRedirection Middleware
   - Checks if HTTPS
   - Continues if OK
   ↓

4. Routing Middleware
   - Matches route to AuthController.Create
   - Continues
   ↓

5. Authentication Middleware
   - Checks if user is authenticated
   - Sets User.Identity
   - Continues
   ↓

6. Authorization Middleware
   - Checks [Authorize] attribute
   - Verifies user has Administrator or Supervisor role
   - Continues if authorized
   ↓

7. ControllerActionFilter.OnActionExecuting
   - Stores controller in HttpContext.Items["__Controller"]
   - Continues
   ↓

8. Model Binding
   - Binds POST data to CreateUserDto
   - Validates model
   ↓

9. Action Execution: AuthController.Create
   - Checks ModelState.IsValid
   - Calls _authService.CreateUserAsync(createUserDto)
     ↓
     AuthService.CreateUserAsync:
       - Maps DTO to ApplicationUser (AutoMapper)
       - Calls _userManager.CreateAsync(user, password)
       - If user exists → throws InvalidOperationException
       - Assigns role
       - Returns UserDto
   ↓

10. Exception Occurs (example: user already exists)
    InvalidOperationException thrown
    ↓

11. ExceptionHandlingFilter.OnExceptionAsync
    - Logs: "Exception in AuthController.Create: User already exists"
    - Stores error in HttpContext.Items["ExceptionErrorMessage"]
    - Determines result: RedirectToAction (since POST)
    - Sets context.Result
    - Marks exception as handled
    ↓

12. ControllerActionFilter.OnActionExecuted
    - Gets error from HttpContext.Items
    - Gets controller from HttpContext.Items
    - Sets TempData["Error"] = "User already exists"
    ↓

13. Result Execution
    - Redirects to /Auth/Index
    ↓

14. New Request: GET /Auth/Index
    - Goes through middleware again
    - Executes AuthController.Index
    - View reads TempData["Error"]
    - Displays error message to user
    ↓

15. Response Sent
    - HTML with error message displayed
    ↓

16. Middleware Pipeline (reverse)
    - All middleware processes response
    ↓

17. HTTP Response
    Status: 302 Redirect (then 200 OK for Index)
    Location: /Auth/Index
```

### Example: Successful User Creation

```
1-8. Same as above
   ↓

9. Action Execution: AuthController.Create
   - ModelState.IsValid = true
   - Calls _authService.CreateUserAsync(createUserDto)
     ↓
     AuthService.CreateUserAsync:
       - Creates user successfully
       - Returns UserDto
   ↓

10. No Exception
    ↓

11. Action Returns
    - Sets TempData["Success"] = "User 'user@example.com' created successfully."
    - Returns RedirectToAction("Index")
    ↓

12. ControllerActionFilter.OnActionExecuted
    - No error in HttpContext.Items
    - Continues normally
    ↓

13. Result Execution
    - Redirects to /Auth/Index
    ↓

14. New Request: GET /Auth/Index
    - Executes AuthController.Index
    - View reads TempData["Success"]
    - Displays success message
    ↓

15. Response Sent
    - HTML with success message
```

---

## Configuration

### Filter Registration

**Location**: `Da3wa.WebUI/DependencyInjection/ServiceCollectionExtensions.cs`

```csharp
services.AddControllersWithViews(options =>
{
    options.Filters.Add<ExceptionHandlingFilter>();  // Global exception filter
    options.Filters.Add<ControllerActionFilter>();  // Helper filter for TempData
});
```

**Execution Order**:
1. `ControllerActionFilter.OnActionExecuting` (runs first)
2. Action executes
3. If exception: `ExceptionHandlingFilter.OnExceptionAsync`
4. `ControllerActionFilter.OnActionExecuted` (always runs)

### Middleware Registration

**Location**: `Da3wa.WebUI/Program.cs`

```csharp
// Global exception handling (first)
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Other middleware...
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
```

### Logging Configuration

**Location**: `appsettings.json` and `appsettings.Development.json`

**Development** (more verbose):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**Production** (less verbose):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error"
    }
  }
}
```

---

## Key Takeaways

### 1. Middleware vs Filters

| Aspect | Middleware | Filters |
|--------|-----------|---------|
| **Scope** | Entire application | MVC actions only |
| **Execution** | Every request | Only MVC requests |
| **Order** | Registration order | Filter type order |
| **Access** | HttpContext | ActionContext/ExceptionContext |
| **Use Case** | Cross-cutting concerns | MVC-specific logic |

### 2. Exception Handling Strategy

1. **First Line**: Action-level try-catch (optional, we removed these)
2. **Second Line**: `ExceptionHandlingFilter` (catches all action exceptions)
3. **Last Resort**: `GlobalExceptionHandlingMiddleware` (catches unhandled exceptions)

### 3. TempData Flow

```
Exception → HttpContext.Items → ControllerActionFilter → TempData → View
```

### 4. Logging Best Practices

- **Log at appropriate levels**: Error for exceptions, Information for important events
- **Include context**: Controller, action, user info
- **Use structured logging**: Include parameters in message template
- **Don't log sensitive data**: Passwords, tokens, etc.

### 5. Request Flow Summary

```
Request → Middleware → Routing → Auth → Authorization → 
Action Filter (Before) → Action → Exception? → Exception Filter → 
Action Filter (After) → Result → View → Response
```

---

## Troubleshooting

### Exception Not Caught

**Problem**: Exception not being handled by filter

**Check**:
1. Is filter registered in `ServiceCollectionExtensions`?
2. Is exception thrown in controller action (not in middleware)?
3. Is `context.ExceptionHandled = true` set?

### TempData Not Showing Error

**Problem**: Error message not displayed in view

**Check**:
1. Is `ControllerActionFilter` registered?
2. Is error stored in `HttpContext.Items["ExceptionErrorMessage"]`?
3. Is view reading `TempData["Error"]`?
4. Is redirect happening (TempData only works across redirects)?

### Logs Not Appearing

**Problem**: No logs in output

**Check**:
1. Log level in `appsettings.json`
2. Is logger injected correctly?
3. Check console output or configured log provider

---

## Additional Resources

- [ASP.NET Core Middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)
- [ASP.NET Core Filters](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/filters)
- [ASP.NET Core Logging](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/)
- [Exception Handling in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling)

---

**Last Updated**: 2024
**Version**: 1.0

