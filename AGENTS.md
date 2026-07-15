# MaCo.Extensions.Logging Project Rules

This repository implements a custom extension ecosystem for `Microsoft.Extensions.Logging` targeting both **WPF** and **ASP.NET Core**. It captures exceptions, user behavior, and telemetry, batches them, and uploads to a self-hosted open-source analytics server, with robust local file-system persistence as a fallback.

## Core Architectural Rules & Guardrails

### 1. Data Preservation & Local File Fallback (Critical)
- **Zero Data Loss Policy**: Never lose log data under any circumstances (network failure, crash, server downtime).
- Always maintain a robust local file buffer.
- Respect settings like `LogKeepDataOnLimitReachedPercent` and `LogRowLimitPerContainer`.
- On startup or successful upload cycle, recover and re-transmit any pending local logs before purging.

### 2. Library Paradigm & API Consistency
- Primary entry point must remain `Log.Instance.WriteNew(...)`.
- All features must respect `Log.Instance.Settings`.
- Preserve full multi-line detail support.

### 3. Analytics Server Integration
- Payloads must strictly match the external analytics server's ingestion schema.
- Map `LogMessageType.Exception` → Error/Crash telemetry.
- Map `LogMessageType.DataLog` / `Information` → Custom Events / User Behavior.
- Use `IHttpClientFactory` (or properly managed `HttpClient`) to avoid socket exhaustion.

### 4. Non-Blocking & Cross-Platform Execution
- `WriteNew` must be non-blocking (use concurrent queues + background workers).
- Use `PeriodicTimer` or equivalent for scheduled flushing.
- Never block the WPF UI thread or ASP.NET Core request pipeline.

### 5. Configuration & Hosting
- Fully support fluent configuration via `IHostBuilder` / `ILoggingBuilder`.
- Bind settings from `appsettings.json` using `Microsoft.Extensions.Configuration.Binder`.

### 6. Agent Instructions (OpenCode)
- **Always load and follow** the `dotnet-aspnetcore` skill for any C#, ASP.NET Core, or library development work.
- Prefer modern .NET patterns (C# 12/13+, nullable enabled, primary constructors, etc.).
- Maintain multi-targeting: `.NET Standard 2.0`, `net6.0`, `net8.0`, and newer.
- When running commands, first ensure the .NET SDK is in PATH:
  ```bash
  export PATH="/home/aghili/.dotnet:$PATH"