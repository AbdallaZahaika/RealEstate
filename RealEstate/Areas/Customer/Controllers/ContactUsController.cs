using Microsoft.AspNetCore.Mvc;
using RealEstate.Core;
using RealEstate.DBContext;
using RealEstate.Models;

namespace RealEstate.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ContactUsController : BaseController
    {
        private readonly AppDbContext _context;
        public ContactUsController(IConfiguration configuration, AppDbContext dbContext) : base(configuration, dbContext)
        {
            _context = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Action(ActionPayloadDataModel payloadData)
        {
            if (ModelState.IsValid)
            {
                ContactUs ContactUsInsertData = new ContactUs { Message = payloadData.Message, Email = payloadData.Email, Name = payloadData.Name, Subject = payloadData.Subject };
                _context.ContactUs.Add(ContactUsInsertData);
                _context.SaveChanges();
                return Json(new { result = true, message = "success" });
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
    public class ActionPayloadDataModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public string Subject { get; set; }
    }
}
