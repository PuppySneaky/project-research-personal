using Film_website.Models;
using Film_website.Repositories.Interfaces;

namespace Film_website.Services
{
    public class UserActivityService
    {
        private readonly IUserActivityRepository _activityRepository;
        private readonly ILogger<UserActivityService> _logger;

        public UserActivityService(IUserActivityRepository activityRepository, ILogger<UserActivityService> logger)
        {
            _activityRepository = activityRepository;
            _logger = logger;
        }

        public async Task LogActivityAsync(string userId, string activityType, string description, HttpContext? httpContext = null)
        {
            try
            {
                string? ipAddress = null;
                string? userAgent = null;

                if (httpContext != null)
                {
                    ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                    userAgent = httpContext.Request.Headers.UserAgent.ToString();
                }

                await _activityRepository.LogActivityAsync(userId, activityType, description, ipAddress, userAgent);
                _logger.LogInformation($"Activity logged: {activityType} for user {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to log activity {activityType} for user {userId}");
            }
        }

        public async Task<List<UserActivity>> GetUserActivitiesAsync(string userId, int pageNumber = 1, int pageSize = 50)
        {
            return await _activityRepository.GetUserActivitiesAsync(userId, pageNumber, pageSize);
        }

        public async Task<List<UserActivity>> GetRecentActivitiesAsync(string userId, int count = 10)
        {
            return await _activityRepository.GetRecentActivitiesAsync(userId, count);
        }

        public async Task<int> GetTotalUserActivitiesCountAsync(string userId)
        {
            return await _activityRepository.GetTotalUserActivitiesCountAsync(userId);
        }

        public async Task<List<UserActivity>> GetActivitiesByTypeAsync(string userId, string activityType)
        {
            return await _activityRepository.GetActivitiesByTypeAsync(userId, activityType);
        }

        public async Task<List<UserActivity>> GetActivitiesInDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
        {
            return await _activityRepository.GetActivitiesInDateRangeAsync(userId, startDate, endDate);
        }

        // Helper methods for common activities
        public async Task LogLoginAsync(string userId, HttpContext httpContext)
        {
            await LogActivityAsync(userId, ActivityTypes.Login, "User logged in successfully", httpContext);
        }

        public async Task LogLogoutAsync(string userId, HttpContext httpContext)
        {
            await LogActivityAsync(userId, ActivityTypes.Logout, "User logged out", httpContext);
        }

        public async Task LogRegistrationAsync(string userId, HttpContext httpContext)
        {
            await LogActivityAsync(userId, ActivityTypes.Register, "User account created", httpContext);
        }

        public async Task LogProfileUpdateAsync(string userId, HttpContext httpContext)
        {
            await LogActivityAsync(userId, ActivityTypes.ProfileUpdate, "User profile updated", httpContext);
        }

        public async Task LogPasswordChangeAsync(string userId, HttpContext httpContext)
        {
            await LogActivityAsync(userId, ActivityTypes.PasswordChange, "User password changed", httpContext);
        }

        public async Task LogPasswordResetAsync(string userId, HttpContext httpContext)
        {
            await LogActivityAsync(userId, ActivityTypes.PasswordReset, "User password reset", httpContext);
        }

        public async Task LogRoleChangeAsync(string userId, string oldRole, string newRole, HttpContext httpContext)
        {
            await LogActivityAsync(userId, ActivityTypes.RoleChanged, $"User role changed from {oldRole} to {newRole}", httpContext);
        }

        public async Task LogAdminAccessAsync(string userId, string adminAction, HttpContext httpContext)
        {
            await LogActivityAsync(userId, ActivityTypes.AdminAccess, $"Admin action: {adminAction}", httpContext);
        }

        public async Task CleanupOldActivitiesAsync(int daysToKeep = 90)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
            await _activityRepository.DeleteOldActivitiesAsync(cutoffDate);
            _logger.LogInformation($"Cleaned up activities older than {daysToKeep} days");
        }
    }
}