using YousifAccounting.Application.DTOs;
using YousifAccounting.Domain.Common;

namespace YousifAccounting.Application.Services;

public interface ICurrencyConversionService
{
    Task<Result<ConversionResult>> ConvertToDefaultAsync(decimal amount, string fromCurrencyCode);
    Task<Result> RefreshRatesAsync();
    Task<DateTime?> GetLastRefreshTimeAsync();
    Task<Result> ReconvertAllRecordsAsync();
}
