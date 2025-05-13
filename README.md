
---

# BonyadCode.Resulter

A set of utilities for returning standardized API results, supporting both controllers and minimal APIs in ASP.NET Core. This package provides a flexible and reusable way to handle common HTTP responses (success, failure, validation errors) and can be easily integrated into your web API projects.

---

## Features

* **Standardized Results**: Easily return consistent HTTP results (success, failure, validation errors) from your APIs.
* **Supports Controllers & Minimal APIs**: Works seamlessly with both traditional controllers (`IActionResult`) and minimal APIs (`IResult`).
* **Customizable Error Details**: Define custom metadata for validation errors and problem details using `ProblemDetailsOptions`.
* **Clean & Concise**: Simplifies API responses, reduces boilerplate code, and improves maintainability.

---

## Installation

You can install **BonyadCode.Resulter** via NuGet Package Manager or the .NET CLI:

### Using .NET CLI

```bash
dotnet add package BonyadCode.Resulter
```

### Using NuGet Package Manager

Search for **BonyadCode.Resulter** in the NuGet Package Manager within your IDE, or use the following command:

```bash
Install-Package BonyadCode.Resulter
```

---

## Usage

### 1. **ResultBuilder Class**

The core of this package is the `ResultBuilder<T>`. Use this class to wrap your operation results with data, status code, and errors.

#### Example: Returning a Success Result

```csharp
public class ExampleController : ControllerBase
{
    public IActionResult GetExample()
    {
        var result = ResultBuilder<string>.Success("Success message");
        return result.ToHttpResultController();
    }
}
```

#### Example: Returning a Failure Result with Errors

```csharp
public class ExampleController : ControllerBase
{
    public IActionResult GetExample()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "username", new[] { "Username is required." } },
            { "password", new[] { "Password is too short." } }
        };

        var result = ResultBuilder<string>.Failure(errors);
        return result.ToHttpResultController();
    }
}
```

#### Example: Returning a Success Result in Minimal APIs

```csharp
var app = WebApplication.CreateBuilder(args).Build();

app.MapGet("/example", () =>
{
    var result = ResultBuilder<string>.Success("Success message");
    return result.ToHttpResultMinimal();
});

app.Run();
```

#### Example: Returning a Failure Result in Minimal APIs

```csharp
var app = WebApplication.CreateBuilder(args).Build();

app.MapGet("/example", () =>
{
    var errors = new Dictionary<string, string[]>
    {
        { "username", new[] { "Username is required." } },
        { "password", new[] { "Password is too short." } }
    };

    var result = ResultBuilder<string>.Failure(errors);
    return result.ToHttpResultMinimal();
});

app.Run();
```

---

## Problem Details Customization

You can customize the `ProblemDetails` metadata (such as the `Title`, `Type`, and `Instance`) through the `ProblemDetailsOptions` class.

#### Example: Customizing Problem Details

```csharp
var options = new ProblemDetailsOptions
{
    Title = "Custom Validation Error",
    Type = "https://myapi.com/problems/validation",
    Instance = "/api/endpoint"
};

var result = ResultBuilder<string>.Failure(errors);
return result.ToHttpResultController(options);
```

---

## Configuration

The package is designed to be minimal, but you can customize it through the `ProblemDetailsOptions` class, allowing you to define:

* **Title**: Custom title for the validation error details.
* **Type**: URL to identify the error type.
* **Instance**: Optional, typically used for identifying the specific instance (e.g., the URI) of the error.

---

## Example with Custom Options

```csharp
var options = new ProblemDetailsOptions
{
    Title = "Invalid Operation",
    Type = "https://example.com/probs/invalid-operation",
    Instance = "/api/users"
};

var errors = new Dictionary<string, string[]>
{
    { "email", new[] { "Email is invalid." } }
};

var result = ResultBuilder<string>.Failure(errors);
return result.ToHttpResultController(options);
```

---

## API Result Flow

1. **Success**: When an operation is successful, you can return data along with the HTTP status code (default 200 OK).
2. **Failure**: When an operation fails, you can include error details in a standardized format using `ProblemDetails`.
3. **Validation Errors**: The package integrates with validation errors, providing detailed `ValidationProblemDetails`.

---

## Contributing

Contributions are welcome! If you find a bug or have an idea for a new feature, feel free to open an issue or submit a pull request.

---

## License

MIT License. See the [LICENSE](LICENSE) file for details.

---

## Links

* [GitHub Repository](https://github.com/bonyadcode/Resulter)
* [NuGet Package](https://www.nuget.org/packages/BonyadCode.Resulter)

---
