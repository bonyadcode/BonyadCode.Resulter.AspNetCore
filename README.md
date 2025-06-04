In the name of God

# BonyadCode.Resulter.AspNetCore

A utility library to standardize API responses in ASP.NET Core. It supports both Controllers and Minimal APIs with consistent handling of success, failure, validation errors, and exceptions, using rich ProblemDetails following RFC7807.
---

## ✨ Features

* **Unified Result Model**: Consistent `ResultBuilder<T>` and `ResultBuilder` for all HTTP outcomes.
* **Support for Minimal APIs & Controllers**: Seamlessly convert to `IActionResult` or `IResult`.
* **ProblemDetails Integration**: Built-in helpers to enrich error responses with metadata.
* **Zero Boilerplate**: Clean, expressive syntax that reduces repetitive response code.
* **Validation**: and Error Support: Maps errors from FluentValidation, DataAnnotations, and IdentityResult into
  structured responses.

---

## 📦 Installation

```bash
dotnet add package BonyadCode.Resulter.AspNetCore
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
app.MapGet("/hello", () =>
{
    var result = ResultBuilder<string>.Success("Hello, world!");
    return result.ToHttpResultMinimal();
});
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
});
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
        .AddProblemDetailsFromKeyValuePairs("Email", "Email is required.")
        .AddProblemDetailsFromKeyValuePairs("Password", "Password must be at least 8 characters." );

    return result.ToHttpResultController();
}
```

or

```csharp
[HttpPost("register")]
public IActionResult RegisterUser(UserDto dto)
{
    var result = ResultBuilder<string>.Failure()
        .AddProblemDetailsFromKeyValuePairs(["Email","Password"], ["Email is required.","Password must be at least 8 characters." ]);

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
    "detail": "A problem occurred (400 BadRequest).",
    "status": 400,
    "extensions": {
      "Email": [
        "Email is required."
      ],
      "Password": [
        "Password must be at least 8 characters."
      ]
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

or

```csharp
app.MapGet("/status", () =>
{
    var result = ResultBuilder<string>.Success("All systems operational.");
    return result.ToHttpResultMinimal(HttpStatusCode.NoContent);
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
app.MapPost("/login", (LoginRequest request, HttpContext? httpContext, IValidator<LoginRequest> validator) =>
{
    var validationResult = validator.Validate(request);

    if(!validationResult.Succeeded)
    {
        var result = ResultBuilder<string>.Failure()
            .AddProblemDetailsFromFluentValidationResult(validationResult);

        return result.ToHttpResultMinimal(HttpStatusCode.Conflict, httpContext);
    }

    return ResultBuilder<string>.Success("Logged in").ToHttpResultMinimal();
});
```

**Response JSON:**

```json
{
  "succeeded": false,
  "statusCode": 409,
  "data": null,
  "problemDetails": {
    "type": "https://example.com/problems/validation",
    "title": "A validation problem occurred.",
    "detail": "One or more validation failures occured.",
    "status": 409,
    "instance": "/api/register",
    "extensions": {
      "Username": [
        "Username already exists."
      ]
    }
  }
}
```

---

## ⚙️ Custom ProblemDetails Examples

### 🛠️ Attach Custom Metadata

```csharp
var result = ResultBuilder<string>.Failure()
    .AddCustomProblemDetails(
        type: "https://example.com/problems/validation",
        title: "A custom problem occurred.",
        details: "One or more custom failures occured.",
        statusCode: HttpStatusCode.Conflict,
        instance: "/api/register",
        extensions: new Dictionary<string, object?>
        {
            { "Username", new[] { "Username already exists." } }
        }).ToHttpResultMinimal(HttpStatusCode.Conflict, httpContext);
```

Note: You can pass the httpContext to the above method so that "instance" is extracted automatically from it.

**Response JSON:**

```json
{
  "succeeded": false,
  "statusCode": 409,
  "data": null,
  "problemDetails": {
    "type": "https://example.com/problems/validation",
    "title": "A custom problem occurred.",
    "detail": "One or more custom failures occured.",
    "status": 409,
    "instance": "/api/register",
    "extensions": {
      "Username": [
        "Username already exists."
      ]
    }
  }
}
```

---

## 🧐 Validation Sources

### 🔧 DataAnnotations

```csharp
var validationResult = new ValidationResult("Email", new[] { "Invalid email" });
var result = ResultBuilder.Failure()
    .AddProblemDetailsFromValidationResult(validationResult);
```

### 🧪 FluentValidation

```csharp
var result = ResultBuilder.Failure()
    .AddProblemDetailsFromFluentValidationResult(validationResult);
```

### 🔐 IdentityResult

```csharp
var result = ResultBuilder.Failure()
    .AddProblemDetailsFromIdentityError(identityResult);
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
        .AddProblemDetailsFromException(ex, httpContext);

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
    "title": "A Server problem occurred.",
    "detail": {
      "message": "Something broke."
    },
    "status": 500,
    "instance": "/api/failing-endpoint",
    "extensions": {
      "Message": [
        "Something broke."
      ],
      "Source": [
        "MyApp"
      ]
    }
  }
}
```

---

## 📚 Extension Reference

Endpoint Result Methods:

| Method                        | Description                                                                     |
|-------------------------------|---------------------------------------------------------------------------------|
| `ToHttpResultController()`    | returns an `IActionResult` result for use in Controllers                        |
| `ToHttpResultController(...)` | returns an `IActionResult` result for use in Controllers (accepting parameters) |
| `ToHttpResultMinimal()`       | returns an `IResult` result for use in Minimal                                  |
| `ToHttpResultMinimal(...)`    | returns an `IResult` result for use in Minimal APIs (accepting parameters)      |

ProblemDetails Helper Methods:

| Method                                             | Description                        |
|----------------------------------------------------|------------------------------------|
| `AddSimpleProblemDetails()`                        | Attaches default problem info      |
| `AddCustomProblemDetails(...)`                     | Fully customized metadata          |
| `AddProblemDetailsFromException(...)`              | Adds public exception properties   |
| `AddProblemDetailsFromIdentityError(...)`          | Maps IdentityResult errors         |
| `AddProblemDetailsFromFluentValidationResult(...)` | Maps FluentValidation errors       |
| `AddProblemDetailsFromValidationResult(...)`       | Maps DataAnnotation errors         |
| `AddProblemDetailsFromKeyValuePairs(...)`          | Adds custom errors by key/value(s) |

---

## 🧪 API Behavior Matrix

| Scenario | Method                    | Status Code                    | Description                                                       |
|----------|---------------------------|--------------------------------|-------------------------------------------------------------------|
| Success  | `ResultBuilder.Success()` | Any (Default: 200 OK)          | Standard success with optional data                               |
| Failure  | `ResultBuilder.Failure()` | Any (Default: 400 Bad Request) | Standard failure with optional data (exception, validation, etc.) |
| Create   | `ResultBuilder.Create()`  | Any                            | User-defined structured error response                            |

---

## 🤝 Contributing

PRs and feedback welcome! [GitHub →](https://github.com/bonyadcode/BonyadCode.Resulter.AspNetCore)

## 📄 License

Apache 2.0 — see the [LICENSE](LICENSE) file.

## 📦 Links

* [NuGet](https://www.nuget.org/packages/BonyadCode.Resulter.AspNetCore)
* [GitHub](https://github.com/bonyadcode/BonyadCode.Resulter.AspNetCore)
