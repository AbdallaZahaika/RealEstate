using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using RealEstate.Core;
using RealEstate.DBContext;
using RealEstate.Models;
using System.Text.Json;

namespace RealEstate.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class AccountController : BaseController
    {
        private readonly AppDbContext _context;

        public AccountController(IConfiguration configuration, AppDbContext dbContext) : base(configuration, dbContext)
        {
            _context = dbContext;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            HttpContext.Session.Clear();
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterPayloadData payloadData)
        {
            // Set up JsonSerializerSettings to preserve property names' case
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                WriteIndented = true // Optional: Use false if you don't want indentation
            };

            if (ModelState.IsValid)
            {

                // check Email Exists
                if (_context.Users.Any(u => u.Email == payloadData.Email))
                {
                    return Json(new { success = false, message = "Email already exists. Please choose a different Email." });
                }

                // check Email Validation
                else if (!CheckValidateEmail(payloadData.Email))
                {
                    return Json(new { success = false, message = "Email must be writes in english and the maximum characters 100" });
                }

                // check Phone Number Exists
                if (_context.Users.Any(u => u.PhoneNumber == payloadData.PhoneNumber && u.IsDeleted == 0))
                {
                    return Json(new { success = false, message = "PhoneNumber already exists. Please choose a different PhoneNumber." });
                }
                // check Phone Number Validation
                else if (!CheckIsraeliPhoneNumber(payloadData.PhoneNumber))
                {
                    return Json(new { success = false, message = "Please enter vaild phone" });
                }

                // check Name Exists
                if (_context.Users.Any(u => u.Name == payloadData.Name && u.IsDeleted == 0))
                {
                    return Json(new { success = false, message = "Name already exists. Please choose a different Name." });
                }

                // check Name Validation
                if (payloadData.Name.Length > 250)
                {
                    return Json(new { success = false, message = "Name is required and the maximum characters 250" });
                }


                // check Address Validation
                if (payloadData.Address.Length > 250)
                {
                    return Json(new { success = false, message = "The Address is required and the maximum characters 250" });
                }

                // check Password Validation
                if (!CheckValidatePassword(payloadData.Password))
                {
                    return Json(new { success = false, message = "Password must only contain english letters or numbers and with at least one uppercase letter one lowercase letter and one number" });
                }

                // check Password Confirm Validation
                if (payloadData.Password != payloadData.PasswordConfirm)
                {
                    return Json(new { success = false, message = "Password is not the same" });
                }

                int UserLevel = 2;
                if (payloadData.AccountType == AccountType.Customer)
                {
                    UserLevel = 3;
                }

                User UserInsertData = new User
                {
                    Name = payloadData.Name,
                    Email = payloadData.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(payloadData.Password),
                    Address = payloadData.Address,
                    PhoneNumber = payloadData.PhoneNumber,
                    Status = Models.User.StatusType.Active,
                    Level = UserLevel,
                };

                _context.Users.Add(UserInsertData);
                _context.SaveChanges();
                return Json(new { success = true, message = "success" });
            }
            else
            {
                List<string> errors = ModelState.Values
                                     .SelectMany(v => v.Errors)
                                     .Select(e => e.ErrorMessage)
                                     .ToList();

                string combinedErrors = string.Join(" ", errors);

                return Json(new { result = false, message = combinedErrors });
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.Get("user") == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }
        }

        [HttpPost]
        public IActionResult Login(User user)
        {
            if (HttpContext.Session.Get("user") == null)
            {
                if (user != null && !string.IsNullOrEmpty(user!.Email) && !string.IsNullOrEmpty(user!.Password))
                {
                    User? authUser = _context.Users.FirstOrDefault(u => u.Email == user.Email && u.IsDeleted == 0);
                    if (authUser != null)
                    {
                        if (BCrypt.Net.BCrypt.Verify(user.Password, authUser.Password))
                        {

                            if (authUser.Status == Models.User.StatusType.Block)
                            {
                                ModelState.AddModelError("Password", "User is blocked");
                                return View(user);
                            }
                            else if (authUser.Status == Models.User.StatusType.UnActive)
                            {
                                ModelState.AddModelError("Password", "User is unActive");
                                return View(user);
                            }
                            else
                            {
                                string userJson = JsonConvert.SerializeObject(authUser);

                                if (authUser.Level == 1)
                                {
                                    HttpContext.Session.SetString("user", userJson);
                                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                                }
                                else if (authUser.Level == 2)
                                {
                                    HttpContext.Session.SetString("user", userJson);
                                    return RedirectToAction("Index", "Dashboard", new { area = "Provider" });
                                }
                                else if (authUser.Level == 3)
                                {
                                    HttpContext.Session.SetString("user", userJson);
                                    return RedirectToAction("Index", "Home", new { area = "Customer" });
                                }
                                else
                                {
                                    ModelState.AddModelError("Password", "User is Not Authorized");
                                    return View(user);
                                }
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("Password", "Username or Password Is Incorrect");
                            return View(user);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Password", "Username or Password Is Incorrect");
                        return View(user);
                    }
                }
                else
                {
                    return View(user);
                }
            }
            else
            {
                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }
        }



        public IActionResult Logout()
        {
            /* clear Session */
            HttpContext.Session.Clear();

            /* Redirect to login */
            return RedirectToAction("Login", "Account", new { area = "Customer" });
        }
    }

    public class RegisterPayloadData
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
        public AccountType AccountType { get; set; }
    }

    public enum AccountType
    {
        Customer,
        Provider
    }
}
