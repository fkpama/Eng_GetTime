using System.Diagnostics.Tracing;

static partial class LogMessages
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Socket Connected {host}"
    )]
    internal static partial void SocketConnected(this ILogger<Program> log, string host);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Warning,
        Message = "Invalid request from {ep}"
    )]
    internal static partial void InvalidRequest(this ILogger<Program> log, string? ep);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Warning,
        Message = $"A call to {nameof(PriceGenerator.AllocPrices)} was made without any price to write"
    )]
    internal static partial void NothingToWrite(this ILogger<PriceGenerator> log);
 
    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "Socket Closed unexpectedly: {message}"
    )]
    internal static partial void SocketClosed(this ILogger<PriceDispatcher> log, string message);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "Starting (or restarting) Client Listener"
    )]
    internal static partial void StartingClientListener(this ILogger<PriceDispatcher> log);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Information,
        Message = "No more client listening to updates"
    )]
    internal static partial void NoMoreClientListening(this ILogger<PriceDispatcher> log);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Information,
        Message = "Server started. Waiting for connections ({url})..."
    )]
    internal static partial void ServerStarted(this ILogger<Program> log, string url);
}