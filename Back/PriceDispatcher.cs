
using System.Buffers;

public enum DispatcherMessageType
{
    Binary,
    Text
}
sealed class PriceDispatcher
{

    #region Fields

    private static ArrayPool<WebSocket> s_pool = ArrayPool<WebSocket>.Shared;
    private readonly HashSet<WebSocket> _allSockets = [];
    private readonly ManualResetEvent _clientConnectedEvent = new(false);
    private readonly WebSocketMessageType _messageType;
    private readonly ILogger<PriceDispatcher> log;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Notifie que le dispatcher a au moint 1 client en attente.
    /// </summary>
    public WaitHandle Handle => this._clientConnectedEvent;

    /// <summary>
    /// Number of clients still connected to the server.
    /// </summary>
    public int ClientCount {
        get {
            lock(this._allSockets)
            {
                return this._allSockets.Count;
            }
        }
    }

    #endregion Properties

    #region Constructors

    public PriceDispatcher(DispatcherMessageType messageType, ILogger<PriceDispatcher>? logger = null)
    {
        this.log = logger ?? NullLogger<PriceDispatcher>.Instance;
        this._messageType = messageType == DispatcherMessageType.Binary ? WebSocketMessageType.Binary : WebSocketMessageType.Text;
    }

    #endregion Constructors

    #region Methods

    /// <summary>
    /// Dispatch encoded prices to all registered sockets
    /// </summary>
    /// <param name="encodedPrices">The currency conversion encoded according to </param>
    /// <param name="options">
    /// By reusing/passing the same <see cref="ParallelOptions"/>, we avoid creating one
    /// for the GC at each call. Usefull for the <see cref="ParallelOptions.CancellationToken"/>
    /// member
    /// </param>
    /// <returns></returns>
    public async Task DispatchAsync(ReadOnlyMemory<byte> encodedPrices, ParallelOptions options)
    {
        int count;
        WebSocket[] sockets;
        lock(this._allSockets)
        {
            count = this._allSockets.Count;
            if (count == 0)
            {
                return;
            }
            sockets = s_pool.Rent(count);
            this._allSockets.CopyTo(sockets, 0);
        }

        try
        {
            await Parallel.ForEachAsync(
                new ArraySegment<WebSocket>(sockets, 0, count),
                options,
                async (socket, ct) => {
                    try {
                        await socket
                            .SendAsync(encodedPrices, this._messageType, true, ct)
                            .ConfigureAwait(false);
                    }
                    catch(Exception ex)
                    when (ex is System.Net.Sockets.SocketException || ex is IOException || ex is WebSocketException)
                    {
                        log.SocketClosed(ex.Message);
                        this._removeSocket(socket);
                    }
                    catch(Exception ex)
                    {
                        log.LogError(ex, "Failed to send message to socket");
                        this._removeSocket(socket);   
                    }
                }).ConfigureAwait(false);
        } finally{
            s_pool.Return(sockets);
        }
    }

    /// <summary>
    /// Register a <see cref="WebSocket"/> to send the conversions.
    /// </summary>
    /// <param name="socket"></param>
    public void Register(WebSocket socket)
    {
        lock(this)
        {
            this._allSockets.Add(socket);
            if(this.ClientCount == 1)
            {
                // We can (re)start the main loop
                log.StartingClientListener();
                this._clientConnectedEvent.Set();
            }
        }
    }

    private void _removeSocket(WebSocket socket)
    {
        lock(this._allSockets)
        {
            this._allSockets.Remove(socket);
            if (this.ClientCount == 0)
            {
                log.NoMoreClientListening();
                this._clientConnectedEvent.Reset();
            }
        }
    }

    #endregion Methods
}