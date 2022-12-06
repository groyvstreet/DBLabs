using BankSystem.Data;
using BankSystem.Models.UserModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using BankSystem.Models.Entities;

namespace BankSystem.Controllers
{
    public class UserController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var command = DbConnection.getCommand();
                command.CommandText = $"select users.email, roles.name as role from users inner join roles " +
                    $"on roles.id = users.role_id where email = '{model.Email}' and password = '{model.Password}'";
                var dataReader = command.ExecuteReader();

                if (dataReader.Read())
                {
                    var email = dataReader.GetValue(dataReader.GetOrdinal("email")).ToString()!;
                    var role = dataReader.GetValue(dataReader.GetOrdinal("role")).ToString()!;
                    await Authenticate(email, role);
                    return RedirectToAction("Profile", "User");
                }

                ModelState.AddModelError("", "Некорректные логин и(или) пароль");
            }
            return View(model);
        }

        [Authorize]
        public IActionResult Profile()
        {
            var role = "";

            if (User.IsInRole("admin"))
            {
                role = "admin";
            } else if (User.IsInRole("client"))
            {
                role = "client";
            } else if (User.IsInRole("manager"))
            {
                role = "manager";
            } else if (User.IsInRole("moderator"))
            {
                role = "moderator";
            }

            switch (role)
            {
                case "admin":
                    return RedirectToAction("Profile", "Admin");
                case "client":
                    return RedirectToAction("Profile", "Client");
                case "manager":
                    return RedirectToAction("Profile", "Manager");
                case "moderator":
                    return RedirectToAction("Profile", "Moderator");
            }

            return View();
        }

        private async Task Authenticate(string email, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, email),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, role)
            };

            var id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Banks", "Bank");
        }

        public IActionResult Actions()
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select date_time, action from logs";
            var dataReader = command.ExecuteReader();

            var actions = new List<Models.Entities.Action>();

            while (dataReader.Read())
            {
                var action = new Models.Entities.Action
                {
                    DateTime = dataReader.GetValue(dataReader.GetOrdinal("date_time")).ToString()!,
                    Text = dataReader.GetValue(dataReader.GetOrdinal("action")).ToString()!
                };

                actions.Add(action);
            }

            return View(actions);
        }
    }
}
