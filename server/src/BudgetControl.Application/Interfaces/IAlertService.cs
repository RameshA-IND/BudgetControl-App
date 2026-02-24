using BudgetControl.Application.DTOs.Alert;

namespace BudgetControl.Application.Interfaces;

public interface IAlertService
{
    Task CheckAndCreateAlertsAsync(int budgetId);
    Task<IEnumerable<AlertResponseDto>> GetAllAsync();
    Task<IEnumerable<AlertResponseDto>> GetUnreadAsync();
    Task MarkAsReadAsync(int alertId);
    Task<int> GetUnreadCountAsync();
}
