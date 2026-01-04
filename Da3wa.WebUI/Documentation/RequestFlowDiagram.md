# Request Flow Visual Diagrams

## Complete Request Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                    HTTP REQUEST ARRIVES                          │
│              POST /Auth/Create { Email: "..." }                  │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│        1. GlobalExceptionHandlingMiddleware                      │
│           ┌─────────────────────────────────────┐               │
│           │ try {                                │               │
│           │   await _next(context);             │               │
│           │ } catch {                            │               │
│           │   Log & Handle exception             │               │
│           │ }                                    │               │
│           └─────────────────────────────────────┘               │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│        2. HttpsRedirection Middleware                            │
│           Check if HTTPS, redirect if needed                     │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│        3. Static Files Middleware                               │
│           Serve static files if request matches                  │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│        4. Routing Middleware                                    │
│           Match: /Auth/Create → AuthController.Create            │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│        5. Authentication Middleware                            │
│           Check: Is user logged in?                             │
│           Set: User.Identity                                    │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│        6. Authorization Middleware                              │
│           Check: [Authorize(Roles = "Administrator")]           │
│           Result: Allow or Deny                                 │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                    MVC PIPELINE STARTS                          │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│    ControllerActionFilter.OnActionExecuting                     │
│    ┌─────────────────────────────────────────┐                   │
│    │ Store controller in                    │                   │
│    │ HttpContext.Items["__Controller"]       │                   │
│    └─────────────────────────────────────────┘                   │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                    MODEL BINDING                                │
│    POST data → CreateUserDto                                    │
│    Validation: ModelState.IsValid                               │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              ACTION EXECUTION                                    │
│    AuthController.Create(CreateUserDto dto)                      │
│    ┌─────────────────────────────────────────┐                 │
│    │ if (!ModelState.IsValid)                │                 │
│    │   return View(dto);                     │                 │
│    │                                         │                 │
│    │ var user = await                        │                 │
│    │   _authService.CreateUserAsync(dto);    │                 │
│    │                                         │                 │
│    │ // Exception might occur here           │                 │
│    │ // InvalidOperationException            │                 │
│    │ // KeyNotFoundException                 │                 │
│    └─────────────────────────────────────────┘                 │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                    ┌───────┴───────┐
                    │               │
            ┌───────▼───────┐ ┌─────▼──────┐
            │   SUCCESS     │ │ EXCEPTION  │
            └───────┬───────┘ └─────┬──────┘
                    │               │
                    │       ┌───────▼──────────────────────────┐
                    │       │ ExceptionHandlingFilter          │
                    │       │ OnExceptionAsync                  │
                    │       │ ┌──────────────────────────────┐│
                    │       │ │ 1. Log exception              ││
                    │       │ │ 2. Store error in              ││
                    │       │ │    HttpContext.Items           ││
                    │       │ │ 3. Determine result            ││
                    │       │ │ 4. Set context.Result          ││
                    │       │ │ 5. Mark as handled             ││
                    │       │ └──────────────────────────────┘│
                    │       └───────┬──────────────────────────┘
                    │               │
                    └───────┬───────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│    ControllerActionFilter.OnActionExecuted                      │
│    ┌─────────────────────────────────────────┐                  │
│    │ if (error in HttpContext.Items) {       │                  │
│    │   Get controller from Items             │                  │
│    │   Set TempData["Error"] = message      │                  │
│    │ }                                       │                  │
│    └─────────────────────────────────────────┘                  │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                    RESULT EXECUTION                             │
│    RedirectToAction("Index") or ViewResult                      │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              RESPONSE PIPELINE (Reverse)                        │
│    All middleware processes response in reverse order            │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                    HTTP RESPONSE                                │
│    Status: 302 Redirect → /Auth/Index                           │
│    Then: 200 OK with HTML (showing error message)               │
└─────────────────────────────────────────────────────────────────┘
```

## Exception Handling Flow

```
┌─────────────────────────────────────────────────────────────┐
│              EXCEPTION OCCURS IN ACTION                      │
│    InvalidOperationException: "User already exists"          │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│    ExceptionHandlingFilter.OnExceptionAsync                  │
│    ┌───────────────────────────────────────────┐             │
│    │ 1. Log Error:                             │             │
│    │    "Exception in AuthController.Create:   │             │
│    │     User already exists"                  │             │
│    │                                            │             │
│    │ 2. Store in HttpContext.Items:            │             │
│    │    ["ExceptionErrorMessage"] =             │             │
│    │    "User already exists"                  │             │
│    │                                            │             │
│    │ 3. Determine Result:                      │             │
│    │    - Check exception type                 │             │
│    │    - Check if AJAX request                │             │
│    │    - Create appropriate result            │             │
│    │                                            │             │
│    │ 4. Set context.Result =                   │             │
│    │    RedirectToAction("Index")              │             │
│    │                                            │             │
│    │ 5. context.ExceptionHandled = true        │             │
│    └───────────────────────────────────────────┘             │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│    ControllerActionFilter.OnActionExecuted                   │
│    ┌───────────────────────────────────────────┐             │
│    │ 1. Check HttpContext.Items:               │             │
│    │    ["ExceptionErrorMessage"] exists?       │             │
│    │                                            │             │
│    │ 2. Get controller from Items:              │             │
│    │    ["__Controller"]                        │             │
│    │                                            │             │
│    │ 3. Transfer to TempData:                  │             │
│    │    controller.TempData["Error"] =          │             │
│    │      HttpContext.Items["Exception..."]     │             │
│    └───────────────────────────────────────────┘             │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│              REDIRECT TO /Auth/Index                         │
│    (New request starts, goes through middleware again)       │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│    AuthController.Index executes                              │
│    View renders and reads TempData["Error"]                  │
│    Displays: "User already exists"                           │
└─────────────────────────────────────────────────────────────┘
```

## Filter Execution Order

```
┌─────────────────────────────────────────────────────────────┐
│                    REQUEST ARRIVES                           │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│    1. Authorization Filters                                  │
│       [Authorize] attributes                                 │
│       - Can short-circuit (return 401/403)                  │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│    2. Resource Filters                                       │
│       - Run around model binding                             │
│       - Can cache responses                                  │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│    3. Action Filters (Before)                                │
│       ControllerActionFilter.OnActionExecuting               │
│       - Store controller in HttpContext.Items                │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│    4. ACTION EXECUTION                                       │
│       Controller action method runs                          │
│       - Model binding                                        │
│       - Validation                                           │
│       - Business logic                                       │
│       - Service calls                                        │
└───────────────────────────┬─────────────────────────────────┘
                            │
                    ┌───────┴───────┐
                    │               │
            ┌───────▼───────┐ ┌─────▼──────┐
            │   SUCCESS    │ │ EXCEPTION  │
            └───────┬───────┘ └─────┬──────┘
                    │               │
                    │       ┌───────▼──────────────────────┐
                    │       │ 5. Exception Filters          │
                    │       │ ExceptionHandlingFilter       │
                    │       │ - Handle exception            │
                    │       │ - Set result                  │
                    │       └───────┬──────────────────────┘
                    │               │
                    └───────┬───────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│    6. Action Filters (After)                                │
│       ControllerActionFilter.OnActionExecuted                │
│       - Transfer error to TempData                          │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│    7. Result Filters                                        │
│       - Can modify result                                    │
│       - Can add headers                                      │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    RESPONSE SENT                             │
└─────────────────────────────────────────────────────────────┘
```

## Logging Flow

```
┌─────────────────────────────────────────────────────────────┐
│              ACTION EXECUTION                               │
│    AuthController.Create()                                  │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│    Service Call: AuthService.CreateUserAsync()              │
│    ┌──────────────────────────────────────────┐              │
│    │ _logger.LogInformation(                 │              │
│    │   "Creating user: {Email}",             │              │
│    │   createUserDto.Email);                 │              │
│    └──────────────────────────────────────────┘              │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│              EXCEPTION OCCURS                                │
│    InvalidOperationException: "User already exists"          │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│    ExceptionHandlingFilter.OnExceptionAsync                  │
│    ┌──────────────────────────────────────────┐              │
│    │ _logger.LogError(                       │              │
│    │   exception,                             │              │
│    │   "Exception in {Controller}.{Action}:   │              │
│    │    {Message}",                           │              │
│    │   "AuthController",                      │              │
│    │   "Create",                               │              │
│    │   exception.Message);                     │              │
│    └──────────────────────────────────────────┘              │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│              LOG OUTPUT                                      │
│    [2024-01-04 12:00:00] [Error]                            │
│    Exception in AuthController.Create:                       │
│    User already exists                                        │
│    StackTrace: ...                                           │
└─────────────────────────────────────────────────────────────┘
```

## TempData Flow

```
┌─────────────────────────────────────────────────────────────┐
│    ExceptionHandlingFilter                                   │
│    ┌──────────────────────────────────────────┐              │
│    │ context.HttpContext.Items                │              │
│    │   ["ExceptionErrorMessage"] =            │              │
│    │   "User already exists"                  │              │
│    └──────────────────────────────────────────┘              │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│    ControllerActionFilter.OnActionExecuted                   │
│    ┌──────────────────────────────────────────┐              │
│    │ var error = HttpContext.Items            │              │
│    │   ["ExceptionErrorMessage"];             │              │
│    │                                          │              │
│    │ var controller = HttpContext.Items       │              │
│    │   ["__Controller"];                      │              │
│    │                                          │              │
│    │ controller.TempData["Error"] = error;    │              │
│    └──────────────────────────────────────────┘              │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│              REDIRECT                                         │
│    RedirectToAction("Index")                                  │
│    TempData survives redirect                                 │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│    AuthController.Index                                       │
│    View renders                                              │
│    ┌──────────────────────────────────────────┐              │
│    │ @if (TempData["Error"] != null) {        │              │
│    │   <div class="alert alert-danger">        │              │
│    │     @TempData["Error"]                    │              │
│    │   </div>                                  │              │
│    │ }                                         │              │
│    └──────────────────────────────────────────┘              │
└─────────────────────────────────────────────────────────────┘
```

---

**Note**: These diagrams show the flow for a typical MVC request. The actual flow may vary slightly based on the specific request type (API vs MVC, GET vs POST, etc.).

