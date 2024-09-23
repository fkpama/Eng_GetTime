
using System.Buffers.Binary;
using System.Buffers;

/// <summary>
/// Generates random currency conversion rates.
/// </summary>
sealed class PriceGenerator
{

    #region Fields

    private static ArrayPool<byte> s_pool = ArrayPool<byte>.Shared;
    private readonly byte[] encodedPrices = new byte[Constants.MaxBufferLength * 2];
    private readonly MemoryStream encodedPriceStream;
    private DispatcherMessageType _messageType;
    private static JsonSerializerOptions s_options = new()
    {
        DictionaryKeyPolicy =  JsonNamingPolicy.CamelCase,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };
    private ILogger<PriceGenerator> log;
    private readonly HashSet<CurrencyConversion> prices;

    #endregion Private Fields
    
    #region Properties

    public int Length { get; private set; }

    #endregion Properties

    #region Constructors

    public PriceGenerator(
        DispatcherMessageType messageType,
        ILogger<PriceGenerator>? log = null)
        : this(messageType, Constants.Currencies)
    {
    }

    public PriceGenerator(
        DispatcherMessageType messageType,
        IEnumerable<CurrencyConversion> currencies,
        ILogger<PriceGenerator>? log = null)
    {
        this.log = log ?? NullLogger<PriceGenerator>.Instance;
        this._messageType = messageType;
        this.encodedPriceStream = new(this.encodedPrices);
        this.prices = new(currencies);
        this.GeneratePrices();
    }

    #endregion Constructors

    #region Methods
    public void GeneratePrices()
    {
        lock(prices)
        {
            foreach(var conversion in prices)
            {
                conversion.Rate = Random.Shared.NextDouble() * 10;
            }

            if (this._messageType == DispatcherMessageType.Binary)
            {
                writePrices();
            }
            else
            {
                writeJsonPrices();
            }
        }


        // Methodes pour l'encodage. Mise ici car
        // elle doit absolument tourner dans le lock ci dessus
        void writeJsonPrices()
        {
            this.encodedPriceStream.Seek(0, SeekOrigin.Begin);
            this.encodedPriceStream.SetLength(0);
            JsonSerializer.Serialize(this.encodedPriceStream, this.prices, s_options);
            this.Length = (int)this.encodedPriceStream.Length;
        }


        void writePrices()
        {

            // On a un nombre de currency relativement bas
            // qui devrait donc largement suffir dans le
            // buffer de 2048. Pas de check
            int index = 0;
            foreach(var conversion in prices)
            {
                var count = Encoding.UTF8.GetBytes(conversion.ToString().AsSpan(),
                                                    new Span<byte>(this.encodedPrices, index, this.encodedPrices.Length - index));
                index += count;
                //. this.encodedPrices[index++] = 0;
                // this.encodedPrices[index++] = 0;
                Console.WriteLine($"Writting {conversion}");
                BinaryPrimitives.WriteDoubleLittleEndian(this.encodedPrices.AsSpan(index), conversion.Rate);
                index+= sizeof(double);
                this.Length = index;
            }
        }
    }

    /// <summary>
    /// Allocates and writes <paramref name="encoded"/> with
    /// the encoded currency conversions (according to <see cref="MessageType"/>).
    /// The Buffer <b>MUST</b> be returned via <see cref="ReturnPrices"/>
    /// </summary>
    /// <param name="encoded"></param>
    public void AllocPrices(ref ArraySegment<byte> encoded)
    {
        lock(this.prices)
        {
            if (this.Length == 0)
            {
                log.NothingToWrite();
                return;
            }

            var ar = s_pool.Rent(this.Length);
            this.encodedPrices.AsSpan(0, this.Length).CopyTo(ar);
            encoded = new(ar, 0, this.Length);
        }
    }

    /// <summary>
    /// Return a buffer allocated by <see cref="AllocPrices"/>.
    /// Put here to manage the buffers in a single place, for
    /// maintainability
    /// </summary>
    /// <param name="bytes"></param>
    public void ReturnPrices(ArraySegment<byte> bytes)
    {
        if (bytes.Array?.Length > 0)
        {
            s_pool.Return(bytes.Array);
        }
    }

    /// <summary>
    /// Add a conversion currency to be dispatched to clients
    /// </summary>
    /// <param name="conversion"></param>
    /// <param name="factor"></param>
    public void Add(CurrencyConversion conversion, decimal factor)
    {
        lock(this.prices)
        {
            this.Add(conversion, factor);
        }
    }

    #endregion Methods

}