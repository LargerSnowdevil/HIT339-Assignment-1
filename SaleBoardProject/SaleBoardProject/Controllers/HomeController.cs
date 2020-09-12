using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SaleBoardProject.Data;
using SaleBoardProject.Models;
using SQLitePCL;

namespace SaleBoardProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly SaleBoardDBContext _context;

        public HomeController(ILogger<HomeController> logger, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, SaleBoardDBContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [Authorize]
        public IActionResult Index()
        {
            //return View();
            return RedirectToAction("Index", "Items");
        }

        [Authorize]
        public IActionResult Test()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(String username, String password)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

                if (result.Succeeded)
                {
                    //todo redirect to all items report
                    return RedirectToAction("Index");
                }
            }
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(String username, String name, String address, int age, IFormFile userAvatar, String password)
        {
            var managedUser = new IdentityUser
            {
                UserName = username
            };

            var result = await _userManager.CreateAsync(managedUser, password);

            if (result.Succeeded)
            {
                //byte[] image;
                //if (userAvatar != null && userAvatar.Length > 0)
                //{
                //    var ms = new MemoryStream();
                //    userAvatar.CopyTo(ms);
                //    image = ms.ToArray();
                //} else
                //{
                //    image = null;
                //}

                //Todo change this to Seed the admin if i have time
                bool isAdmin = false;
                if (username.CompareTo("Admin") == 0)
                {
                    isAdmin = true;
                }

                var user = new User
                {
                    Username = username,
                    name = name,
                    address = address,
                    age = age,
                    //userAvatar = image,
                    isAdmin = isAdmin
                };
                _context.RegisteredUsers.Add(user);
                _context.SaveChanges();

                var signInResult = await _signInManager.PasswordSignInAsync(managedUser, password, false, false);

                if (signInResult.Succeeded)
                {
                    //todo redirect to all items report
                    return RedirectToAction("Index");
                }
            }

            //todo redirect to all items report
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
