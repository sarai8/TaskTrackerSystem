using System.ComponentModel.DataAnnotations;

namespace TaskTrackerSystem.Web.ViewModels;

public class RegisterViewModel
{
    [Required]
    [Display(Name = "Ad Soyad")]
    public string Name { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    [MinLength(3)]
    [RegularExpression(@"^[a-zA-Z0-9._]{3,20}$",
    ErrorMessage = "Kullanıcı adı yalnızca harf, rakam, nokta ve alt çizgi içerebilir; boşluk veya @ işareti kullanılamaz.")]
    [Display(Name = "Kullanıcı Adı")]
    public string UserName { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    [MinLength(6)]
    public string Password { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = "";
}

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public bool RememberMe { get; set; }
}
public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";
}

public class ResetPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Token { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    [MinLength(6)]
    public string Password { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = "";
}