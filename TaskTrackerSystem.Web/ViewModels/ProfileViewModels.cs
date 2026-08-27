using System.ComponentModel.DataAnnotations;

namespace TaskTrackerSystem.Web.ViewModels;

public class ProfileViewModel
{
    public string Name { get; set; } = "";

    public string Email { get; set; } = "";

    public string UserName { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public string AvatarId { get; set; } = "avatar-1";

    public List<DailyTimeChartItem> TimeChart { get; set; } = new();
}

public class ProfileEditViewModel
{
    [Required]
    [Display(Name = "Ad Soyad")]
    public string Name { get; set; } = "";

    [Required]
    [EmailAddress]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$",
        ErrorMessage = "Geçerli bir e-posta adresi girin (örn: ornek@gmail.com).")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = "";

    [Required]
    [MinLength(3)]
    [RegularExpression(@"^[a-zA-Z0-9._]{3,20}$",
        ErrorMessage = "Kullanıcı adı yalnızca harf, rakam, nokta ve alt çizgi içerebilir; boşluk veya @ işareti kullanılamaz.")]
    [Display(Name = "Kullanıcı Adı")]
    public string UserName { get; set; } = "";
}

public class DailyTimeChartItem
{
    public string DateLabel { get; set; } = "";

    public double Minutes { get; set; }
}