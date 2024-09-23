using System.Diagnostics;
using System.Runtime.CompilerServices;

static class Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerStepThrough]
    internal static bool IsClosed(this WebSocketState status)
    {
        return status == WebSocketState.Aborted
        || status == WebSocketState.Closed
        || status == WebSocketState.None;
    }

    internal static bool IsClosed(this WebSocket socket) => socket.State.IsClosed();
}