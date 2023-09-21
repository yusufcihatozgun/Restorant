using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restorant.Data;
using Restorant.Models;
using Restorant.Repository.Shared.Abstract;
using Restorant.Repository.Shared.Concrete;
using System.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Identity;

namespace Restorant.Web.Controllers
{
    public class AppUserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AppUserController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Login()
        {
            return View();
        }

        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }


        [Authorize(Roles = "Admin"), HttpPost]
        public IActionResult Create(AppUser appUser)
        {
            if (_unitOfWork.AppUsers.GetAll().Any(u => u.UserName == appUser.UserName))
            {
                return UnprocessableEntity("Bu kullanıcı adı zaten kullanılıyor.");
            }
            else
            {
                appUser.Password = "Restorant1!";
                var passwordHasher = new PasswordHasher<AppUser>();
                var hashedPassword = passwordHasher.HashPassword(appUser, appUser.Password);

                appUser.Password = hashedPassword;


                _unitOfWork.AppUsers.Add(appUser);
                _unitOfWork.Save();
                return Ok();
            }
        }


        [Authorize(Roles = "Admin")]
        public IActionResult GetAll()
        {
            return Json(new { data = _unitOfWork.AppUsers.GetAll().Include(u => u.Position) });
        }


        [Authorize(Roles = "Admin")]
        public IActionResult GetById(int id)
        {
            return Json(_unitOfWork.AppUsers.GetFirstOrDefault(e => e.Id == id));
        }


        [Authorize(Roles = "Admin"), HttpPatch]
        public IActionResult Update(AppUser appUser)
        {
            AppUser usr = _unitOfWork.AppUsers.GetFirstOrDefault(e => e.Id == appUser.Id);

            if (appUser == null || usr == null)
            {
                return UnprocessableEntity("Kullanıcı bulunamadı.");

            }
            else
            {
                if (_unitOfWork.AppUsers.GetAll(u => u.Id != appUser.Id).Any(u => u.UserName == appUser.UserName))
                {
                    return UnprocessableEntity("Bu kullanıcı adı zaten kullanılıyor.");
                }
                else
                {
                    appUser.IsRememberMe = usr.IsRememberMe;
                    appUser.IsDeleted = usr.IsDeleted;
                    appUser.DateCreated = usr.DateCreated;
                    appUser.Orders = usr.Orders;
                    appUser.IsActive = appUser.IsActive;

                    _unitOfWork.AppUsers.Update(appUser);
                    _unitOfWork.Save();
                    return Ok();
                }
            }

        }


        [Authorize(Roles = "Admin"), HttpDelete]
        public IActionResult Delete(int id)
        {
            _unitOfWork.AppUsers.Remove(id);
            _unitOfWork.Save();
            return Ok();
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }


        [Authorize(Roles = "Admin"), HttpPost]
        public IActionResult ChangeStatus(int id)
        {
            AppUser usr = _unitOfWork.AppUsers.GetById(id);
            if (usr.IsActive)
            {
                usr.IsActive = false;
                _unitOfWork.AppUsers.Update(usr);
                _unitOfWork.Save();
                return Ok();
            }
            else
            {
                usr.IsActive = true;
                usr.FailedLoginAttempts = 0;
                _unitOfWork.AppUsers.Update(usr);
                _unitOfWork.Save();
                return Ok();
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> Login(AppUser user)
        //{
        //    if (user.UserName != null && user.Password != null)
        //    {
        //        List<AppUser> usrs = _unitOfWork.AppUsers.GetAll(u => u.UserName == user.UserName && u.Password == user.Password && u.IsActive && !u.IsDeleted).Include(u => u.Position).ToList();

        //        if (usrs.Count() > 0)
        //        {
        //            //List<Claim> claims = new List<Claim>();
        //            //claims.Add(new Claim(ClaimTypes.NameIdentifier, usr.Id.ToString()));
        //            //claims.Add(new Claim(ClaimTypes.Name, usr.FirstName + " " + usr.LastName));
        //            //if (usr.Position.Name == "Admin")
        //            //    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        //            //else if (usr.Position.Name == "Garson")
        //            //    claims.Add(new Claim(ClaimTypes.Role, "Garson"));
        //            //else if (usr.Position.Name == "Aşçı")
        //            //    claims.Add(new Claim(ClaimTypes.Role, "Aşçı"));
        //            //else if (usr.Position.Name == "Kasiyer")
        //            //    claims.Add(new Claim(ClaimTypes.Role, "Kasiyer"));
        //            //else
        //            //    claims.Add(new Claim(ClaimTypes.Role, "Pozisyon Atanmamış"));

        //            var claims = new List<Claim>
        //            {
        //                new Claim(ClaimTypes.NameIdentifier, usrs[0].Id.ToString()),
        //                new Claim(ClaimTypes.Name, usrs[0].FirstName + " " + usrs[0].LastName),
        //                new Claim(ClaimTypes.Role, usrs[0].Position.Name)
        //            };


        //            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        //            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);


        //            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties { IsPersistent = usrs[0].IsRememberMe });

        //            return RedirectToAction("Index", "Home");
        //        }
        //        else
        //        {
        //            return View();
        //        }

        //    }
        //    else
        //    {
        //        return View();
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> Login(AppUser user)
        {
            if (string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(user.Password))
            {
                return BadRequest("Kullanıcı Adı ve Şifre alanları boş bırakılamaz");
            }

            var foundUser = _unitOfWork.AppUsers.GetAll(u => u.UserName == user.UserName && !u.IsDeleted).Include(u => u.Position).FirstOrDefault();

            if (foundUser == null)
            {
                return BadRequest("Geçersiz kullanıcı adı veya şifre");
            }

            if (foundUser.IsActive == false)
            {
                foundUser.FailedLoginAttempts++;
                _unitOfWork.AppUsers.Update(foundUser);
                _unitOfWork.Save();

                return BadRequest("Hesabınız geçici olarak kilitlendi. Lütfen destek ile iletişime geçin.");
            }

            var passwordHasher = new PasswordHasher<AppUser>();
            var result = passwordHasher.VerifyHashedPassword(foundUser, foundUser.Password, user.Password);

            if (result == PasswordVerificationResult.Success)
            {
                foundUser.FailedLoginAttempts = 0;
                _unitOfWork.AppUsers.Update(foundUser);
                _unitOfWork.Save();

                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, foundUser.Id.ToString()),
                    new Claim(ClaimTypes.Name, foundUser.FirstName + " " + foundUser.LastName),
                    new Claim(ClaimTypes.Role, foundUser.Position.Name)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = user.IsRememberMe
                };

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                return Ok();
            }
            else
            {
                foundUser.FailedLoginAttempts++;
                if (foundUser.FailedLoginAttempts >= 5)
                {
                    foundUser.IsActive = false;
                    _unitOfWork.AppUsers.Update(foundUser);
                    _unitOfWork.Save();
                    return BadRequest("Hesabınız geçici olarak kilitlendi. Lütfen destek ile iletişime geçin.");
                }

                _unitOfWork.AppUsers.Update(foundUser);
                _unitOfWork.Save();

                return BadRequest("Geçersiz kullanıcı adı veya şifre");
            }
        }



        [Authorize, HttpPost]
        public IActionResult ChangePassword(string newPassword, string oldPassword)
        {
            int userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = _unitOfWork.AppUsers.GetFirstOrDefault(u => u.Id == userId);

            if (user != null)
            {
                var passwordHasher = new PasswordHasher<AppUser>();

                var result = passwordHasher.VerifyHashedPassword(user, user.Password, oldPassword);

                if (result == PasswordVerificationResult.Success)
                {
                    if (IsStrongPassword(newPassword))
                    {
                        var hashedNewPassword = passwordHasher.HashPassword(user, newPassword);
                        user.Password = hashedNewPassword;
                        _unitOfWork.AppUsers.Update(user);
                        _unitOfWork.Save();
                        return Ok();
                    }
                    else
                    {
                        return BadRequest("Lütfen güçlü bir şifre oluşturun");
                    }
                }
                else
                {
                    return BadRequest("Eski şifreniz hatalı");
                }
            }
            else
            {
                return BadRequest("Bir hata oluştu");
            }


        }


        private bool IsStrongPassword(string password)
        {
            return password.Length >= 8 && password.Any(char.IsUpper) && password.Any(char.IsLower) && password.Any(char.IsDigit) && password.Any(ch => !char.IsLetterOrDigit(ch));
        }

    }
}
