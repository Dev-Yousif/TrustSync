using TrustSync.Domain.Common;
using TrustSync.Domain.Entities;

namespace TrustSync.Application.Services;

public interface IReminderService
{
    Task<List<Reminder>> GetAllAsync();
    Task<Result> CreateAsync(Reminder reminder);
    Task<Result> UpdateAsync(Reminder reminder);
    Task<Result> DeleteAsync(int id);
    Task<List<Reminder>> GetDueRemindersAsync();
    Task MarkFiredAsync(int id);
    DateTime CalculateNextFireAt(Reminder reminder);
}
