using Film_website.Models;
using Microsoft.AspNetCore.Identity;

namespace Film_website.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<IdentityResult> CreateUserAsync(User user, string password);
        Task<User?> FindByEmailAsync(string email);
        Task<User?> FindByUserNameAsync(string userName);
        Task<User?> FindByEmailOrUserNameAsync(string emailOrUserName);
        Task<SignInResult> PasswordSignInAsync(string userNameOrEmail, string password, bool rememberMe);
        Task SignOutAsync();
        Task<IdentityResult> AddToRoleAsync(User user, string role);
        Task<IList<string>> GetRolesAsync(User user);
        Task<bool> IsUserNameTakenAsync(string userName);
        Task<bool> IsEmailTakenAsync(string email);

        // New methods for forgot password functionality
        Task<User?> FindByEmailAndUserNameAsync(string email, string userName);
        Task<IdentityResult> ResetPasswordAsync(User user, string newPassword);

        // NEW METHOD: Get all users for admin management
        Task<List<User>> GetAllUsersAsync();
        Task<List<User>> GetAllUsersWithRolesAsync();
    }
}