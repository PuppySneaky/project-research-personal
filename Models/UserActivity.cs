using System.ComponentModel.DataAnnotations;

namespace Film_website.Models
{
    public class UserActivity
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ActivityType { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(50)]
        public string? IpAddress { get; set; }

        [StringLength(200)]
        public string? UserAgent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? Location { get; set; }

        // Navigation property
        public User User { get; set; } = null!;
    }

    // Activity types enum for consistency
    public static class ActivityTypes
    {
        public const string Login = "Login";
        public const string Logout = "Logout";
        public const string Register = "Register";
        public const string ProfileUpdate = "Profile Update";
        public const string PasswordChange = "Password Change";
        public const string PasswordReset = "Password Reset";
        public const string AccountLocked = "Account Locked";
        public const string AccountUnlocked = "Account Unlocked";
        public const string RoleChanged = "Role Changed";
        public const string EmailConfirmed = "Email Confirmed";
        public const string DataExport = "Data Export";
        public const string AdminAccess = "Admin Access";
        public const string MovieViewed = "Movie Viewed";
        public const string PreferencesUpdated = "Preferences Updated";
    }
}