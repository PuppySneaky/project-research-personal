using Film_website.Models;

namespace Film_website.Repositories.Interfaces
{
    public interface IUserActivityRepository
    {
        Task LogActivityAsync(string userId, string activityType, string description, string? ipAddress = null, string? userAgent = null, string? location = null);
        Task<List<UserActivity>> GetUserActivitiesAsync(string userId, int pageNumber = 1, int pageSize = 50);
        Task<List<UserActivity>> GetRecentActivitiesAsync(string userId, int count = 10);
        Task<int> GetTotalUserActivitiesCountAsync(string userId);
        Task<List<UserActivity>> GetActivitiesByTypeAsync(string userId, string activityType);
        Task<List<UserActivity>> GetActivitiesInDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
        Task DeleteOldActivitiesAsync(DateTime cutoffDate);
        Task<UserActivity?> GetActivityByIdAsync(int activityId);
    }
}