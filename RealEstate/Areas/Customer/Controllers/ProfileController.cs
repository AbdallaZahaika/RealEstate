using RealEstate.DBContext;
using RealEstate.Models;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Core;

namespace RealEstate.Areas.Admin.Controllers
{
    [Area("Customer")]
    public class ProfileController : CustomerCoreController
    {
        private readonly AppDbContext _context;
        public ProfileController(IConfiguration configuration, AppDbContext dbContext) : base(configuration, dbContext)
        {
            _context = dbContext;
        }

        public IActionResult Index()
        {
            int userId = GetUserId();
            User? customer = _context.Users
                                      .Where(c => c.Id == userId && c.IsDeleted == 0 && c.Level == 3)
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
            int userId = GetUserId();
            User? customerDb = _context.Users
                    .Where(c => c.Id == userId && c.IsDeleted == 0 && c.Level == 3)
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
    }
}
