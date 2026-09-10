using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechStore.Models.Entities;
using TechStore.ViewModels.Account;
using TechStore.Services;

namespace TechStore.Controllers {
    public class AccountController : Controller {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender) {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
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
                    return RedirectToAction("RegisterConfirmation");
                }

                if (existingUser != null && existingUser.EmailConfirmed) {
                    ModelState.AddModelError(string.Empty, "Этот email уже зарегистрирован. Попробуйте войти.");
                    return View(model);
                }

                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded) {
                    await SendConfirmationEmail(user);
                    return View("InfoMessage", new InfoMessageViewModel {
                        Icon = "📧",
                        Title = "Проверьте почту",
                        Message = "Мы отправили письмо с подтверждением на вашу почту. Перейдите по ссылке в письме, чтобы завершить регистрацию.",
                        ButtonText = "Перейти ко входу",
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
                TempData["Message"] = "Email подтверждён! Теперь вы можете войти.";
            }
            else {
                TempData["Message"] = "Не удалось подтвердить email. Ссылка недействительна или устарела.";
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

                    var emailBody = $"<h3>Сброс пароля — TechStore</h3>" +
                                     $"<p>Вы запросили сброс пароля. Перейдите по ссылке, чтобы задать новый пароль:</p>" +
                                     $"<a href='{resetLink}'>Сбросить пароль</a>";

                    await _emailSender.SendEmailAsync(user.Email!, "Сброс пароля — TechStore", emailBody);
                }

                return View("InfoMessage", new InfoMessageViewModel {
                    Icon = "📧",
                    Title = "Проверьте почту",
                    Message = "Если такой email зарегистрирован у нас, мы отправили на него ссылку для сброса пароля.",
                    ButtonText = "Вернуться ко входу",
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
                        Title = "Пароль изменён",
                        Message = "Ваш пароль успешно изменён. Теперь вы можете войти с новым паролем.",
                        ButtonText = "Перейти ко входу",
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

            var emailBody = $"<h3>Добро пожаловать в TechStore!</h3>" +
                             $"<p>Подтвердите ваш email, перейдя по ссылке:</p>" +
                             $"<a href='{confirmationLink}'>Подтвердить email</a>";

            await _emailSender.SendEmailAsync(user.Email!, "Подтверждение регистрации — TechStore", emailBody);
        }
    }
}
