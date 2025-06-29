using System.ComponentModel.DataAnnotations;

namespace Film_website.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email hoặc tên người dùng là bắt buộc")]
        [Display(Name = "Email hoặc tên người dùng")]
        public string EmailOrUserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }
    }
}