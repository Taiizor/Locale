# ADR-0002: Format Registry Pattern

## Status

Accepted

Date: 2024-12-06

## Context

With multiple localization formats supported (JSON, YAML, RESX, PO, XLIFF, etc.), we need a mechanism to:
- Discover available format handlers at runtime
- Select appropriate format for a given file
- Allow users to register custom formats
- Provide a default set of built-in formats

**Requirements:**
- Automatic format detection based on file extension and content
- Support for custom format registration
- Thread-safe access
- Default registry with all built-in formats
- Ability to create custom registries for specific use cases

**Related:** [ADR-0001: Multi-Format Architecture](./0001-multi-format-architecture.md)

## Decision

Implement the **Registry Pattern** with a singleton default instance:

```csharp
public sealed class FormatRegistry
{
    private readonly Dictionary<string, ILocalizationFormat> _formats;
    private readonly List<ILocalizationFormat> _formatList;
    
    // Singleton default instance with all built-in formats
    public static FormatRegistry Default { get; } = CreateDefault();
    
    // Allow custom instances
    public FormatRegistry() { }
    
    public void Register(ILocalizationFormat format);
    public ILocalizationFormat? GetFormat(string formatId);
    public ILocalizationFormat? GetFormatForFile(string filePath);
    public bool IsSupported(string filePath);
}
```

**Key Features:**
1. **Singleton Default**: `FormatRegistry.Default` provides all built-in formats
2. **Custom Instances**: Users can create custom registries for specific needs
3. **Format Detection**: File extension-based detection with fallback to content analysis
4. **Registration API**: Simple `Register()` method for custom formats
5. **Query API**: Multiple ways to find formats (by ID, file path, or extension)

**Format Detection Strategy:**
1. Check file extension first (fast)
2. Iterate through registered formats calling `CanHandle()`
3. Return first match or null

## Consequences

### Positive

- **Discoverability**: Central place to find all available formats
- **Extensibility**: Easy to add custom formats via `Register()`
- **Convenience**: Default registry pre-configured with all built-in formats
- **Flexibility**: Can create custom registries for specialized scenarios
- **Performance**: Extension-based lookup is O(1), iteration is O(n) where n is small
- **Type Safety**: Compile-time checking for format IDs
- **Testing**: Easy to create test-specific registries

### Negative

- **Global State**: Singleton pattern introduces global state (mitigated by allowing custom instances)
- **Registration Order**: Order matters for ambiguous files (mitigated by explicit extension checks)
- **Thread Safety**: Requires synchronization if registering formats after initialization (currently assumes registration at startup)
- **Hidden Dependency**: Code depending on `Default` has implicit dependency (mitigated by constructor injection support)

### Neutral

- **Memory**: All format handlers kept in memory (acceptable since they're stateless)
- **Immutability**: Registry is mutable after creation (necessary for custom format registration)

## Alternatives Considered

### Alternative 1: Service Locator

**Description:** Use a service locator pattern with dependency injection container

**Pros:**
- Standard DI pattern
- Better testability
- No global state
- Framework integration

**Cons:**
- Requires DI framework
- More complex setup
- Overkill for simple use case
- Library consumers forced to use DI

**Why not chosen:** Adds unnecessary dependency and complexity for a library

### Alternative 2: Static Registry Class

**Description:** Pure static class with no instances

```csharp
public static class FormatRegistry
{
    public static void Register(ILocalizationFormat format);
    public static ILocalizationFormat? GetFormat(string formatId);
}
```

**Pros:**
- Simpler API
- No instances to manage
- Clear global registry

**Cons:**
- No custom registries possible
- Harder to test (global state)
- Can't inject different registries
- Limited flexibility

**Why not chosen:** Too rigid, prevents testing and customization

### Alternative 3: Factory Pattern

**Description:** Factory class that creates format instances on demand

```csharp
public class FormatFactory
{
    public ILocalizationFormat CreateFormat(string formatId);
}
```

**Pros:**
- Can create fresh instances
- Clear responsibility
- Testable

**Cons:**
- Stateless format handlers don't need factory
- More complex for simple registration
- Doesn't solve format discovery
- Extra indirection

**Why not chosen:** Format handlers are stateless, factory adds unnecessary complexity

### Alternative 4: Convention-Based Discovery

**Description:** Automatically discover formats via reflection or assembly scanning

```csharp
// Automatically find all ILocalizationFormat implementations
FormatRegistry.DiscoverFormats(Assembly.GetExecutingAssembly());
```

**Pros:**
- No explicit registration needed
- Automatic discovery of custom formats
- Less code

**Cons:**
- Performance overhead (reflection)
- Harder to control which formats are loaded
- Magic behavior (less explicit)
- Doesn't work well with trimming/AOT
- Security concerns with scanning unknown assemblies

**Why not chosen:** Too much magic, performance concerns, explicit is better than implicit

## Implementation Notes

**Default Registry Creation:**
```csharp
private static FormatRegistry CreateDefault()
{
    var registry = new FormatRegistry();
    
    // Order matters: more specific formats first
    registry.Register(new I18nextJsonLocalizationFormat());  // .i18n.json
    registry.Register(new VbResourceLocalizationFormat());   // .vb
    registry.Register(new FluentFtlLocalizationFormat());    // .ftl
    registry.Register(new XliffLocalizationFormat());        // .xlf, .xliff
    registry.Register(new YamlLocalizationFormat());         // .yaml, .yml
    registry.Register(new ResxLocalizationFormat());         // .resx
    registry.Register(new JsonLocalizationFormat());         // .json
    registry.Register(new VttLocalizationFormat());          // .vtt
    registry.Register(new SrtLocalizationFormat());          // .srt
    registry.Register(new CsvLocalizationFormat());          // .csv
    registry.Register(new PoLocalizationFormat());           // .po
    
    return registry;
}
```

**Service Usage:**
```csharp
// Services accept registry via constructor injection
public sealed class ScanService
{
    private readonly FormatRegistry _registry;
    
    public ScanService() : this(FormatRegistry.Default) { }
    public ScanService(FormatRegistry registry) => _registry = registry;
}
```

**Custom Registry Example:**
```csharp
// Create custom registry for specific scenario
var customRegistry = new FormatRegistry();
customRegistry.Register(new JsonLocalizationFormat());
customRegistry.Register(new MyCustomFormat());

var service = new ScanService(customRegistry);
```

## References

- [Registry Pattern](https://martinfowler.com/eaaCatalog/registry.html)
- [Service Locator Anti-Pattern](https://blog.ploeh.dk/2010/02/03/ServiceLocatorisanAnti-Pattern/)
- [Singleton Pattern](https://en.wikipedia.org/wiki/Singleton_pattern)
- Related ADR: [0001-multi-format-architecture.md](./0001-multi-format-architecture.md)