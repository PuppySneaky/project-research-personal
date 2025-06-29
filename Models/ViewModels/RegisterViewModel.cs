using System.ComponentModel.DataAnnotations;

namespace Film_website.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Họ là bắt buộc")]
        [StringLength(50, ErrorMessage = "Họ không được quá 50 ký tự")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên là bắt buộc")]
        [StringLength(50, ErrorMessage = "Tên không được quá 50 ký tự")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên người dùng là bắt buộc")]
        [StringLength(30, ErrorMessage = "Tên người dùng phải từ {2} đến {1} ký tự", MinimumLength = 3)]
        [RegularExpression("^[a-zA-Z0-9_]+$", ErrorMessage = "Tên người dùng chỉ được chứa chữ cái, số và dấu gạch dưới")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}