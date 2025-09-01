using ALLINONEPROJECTWITHOUTJS.Data;
using ALLINONEPROJECTWITHOUTJS.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace ALLINONEPROJECTWITHOUTJS.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly string _connectionString;
        public AccountController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("ConnectionString");
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string userName, string userPassword)
        {
            var user = _context.Users.FirstOrDefault(x => x.UserName == userName && x.Password == userPassword);
            if(user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    // add more claims if needed
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Issue the authentication cookie
                HttpContext.SignInAsync(
                   CookieAuthenticationDefaults.AuthenticationScheme,
                   new ClaimsPrincipal(claimsIdentity),
                   new AuthenticationProperties { IsPersistent = true });

                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Role", user.Role);

                return RedirectToAction("Item","Item");
            }
            // Pass message to View
            ViewBag.Message = "Invalid username or password.";
            return View();
        }
        [HttpPost]
        public IActionResult Register(string userName, string userEmail, string userPassword)
        {
            //var user = new User
            //{
            //    UserName = userName,
            //    Password = userPassword,
            //    Email = userEmail,
            //    Role = "User"
            //};
            //_context.Users.Add(user);

            if (_context.Users.Any(x => x.UserName == userName))
                return Json("User already exists!");

            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_insertUser", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Username", userName);
            cmd.Parameters.AddWithValue("@Email", userEmail);
            cmd.Parameters.AddWithValue("@Password", userPassword);
            cmd.Parameters.AddWithValue("@Role", "User");
            con.Open();
            cmd.ExecuteNonQuery();
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();    
            return RedirectToAction("Login");
        }
    }
}
