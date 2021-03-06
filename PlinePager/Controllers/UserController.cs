using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlinePager.Data;
using PlinePager.Models;
using PlinePager.Models.Users;

namespace PlineFaxServer.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly PlinePagerContext _context;
        private readonly SignInManager<TblUser> _signInManager;
        private readonly UserManager<TblUser> _userManage;
        private readonly INotyfService _notifyService;

        public UserController(PlinePagerContext context, UserManager<TblUser> userManager,
            SignInManager<TblUser> signInManager, INotyfService notifyService)
        {
            _context = context;
            _userManage = userManager;
            _signInManager = signInManager;
            _notifyService = notifyService;
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_Index", _context.Set<TblUser>());
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View(new UserManagementCreate()
            {
                UserChanePassword = true
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(
            [Bind("FirstName,LastName,Username,Password,RepPassword,Department,UserChanePassword")]
            UserManagementCreate userManagement)
        {
            if (ModelState.IsValid)
            {
                var cnt = await _userManage.Users
                    .CountAsync(m => m.UserName == userManagement.Username);
                if (cnt > 0)
                {
                    ModelState.AddModelError("Username", "?????? ?????? ???????????? ???????? ?????????????? ?????? ??????.");
                    return View(userManagement);
                }

                if (!string.Equals(userManagement.Password.Trim(), userManagement.RepPassword.Trim(),
                    StringComparison.CurrentCultureIgnoreCase))
                {
                    ModelState.AddModelError("RepPassword", "???????? ???????? ???? ?????????? ???? ???????????? ??????????.");
                    return View(userManagement);
                }

                try
                {
                    var tblUser = new TblUser()
                    {
                        FirstName = userManagement.FirstName,
                        LastName = userManagement.LastName,
                        UserName = userManagement.Username,
                        Email = $"{userManagement.Username}@localhost.local",
                        Department = userManagement.Department,
                        UserChanePassword = userManagement.UserChanePassword
                    };
                    var resultUser = _userManage.CreateAsync(tblUser, userManagement.Password);
                    if (resultUser.Result != IdentityResult.Success)
                    {
                        var errors = UserController.TranslateError();
                        ModelState.AddModelError("", "?????? ???? ?????????? ??????????");
                        foreach (var item in resultUser.Result.Errors)
                        {
                            var e = from string[] val in errors
                                    where (item.Description.Contains(val[0]))
                                    select new string[] { val[1] };

                            if (e.Any())
                            {
                                ModelState.AddModelError("", e.ToArray()[0][0]);
                            }
                        }

                        return View(userManagement);
                    }

                    await _userManage.AddClaimAsync(tblUser, new Claim(ClaimTypes.Role, "User"));
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return View(userManagement);
                }
            }

            return View(userManagement);
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(string id)
        {
            var model = await _userManage.FindByIdAsync(id);
            if (model == null)
            {
                _notifyService.Error("?????????? ???????? ?????? ???????? ??????.");
                return RedirectToAction("Index", "User");
            }

            return View(new UserManagementEdit()
            {
                Id = id,
                Department = model.Department,
                Username = model.UserName,
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserChanePassword = model.UserChanePassword
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditConfirm(
            [Bind("Id,FirstName,LastName,Username,Password,RepPassword,Department,UserChanePassword")]
            UserManagementEdit userManagement)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var selectUser = await _userManage.FindByIdAsync(userManagement.Id);
                    var cnt = await _userManage.Users
                        .CountAsync(m => m.UserName == userManagement.Username && m.Id != userManagement.Id);
                    if (cnt > 0)
                    {
                        ModelState.AddModelError("Username", "?????? ?????? ???????????? ???????? ?????????????? ?????? ??????.");
                        return View(userManagement);
                    }

                    if (userManagement.Password != null || userManagement.RepPassword != null)
                    {
                        if (userManagement.RepPassword == null || userManagement.Password == null ||
                            !string.Equals(userManagement.Password.Trim(), userManagement.RepPassword.Trim(),
                                StringComparison.CurrentCultureIgnoreCase))
                        {
                            ModelState.AddModelError("RepPassword", "???????? ???????? ???? ?????????? ???? ???????????? ??????????.");
                            return View(userManagement);
                        }
                        else
                        {
                            var token = await _userManage.GeneratePasswordResetTokenAsync(selectUser);
                            var resetPassResult =
                                await _userManage.ResetPasswordAsync(selectUser, token, userManagement.Password);
                            if (!resetPassResult.Succeeded)
                            {
                                var errors = UserController.TranslateError();
                                ModelState.AddModelError("", "?????? ???? ???????????? ??????????");
                                foreach (var item in resetPassResult.Errors)
                                {
                                    var e = from string[] val in errors
                                            where (item.Description.Contains(val[0]))
                                            select new string[] { val[1] };

                                    if (e.Any())
                                    {
                                        ModelState.AddModelError("", e.ToArray()[0][0]);
                                    }
                                }

                                return View(userManagement);
                            }
                        }
                    }

                    selectUser.Department = userManagement.Department;
                    selectUser.FirstName = userManagement.FirstName;
                    selectUser.LastName = userManagement.LastName;
                    selectUser.UserChanePassword = userManagement.UserChanePassword;
                    var result = await _userManage.UpdateAsync(selectUser);
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("",
                            "?????? ???? ???????????? ?????????????? ??????????. ???????? ???? ?????????? ?????????? ???????? ????????????.");
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return View(userManagement);
                }
            }

            return View(userManagement);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                var user = _userManage.FindByIdAsync(id);
                if (user.Result.UserName == "admin")
                {
                    return Json(new { error = "?????????? ?????? ?????????? admin ???????? ??????????." });
                }

                var res = await _userManage.DeleteAsync(user.Result);
                if (!res.Succeeded)
                {
                    return Json(new
                    {
                        error = "?????? ???? ?????? ??????????. ???????? ???? ?????????? ?????????? ???????? ????????????."
                    });
                }

                return Json(new { error = "" });
            }
            catch
            {
                return Json(new
                {
                    error = "?????? ???? ?????? ??????????. ???????? ???? ?????????? ?????????? ???????? ????????????."
                });
            }
        }

        [AllowAnonymous]
        [HttpPost, HttpGet]
        public async Task<IActionResult> Login(LoginPage login)
        {
            if (login.Username == null || login.Password == null)
            {
                ModelState.Clear();
                return View(login);
            }

            if (_userManage.Users.ToList().Count == 0)
            {
                TblUser tblUser = new TblUser()
                {
                    FirstName = "??????????",
                    LastName = "????????",
                    UserName = "admin",
                    Email = "admin@localhost.local",
                    Department = "???????????? ??????????????",
                    Enable = true,
                };
                var resultUser = _userManage.CreateAsync(tblUser, "Admin@123");
                if (resultUser.Result != IdentityResult.Success)
                {
                    ModelState.AddModelError("", "?????? ???? ?????????? ??????????");
                }

                await _userManage.AddClaimAsync(tblUser, new Claim(ClaimTypes.Role, "Admin"));
            }

            if (ModelState.IsValid)
            {
                var result =
                    await _signInManager.PasswordSignInAsync(login.Username, login.Password, login.RememberMe, true);
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError("Username", "?????????? ???? ???????? ???????? ???? ?????????? ???????? ????.");
                }
                else if (result.Succeeded)
                {
                    var user = await _userManage.FindByNameAsync(login.Username);
                    if (user.Enable)
                    {
                        if (user.UserChanePassword)
                        {
                            _notifyService.Warning("?????? ???? ?????? ?????????????? ???? ???????? ???????? ?????? ?????? ???? ??????????. ?????? ???????????? ?????????? ???????? ???????? ???? ?????????? ???????? ???????? ?????????? ????????????.");
                            return RedirectToAction("ChangePassword", "User");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "???????????? ?????? ?????? ???????? ?????? ??????. ???????? ???? ?????????? ?????????? ???????? ????????????.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "?????? ???????????? ???? ???????? ???????? ???????? ?????? ????????.");
                }
            }

            login.RememberMe = true;
            return View(login);
        }

        public IActionResult Logout()
        {
            _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [ActionName("ChangePassword"), HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePasswordConfirm([Bind("OldPassword,Password,RepPassword")]
            ChangePassword changePassword)
        {
            ViewBag.Ok = false;
            if (ModelState.IsValid)
            {
                var curUser = await _userManage.GetUserAsync(User);
                var passResult =
                    _userManage.PasswordHasher.VerifyHashedPassword(curUser, curUser.PasswordHash,
                        changePassword.OldPassword);
                if (passResult == PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError("OldPassword", "???????? ???????? ???????? ?????? ???????? ?????? ????????");
                    return View(new ChangePassword());
                }

                if (changePassword.Password == null
                    || changePassword.RepPassword == null
                    || !changePassword.Password.Equals(changePassword.RepPassword,
                        StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("RepPassword", "???????? ???????? ???????? ?????? ???? ?????????? ???? ???????????? ??????????.");
                    return View(new ChangePassword());
                }

                var token = await _userManage.GeneratePasswordResetTokenAsync(curUser);
                var resetPassResult =
                    await _userManage.ResetPasswordAsync(curUser, token, changePassword.Password);
                if (resetPassResult.Succeeded)
                {
                    curUser.UserChanePassword = false;
                    await _userManage.UpdateAsync(curUser);
                    ViewBag.Ok = true;
                }
                else
                {
                    var errors = TranslateError();
                    ModelState.AddModelError("", "?????? ???? ???????????? ??????????");
                    foreach (var item in resetPassResult.Errors)
                    {
                        var e = from string[] val in errors
                                where (item.Description.Contains(val[0]))
                                select new string[] { val[1] };

                        if (e.Any())
                        {
                            ModelState.AddModelError("", e.ToArray()[0][0]);
                        }
                    }
                }

                return View(new ChangePassword());
            }

            return View(new ChangePassword());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Status(string id, bool status)
        {
            try
            {
                var curUser = await _userManage.FindByIdAsync(id);
                if (curUser == null)
                {
                    return Json(new { error = "?????????? ???????? ?????? ???????? ??????" });
                }

                if (curUser.UserName.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { error = "?????????? ?????? ???????? ???????? ?????????? admin ???????? ??????????" });
                }

                curUser.Enable = status;
                var result = await _userManage.UpdateAsync(curUser);
                if (result.Succeeded)
                    return Json(new { error = "" });
            }
            catch
            {
                return Json(new { error = "?????? ???? ?????? ??????????????. ???????? ???? ?????????? ?????????? ???????? ????????????." });
            }

            return Json(new { error = "?????? ???? ?????? ??????????????. ???????? ???? ?????????? ?????????? ???????? ????????????." });
        }

        private static List<string[]> TranslateError()
        {
            return new List<string[]>()
            {
                new string[]
                {
                    "Passwords must be at least 6 characters.",
                    "???????? ???????? ???????? ?????????? 6 ?????????????? ?????????? ????????"
                },
                new string[]
                {
                    "Passwords must have at least one non alphanumeric character.",
                    "???????? ???????? ???????? ?????????? ???????? ???? ?????? ????????"
                },
                new string[]
                {
                    "Passwords must have at least one lowercase ('a'-'z').",
                    "???????? ???????? ???????? ?????????? ???????? ???? ?????? ???????? ???? a-z ?????????? ????????"
                },
                new string[]
                {
                    "Passwords must have at least one uppercase ('A'-'Z').",
                    "???????? ???????? ???????? ?????????? ???????? ???? ?????? ???????? ???? A-Z ?????????? ????????"
                },
            };
        }
    }
}