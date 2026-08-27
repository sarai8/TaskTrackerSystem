using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerSystem.Application.Interfaces;
using TaskTrackerSystem.Infrastructure.Persistence;
using TaskTrackerSystem.Web.ViewModels;

namespace TaskTrackerSystem.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly IEmailSender _emailSender;

    public AccountController(
        UserManager<ApplicationUser> users,
        SignInManager<ApplicationUser> signIn,
        IEmailSender emailSender)
    {
        _users = users;
        _signIn = signIn;
        _emailSender = emailSender;
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel m)
    {
        if (!ModelState.IsValid)
            return View(m);

        var u = new ApplicationUser
        {
            UserName = m.UserName,
            Email = m.Email,
            Name = m.Name,
            CreatedAt = DateTime.UtcNow
        };

        var r = await _users.CreateAsync(u, m.Password);

        if (r.Succeeded)
        {
            await _signIn.SignInAsync(u, false);
            return RedirectToAction("Index", "Dashboard");
        }

        foreach (var e in r.Errors)
            ModelState.AddModelError("", e.Description);

        return View(m);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
        => View(new LoginViewModel());

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel m,
        string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(m);

        var user = await _users.FindByEmailAsync(m.Email);

        if (user == null)
        {
            ModelState.AddModelError("", "E-posta veya şifre hatalı.");
            return View(m);
        }

        var r = await _signIn.PasswordSignInAsync(user, m.Password, m.RememberMe, false);

        if (r.Succeeded)
            return Redirect(returnUrl ?? Url.Action("Index", "Dashboard")!);

        if (r.IsLockedOut)
            ModelState.AddModelError("", "Hesabınız pasif durumda. Lütfen yönetici ile iletişime geçin.");
        else
            ModelState.AddModelError("", "E-posta veya şifre hatalı.");

        return View(m);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();

        return RedirectToAction("Login");
    }



    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel m)
    {
        if (!ModelState.IsValid)
            return View(m);

        var user = await _users.FindByEmailAsync(m.Email);

        if (user == null)
        {
            return View("ForgotPasswordConfirmation");
        }

        var token = await _users.GeneratePasswordResetTokenAsync(user);

        var resetLink = Url.Action(
            "ResetPassword",
            "Account",
            new
            {
                email = user.Email,
                token = token
            },
            Request.Scheme);

        var emailBody = $@"
            <p>Merhaba {user.Name},</p>
            <p>Şifreni sıfırlamak için aşağıdaki bağlantıya tıkla:</p>
            <p><a href='{resetLink}'>Şifremi Sıfırla</a></p>
            <p>Bu isteği sen yapmadıysan bu e-postayı görmezden gelebilirsin.</p>";

        try
        {
            await _emailSender.SendAsync(user.Email!, "Şifre Sıfırlama", emailBody);
        }
        catch (Exception ex) { ModelState.AddModelError("", "E-posta gönderilemedi: " + ex.Message + (ex.InnerException != null ? " | " + ex.InnerException.Message : "")); return View(m); }

        return View("ForgotPasswordConfirmation");
    }

    [HttpGet]
    public IActionResult ResetPassword(string? email, string? token)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            return RedirectToAction("Login");

        var model = new ResetPasswordViewModel
        {
            Email = email,
            Token = token
        };

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel m)
    {
        if (!ModelState.IsValid)
            return View(m);

        var user = await _users.FindByEmailAsync(m.Email);

        if (user == null)
        {
            ModelState.AddModelError("", "Geçersiz şifre sıfırlama isteği.");
            return View(m);
        }

        var result = await _users.ResetPasswordAsync(
            user,
            m.Token,
            m.Password);

        if (result.Succeeded)
            return RedirectToAction("Login");

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return View(m);
    }
}