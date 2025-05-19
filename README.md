
# BonyadCode.Resulter

A flexible utility library for returning standardized API results in ASP.NET Core. It supports both Controllers and Minimal APIs, simplifying your HTTP response logic while providing clear and consistent response structures, including success, failure, and detailed error feedback.

---

## ✨ Features

- **Standardized Responses**: Unified handling of success, failure, exceptions, and validation errors.
- **Minimal APIs & Controllers Support**: Convert results to `IResult` or `IActionResult` effortlessly.
- **Integrated Problem Details**: Leverages `ProblemDetails` for structured error reporting.
- **Exception Handling Helpers**: Attach exception info cleanly to your response pipeline.
- **Zero-Boilerplate Syntax**: Focus on your business logic, not HTTP result plumbing.

---

## 📦 Installation

Install via the .NET CLI:

```
dotnet add package BonyadCode.Resulter
```

Or via the NuGet Package Manager:

```
Install-Package BonyadCode.Resulter
```

---

## 🚀 Quick Start

### ✅ Success Result — Controller Example

```csharp
[HttpGet("hello")]
public IActionResult GetHello()
{
    var result = ResultBuilder<string>.Success("Hello, world!");
    return result.ToHttpResultController();
}
```

### ❌ Failure Result with Validation Errors — Controller Example

```csharp
[HttpPost("register")]
public IActionResult RegisterUser(UserDto dto)
{
    var errors = new Dictionary<string, string[]>
    {
        { "Email", new[] { "Email is required." } },
        { "Password", new[] { "Password must be at least 8 characters." } }
    };

    var result = ResultBuilder<string>.Failure(errors);
    return result.ToHttpResultController();
}
```

---

## 🌐 Minimal API Usage

### ✅ Success Result

```csharp
app.MapGet("/status", () =>
{
    var result = ResultBuilder<string>.Success("Service is up");
    return result.ToHttpResultMinimal();
});
```

### ❌ Failure Result

```csharp
app.MapPost("/login", (LoginRequest request) =>
{
    var errors = new Dictionary<string, string[]>
    {
        { "Username", new[] { "Username is required." } },
        { "Password", new[] { "Password cannot be empty." } }
    };

    var result = ResultBuilder<string>.Failure(errors);
    return result.ToHttpResultMinimal();
});
```

---

## ⚙️ ProblemDetails Customization

You can attach detailed error metadata using the built-in helpers.

### 🛠️ Attach Custom ProblemDetails

```csharp
var result = ResultBuilder<string>.Failure("Invalid input");
result = result.WithDetailedProblemDetails(
    type: "https://yourdomain.com/problems/validation",
    title: "Validation Failed",
    details: "One or more fields are invalid.",
    statusCode: HttpStatusCode.UnprocessableEntity,
    instance: "/api/register",
    errors: new Dictionary<string, string[]>
    {
        { "username", new[] { "Username already exists." } }
    });

return result.ToHttpResultController();
```

---

## 🔥 Exception Handling Example

Gracefully wrap unhandled exceptions:

```csharp
try
{
    // Some logic that throws
}
catch (Exception ex)
{
    var result = ResultBuilder.Failure()
        .WithExceptionProblemDetails(ex, httpContextAccessor);

    return result.ToHttpResultController();
}
```

---

## 🧪 Tips for Best Use

- Use `.Success(data)` and `.Failure(errors)` for simple flows.
- Customize `.WithDetailedProblemDetails(...)` for fine-grained error metadata.
- Use `.WithExceptionProblemDetails(...)` to standardize error reporting for thrown exceptions.
- Use `.ToHttpResultController()` and `.ToHttpResultMinimal()` based on your API style.

---

## 📚 API Response Pattern

| Scenario         | Method                        | Status Code | Description                          |
|------------------|-------------------------------|-------------|--------------------------------------|
| Success          | `ResultBuilder.Success(...)`  | 200 OK      | Standard successful result           |
| Failure          | `ResultBuilder.Failure(...)`  | 400 Bad Request | Generic or validation failure    |
| Exception        | `WithExceptionProblemDetails` | 500 Internal Server Error | Server-side error |
| Custom Error     | `WithDetailedProblemDetails`  | Any         | Fully customized error response      |

---

## 🤝 Contributing

Contributions are welcome! Please open issues or submit pull requests on [GitHub](https://github.com/bonyadcode/Resulter).

---

## 📄 License

APACHE License. See the [LICENSE](LICENSE) file for more details.

---

## 🔗 Links

- 📦 [NuGet Package](https://www.nuget.org/packages/BonyadCode.Resulter)
- 💻 [GitHub Repository](https://github.com/bonyadcode/Resulter)

---
