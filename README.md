# BonyadCode.Resulter

A robust utility library to standardize API responses in ASP.NET Core. Supports both Controllers and Minimal APIs with unified handling of success, failure, validation errors, and exceptions — using rich `ProblemDetails` in accordance with `RFC7807` and `RFC9457` standards.

---

## ✨ Features

- **Unified Result Model**: Consistent `ResultBuilder<T>` and `ResultBuilder` for all HTTP outcomes.
- **Support for Minimal APIs & Controllers**: Seamlessly convert to `IActionResult` or `IResult`.
- **ProblemDetails Integration**: Built-in helpers to enrich error responses with metadata.
- **Zero Boilerplate**: Clean, expressive syntax that reduces repetitive response code.

---

## 📦 Installation

```bash
dotnet add package BonyadCode.Resulter
```

---

## 🚀 Quick Examples

### ✅ Success (Controller)
```csharp
[HttpGet("hello")]
public IActionResult GetHello()
{
    var result = ResultBuilder<string>.Success("Hello, world!");
    return result.ToHttpResultController();
}
```

**Response JSON:**
```json
{
  "succeeded": true,
  "statusCode": 200,
  "data": "Hello, world!",
  "problemDetails": null
}
```

### ✅ Success (Minimal Api)
```csharp
[HttpGet("hello")]
public IActionResult GetHello()
{
    var result = ResultBuilder<string>.Success("Hello, world!");
    return result.ToHttpResultController();
}
```

**Response JSON:**
```json
{
  "succeeded": true,
  "statusCode": 200,
  "data": "Hello, world!",
  "problemDetails": null
}
```

### ✅ Custom Success (Controller)
```csharp
[HttpGet("hello")]
public IActionResult GetHello()
{
    var result = ResultBuilder.Success("User was Created", HttpStatusCode.Created);
    return result.ToHttpResultController();
}
```

**Response JSON:**
```json
{
  "succeeded": true,
  "statusCode": 201,
  "data": "User was Created",
  "problemDetails": null
}
```

### ✅ Custom Success (Minimal Api)
```csharp
app.MapGet("/hello", () =>
{
    var result = ResultBuilder.Success("User was Created", HttpStatusCode.Created);
    return result.ToHttpResultMinimal();
}
```

**Response JSON:**
```json
{
  "succeeded": true,
  "statusCode": 201,
  "data": "User was Created",
  "problemDetails": null
}
```

---

### ❌ Validation Failure (Controller)
```csharp
[HttpPost("register")]
public IActionResult RegisterUser(UserDto dto)
{
    var result = ResultBuilder<string>.Failure()
        .AddErrorsFromKeyValuePairs("Email", "Email is required.")
        .AddErrorsFromKeyValuePairs("Password", new List<string> { "Password must be at least 8 characters." });

    return result.ToHttpResultController();
}
```

**Response JSON:**
```json
{
  "succeeded": false,
  "statusCode": 400,
  "data": null,
  "problemDetails": {
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "A problem occurred.",
    "detail": "A problem occurred.",
    "status": 400,
    "extensions": {
      "Email": ["Email is required."],
      "Password": ["Password must be at least 8 characters."]
    }
  }
}
```

---

## 🌐 Minimal API Usage

### ✅ Success
```csharp
app.MapGet("/status", () =>
{
    var result = ResultBuilder<string>.Success("All systems operational.", HttpStatusCode.NoContent);
    return result.ToHttpResultMinimal();
});
```
**Response JSON:**
```json
{
  "succeeded": true,
  "statusCode": 204,
  "data": "All systems operational.",
  "problemDetails": null
}
```

### ❌ FluentValidation Errors
```csharp
app.MapPost("/login", (LoginRequest request, IValidator<LoginRequest> validator) =>
{
    var validationResult = validator.Validate(request);

    if(!validationResult.Succeeded)
    {
        var result = ResultBuilder<string>.Failure()
            .AddErrorsFromFluentValidationResult(validationResult);
    }
    
    return result.ToHttpResultMinimal();
});
```

---

## ⚙️ Custom ProblemDetails Examples

### 🛠️ Attach Custom Metadata
```csharp
var result = ResultBuilder<string>.Failure()
    .WithCustomProblemDetails(
        type: "https://example.com/problems/validation",
        title: "Validation Error",
        details: "One or more validation failures occured.",
        statusCode: HttpStatusCode.Conflict,
        instance: "/api/register",
        errors: new Dictionary<string, object?>
        {
            { "Username", new[] { "Username already exists." } }
        });
```
Note: You can pass the httpContext to the above method so that "instance" is extracted automatically from it.

**Response JSON:**
```json
{
  "succeeded": false,
  "statusCode": 400,
  "data": null,
  "problemDetails": {
    "type": "https://example.com/problems/validation",
    "title": "Validation Error",
    "detail": "One or more validation failures occured.",
    "status": 409,
    "instance": "/api/register",
    "extensions": {
      "Username": ["Username already exists."]
    }
  }
}
```

---

## 🧠 Validation Sources

### 🔧 DataAnnotations
```csharp
var validationResult = new ValidationResult("Email", new[] { "Invalid email" });
var result = ResultBuilder.Failure()
    .AddErrorsFromValidationResult(validationResult);
```

### 🧪 FluentValidation
```csharp
var result = ResultBuilder.Failure()
    .AddErrorsFromFluentValidationResult(validationResult);
```

### 🔐 IdentityResult
```csharp
var result = ResultBuilder.Failure()
    .AddErrorsFromIdentityError(identityResult);
```

---

## 🔥 Exception Handling
```csharp
try
{
    throw new InvalidOperationException("Something broke.");
}
catch (Exception ex)
{
    var result = ResultBuilder.Failure()
        .WithExceptionProblemDetails(ex, httpContext);

    return result.ToHttpResultController();
}
```

**Response JSON (truncated):**
```json
{
  "succeeded": false,
  "statusCode": 500,
  "data": null,
  "problemDetails": {
    "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
    "title": "An exception was thrown.",
    "detail": "Something broke.",
    "status": 500,
    "instance": "/api/failing-endpoint",
    "extensions": {
      "Message": ["Something broke."],
      "Source": ["MyApp"]
    }
  }
}
```

---

## 📚 Extension Reference

| Method                                             | Description                                      |
|----------------------------------------------------|--------------------------------------------------|
| `WithSimpleProblemDetails()`                       | Attaches default problem info                   |
| `WithCustomProblemDetails(...)`                    | Fully customized metadata                       |
| `WithExceptionProblemDetails(...)`                 | Wraps exception details                         |
| `AddErrorsFromKeyValuePairs(...)`         | Adds custom errors by key/value(s)              |
| `AddErrorsFromFluentValidationResult(...)`| Maps FluentValidation errors                    |
| `AddErrorsFromValidationResult(...)`      | Maps DataAnnotation errors                      |
| `AddErrorsFromIdentityError(...)`         | Maps IdentityResult errors                      |
| `AddProblemDetailsErrorExtensionsFromException(...)`| Adds public exception properties                |

---

## 🧪 API Behavior Matrix

| Scenario         | Method                             | Status Code           | Description                            |
|------------------|------------------------------------|------------------------|----------------------------------------|
| Success          | `ResultBuilder.Success()`          | 200 OK                 | Standard success with optional data    |
| Failure          | `ResultBuilder.Failure()`          | 400 Bad Request        | Generic or validation failure          |
| Exception        | `.WithExceptionProblemDetails()`   | 500 Internal Server Error | Captures exception stack trace      |
| Custom Error     | `.WithCustomProblemDetails(...)`   | Any                    | User-defined structured error response |

---

## 🤝 Contributing
PRs and feedback welcome! [GitHub →](https://github.com/bonyadcode/Resulter)

## 📄 License
Apache 2.0 — see the [LICENSE](LICENSE) file.

## 📦 Links
- [NuGet](https://www.nuget.org/packages/BonyadCode.Resulter)
- [GitHub](https://github.com/bonyadcode/Resulter)