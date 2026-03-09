namespace TrustSync.Application.DTOs;

public class ConversionResult
{
    public decimal ConvertedAmount { get; set; }
    public decimal ExchangeRateUsed { get; set; }
    public string TargetCurrencyCode { get; set; } = "USD";
}
