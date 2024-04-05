using RealEstate.DBContext;
using RealEstate.Models;
using Microsoft.AspNetCore.Mvc;
using static RealEstate.Models.Property;
using RealEstate.Core;
namespace RealEstate.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class PropertiesController : CustomerCoreController
    {
        private readonly AppDbContext _context;
        public PropertiesController(IConfiguration configuration, AppDbContext dbContext) : base(configuration, dbContext)
        {
            _context = dbContext;
        }
        public IActionResult Index()
        {
            int userId = GetUserId();
            List<PropertyDataTableRowDataModal> Properties = new List<PropertyDataTableRowDataModal>();
            List<Property> getProperties = _context.Properties
                                    .Where(property => property.OwnerLevel == 3 && property.OwnerUserId == userId && property.IsDeleted == 0)
                                    .OrderByDescending(property => property.CreatedAt)
                                    .ToList();

            foreach (Property property in getProperties)
            {
                string StatusColor = "";
                if (property.Status == PropertyStatus.Pending)
                {
                    StatusColor = "orange";
                }
                else if (property.Status == PropertyStatus.Canceled)
                {
                    StatusColor = "red";
                }
                else if (property.Status == PropertyStatus.Confirmed)
                {
                    StatusColor = "green";
                }

                Properties.Add(new PropertyDataTableRowDataModal
                {
                    Id = property.Id,
                    City = GetCityName(property.CityId),
                    Region = GetRegionName(property.RegionId),
                    FullTotal = property.FullTotal,
                    Title = property.Title,
                    ServiceType = property.ServiceType,
                    Type = property.Type,
                    Status = property.Status,
                    StatusColor = StatusColor,
                    WithMaintenanceContract = property.WithMaintenanceContract == true ? "Yes" : "No"
                });
            }


            return View(Properties);
        }
        [HttpGet]
        public IActionResult View(int id)
        {
            int userId = GetUserId();
            Property? property = _context.Properties
                            .Where(property => property.OwnerLevel == 3 && property.OwnerUserId == userId && property.Id == id && property.IsDeleted == 0)
                            .FirstOrDefault();
            if (property == null)
            {
                return NotFound();
            }
            List<RealEstateFile>? Documents = _context.RealEstateFiles
                .Where(realEstateFile => realEstateFile.TableId == property.Id && realEstateFile.TableName == "Properties" && realEstateFile.IsDeleted == 0)
                .ToList();
            ViewBag.Documents = Documents;
            ViewBag.CityTitle = GetCityName(property.CityId);
            ViewBag.OwnerName = GetUserName(property.OwnerUserId);
            return View(property);
        }

    }
}
