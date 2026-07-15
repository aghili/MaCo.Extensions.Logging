using MaCo.Extensions.Logging;
using MaCo.Extensions.Logging.Classes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace MaCo.Extensions.Logging.Tests;

internal class InMemoryAdapter : ILogWrite, IDisposable, IEquatable<LogType>
{
    public List<string> Entries { get; } = new List<string>();

    public IWriterOption WriteOptions { get; set; } = new WriteOption();

    public LogType WriterType { get; set; } = LogType.File;

    public event EventHandler<ShrinkEventArgs>? OnShrinkRise;

    public void Write(LogMessageType type, string path, string message) =>
        Entries.Add($"{type}|{path}|{message}");

    public void Write(LogLevel type, string path, string message) =>
        Entries.Add($"{type}|{path}|{message}");

    public bool Equals(LogType other) => WriterType == other;

    public void Dispose()
    {
    }
}
