namespace YousifAccounting.Domain.Entities;

public sealed class Currency
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Symbol { get; set; } = null!;
    public int DecimalPlaces { get; set; } = 2;
}
