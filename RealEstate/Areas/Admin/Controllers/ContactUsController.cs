using RealEstate.Core;
using RealEstate.DBContext;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Models;

namespace RealEstate.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ContactUsController : AdminCoreController
    {
        private readonly AppDbContext _context;
        public ContactUsController(IConfiguration configuration, AppDbContext dbContext) : base(configuration, dbContext)
        {
            _context = dbContext;
        }

        public IActionResult Index()
        {
            List<ContactUs> contactUs = _context.ContactUs.Where(item => item.IsDeleted == 0).OrderByDescending(item => item.CreatedAt).ToList();
            return View(contactUs);
        }
    }
}
