namespace YousifAccounting.Domain.Entities;

public sealed class ExchangeRate
{
    public string CurrencyCode { get; set; } = null!;
    public decimal RateToUsd { get; set; }
    public DateTime FetchedAt { get; set; }
}
