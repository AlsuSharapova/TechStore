using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechStore.Models.Entities;
using TechStore.ViewModels.Account;
using TechStore.Services;
using TechStore.ViewModels;
using Microsoft.Extensions.Localization;
using TechStore.Resources;

namespace TechStore.Controllers {
    public class AccountController : Controller {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IStringLocalizer<SharedResource> _localizer;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, IStringLocalizer<SharedResource> localizer) {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _localizer = localizer;
        }

        [HttpGet]
        public IActionResult Register() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model) {
            if (ModelState.IsValid) {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);

                if (existingUser != null && !existingUser.EmailConfirmed) {
                    await SendConfirmationEmail(existingUser);
                    return CheckEmailMessage();
                }

                if (existingUser != null && existingUser.EmailConfirmed) {
                    ModelState.AddModelError(string.Empty, _localizer["EmailAlreadyRegistered"].Value);
                    return View(model);
                }

                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded) {
                    await _userManager.AddToRoleAsync(user, "User");
                    await SendConfirmationEmail(user);
                    return CheckEmailMessage();
                }

                foreach (var error in result.Errors) {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token) {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token)) {
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) {
                return RedirectToAction("Login");
            }

            var decodedToken = System.Net.WebUtility.UrlDecode(token);
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (result.Succeeded) {
                TempData["Message"] = _localizer["EmailConfirmed"].Value;
            }
            else {
                TempData["Message"] = _localizer["EmailConfirmFailed"].Value;
            }

            return RedirectToAction("Login");
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
                    var isLockedOut = await _userManager.IsLockedOutAsync(user);

                    if (isLockedOut) {
                        ModelState.AddModelError(string.Empty, _localizer["AccountBlocked"].Value);
                        return View(model);
                    }

                    var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);

                    if (passwordCheck.Succeeded) {
                        var claims = new List<System.Security.Claims.Claim>{
                            new System.Security.Claims.Claim("FullName", user.FullName)
                        };

                        await _signInManager.SignInWithClaimsAsync(user, model.RememberMe, claims);
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError(string.Empty, _localizer["InvalidCredentials"].Value);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout() {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult VerifyEmail() {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model) {
            if (ModelState.IsValid) {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null && user.EmailConfirmed) {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var encodedToken = System.Net.WebUtility.UrlEncode(token);

                    var resetLink = Url.Action(
                        "ResetPassword", "Account",
                        new { userId = user.Id, token = encodedToken },
                        protocol: Request.Scheme);

                    var emailBody = $"<h3>{_localizer["ResetEmailSubject"].Value}</h3>" +
                        $"<p>{_localizer["ResetEmailIntro"].Value}</p>" +
                        $"<a href='{resetLink}'>{_localizer["ResetEmailLink"].Value}</a>";

                    await _emailSender.SendEmailAsync(user.Email!, _localizer["ResetEmailSubject"].Value, emailBody);
                }

                return View("InfoMessage", new InfoMessageViewModel {
                    Icon = "📧",
                    Title = _localizer["CheckEmail"].Value,
                    Message = _localizer["ResetEmailSentText"].Value,
                    ButtonText = _localizer["BackToLogin"].Value,
                    ButtonController = "Account",
                    ButtonAction = "Login"
                });
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string userId, string token) {

            if( string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token)) {
                return RedirectToAction("Login");
            }
            var model = new ResetPasswordViewModel { UserId = userId, Token = token };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model) {
            if (ModelState.IsValid) {
                var user = await _userManager.FindByIdAsync(model.UserId);

                if (user == null) {
                    return RedirectToAction("Login");
                }

                var decodedToken = System.Net.WebUtility.UrlDecode(model.Token);
                var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.Password);

                if (result.Succeeded) {
                    return View("InfoMessage", new InfoMessageViewModel {
                        Icon = "✅",
                        Title = _localizer["PasswordChanged"].Value,
                        Message = _localizer["PasswordChangedText"].Value,
                        ButtonText = _localizer["GoToLogin"].Value,
                        ButtonController = "Account",
                        ButtonAction = "Login"
                    });
                }

                foreach (var error in result.Errors) {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        //HELPERS
        private async Task SendConfirmationEmail(ApplicationUser user) {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = System.Net.WebUtility.UrlEncode(token);

            var confirmationLink = Url.Action(
                "ConfirmEmail", "Account",
                new { userId = user.Id, token = encodedToken },
                protocol: Request.Scheme);

            var emailBody = $"<h3>{_localizer["ConfirmEmailTitle"].Value}</h3>" +
                 $"<p>{_localizer["ConfirmEmailIntro"].Value}</p>" +
                 $"<a href='{confirmationLink}'>{_localizer["ConfirmEmailLink"].Value}</a>";

            await _emailSender.SendEmailAsync(user.Email!, _localizer["ConfirmEmailSubject"].Value, emailBody);
        }
        private IActionResult CheckEmailMessage() {
            return View("InfoMessage", new InfoMessageViewModel {
                Icon = "📧",
                Title = _localizer["CheckEmail"].Value,
                Message = _localizer["RegisterConfirmationText"].Value,
                ButtonText = _localizer["GoToLogin"].Value,
                ButtonController = "Account",
                ButtonAction = "Login"
            });
        }
    }
}
