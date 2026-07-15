# MaCo.Extensions.Logging — Architecture Assessment

> Generated: 2026-07-15 | Branch: feature/add_microsoft_logging_extension

---

## 1. Solution Structure

### Projects

| Project | Type | Target Frameworks |
|---|---|---|
| `MaCo.Extensions.Logging` | Class Library (NuGet) | `netstandard2.0`, `net6.0`, `net7.0`, `net8.0` |
| `MaCo.Extensions.Logging.Tests` | MSTest Test Project | `net10.0` |

### SDK & Build

- **.NET SDK:** 10.0.101 (pinned via `global.json`)
- **Language:** C# latest (via `Directory.Build.props`)
- **Nullable:** Enabled globally
- **Package output:** Auto-generated on build (`GeneratePackageOnBuild=true`)

### NuGet Dependencies

| Package | Version | Scope |
|---|---|---|
| `Microsoft.Extensions.Logging` | 8.0.1 | Library |
| `Microsoft.Extensions.Hosting` | 8.0.1 | Library |
| `Microsoft.Extensions.Configuration.Binder` | 8.0.2 | Library + Tests |
| `System.Text.Json` | 8.0.5 | Library |
| `Microsoft.NET.Test.Sdk` | 17.8.0 | Tests |
| `MSTest.TestAdapter` | 3.1.1 | Tests |
| `MSTest.TestFramework` | 3.1.1 | Tests |
| `coverlet.collector` | 6.0.0 | Tests |

### Package Metadata Issues

- `PackageProjectUrl` still points to `https://www.nuget.org/packages/Aghili.Logging` — stale after namespace rename
- `AssemblyVersion` is `8.0.0` while `Version` is `8.1.0.0` — mismatch
- No `PackageReleaseNotes` element

---

## 2. Architecture

### Style

This is a **class library** (not a web app or service). It follows a **Singleton + Adapter** pattern to extend `Microsoft.Extensions.Logging` with custom file, online, and Windows Event Log backends.

Two consumption paths coexist:

1. **Direct singleton API:** `Log.Instance.WriteNew(...)` — uses `StackTrace` for caller info
2. **ILogger bridge:** `MaCoLogger` implements `ILogger`, delegates to the singleton

### Component Map

```
Consumer Code
    │
    ├── Log.Instance.WriteNew(type, msg...)          [StackTrace path]
    ├── Log.Instance.WriteNew(level, msg...)         [StackTrace path]
    ├── Log.Instance.WriteNew(exception, msg...)     [StackTrace path]
    ├── logger.LogInformation(msg)                   [CallerInfo path via extensions]
    └── logger.Log<TState>(...)                      [ILogger bridge → singleton]
            │
            ▼
    ┌─── Log (singleton) ───────────────────────────┐
    │  • Settings management (JSON file / IConfiguration) │
    │  • CallerContext resolution (StackTrace or CallerInfo) │
    │  • Message building (params object[] → string) │
    │  • Adapter dispatch (writeAdapterWrite)        │
    └───────────────┬───────────────────────────────┘
                    │
        ┌───────────┼───────────┐
        ▼           ▼           ▼
  LogFileAdapter  LogOnlineAdapter  LogWindowsEventAdapter
  (Thread-based)  (Task-based)      (empty stub)
```

### Background Execution

| Adapter | Mechanism | Signal |
|---|---|---|
| `LogFileAdapter` | Dedicated `System.Threading.Thread` | `AutoResetEvent` |
| `LogOnlineAdapter` | `Task.Run(UploadLoop)` | `CancellationTokenSource` + `Task.Delay` |

Neither uses `IHostedService` or `BackgroundService`. The library manages its own threading internally.

### Configuration Sources

1. **JSON file:** `{ExecPath}/Log/Settings.json` — read on construction, written if missing
2. **IConfiguration:** `Log.Configure(IConfiguration, "Logging:MaCo")` — binds section to `LogSettings`
3. **Programmatic:** `Log.Instance.Settings.*` — direct property mutation

---

## 3. ASP.NET Core Integration

This library is not an ASP.NET Core application. It integrates with the ASP.NET Core ecosystem through:

- `ILogger` / `ILoggerProvider` / `ILoggingBuilder` abstractions
- `IConfiguration` binding for settings
- Extension methods on `ILoggingBuilder`

**Not present:** Controllers, Minimal APIs, endpoint routing, filters, middleware, authentication, authorization.

---

## 4. Logging Architecture

### Adapter Interface

```csharp
public interface ILogWrite : IDisposable, IEquatable<LogType>
{
    IWriterOption WriteOptions { get; set; }
    LogType WriterType { get; }
    void Write(LogMessageType type, string path, string message);
    void Write(LogLevel type, string path, string message);
    event EventHandler<ShrinkEventArgs> OnShrinkRise;
}
```

### Adapter Implementations

| Adapter | Visibility | Threading | Retry | Offline |
|---|---|---|---|---|
| `LogFileAdapter` | public | Background `Thread` | 100 retries + dead-letter | N/A (local files) |
| `LogOnlineAdapter` | internal | `Task.Run` loop | Persists to JSON files | Recovery on startup |
| `LogWindowsEventAdapter` | internal | Synchronous (no-op) | None | N/A |

### Message Flow

1. Consumer calls `WriteNew(type, msg)` or `ILogger.Log()`
2. Caller context resolved (StackTrace or CallerInfo attributes)
3. `BuildMessage()` concatenates `params object[]` with `=>` separator
4. `WriteNewCore()` dispatches to adapters with three file paths per message:
   - `{Module}/{Class}/{Method}/{Type}.log`
   - `{Module}/{Class}/{Method}/AllMessages.log`
   - `AllMessages.log` (global)

### Structured Logging

**Not implemented.** All output is flat text. No message template parsing, no property bags, no JSON serialization of log events.

### Log Levels Mapping

| `Microsoft.Extensions.Logging.LogLevel` | `LogMessageType` |
|---|---|
| `Trace`, `Debug`, `Information` | `Information` |
| `Warning` | `Warning` |
| `Error`, `Critical` | `Exception` |

`LogLevel.None` and `LogLevel.Trace`/`Debug` lose granularity — they all map to `Information`.

---

## 5. Observability

| Technology | Status |
|---|---|
| OpenTelemetry | Not present |
| ActivitySource / Activities | Not present |
| Meter / Metrics | Not present |
| Distributed Tracing | Not present |
| Health Checks | Not present |
| Prometheus exporter | Not present |
| Grafana dashboards | Not present |
| Application Insights | Not present |
| Sentry | Not present |
| Seq | Not present |
| OpenObserve | Not present |
| SigNoz | Not present |

**Assessment:** The library has zero integration with any observability platform. There are no hooks, events, or extension points that would allow external telemetry systems to consume log data.

---

## 6. Dependency Injection

### Registration Methods

| Extension Method | Target | Behavior |
|---|---|---|
| `AddMaCoLogging()` | `ILoggingBuilder` | Creates default `MaCoLoggerConfiguration`, registers `MaCoLoggerProvider` |
| `AddMaCoLogging(MaCoLoggerConfiguration)` | `ILoggingBuilder` | Uses caller-supplied config instance |
| `AddMaCoLogging(IConfiguration, string)` | `ILoggingBuilder` | Calls `Log.Configure()` to bind from config section, then registers provider |

### What Gets Registered

Only `MaCoLoggerProvider` as an `ILoggerProvider`. No services, no hosted services, no options pattern integration.

### Missing DI Patterns

- No `AddMaCoLogging(Action<MaCoLoggerConfiguration>)` overload (lambda-based configuration)
- No `IOptions<T>` / `IOptionsSnapshot<T>` integration
- No named/typed logger support beyond category name

---

## 7. Cross-cutting Concerns

| Pattern | Present | Notes |
|---|---|---|
| Middleware | No | Library, not a pipeline |
| Action filters | No | — |
| Decorators | No | — |
| Base controllers | No | — |
| Base services | No | — |
| CQRS / MediatR | No | — |
| Event bus | No | — |
| Pipeline behaviors | No | — |

The only cross-cutting mechanism is the **adapter pattern** — all log writes flow through `writeAdapterWrite()` which dispatches to registered `ILogWrite` implementations.

---

## 8. Error Handling

### Within the Library

| Location | Strategy |
|---|---|
| `Log.LoadSettings()` | Silent catch — falls back to default settings, writes default JSON |
| `Log.ReadSettingContent()` | Throws `FileNotFoundException` if settings file missing |
| `WriteNewCore()` | Catches exceptions from adapter writes, routes to `WriteErrorLog()` |
| `WriteErrorLog()` | Writes error details to `LogError.log`, swallows its own exceptions |
| `LogFileAdapter.WriteLogEntity()` | Increments retry counter, returns false on failure |
| `LogFileAdapter.DeadLetter()` | Moves failed messages to `.dead` file after 100 retries |
| `LogOnlineAdapter.FlushAsync()` | On HTTP failure or non-success, persists batch to offline JSON |
| `LogOnlineAdapter.PersistOfflineAsync()` | Silent catch — data loss if disk write fails |
| `PermissionsHelper.EnsurePermissions()` | Silent catch — best-effort directory creation |

### Exception Handling Philosophy

The library follows a **fail-silent** approach: logging failures are swallowed to prevent the logging system from crashing the host application. This is appropriate for a logging library but means silent data loss is possible.

### Missing Error Handling Patterns

- No `IExceptionHandler` or `ProblemDetails` integration (not applicable for a library)
- No circuit breaker for the online adapter
- No exponential backoff for HTTP retries
- No dead-letter queue monitoring or alerting

---

## 9. Telemetry Opportunities

Where automatic instrumentation could be added:

| Area | Opportunity | Complexity |
|---|---|---|
| HTTP uploads | Add `ActivitySource` spans around `LogOnlineAdapter.FlushAsync()` | Low |
| File writes | Add metrics counter for bytes written, messages buffered | Low |
| Adapter lifecycle | Emit events on adapter creation, disposal, rebuild | Low |
| Offline persistence | Track offline file count, recovery success rate | Medium |
| Message throughput | `Meter` for messages/sec by type and adapter | Medium |
| Error rate | Counter for failed writes, dead-letter events | Low |
| `Log.Configure()` | Activity span for configuration binding | Low |

---

## 10. Feature Tracking Opportunities

Where business events could be emitted:

| Location | Event | Payload |
|---|---|---|
| `Log.CreateLogAdapter()` | Adapter registered | Adapter type, settings |
| `Log.RebuildAdapters()` | Configuration changed | Old/new settings diff |
| `LogFileAdapter.Backup()` | File rotated | File path, record count |
| `LogFileAdapter.DeadLetter()` | Messages dead-lettered | Count, file path |
| `LogOnlineAdapter.FlushAsync()` | Batch uploaded | Count, endpoint, latency |
| `LogOnlineAdapter.PersistOfflineAsync()` | Offline persistence | Batch size, reason |
| `LogOnlineAdapter.RecoverOfflineFiles()` | Offline recovery | File count, messages recovered |

---

## 11. Public Extension Methods

### MaCoLoggingBuilderExtensions

```csharp
public static ILoggingBuilder AddMaCoLogging(this ILoggingBuilder builder)
public static ILoggingBuilder AddMaCoLogging(this ILoggingBuilder builder, MaCoLoggerConfiguration configuration)
public static ILoggingBuilder AddMaCoLogging(this ILoggingBuilder builder, IConfiguration configuration, string sectionName = "Logging:MaCo")
```

### MaCoLoggerExtensions

```csharp
public static void LogTrace(this ILogger logger, string message, ...)
public static void LogDebug(this ILogger logger, string message, ...)
public static void LogInformation(this ILogger logger, string message, ...)
public static void LogWarning(this ILogger logger, string message, ...)
public static void LogError(this ILogger logger, string message, ...)
public static void LogError(this ILogger logger, Exception ex, string message, ...)
public static void LogCritical(this ILogger logger, string message, ...)
```

All seven accept `[CallerMemberName]`, `[CallerFilePath]`, `[CallerLineNumber]` for accurate source location when used with `MaCoLogger`.

### Deprecated Extensions (Aghili.Logging namespace)

```csharp
// Aghili.Logging.Log static class
public static void Warrning(this Log log, params object[] msg)

// Aghili.Logging.LogSettingsExtensions
public static LogMesssageType GetMesssageTypes(this Log.LogSettings settings)
public static void SetMesssageTypes(this Log.LogSettings settings, LogMesssageType value)
```

---

## 12. Recommendations

### Weaknesses

1. **Dual API surface creates confusion.** `Log.Instance.WriteNew()` and `ILogger.Log()` follow different caller-resolution strategies (StackTrace vs CallerInfo). Users get inconsistent file/line info depending on which path they use.

2. **`Log` singleton is not thread-safe for initialization.** The constructor reads from the file system and creates adapters. If two threads access `Log.Instance` simultaneously during startup, race conditions can occur.

3. **`writeAdapter` is a public `List<ILogWrite>` field.** External code can mutate the adapter list without synchronization, causing `InvalidOperationException` during enumeration.

4. **`StackTrace`-based caller resolution is fragile.** JIT inlining, Release builds, and AOT compilation can eliminate stack frames, producing wrong file/line info. The `CallerInfo` overloads solve this but only work when callers use the extension methods.

5. **No structured logging support.** Messages are flat strings joined with `=>`. Modern logging systems (Seq, Elastic, Loki) benefit enormously from structured properties.

6. **`LogWindowsEventAdapter` is a dead stub.** Its `Write()` methods are empty. It should either be implemented or removed.

7. **`MaCoLoggerConfiguration` is a separate POCO from `LogSettings`.** They hold overlapping data (`LogType`, `LogKeepDataOnLimitRichedPercent`, `LogRowLimitPerContainer`) but are never synchronized. Changes to one don't reflect in the other.

8. **`MaCoLogger.BeginScope()` returns null.** Scope-based logging (request correlation, ambient context) is unsupported.

9. **No `IOptions<T>` integration.** Configuration changes via `IOptionsMonitor` don't propagate to the logging system.

10. **`net7.0` target is EOL.** Should be removed or replaced with `net9.0`/`net10.0`.

### Missing Capabilities

- **No `IHostedService` registration** — background adapters manage their own threads instead of participating in the host lifecycle
- **No health check** — no way to verify the online adapter can reach its endpoint
- **No metrics** — no counters for messages written, bytes uploaded, errors encountered
- **No correlation ID propagation** — log entries from the same request cannot be linked
- **No log level filtering at the adapter level** — all filtering happens in `Log.WriteNew()` before dispatch
- **No async write path** — `LogFileAdapter` uses raw `Thread` instead of `Channel<T>` or `ChannelWriter<T>`
- **No `IConfiguration` hot-reload** — settings are read once at construction; `IOptionsMonitor` changes are ignored

### Extension Points

- **`ILogWrite` interface** — consumers can create custom adapters (e.g., database, message queue, Seq)
- **`IWriterOption` interface** — configurable per-adapter limits
- **`OnShrinkRise` event** — notification when files are rotated or dead-lettered
- **`Log.Configure()`** — runtime reconfiguration from `IConfiguration`

### Suggested Improvements (Priority Order)

1. **Make `writeAdapter` private** — expose via thread-safe accessor
2. **Add `IOptions<MaCoLoggerSettings>` integration** for hot-reload support
3. **Replace `Thread` with `Channel<T>`** in `LogFileAdapter` for modern async I/O
4. **Implement `LogWindowsEventAdapter`** or remove it entirely
5. **Unify `MaCoLoggerConfiguration` and `LogSettings`** into a single options class
6. **Add `BeginScope` support** in `MaCoLogger` for request correlation
7. **Drop `net7.0`**, add `net9.0` and `net10.0` targets
8. **Fix stale `PackageProjectUrl`** in csproj
9. **Add `ActivitySource`** for distributed tracing integration
10. **Add structured logging** via message template parsing
