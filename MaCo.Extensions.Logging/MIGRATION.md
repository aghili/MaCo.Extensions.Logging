# Migration Guide: v1.x → v2.0

## Breaking Changes Summary

| Change | Old (v1.x) | New (v2.0) | Migration |
|--------|-------------|------------|-----------|
| Namespace | `Aghili.Logging` | `MaCo.Extensions.Logging` | Update `using` statements |
| Enum | `LogMesssageType` | `LogMessageType` | Rename type references |
| Enum value | `Warrning` | `Warning` | Rename value references |
| Property | `MesssageTypes` | `MessageTypes` | Rename property access |
| Class | `ShirinkEventArgs` | `ShrinkEventArgs` | Rename type references |
| Enum | `ShirinkType` | `ShrinkType` | Rename type references |
| Event | `OnShiringRise` | `OnShrinkRise` | Rename event subscriptions |
| Method | `Warrning()` | `Warning()` | Rename method calls |

## Namespace Migration

```csharp
// Before (v1.x)
using Aghili.Logging;
using Aghili.Logging.Classes;

// After (v2.0)
using MaCo.Extensions.Logging;
using MaCo.Extensions.Logging.Classes;
```

## Type Renames

### LogMesssageType → LogMessageType

```csharp
// Before
LogMesssageType type = LogMesssageType.Exception;

// After
LogMessageType type = LogMessageType.Exception;
```

### Warrning → Warning

```csharp
// Before
Log.Instance.WriteNew(LogMesssageType.Warrning, "message");
Log.Instance.Warrning("message");

// After
Log.Instance.WriteNew(LogMessageType.Warning, "message");
Log.Instance.Warning("message");
```

### MesssageTypes → MessageTypes

```csharp
// Before
Log.Instance.Settings.MesssageTypes = LogMesssageType.Exception | LogMesssageType.Warrning;

// After
Log.Instance.Settings.MessageTypes = LogMessageType.Exception | LogMessageType.Warning;
```

### ShirinkEventArgs → ShrinkEventArgs

```csharp
// Before
adapter.OnShiringRise += (sender, e) => { ... };

// After
adapter.OnShrinkRise += (sender, e) => { ... };
```

### ShirinkType → ShrinkType

```csharp
// Before
if (e.Type == ShirinkType.Backup) { ... }

// After
if (e.Type == ShrinkType.Backup) { ... }
```

## New Features in v2.0

### IConfiguration Binding

```csharp
// New: Automatic configuration from appsettings.json
builder.Logging.AddMaCoLogging(configuration);

// appsettings.json
{
  "Logging": {
    "MaCo": {
      "Enabled": true,
      "MessageTypes": "Exception,Warning,Information",
      "LogType": "File",
      "Online": {
        "Enabled": false,
        "ApiEndpoint": "",
        "ApiKey": ""
      }
    }
  }
}
```

### Caller Info Attributes

New overloads use compiler-provided caller information for better performance:

```csharp
// These overloads avoid expensive StackTrace construction
Log.Instance.WriteNew(LogMessageType.Information, "message");
Log.Instance.WriteNew(LogLevel.Information, "message");
```

### Category Name Forwarding

`MaCoLogger` now includes the logger category in messages:

```
[2024-01-15 10:30:00][L:42][Information][MyNamespace.MyClass] Hello World
```

## Backward Compatibility

Deprecated types are available in the original `Aghili.Logging` and `Aghili.Logging.Classes` namespaces:

```csharp
// Old namespace still works but shows deprecation warnings
using Aghili.Logging;
using Aghili.Logging.Classes;

LogMesssageType type = LogMesssageType.Warrning;
adapter.OnShiringRise += (s, e) => { ... };
```

### Available Deprecated Types

| Old Type | New Type | Namespace |
|----------|----------|-----------|
| `Aghili.Logging.LogMesssageType` | `MaCo.Extensions.Logging.LogMessageType` | `Aghili.Logging` |
| `Aghili.Logging.Log` | `MaCo.Extensions.Logging.Log` | `Aghili.Logging` |
| `Aghili.Logging.Classes.ShirinkEventArgs` | `MaCo.Extensions.Logging.Classes.ShrinkEventArgs` | `Aghili.Logging.Classes` |
| `Aghili.Logging.Classes.ShirinkType` | `MaCo.Extensions.Logging.Classes.ShrinkType` | `Aghili.Logging.Classes` |
| `Aghili.Logging.Classes.ILogWrite.OnShiringRise` | `MaCo.Extensions.Logging.Classes.ILogWrite.OnShrinkRise` | `Aghili.Logging.Classes` |

**Note:** These compatibility shims will be removed in v2.0.

## Automated Migration

Use the following find-and-replace patterns:

| Find | Replace |
|------|---------|
| `using Aghili.Logging;` | `using MaCo.Extensions.Logging;` |
| `using Aghili.Logging.Classes;` | `using MaCo.Extensions.Logging.Classes;` |
| `LogMesssageType` | `LogMessageType` |
| `MesssageTypes` | `MessageTypes` |
| `.Warrning(` | `.Warning(` |
| `ShirinkEventArgs` | `ShrinkEventArgs` |
| `ShirinkType` | `ShrinkType` |
| `OnShiringRise` | `OnShrinkRise` |
