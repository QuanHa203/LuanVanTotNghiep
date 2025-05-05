using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CarServer.Databases;
using CarServer.Models;
using CarServer.Services.Email;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarServer.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly PasswordHasher<AppUser> _passwordHasher = new();
    private readonly CarServerDbContext _context;
    private readonly IEmailService _emailService;

    public AccountController(CarServerDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpGet]
    public IActionResult Register() => View();

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpGet]
    public IActionResult ResetPassword(string resetPasswordToken)
        => View(new ResetPasswordModel
        {
            Token = resetPasswordToken
        });


    [HttpPost]
    public async Task<IActionResult> Login(AppUserLogin appUserLogin)
    {
        if (!ModelState.IsValid)
            return View();

        var appUser = _context.AppUsers.Include(aU => aU.Role).FirstOrDefault(aU => aU.UserName == appUserLogin.UserNameOrEmail.Trim() || aU.Email.Trim() == appUserLogin.UserNameOrEmail.Trim());

        if (appUser == null)
        {
            ModelState.AddModelError(nameof(appUserLogin.UserNameOrEmail), "User name hoặc email không tồn tại");
            return View();
        }

        var result = _passwordHasher.VerifyHashedPassword(appUser, appUser.PasswordHash, appUserLogin.Password);
        if (result != PasswordVerificationResult.Success)
        {
            ModelState.AddModelError(nameof(appUserLogin.Password), "Mật khẩu không chính xác");
            return View();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, appUser.UserName),
            new Claim(ClaimTypes.Email, appUser.Email),
            new Claim(ClaimTypes.Role, appUser.Role.RoleName)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        AuthenticationProperties authenticationProperties = new AuthenticationProperties()
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(20)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authenticationProperties);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Register(AppUserRegister appUserRegister)
    {
        if (!ModelState.IsValid)
            return View();

        if (await _context.AppUsers.FirstOrDefaultAsync(appUser => appUser.Email == appUserRegister.Email) != null)
        {
            ModelState.AddModelError(nameof(appUserRegister.Email), "Email đã tồn tại");
            return View();
        }

        if (await _context.AppUsers.FirstOrDefaultAsync(appUser => appUser.UserName == appUserRegister.UserName) != null)
        {
            ModelState.AddModelError(nameof(appUserRegister.UserName), "User name đã tồn tại");
            return View();
        }

        string hashedPassword = _passwordHasher.HashPassword(null!, appUserRegister.Password);
        AppUser appUser = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = appUserRegister.UserName,
            Email = appUserRegister.Email.ToLower(),
            PasswordHash = hashedPassword,
            IdRole = 2
        };

        _context.AppUsers.Add(appUser);
        _context.SaveChanges();
        TempData["SuccessMessage"] = "Đăng ký tài khoản thành công";
        return RedirectToAction(nameof(ResultForm));
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        var appUser = _context.AppUsers.FirstOrDefault(aU => aU.Email == email);
        if (appUser == null)
        {
            ModelState.AddModelError("email", "Email không tồn tại");
            return View();
        }

        string resetPasswordToken = Guid.NewGuid().ToString();
        DateTime expiry = DateTime.UtcNow.AddHours(1);            

        appUser.PasswordResetToken = resetPasswordToken;
        appUser.PasswordResetTokenExpiry = expiry;

        _context.AppUsers.Update(appUser);
        await _context.SaveChangesAsync();

        string resetLink = Url.Action(nameof(ResetPassword), "Account", new { resetPasswordToken = resetPasswordToken }, Request.Scheme)!;

        string html = $"<p>Nhấn vào <a href='{resetLink}'>đây</a> để đặt lại mật khẩu. Link này sẽ hết hạn sau 1 giờ.</p>";
        _ = _emailService.SendEmailAsync(email, "Reset mật khẩu", html);
        TempData["SuccessMessage"] = "Đã gửi mail reset mật khẩu, vui lòng vào hòm thư để reset mật khẩu!";
        return RedirectToAction(nameof(ResultForm));
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel resetPasswordModel)
    {
        if (!ModelState.IsValid)
            return View();

        if (resetPasswordModel.Token == null)
        {
            ModelState.AddModelError("", "Token không hợp lệ hoặc đã hết hạn");
            return View();
        }

        var appUser = _context.AppUsers.FirstOrDefault(aU => aU.PasswordResetToken == resetPasswordModel.Token && aU.PasswordResetTokenExpiry > DateTime.UtcNow);

        if (appUser == null)
        {
            ModelState.AddModelError("", "Token không hợp lệ hoặc đã hết hạn");
            return View();
        }

        appUser.PasswordResetToken = null;
        appUser.PasswordHash = _passwordHasher.HashPassword(null!, resetPasswordModel.NewPassword);
        _context.AppUsers.Update(appUser);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Reset mật khẩu thành công!";
        return RedirectToAction(nameof(ResultForm));
    }

    public IActionResult ResultForm() => View();

    public class AppUserLogin
    {
        [Required(ErrorMessage = "{0} phải nhập")]
        [Display(Name = "Tên đăng nhập hoặc mật khẩu")]
        public string UserNameOrEmail { get; set; } = null!;

        [Required(ErrorMessage = "{0} phải nhập")]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = null!;
    }

    public class AppUserRegister
    {
        [Required(ErrorMessage = "{0} phải nhập")]
        [Display(Name = "Tên đăng nhập")]
        [StringLength(maximumLength: 50, MinimumLength = 3, ErrorMessage = "{0} dài từ {2} - {1}")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "{0} phải nhập")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "{0} phải nhập")]
        [Display(Name = "Mật khẩu")]
        [StringLength(maximumLength: 50, MinimumLength = 3, ErrorMessage = "{0} dài từ {2} ký tự")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "{0} phải nhập")]
        [Display(Name = "Mật khẩu lặp lại")]
        [Compare("Password", ErrorMessage = "{0} không trùng khớp")]
        public string ReEnterPassword { get; set; } = null!;
    }

    public class ResetPasswordModel
    {
        public string Token { get; set; } = null!;

        [Required(ErrorMessage = "{0} phải nhập")]
        [Display(Name = "Mật khẩu")]
        [StringLength(maximumLength: 50, MinimumLength = 3, ErrorMessage = "{0} dài từ {2} ký tự")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "{0} phải nhập")]
        [Display(Name = "Mật khẩu lặp lại")]
        [Compare("NewPassword", ErrorMessage = "{0} không trùng khớp")]
        public string ReEnterNewPassword { get; set; } = null!;
    }
}
