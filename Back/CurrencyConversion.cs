
/// <summary>
/// Classe de context <b>et de transfert (json)</b> des conversions
/// </summary>
sealed class CurrencyConversion : IEquatable<CurrencyConversion>, IComparable<CurrencyConversion>
{
    private readonly int _hashCode;
    private readonly string _text;
    public string Text => this._text;

    public double Rate { get; set; }

    public CurrencyConversion(string From, string To)
    {
        this._text = $"{From}/{To}".ToUpperInvariant();
        this._hashCode = this._text.GetHashCode();
    }
    public override int GetHashCode()
    {
        return this._hashCode;
    }

    public override string ToString()
    {
        return this._text;
    }

    public bool Equals(CurrencyConversion? s)
    {
        return string.Equals(s?._text, this._text, StringComparison.OrdinalIgnoreCase);
    }
    
    // override object.Equals
    public override bool Equals(object? obj)
    => obj is CurrencyConversion s && Equals(s);

    public int CompareTo(CurrencyConversion? other)
    {
        return this._text.CompareTo(other?._text);
    }
}