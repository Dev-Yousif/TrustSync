using TrustSync.Application.DTOs;
using TrustSync.Domain.Common;

namespace TrustSync.Application.Services;

public interface ICurrencyConversionService
{
    Task<Result<ConversionResult>> ConvertToDefaultAsync(decimal amount, string fromCurrencyCode);
    Task<Result> RefreshRatesAsync();
    Task<DateTime?> GetLastRefreshTimeAsync();
    Task<Result> ReconvertAllRecordsAsync();
}
