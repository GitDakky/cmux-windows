using System.IO.Pipes;
using System.Text;
using System.Text.Json;

namespace Cmux.Core.IPC;

/// <summary>
/// Named pipe server for cmux CLI/API communication.
/// Windows equivalent of the Unix domain socket used by cmux on macOS.
/// Pipe name: \\.\pipe\cmux (or \\.\pipe\cmux-{tag} for tagged instances).
/// </summary>
public sealed class NamedPipeServer : IDisposable
{
    // UTF-8 without BOM — Encoding.UTF8 + AutoFlush can flush a BOM at writer construction
    // and deadlock named pipes via FlushFileBuffers when neither side has read yet.
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private readonly string _pipeName;
    private CancellationTokenSource? _cts;
    private Task? _listenTask;

    public string PipeName => _pipeName;

    /// <summary>
    /// Invoked when a command is received. Args: (command, args dictionary).
    /// Returns the response JSON string.
    /// </summary>
    public Func<string, Dictionary<string, string>, Task<string>>? OnCommand { get; set; }

    public NamedPipeServer(string? tag = null)
    {
        _pipeName = string.IsNullOrEmpty(tag) ? "cmux" : $"cmux-{tag}";
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();
        _listenTask = Task.Run(() => ListenLoop(_cts.Token));
    }

    private async Task ListenLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var pipe = new NamedPipeServerStream(
                    _pipeName,
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await pipe.WaitForConnectionAsync(ct);

                _ = Task.Run(() => HandleConnection(pipe, ct), ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (IOException)
            {
                await Task.Delay(100, ct);
            }
        }
    }

    private async Task HandleConnection(NamedPipeServerStream pipe, CancellationToken ct)
    {
        try
        {
            using (pipe)
            {
                using var reader = new StreamReader(pipe, Utf8NoBom, leaveOpen: true);
                using var writer = new StreamWriter(pipe, Utf8NoBom, leaveOpen: true) { AutoFlush = true };

                var requestLine = await reader.ReadLineAsync(ct);
                if (string.IsNullOrEmpty(requestLine)) return;

                var parts = requestLine.Split(' ', 2);
                var command = parts[0].ToUpperInvariant();
                var args = new Dictionary<string, string>();

                if (parts.Length > 1)
                    ParseArgs(parts[1], args);

                string response;
                if (OnCommand != null)
                    response = await OnCommand(command, args);
                else
                    response = JsonSerializer.Serialize(new { error = "No handler registered" });

                await writer.WriteLineAsync(response);
            }
        }
        catch (IOException)
        {
            // Client disconnected
        }
        catch (OperationCanceledException)
        {
            // Server shutting down
        }
    }

    private static void ParseArgs(string argsString, Dictionary<string, string> args)
    {
        var trimmed = argsString.Trim();
        if (trimmed.StartsWith('{'))
        {
            try
            {
                var json = JsonSerializer.Deserialize<Dictionary<string, string>>(trimmed);
                if (json != null)
                {
                    foreach (var kvp in json)
                        args[kvp.Key] = kvp.Value;
                    return;
                }
            }
            catch
            {
                // Fall through to key=value parsing
            }
        }

        foreach (var part in SplitRespectingQuotes(trimmed))
        {
            int eq = part.IndexOf('=');
            if (eq > 0)
            {
                var key = part[..eq];
                var value = part[(eq + 1)..].Trim('"', '\'');
                args[key] = value;
            }
            else
            {
                args.TryAdd("_arg" + args.Count, part);
            }
        }
    }

    private static IEnumerable<string> SplitRespectingQuotes(string input)
    {
        var current = new StringBuilder();
        bool inQuote = false;
        char quoteChar = '\0';

        foreach (var c in input)
        {
            if (inQuote)
            {
                if (c == quoteChar)
                    inQuote = false;
                else
                    current.Append(c);
            }
            else if (c is '"' or '\'')
            {
                inQuote = true;
                quoteChar = c;
            }
            else if (c == ' ')
            {
                if (current.Length > 0)
                {
                    yield return current.ToString();
                    current.Clear();
                }
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0)
            yield return current.ToString();
    }

    public void Stop()
    {
        _cts?.Cancel();
        _listenTask?.Wait(TimeSpan.FromSeconds(2));
    }

    public void Dispose()
    {
        Stop();
        _cts?.Dispose();
    }
}

/// <summary>
/// Client for connecting to the cmux named pipe server (used by the CLI).
/// </summary>
public static class NamedPipeClient
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public static async Task<string> SendCommand(string command, Dictionary<string, string>? args = null, string? tag = null, int timeoutMs = 5000)
    {
        var pipeName = string.IsNullOrEmpty(tag) ? "cmux" : $"cmux-{tag}";

        using var cts = new CancellationTokenSource(timeoutMs);
        using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

        using var timeoutRegistration = cts.Token.Register(static state =>
        {
            try { ((IDisposable)state!).Dispose(); } catch { /* already disposed */ }
        }, pipe);

        try
        {
            await pipe.ConnectAsync(cts.Token);

            using var reader = new StreamReader(pipe, Utf8NoBom, leaveOpen: true);
            using var writer = new StreamWriter(pipe, Utf8NoBom, leaveOpen: true) { AutoFlush = true };

            var sb = new StringBuilder(command);
            if (args != null)
            {
                foreach (var kvp in args)
                {
                    var value = kvp.Value.Contains(' ') ? $"\"{kvp.Value}\"" : kvp.Value;
                    sb.Append($" {kvp.Key}={value}");
                }
            }

            await writer.WriteLineAsync(sb.ToString());

            var response = await reader.ReadLineAsync(cts.Token);
            return response ?? "";
        }
        catch (Exception ex) when (cts.IsCancellationRequested)
        {
            throw new TimeoutException($"cmux did not respond within {timeoutMs} ms.", ex);
        }
    }
}
