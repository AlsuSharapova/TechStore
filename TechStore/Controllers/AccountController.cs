using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechStore.Models.Entities;
using TechStore.ViewModels.Account;
using System.Security.Claims;

namespace TechStore.Controllers {
    public class AccountController : Controller {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model) {
            if (ModelState.IsValid) {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded) {
                    var claims = new List<Claim>
                    {
                        new Claim("FullName", user.FullName)
                    };

                    await _signInManager.SignInWithClaimsAsync(user, isPersistent: false, claims);
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors) {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login() {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model) {
            if (ModelState.IsValid) {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null) {
                    var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

                    if (passwordCheck.Succeeded) {
                        var claims = new List<System.Security.Claims.Claim>
                        {
                    new System.Security.Claims.Claim("FullName", user.FullName)
                };

                        await _signInManager.SignInWithClaimsAsync(user, model.RememberMe, claims);
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError(string.Empty, "Неверный email или пароль");
            }

            return View(model);
        }
    }
}
