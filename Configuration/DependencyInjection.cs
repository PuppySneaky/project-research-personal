using Film_website.Data;
using Film_website.Models;
using Film_website.Repositories;
using Film_website.Repositories.Interfaces;
using Film_website.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Film_website.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Thêm Entity Framework
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Thêm Identity
            services.AddIdentity<User, IdentityRole>(options =>
            {
                // Cài đặt mật khẩu
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;

                // Cài đặt user
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Cấu hình cookie
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            // Đăng ký repositories
            services.AddScoped<IUserRepository, UserRepository>();

            // Đăng ký services
            services.AddScoped<UserService>();

            return services;
        }
    }
}