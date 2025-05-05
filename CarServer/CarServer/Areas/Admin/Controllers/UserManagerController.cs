using CarServer.Models;
using CarServer.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CarServer.Areas.Admin.Controllers;

[Authorize(Roles = "Admin")]
[Area("Admin")]
public class UserManagerController : Controller
{
    private readonly IGenericRepository<AppUser> _appUserRepository;
    private readonly IGenericRepository<Role> _roleRepository;
    private readonly PasswordHasher<AppUser> _passwordHasher = new();

    public UserManagerController(IGenericRepository<AppUser> appUserRepository, IGenericRepository<Role> roleRepository)
    {
        _appUserRepository = appUserRepository;
        _roleRepository = roleRepository;
    }

    public async Task<IActionResult> Index()
    {
        var appUsers = await _appUserRepository.GetDbSet().Include(aU => aU.Role).ToListAsync();
        return View(appUsers);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        if (Guid.Empty == id)
            return NotFound();

        AppUser? appUser = await _appUserRepository.GetByIdAsync(id);
        if (appUser == null)
            return NotFound();
        AppUserModel model = new AppUserModel()
        {
            Id = id,
            UserName = appUser.UserName,
            Email = appUser.Email,
            IdRole = appUser.IdRole
        };
        ViewData["Roles"] = await _roleRepository.GetAllAsync();
        return View("Edit", model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(AppUserModel appUserModel)
    {
        if (ModelState.IsValid)
        {
            var appUser = await _appUserRepository.GetByIdAsync(appUserModel.Id);
            if (appUser == null)
                goto endFunction;

            appUser.UserName = appUserModel.UserName;
            appUser.Email = appUserModel.Email;
            appUser.IdRole = appUserModel.IdRole;
            if (appUserModel.PasswordReset != null)
            {
                string password = _passwordHasher.HashPassword(appUser, appUserModel.PasswordReset);
                appUser.PasswordHash = password;
            }

            await _appUserRepository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    endFunction:
        ViewData["Roles"] = await _roleRepository.GetAllAsync();
        return View(appUserModel);
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        if (Guid.Empty == id)
            return BadRequest();

        var appUser = await _appUserRepository.GetByIdAsync(id);
        if (appUser == null)
            return BadRequest();

        _appUserRepository.Delete(appUser);
        await _appUserRepository.SaveChangesAsync();
        return Ok();
    }

    public class AppUserModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public int IdRole { get; set; }

        [Required(ErrorMessage = "{0} phải nhập")]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "{0} phải nhập")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Display(Name = "Mật khẩu reset")]
        public string? PasswordReset { get; set; } = null!;

        [Display(Name = "Nhập lại mật khẩu reset")]
        [Compare("PasswordReset", ErrorMessage = "Không trùng khớp với mật khẩu reset")]
        public string? ReEnterPasswordReset { get; set; } = null!;
    }
}
