internal static class Constants
{
    internal const int MaxBufferLength = 4096;

    /// <summary>
    /// Default Endpoint (IPAddress or server DNS name) for the server to listen to
    /// </summary>
    internal const string DEFAULT_HOST = "localhost";

    /// <summary>
    /// Port par default.
    /// </summary>
    internal const int DEFAULT_PORT = 8181;
    internal static IEnumerable<CurrencyConversion> Currencies =  [new("EUR", "USD"), new("EUR", "GBP"), new("USD", "GBP")];
}