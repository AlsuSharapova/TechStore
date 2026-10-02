using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechStore.Models.Entities;
using TechStore.ViewModels.Admin;

namespace TechStore.Controllers {
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : Controller{
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminUsersController(UserManager<ApplicationUser> userManager) {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index() {
            var users = _userManager.Users
                .Where(u => u.EmailConfirmed)
                .ToList();

            var userList = new List<AdminUserListItem>();

            foreach (var user in users) {
                var roles = await _userManager.GetRolesAsync(user);
                var isLockedOut = await _userManager.IsLockedOutAsync(user);

                userList.Add(new AdminUserListItem {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    CurrentRole = roles.FirstOrDefault() ?? "User",
                    IsLockedOut = isLockedOut
                });
            }

            return View(userList);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id) {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null) {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var isLockedOut = await _userManager.IsLockedOutAsync(user);

            var model = new AdminUserListItem {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                CurrentRole = roles.FirstOrDefault() ?? "User",
                IsLockedOut = isLockedOut
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(string id, string newRole) {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null) {
                return NotFound();
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Any()) {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            await _userManager.AddToRoleAsync(user, newRole);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLock(string id) {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null) {
                return NotFound();
            }

            var isLockedOut = await _userManager.IsLockedOutAsync(user);

            if (isLockedOut) {
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
            else {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }

            return RedirectToAction("Index");
        }

    }
}
