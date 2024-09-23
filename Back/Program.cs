


using System.Buffers;
using System.Text;
using Microsoft.Extensions.DependencyInjection;



class Program {
    /// <summary>
    /// Helper class to parse app arguments
    /// </summary>
    /// <param name="IpAddress">The server listening IPAddress/Host</param>
    /// <param name="Port">The server listening port</param>
    /// <param name="Dispatcher"></param>
    /// <param name="MessageType"></param>
    /// <param name="Generator"></param>
    /// <param name="CancellationToken"></param>
    record class ConnectionInfo(
        string IpAddress,
        int Port,
        PriceDispatcher Dispatcher,
        PriceGenerator Generator,
        CancellationToken CancellationToken);

    static ILogger<Program> log;
    static ILoggerFactory loggerFactory;

    static Program()
    {
        log = null!;
        loggerFactory = null!;
    }

    public static void StartServer(object? argsO)
    {
        HttpListener listener = new();
        var args = (ConnectionInfo)argsO!;
        var ipAddress = args.IpAddress;
        var port = args.Port;
        var cancellationToken = args.CancellationToken;
        var dispatcher = args.Dispatcher;

        var prefix = $"http://{ipAddress}:{port}/";
        listener.Prefixes.Add(prefix);
        listener.Start();
        log.ServerStarted(prefix);

        while(!cancellationToken.IsCancellationRequested)
        {
            HttpListenerContext context = listener.GetContext();
            if (context.Request.IsWebSocketRequest)
            {
                _ = ProcessWebSocketRequest(context, dispatcher, cancellationToken);
            }
            else
            {

                log.InvalidRequest(context.Request.RemoteEndPoint?.ToString());
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }
    }

    private static async Task ProcessWebSocketRequest(
        HttpListenerContext context,
        PriceDispatcher dispatcher,
        CancellationToken cancellationToken
    )
    {
        HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
        WebSocket socket = webSocketContext.WebSocket;
        log.SocketConnected(webSocketContext.Origin);
        dispatcher.Register(socket);
    }

    internal static async Task Main(string[] args) {
        var cts = new CancellationTokenSource();
        loggerFactory = _initLogging(args);
        log = loggerFactory.CreateLogger<Program>();
        var conInfo = BuildAppComponents(args, cts.Token);
        var conversions = conInfo.Generator;
        var dispatcher = conInfo.Dispatcher;
        ArraySegment<byte> networkBuffer = default;
        WaitHandle[] handles = [dispatcher.Handle, cts.Token.WaitHandle];
        var parallelOptions = new ParallelOptions {
            CancellationToken = cts.Token
        };

        var thread = new Thread(StartServer);
        Console.CancelKeyPress += (o, e) => {
            cts.Cancel();
        };

        thread.Start(conInfo);

        
        // Main loop
        do {
            var delay = Random.Shared.Next(1, 100);
            // On pourrait utiliser le new PeriodicTimer si
            // on a vraiment un besoin d'etre le plus precis
            // possible, mais pour cette application un Task.Delay
            // suffit
            await Task.Delay(delay).ConfigureAwait(false);
            WaitHandle.WaitAny(handles);
            if (cts.IsCancellationRequested)
            {
                break;
            }
            
            try
            {
                conversions.AllocPrices(ref networkBuffer);
                var task = dispatcher.DispatchAsync(networkBuffer, parallelOptions);
                if (!cts.IsCancellationRequested)
                {
                    // For performance, while DispatchAsync is sending the data,
                    // we generate prices here
                    conversions.GeneratePrices();
                }
                await task.ConfigureAwait(false);
            }
            finally
            {
                conversions.ReturnPrices(networkBuffer);
            }
            
        } while (!cts.IsCancellationRequested);
    }

    static ConnectionInfo BuildAppComponents(string[] args, CancellationToken cancellationToken)
    {
        var ipAddress = args.Length == 2 ? args[0] : Constants.DEFAULT_HOST;
        var port = args.Length == 2 ? int.Parse(args[1]) : (args.Length == 1 ? int.Parse(args[0]) : Constants.DEFAULT_PORT);
        
        log.LogInformation($"Starting server {ipAddress}:{port}");
        var messageType = DispatcherMessageType.Text;

        var generator = new PriceGenerator(
            messageType,
            loggerFactory.CreateLogger<PriceGenerator>());
            
        var dispatcher = new PriceDispatcher(
            messageType,
            loggerFactory.CreateLogger<PriceDispatcher>());

        return new(ipAddress, port, dispatcher, generator, cancellationToken);
    }


    static ILoggerFactory _initLogging(string[] args)
    {
        var sp = new ServiceCollection();
        sp.AddLogging(builder => builder.AddConsole());

        return sp.BuildServiceProvider().CreateScope().ServiceProvider.GetRequiredService<ILoggerFactory>();
    }
}