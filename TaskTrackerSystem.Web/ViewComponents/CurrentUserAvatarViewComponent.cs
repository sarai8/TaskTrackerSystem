using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerSystem.Infrastructure.Persistence;
using TaskTrackerSystem.Web.Common;

namespace TaskTrackerSystem.Web.ViewComponents;

public class CurrentUserAvatarViewComponent : ViewComponent
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CurrentUserAvatarViewComponent(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        var avatar = AvatarCatalog.GetOrDefault(user?.AvatarId);

        return View(avatar);
    }
}