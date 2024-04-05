using RealEstate.Core;
using RealEstate.DBContext;
using RealEstate.Models;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Areas.Customer.Controllers;
using System.Text.Json;

namespace RealEstate.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CustomersController : AdminCoreController
    {
        private readonly AppDbContext _context;
        public CustomersController(IConfiguration configuration, AppDbContext dbContext) : base(configuration, dbContext)
        {
            _context = dbContext;
        }

        public IActionResult Index()
        {
            List<User> customers = _context.Users
                                .Where(c => c.IsDeleted == 0 && c.Level == 3)
                                .OrderByDescending(c => c.CreatedAt)
                                .ToList();
            return View(customers);
        }

        [HttpGet]
        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(CustomerCreatePayloadDataModel payloadData)
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

                User UserInsertData = new User
                {
                    Name = payloadData.Name,
                    Email = payloadData.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(payloadData.Password),
                    Address = payloadData.Address,
                    PhoneNumber = payloadData.PhoneNumber,
                    Status = Models.User.StatusType.Active,
                    Level = 3,
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
        public IActionResult Edit(int id)
        {
            User? customer = _context.Users
                            .Where(c => c.Id == id && c.IsDeleted == 0 && c.Level == 3)
                            .FirstOrDefault();
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }
        [HttpPost]
        public IActionResult Edit(User customer)
        {
            User? customerDb = _context.Users
                    .Where(c => c.Id == customer.Id && c.IsDeleted == 0 && c.Level == 3)
                    .FirstOrDefault();
            if (customerDb == null)
            {
                return NotFound();
            }
            ModelState.Remove("Password");
            if (ModelState.IsValid)
            {

                if (_context.Users.Any(u => u.Email == customer.Email && u.Id != customerDb.Id))
                {
                    return Json(new { success = false, message = "Email already exists. Please choose a different Email." });
                }
                if (_context.Users.Any(u => u.PhoneNumber == customer.PhoneNumber && u.IsDeleted == 0 && u.Id != customerDb.Id))
                {
                    return Json(new { success = false, message = "PhoneNumber already exists. Please choose a different PhoneNumber." });
                }

                if (_context.Users.Any(u => u.Name == customer.Name && u.IsDeleted == 0 && u.Id != customerDb.Id))
                {
                    return Json(new { success = false, message = "Name already exists. Please choose a different Name." });

                }
                customerDb.Name = customer.Name;
                customerDb.PhoneNumber = customer.PhoneNumber;
                customerDb.Email = customer.Email;
                if (customer.Password != null)
                {
                    customerDb.Password = BCrypt.Net.BCrypt.HashPassword(customer.Password);
                }
                customerDb.Address = customer.Address;
                customerDb.Status = customer.Status;
                customerDb.LastUpdate = DateTime.Now;
                _context.Users.Update(customerDb);
                _context.SaveChanges();

                return Json(new { success = true, message = "Record updated successfully" });
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
        [HttpPost]
        public IActionResult Delete(int id)
        {
            User? customer = _context.Users
                                .Where(c => c.IsDeleted == 0 && c.Id == id && c.Level == 3)
                                .FirstOrDefault();
            if (customer == null)
            {
                return Json(new { success = false, message = "Something Went Wrong" });
            }
            else
            {
                customer.IsDeleted = 1;
                _context.Users.Update(customer);
                _context.SaveChanges();
                return Json(new { success = true, message = "Record updated successfully" });
            }

        }
    }

    public class CustomerCreatePayloadDataModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
        public Models.User.StatusType Status { get; set; }
    }

}
