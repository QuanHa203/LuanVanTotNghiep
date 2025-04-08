using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CarServer.Databases;
using CarServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarServer.Controllers
{
    public class AccountController : Controller
    {
        private readonly PasswordHasher<AppUser> _passwordHasher = new();
        private readonly CarServerDbContext _context;

        public AccountController(CarServerDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpGet]
        public IActionResult Register() => View();

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> Login(AppUserLogin appUserLogin)
        {
            if (!ModelState.IsValid)
                return View();

            var appUser = _context.AppUsers.Include(aU => aU.IdRoleNavigation).FirstOrDefault(aU => aU.UserName == appUserLogin.UserNameOrEmail.Trim() || aU.Email.Trim() == appUserLogin.UserNameOrEmail.Trim());

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
                new Claim(ClaimTypes.Role, appUser.IdRoleNavigation.RoleName)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {

            return View();
        }

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
    }
}
