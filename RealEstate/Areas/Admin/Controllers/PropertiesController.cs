using RealEstate.Core;
using RealEstate.DBContext;
using RealEstate.Models;
using Microsoft.AspNetCore.Mvc;
using static RealEstate.Models.Property;
using RealEstate.Helper;

namespace RealEstate.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PropertiesController : AdminCoreController
    {
        private readonly AppDbContext _context;
        public PropertiesController(IConfiguration configuration, AppDbContext dbContext) : base(configuration, dbContext)
        {
            _context = dbContext;
        }

        public IActionResult Index()
        {
            List<PropertyDataTableRowDataModal> Properties = new List<PropertyDataTableRowDataModal>();
            List<Property> getProperties = _context.Properties
                                    .Where(property => property.IsDeleted == 0)
                                    .OrderByDescending(property => property.CreatedAt)
                                    .ToList();

            foreach (Property property in getProperties)
            {
                string OwnerName = "";
                string OwnerType = "";
                User? owner_user_data = _context.Users
                                    .Where(user => user.IsDeleted == 0 && user.Level == property.OwnerLevel && user.Id == property.OwnerUserId)
                                    .FirstOrDefault();
                if (owner_user_data != null)
                {
                    OwnerName = owner_user_data.Name;
                    if (owner_user_data.Level == 2)
                    {
                        OwnerType = "Provider";
                    }
                    else if (owner_user_data.Level == 3)
                    {
                        OwnerType = "Customer";
                    }

                }

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
                    OwnerName = OwnerName,
                    OwnerType = OwnerType,
                    City = GetCityName(property.CityId),
                    Region = GetRegionName(property.RegionId),
                    FullTotal = property.FullTotal,
                    Title = property.Title,
                    ServiceType = property.ServiceType,
                    Status = property.Status,
                    Type = property.Type,
                    StatusColor = StatusColor,
                    WithMaintenanceContract = property.WithMaintenanceContract == true ? "Yes" : "No"
                });
            }


            return View(Properties);
        }

        [HttpGet]
        public IActionResult New()
        {
            return View();
        }


        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create(PropertyCreateActionModel payloadData)
        {
            if (ModelState.IsValid)
            {
                /* check owner level */
                if (payloadData.OwnerLevel == 2 || payloadData.OwnerLevel == 3)
                {
                    /* check owner data start */
                    User? owner_user_data = _context.Users
                                    .Where(user => user.IsDeleted == 0 && user.Level == payloadData.OwnerLevel && user.Id == payloadData.OwnerUserId)
                                    .FirstOrDefault();
                    /* check owner data end */
                    if (owner_user_data != null)
                    {
                        /* check region data start */
                        Models.Region? region_data = _context.Regions
                                        .Where(region => region.IsDeleted == 0 && region.Id == payloadData.Region)
                                        .FirstOrDefault();
                        /* check region data end */
                        if (region_data != null)
                        {
                            /* check city data start */
                            City? city_data = _context.Cities
                                            .Where(city => city.IsDeleted == 0 && city.Id == payloadData.City && city.RegionId == payloadData.Region)
                                            .FirstOrDefault();
                            /* check city data end */
                            if (city_data != null)
                            {
                                /* check title data start */
                                if (_context.Properties.Any(property => property.Title == payloadData.Title && property.IsDeleted == 0))
                                {
                                    return Json(new { success = false, message = "Title already exists. Please choose a different Title." });
                                }
                                /* check title data end */

                                int ProviderId = 0;
                                if (payloadData.OwnerLevel == 2)
                                {
                                    ProviderId = payloadData.OwnerUserId;
                                }

                                Property propertyStoreData = new Property
                                {
                                    OwnerUserId = payloadData.OwnerUserId,
                                    ProviderId = ProviderId,
                                    OwnerLevel = payloadData.OwnerLevel,
                                    Title = payloadData.Title,
                                    PropertySize = payloadData.PropertySize,
                                    NumberOfFloors = payloadData.NumberOfFloors,
                                    FloorNumber = payloadData.FloorNumber,
                                    Type = payloadData.Type,
                                    NumberOfRooms = payloadData.NumberOfRooms,
                                    NumberOfBathrooms = payloadData.NumberOfBathrooms,
                                    Note = payloadData.Note,
                                    ServiceType = payloadData.ServiceType,
                                    FullTotal = payloadData.FullTotal,
                                    Status = payloadData.Status,
                                    RegionId = payloadData.Region,
                                    CityId = payloadData.City,
                                    WithMaintenanceContract = payloadData.WithMaintenanceContract,
                                };

                                /* store data start */
                                _context.Properties.Add(propertyStoreData);
                                await _context.SaveChangesAsync();
                                /* store data end */

                                if (propertyStoreData.Id > 0)
                                {
                                    var formData = await HttpContext.Request.ReadFormAsync();
                                    /* upload Documents start */
                                    if (formData.Files.Count > 0)
                                    {
                                        List<string> documentsUrls = UploadHelper.UploadFiles((List<IFormFile>)formData.Files, "images/properties");

                                        foreach (string document_url in documentsUrls)
                                        {
                                            RealEstateFile real_estate_file_data = new RealEstateFile
                                            {
                                                TableId = propertyStoreData.Id,
                                                TableName = "Properties",
                                                Url = document_url,
                                            };

                                            /* store data start */
                                            _context.RealEstateFiles.Add(real_estate_file_data);
                                            _context.SaveChanges();
                                            /* store data end */
                                        }
                                    }
                                    /* upload Documents end */

                                    return Json(new { success = true, message = "Created successfully" });
                                }
                                else
                                {
                                    return Json(new { success = false, message = "Something Went Wrong" });
                                }
                            }
                            else
                            {
                                return Json(new { success = false, message = "Something Went Wrong" });
                            }
                        }
                        else
                        {
                            return Json(new { success = false, message = "Something Went Wrong" });
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "Something Went Wrong" });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Something Went Wrong" });
                }

            }
            else
            {
                List<string> errors = ModelState.Values
                                      .SelectMany(v => v.Errors)
                                      .Select(e => e.ErrorMessage)
                                      .ToList();

                string combinedErrors = string.Join(" ", errors);

                return Json(new
                {
                    result = false,
                    message = combinedErrors
                });
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Property? property = _context.Properties
                            .Where(property => property.Id == id && property.IsDeleted == 0)
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

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(PropertyUpdateActionModel payloadData)
        {
            if (ModelState.IsValid)
            {
                /* check if property exists start */
                Property? check_property_exists = _context.Properties
                .Where(property => property.Id == payloadData.Id && property.IsDeleted == 0)
                .FirstOrDefault();
                if (check_property_exists == null)
                {
                    return Json(new { success = false, message = "Something Went Wrong" });
                }
                /* check if property exists end */

                /* check owner level */
                if (payloadData.OwnerLevel == 2 || payloadData.OwnerLevel == 3)
                {
                    /* check owner data start */
                    User? owner_user_data = _context.Users
                                    .Where(user => user.IsDeleted == 0 && user.Level == payloadData.OwnerLevel && user.Id == payloadData.OwnerUserId)
                                    .FirstOrDefault();
                    /* check owner data end */
                    if (owner_user_data != null)
                    {
                        /* check region data start */
                        Models.Region? region_data = _context.Regions
                                        .Where(region => region.IsDeleted == 0 && region.Id == payloadData.Region)
                                        .FirstOrDefault();
                        /* check region data end */
                        if (region_data != null)
                        {
                            /* check city data start */
                            City? city_data = _context.Cities
                                            .Where(city => city.IsDeleted == 0 && city.Id == payloadData.City && city.RegionId == payloadData.Region)
                                            .FirstOrDefault();
                            /* check city data end */
                            if (city_data != null)
                            {
                                /* check title data start */
                                if (_context.Properties.Any(property => property.Id != check_property_exists.Id && property.Title == payloadData.Title && property.IsDeleted == 0))
                                {
                                    return Json(new { success = false, message = "Title already exists. Please choose a different Title." });
                                }
                                /* check title data end */

                                /* set the new values start */
                                check_property_exists.OwnerUserId = payloadData.OwnerUserId;
                                check_property_exists.OwnerLevel = payloadData.OwnerLevel;
                                check_property_exists.Title = payloadData.Title;
                                check_property_exists.PropertySize = payloadData.PropertySize;
                                check_property_exists.NumberOfFloors = payloadData.NumberOfFloors;
                                check_property_exists.FloorNumber = payloadData.FloorNumber;
                                check_property_exists.Type = payloadData.Type;
                                check_property_exists.NumberOfRooms = payloadData.NumberOfRooms;
                                check_property_exists.NumberOfBathrooms = payloadData.NumberOfBathrooms;
                                check_property_exists.Note = payloadData.Note;
                                check_property_exists.ServiceType = payloadData.ServiceType;
                                check_property_exists.FullTotal = payloadData.FullTotal;
                                check_property_exists.Status = payloadData.Status;
                                check_property_exists.RegionId = payloadData.Region;
                                check_property_exists.CityId = payloadData.City;
                                check_property_exists.WithMaintenanceContract = payloadData.WithMaintenanceContract;
                                check_property_exists.LastUpdate = DateTime.Now;

                                if (payloadData.OwnerLevel == 2)
                                {
                                    check_property_exists.ProviderId = payloadData.OwnerUserId;
                                }
                                /* set the new values start */

                                /* delete Documents start */
                                if (payloadData.DeletedDocumentsIds != null && payloadData.DeletedDocumentsIds.Count > 0)
                                {
                                    foreach (var DocumentId in payloadData.DeletedDocumentsIds)
                                    {
                                        RealEstateFile? check_documen_exists = _context.RealEstateFiles
                                                                                .Where(rowData => rowData.IsDeleted == 0 && rowData.Id == DocumentId)
                                                                                .FirstOrDefault();
                                        if (check_documen_exists != null)
                                        {
                                            check_documen_exists.IsDeleted = 1;
                                            _context.RealEstateFiles.Update(check_documen_exists);
                                            _context.SaveChanges();
                                        }
                                    }
                                }
                                /* delete Documents end */

                                var formData = await HttpContext.Request.ReadFormAsync();
                                /* upload Documents start */
                                if (formData.Files.Count > 0)
                                {
                                    List<string> documentsUrls = UploadHelper.UploadFiles((List<IFormFile>)formData.Files, "images/properties");

                                    foreach (string document_url in documentsUrls)
                                    {
                                        RealEstateFile real_estate_file_data = new RealEstateFile
                                        {
                                            TableId = check_property_exists.Id,
                                            TableName = "Properties",
                                            Url = document_url,
                                        };

                                        /* store data start */
                                        _context.RealEstateFiles.Add(real_estate_file_data);
                                        _context.SaveChanges();
                                        /* store data end */
                                    }
                                }
                                /* upload Documents end */

                                /* update data start */
                                _context.Properties.Update(check_property_exists);
                                _context.SaveChanges();
                                return Json(new { success = true, message = "Updated successfully" });
                                /* update data end */
                            }
                            else
                            {
                                return Json(new { success = false, message = "Something Went Wrong" });
                            }
                        }
                        else
                        {
                            return Json(new { success = false, message = "Something Went Wrong" });
                        }
                    }
                    else
                    {
                        return Json(new { success = false, message = "Something Went Wrong" });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Something Went Wrong" });
                }

            }
            else
            {
                List<string> errors = ModelState.Values
                                      .SelectMany(v => v.Errors)
                                      .Select(e => e.ErrorMessage)
                                      .ToList();

                string combinedErrors = string.Join(" ", errors);

                return Json(new
                {
                    result = false,
                    message = combinedErrors
                });
            }
        }


        [HttpPost]
        public IActionResult Delete(int id)
        {
            Property? property = _context.Properties
                                .Where(p => p.IsDeleted == 0 && p.Id == id)
                                .FirstOrDefault();
            if (property == null)
            {
                return Json(new { success = false, message = "Something Went Wrong" });
            }
            else
            {
                property.IsDeleted = 1;
                _context.Properties.Update(property);
                _context.SaveChanges();
                return Json(new { success = true, message = "Record updated successfully" });
            }

        }
    }

    public class PropertyCreateActionModel
    {
        public int OwnerLevel { get; set; }
        public int OwnerUserId { get; set; }
        public string Title { get; set; }
        public double PropertySize { get; set; }
        public int NumberOfFloors { get; set; }
        public int FloorNumber { get; set; }
        public int NumberOfRooms { get; set; }
        public int NumberOfBathrooms { get; set; }
        public Property.PropertyTypes Type { get; set; }
        public Property.ServiceTypes ServiceType { get; set; }
        public Property.PropertyStatus Status { get; set; }
        public bool WithMaintenanceContract { get; set; }
        public int Region { get; set; }
        public int City { get; set; }
        public double FullTotal { get; set; }
        public string? Note { get; set; }
    }


    public class PropertyUpdateActionModel
    {
        public int Id { get; set; }
        public int OwnerLevel { get; set; }
        public int OwnerUserId { get; set; }
        public string Title { get; set; }
        public double PropertySize { get; set; }
        public int NumberOfFloors { get; set; }
        public int FloorNumber { get; set; }
        public int NumberOfRooms { get; set; }
        public int NumberOfBathrooms { get; set; }
        public Property.PropertyTypes Type { get; set; }
        public Property.ServiceTypes ServiceType { get; set; }
        public Property.PropertyStatus Status { get; set; }
        public bool WithMaintenanceContract { get; set; }
        public int Region { get; set; }
        public int City { get; set; }
        public double FullTotal { get; set; }
        public string? Note { get; set; }
        public List<int>? DeletedDocumentsIds { get; set; }
    }
}
