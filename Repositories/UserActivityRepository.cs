using Film_website.Data;
using Film_website.Models;
using Film_website.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Film_website.Repositories
{
    public class UserActivityRepository : IUserActivityRepository
    {
        private readonly ApplicationDbContext _context;

        public UserActivityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogActivityAsync(string userId, string activityType, string description, string? ipAddress = null, string? userAgent = null, string? location = null)
        {
            var activity = new UserActivity
            {
                UserId = userId,
                ActivityType = activityType,
                Description = description,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Location = location,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserActivities.Add(activity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserActivity>> GetUserActivitiesAsync(string userId, int pageNumber = 1, int pageSize = 50)
        {
            return await _context.UserActivities
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(a => a.User)
                .ToListAsync();
        }

        public async Task<List<UserActivity>> GetRecentActivitiesAsync(string userId, int count = 10)
        {
            return await _context.UserActivities
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .Include(a => a.User)
                .ToListAsync();
        }

        public async Task<int> GetTotalUserActivitiesCountAsync(string userId)
        {
            return await _context.UserActivities
                .Where(a => a.UserId == userId)
                .CountAsync();
        }

        public async Task<List<UserActivity>> GetActivitiesByTypeAsync(string userId, string activityType)
        {
            return await _context.UserActivities
                .Where(a => a.UserId == userId && a.ActivityType == activityType)
                .OrderByDescending(a => a.CreatedAt)
                .Include(a => a.User)
                .ToListAsync();
        }

        public async Task<List<UserActivity>> GetActivitiesInDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
        {
            return await _context.UserActivities
                .Where(a => a.UserId == userId && a.CreatedAt >= startDate && a.CreatedAt <= endDate)
                .OrderByDescending(a => a.CreatedAt)
                .Include(a => a.User)
                .ToListAsync();
        }

        public async Task DeleteOldActivitiesAsync(DateTime cutoffDate)
        {
            var oldActivities = await _context.UserActivities
                .Where(a => a.CreatedAt < cutoffDate)
                .ToListAsync();

            if (oldActivities.Any())
            {
                _context.UserActivities.RemoveRange(oldActivities);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<UserActivity?> GetActivityByIdAsync(int activityId)
        {
            return await _context.UserActivities
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == activityId);
        }
    }
}