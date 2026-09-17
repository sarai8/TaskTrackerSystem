using System.ComponentModel.DataAnnotations;
using TaskTrackerSystem.Domain.Constants;


namespace TaskTrackerSystem.Web.ViewModels;

public class AdminUserListItemViewModel
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string UserName { get; set; } = "";
    public int? DepartmentId { get; set; }
    public string DepartmentName { get; set; } = "";
    public string Position { get; set; } = "";
    public bool IsManager { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class AdminUserEditViewModel
{
    public string Id { get; set; } = "";

    [Required]
    [Display(Name = "Ad Soyad")]
    public string Name { get; set; } = "";

    [Required]
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = "";

    [Required]
    [MinLength(3)]
    [Display(Name = "Kullanıcı Adı")]
    public string UserName { get; set; } = "";

    public int? DepartmentId { get; set; }

    public string Role { get; set; } = Roles.Employee;

    public string DepartmentName { get; set; } = "Departman yok";

    public string Position { get; set; } = "Çalışan";


}

public class AdminUserCreateViewModel
{
    [Required]
    [Display(Name = "Ad Soyad")]
    public string Name { get; set; } = "";

    [Required]
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = "";

    [Required]
    [MinLength(3)]
    [Display(Name = "Kullanıcı Adı")]
    public string UserName { get; set; } = "";

    [Required]
    [MinLength(6)]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password")]
    [Display(Name = "Şifre Tekrar")]
    public string ConfirmPassword { get; set; } = "";

    public int? DepartmentId { get; set; }

    public string Role { get; set; } = Roles.Employee;
}


public class AdminChangePasswordViewModel
{
    public string UserId { get; set; } = "";

    public string UserName { get; set; } = "";

    [Required(ErrorMessage = "Yeni şifre zorunludur.")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    [Display(Name = "Yeni Şifre")]
    public string NewPassword { get; set; } = "";

    [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor.")]
    [Display(Name = "Yeni Şifre Tekrar")]
    public string ConfirmPassword { get; set; } = "";
}