# ADR-0003: Service Layer Design

## Status

Accepted

Date: 2024-12-06

## Context

The Locale library needs to provide high-level operations beyond simple format parsing:
- **Scanning**: Compare base and target cultures to find gaps
- **Diffing**: Compare two files and report differences
- **Checking**: Validate files against configurable rules
- **Converting**: Transform files between formats
- **Generating**: Create skeleton files for new languages
- **Translating**: Auto-translate using external APIs
- **Watching**: Monitor files for changes and re-run operations

**Requirements:**
- Clean separation between core logic and format handling
- Reusable business logic across CLI and library usage
- Testable without UI concerns
- Support for both synchronous and asynchronous operations
- Dependency injection friendly
- Minimal coupling between services

**Related:** [ADR-0001: Multi-Format Architecture](./0001-multi-format-architecture.md)

## Decision

Implement a **Service Layer** pattern with dedicated service classes for each major operation:

**Service Classes:**
1. `ScanService` - Scan and compare localization files
2. `DiffService` - Compare two files
3. `CheckService` - Validate against rules
4. `ConvertService` - Convert between formats
5. `GenerateService` - Generate skeleton files
6. `TranslateService` - Auto-translate content
7. `WatchService` - Monitor file changes

**Design Principles:**
```csharp
public sealed class ScanService
{
    private readonly FormatRegistry _registry;
    
    // Default constructor uses default registry
    public ScanService() : this(FormatRegistry.Default) { }
    
    // Constructor injection for testability
    public ScanService(FormatRegistry registry) => _registry = registry;
    
    // Main operation with options object
    public ScanReport Scan(string path, ScanOptions options)
    {
        // Business logic here
    }
}

// Options objects for configuration
public sealed class ScanOptions
{
    public required string BaseCulture { get; set; }
    public List<string> TargetCultures { get; set; } = [];
    public bool Recursive { get; set; } = true;
    // ... more options
}

// Report objects for results
public sealed class ScanReport
{
    public required string BaseCulture { get; init; }
    public List<CultureComparisonResult> Results { get; init; } = [];
}
```

**Key Design Principles:**
1. **Single Responsibility**: Each service handles one operation
2. **Options Pattern**: Configuration via options objects
3. **Constructor Injection**: Dependencies injected via constructor
4. **Default Constructors**: Convenience constructors using defaults
5. **Report Objects**: Structured result types
6. **Synchronous First**: Sync operations by default, async where needed (translation)
7. **Stateless**: Services are stateless and thread-safe for read operations

## Consequences

### Positive

- **Separation of Concerns**: Business logic separate from UI (CLI) and formats
- **Testability**: Easy to unit test services with mock dependencies
- **Reusability**: Same logic used by CLI and library consumers
- **Flexibility**: Options objects allow easy extension of parameters
- **Discoverability**: Clear API surface for each operation
- **Type Safety**: Strongly-typed options and results
- **DI Friendly**: Works well with dependency injection containers
- **Documentation**: Easy to document each service's purpose

### Negative

- **More Classes**: More types to maintain (services, options, reports)
- **Indirection**: Extra layer between CLI and core logic
- **Learning Curve**: Users need to understand service/options pattern
- **Boilerplate**: Constructor injection creates some repetition

### Neutral

- **Synchronous by Default**: Most operations are sync (except translation)
- **Stateless Services**: No internal state management needed
- **Options Objects**: Trade verbosity for extensibility

## Alternatives Considered

### Alternative 1: Static Utility Classes

**Description:** Static methods for all operations

```csharp
public static class LocaleUtilities
{
    public static ScanReport Scan(string path, ...);
    public static DiffReport Diff(string file1, string file2, ...);
    public static void Convert(string input, string output, ...);
}
```

**Pros:**
- Simple API
- No instances needed
- Easy to call

**Cons:**
- Not testable (can't mock)
- Global state issues
- Hard to inject dependencies
- Doesn't work with DI
- Difficult to extend

**Why not chosen:** Poor testability and extensibility

### Alternative 2: Fluent API Builder

**Description:** Fluent builder pattern for operations

```csharp
var report = Locale.Scan()
    .InDirectory("./locales")
    .WithBaseCulture("en")
    .WithTargets("tr", "de")
    .Recursive()
    .Execute();
```

**Pros:**
- Very readable
- Discoverable API
- Method chaining
- Popular pattern

**Cons:**
- More complex implementation
- Mutable builder state
- Thread safety concerns
- Harder to serialize options
- Overkill for simple operations

**Why not chosen:** Unnecessary complexity for most use cases

### Alternative 3: Single Facade Service

**Description:** One service class with all operations

```csharp
public sealed class LocaleService
{
    public ScanReport Scan(...);
    public DiffReport Diff(...);
    public void Check(...);
    public void Convert(...);
    public void Generate(...);
    public Task TranslateAsync(...);
}
```

**Pros:**
- Single entry point
- Simple discovery
- Fewer classes

**Cons:**
- Violates Single Responsibility Principle
- Large class with many methods
- Harder to test
- All dependencies needed even if only using one operation
- Difficult to extend

**Why not chosen:** Too much responsibility in one class

### Alternative 4: Command Pattern

**Description:** Each operation as a command object

```csharp
public interface ICommand<TResult>
{
    TResult Execute();
}

public class ScanCommand : ICommand<ScanReport>
{
    public ScanCommand(string path, ScanOptions options) { }
    public ScanReport Execute() => ...;
}

var command = new ScanCommand("./locales", options);
var report = command.Execute();
```

**Pros:**
- Very flexible
- Can queue/log commands
- Undo/redo support possible
- Command history

**Cons:**
- Overly complex for this use case
- More boilerplate
- Undo/redo not needed
- Command queuing not needed
- Harder to discover operations

**Why not chosen:** Overkill, no need for command queuing or undo

## Implementation Examples

**Service with Default Constructor:**
```csharp
public sealed class ScanService
{
    private readonly FormatRegistry _registry;
    
    public ScanService() : this(FormatRegistry.Default) { }
    public ScanService(FormatRegistry registry) => _registry = registry;
    
    public ScanReport Scan(string path, ScanOptions options)
    {
        // Implementation
    }
}
```

**Options Object:**
```csharp
public sealed class ScanOptions
{
    public required string BaseCulture { get; set; }
    public List<string> TargetCultures { get; set; } = [];
    public bool Recursive { get; set; } = true;
    public List<string> IgnorePatterns { get; set; } = [];
    public bool CheckPlaceholders { get; set; } = true;
}
```

**Report Object:**
```csharp
public sealed class ScanReport
{
    public required string BaseCulture { get; init; }
    public List<string> TargetCultures { get; init; } = [];
    public List<CultureComparisonResult> Results { get; init; } = [];
}
```

**Library Usage:**
```csharp
var service = new ScanService();
var report = service.Scan("./locales", new ScanOptions
{
    BaseCulture = "en",
    TargetCultures = ["tr", "de", "fr"],
    Recursive = true
});
```

**DI Container Registration:**
```csharp
services.AddSingleton<FormatRegistry>(FormatRegistry.Default);
services.AddScoped<ScanService>();
services.AddScoped<DiffService>();
services.AddScoped<CheckService>();
services.AddScoped<ConvertService>();
services.AddScoped<GenerateService>();
services.AddScoped<TranslateService>();
services.AddScoped<WatchService>();
```

**Async Operations:**
```csharp
public sealed class TranslateService
{
    public async Task TranslateAsync(
        string path, 
        TranslateOptions options,
        CancellationToken cancellationToken = default)
    {
        // Async implementation for I/O-bound translation
    }
}
```

## Testing Benefits

**Unit Testing Services:**
```csharp
[Fact]
public void Scan_WithMissingKeys_ReportsMissing()
{
    // Arrange
    var customRegistry = new FormatRegistry();
    customRegistry.Register(new JsonLocalizationFormat());
    var service = new ScanService(customRegistry);
    
    // Act
    var report = service.Scan("./test-data", new ScanOptions
    {
        BaseCulture = "en",
        TargetCultures = ["tr"]
    });
    
    // Assert
    Assert.Single(report.Results);
    Assert.NotEmpty(report.Results[0].MissingKeys);
}
```

**Mocking Dependencies:**
```csharp
var mockRegistry = new Mock<FormatRegistry>();
var service = new ScanService(mockRegistry.Object);
```

## References

- [Service Layer Pattern](https://martinfowler.com/eaaCatalog/serviceLayer.html)
- [Options Pattern in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/options)
- [Dependency Injection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- Related ADR: [0001-multi-format-architecture.md](./0001-multi-format-architecture.md)
- Related ADR: [0002-format-registry-pattern.md](./0002-format-registry-pattern.md)