using Film_website.Models;
using Film_website.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Film_website.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public UserRepository(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IdentityResult> CreateUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<User?> FindByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<User?> FindByUserNameAsync(string userName)
        {
            return await _userManager.Users
                .FirstOrDefaultAsync(u => u.DisplayUserName == userName);
        }

        public async Task<User?> FindByEmailOrUserNameAsync(string emailOrUserName)
        {
            // First try to find by email
            var user = await _userManager.FindByEmailAsync(emailOrUserName);

            // If not found by email, try to find by custom username
            if (user == null)
            {
                user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.DisplayUserName == emailOrUserName);
            }

            return user;
        }

        public async Task<SignInResult> PasswordSignInAsync(string userNameOrEmail, string password, bool rememberMe)
        {
            // Find user by email or username
            var user = await FindByEmailOrUserNameAsync(userNameOrEmail);

            if (user == null)
            {
                return SignInResult.Failed;
            }

            // Use the actual UserName (which is the email) for SignIn
            return await _signInManager.PasswordSignInAsync(user.UserName!, password, rememberMe, lockoutOnFailure: false);
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> AddToRoleAsync(User user, string role)
        {
            return await _userManager.AddToRoleAsync(user, role);
        }

        public async Task<IList<string>> GetRolesAsync(User user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<bool> IsUserNameTakenAsync(string userName)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.DisplayUserName == userName);
            return user != null;
        }

        public async Task<bool> IsEmailTakenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }

        // Forgot password functionality
        public async Task<User?> FindByEmailAndUserNameAsync(string email, string userName)
        {
            return await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.DisplayUserName == userName);
        }

        public async Task<IdentityResult> ResetPasswordAsync(User user, string newPassword)
        {
            // Remove current password and set new one
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        // NEW METHODS: Get all users for admin management
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _userManager.Users
                .OrderBy(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<User>> GetAllUsersWithRolesAsync()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.CreatedAt)
                .ToListAsync();

            return users;
        }
    }
}