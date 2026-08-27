using System.ComponentModel.DataAnnotations;

namespace TaskTrackerSystem.Web.ViewModels;

public class AdminUserListItemViewModel
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string UserName { get; set; } = "";
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


}