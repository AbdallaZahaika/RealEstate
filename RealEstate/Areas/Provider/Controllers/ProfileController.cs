using Microsoft.AspNetCore.Mvc;
using RealEstate.Core;
using RealEstate.DBContext;
using RealEstate.Models;

namespace RealEstate.Areas.Provider.Controllers
{
    [Area("Provider")]
    public class ProfileController : ProviderCoreController
    {
        private readonly AppDbContext _context;
        public ProfileController(IConfiguration configuration, AppDbContext dbContext) : base(configuration, dbContext)
        {
            _context = dbContext;
        }

        public IActionResult Index()
        {
            int userId = GetUserId();
            User? Provider = _context.Users
                            .Where(c => c.Id == userId && c.IsDeleted == 0 && c.Level == 2)
                            .FirstOrDefault();
            if (Provider == null)
            {
                return NotFound();
            }
            return View(Provider);
        }
        [HttpPost]
        public IActionResult Edit(User Provider)
        {
            int userId = GetUserId();
            User? ProviderDb = _context.Users
                    .Where(c => c.Id == userId && c.IsDeleted == 0 && c.Level == 2)
                    .FirstOrDefault();
            if (ProviderDb == null)
            {
                return NotFound();
            }
            ModelState.Remove("Password");
            if (ModelState.IsValid)
            {

                if (_context.Users.Any(u => u.Email == Provider.Email && u.Id != ProviderDb.Id))
                {
                    return Json(new { success = false, message = "Email already exists. Please choose a different Email." });
                }
                if (_context.Users.Any(u => u.PhoneNumber == Provider.PhoneNumber && u.IsDeleted == 0 && u.Id != ProviderDb.Id))
                {
                    return Json(new { success = false, message = "PhoneNumber already exists. Please choose a different PhoneNumber." });
                }

                if (_context.Users.Any(u => u.Name == Provider.Name && u.IsDeleted == 0 && u.Id != ProviderDb.Id))
                {
                    return Json(new { success = false, message = "Name already exists. Please choose a different Name." });

                }
                ProviderDb.Name = Provider.Name;
                ProviderDb.PhoneNumber = Provider.PhoneNumber;
                ProviderDb.Email = Provider.Email;
                if (Provider.Password != null)
                {
                    ProviderDb.Password = BCrypt.Net.BCrypt.HashPassword(Provider.Password);
                }
                ProviderDb.Address = Provider.Address;
                ProviderDb.LastUpdate = DateTime.Now;
                _context.Users.Update(ProviderDb);
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
