using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Infrastructure.Data.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Notification log)
    {
        await _context.NotificationLogs.AddAsync(log);
    }

    public async Task SaveChangeAsync()
    {
        await _context.SaveChangesAsync();
    }
}