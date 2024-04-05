using Microsoft.AspNetCore.Mvc;
using RealEstate.Core;
using RealEstate.DBContext;
using RealEstate.Models;
using Microsoft.EntityFrameworkCore;
using static RealEstate.Models.Property;
using System.Drawing;

namespace RealEstate.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HousesController : BaseController
    {
        private readonly AppDbContext _context;

        public HousesController(IConfiguration configuration, AppDbContext dbContext) : base(configuration, dbContext)
        {
            _context = dbContext;
        }
        public IActionResult Index(Property.ServiceTypes? service_type, int cityId, int regionId, int numberOfRooms, double minPrice, double maxPrice, Property.PropertyTypes? propertytype)
        {
            string where = "";

            /* check search service type start */
            if (service_type == Property.ServiceTypes.Rent)
            {
                where += " AND [ServiceType] = 0 ";
            }
            else if (service_type == Property.ServiceTypes.Sale)
            {
                where += " AND [ServiceType] = 1 ";
            }
            else
            {
                return NotFound();
            }
            ViewBag.ServiceType = service_type;
            /* check search  type end */

            /* check search type start */
            if (propertytype != null)
            {
                if (propertytype == Property.PropertyTypes.Villa)
                {
                    where += " AND [Type] = 1 ";
                }
                else if (propertytype == Property.PropertyTypes.InBuilding)
                {
                    where += " AND [Type] = 2 ";
                }
                else if (propertytype == Property.PropertyTypes.OrdinaryHouse)
                {
                    where += " AND [Type] = 0 ";
                }
                else
                {
                    return NotFound();
                }
                ViewBag.SelectedType = propertytype;
            }
            /* check search type end */


            /* check search min price start */
            if (minPrice > 0)
            {
                where += $" AND [FullTotal] >= {minPrice} ";
                ViewBag.SelectedMinPrice = minPrice;
            }
            /* check search min price end */

            /* check search max price start */
            if (maxPrice > 0)
            {
                if (maxPrice >= minPrice)
                {
                    where += $" AND [FullTotal] <= {maxPrice} ";
                    ViewBag.SelectedMaxPrice = maxPrice;
                }
                else
                {
                    where += $" AND [FullTotal] <= {minPrice} ";
                    ViewBag.SelectedMaxPrice = minPrice;
                }
            }
            /* check search max price end */

            /* check search number Of Rooms start */
            if (numberOfRooms > 0)
            {
                where += $" AND [numberOfRooms] = {numberOfRooms} ";
                ViewBag.SelectedNumberOfRooms = numberOfRooms;
            }
            /* check search number Of Rooms end */

            /* chekc region start */
            if (regionId > 0)
            {
                /* check region exists start */
                Models.Region? check_region_exists = _context.Regions
                      .Where(region => region.Id == regionId && region.IsDeleted == 0)
                      .FirstOrDefault();
                if (check_region_exists != null)
                {
                    where += $" AND [RegionId] = {regionId} ";
                    ViewBag.SelectedRegionId = regionId;
                }
                else
                {
                    return NotFound();
                }
                /* check region exists end */
            }
            /* chekc region end */

            /* chekc city start */
            if (regionId > 0 && cityId > 0)
            {
                /* check city exists start */
                Models.City? check_city_exists = _context.Cities
                      .Where(city => city.Id == cityId && city.RegionId == regionId && city.IsDeleted == 0)
                      .FirstOrDefault();
                if (check_city_exists != null)
                {
                    where += $" AND [CityId] = {cityId} ";
                    ViewBag.SelectedCityId = cityId;
                    ViewBag.SelectedCityTitle = check_city_exists.Title;
                }
                /* check city exists end */
            }
            /* chekc city end */

            string query = $"SELECT * FROM Properties WHERE [Status] = 0 AND [OwnerLevel] = 2 AND [IsDeleted] = 0 {where}";

            /* get Properties start */
            List<PropertyDataTableRowDataModal> Properties = new List<PropertyDataTableRowDataModal>();
            List<Property> getProperties = _context.Properties.FromSqlRaw(query)
            .ToList();

            foreach (Property property_data in getProperties)
            {
                /* check if Contract completed exists start */
                Contract? checkCompletedContractExists = _context.Contracts
                   .Where(contract => contract.PropertyId == property_data.Id && contract.Status == Contract.ContractStatus.Done && contract.Type == Contract.ContractTypes.Rent && contract.ToDate > DateTime.Today && contract.IsDeleted == 0)
                   .FirstOrDefault();
                /* check if Contract completed exists end */
                if (checkCompletedContractExists == null)
                {
                    string MainImage = "";
                    bool IsHotDeal = false;

                    /* get main image start */
                    RealEstateFile? getMainImage = _context.RealEstateFiles
                             .Where(realEstateFile => realEstateFile.TableId == property_data.Id && realEstateFile.TableName == "Properties" && realEstateFile.IsDeleted == 0)
                             .FirstOrDefault();
                    if (getMainImage != null)
                    {
                        MainImage = getMainImage.Url;
                    }
                    /* get main image end */

                    /* check is hot deal start */
                    Models.Region? checkIsHotDeal = _context.Regions
                             .Where(region => region.Id == property_data.RegionId && property_data.FullTotal < region.DefultPrice && region.IsDeleted == 0)
                             .FirstOrDefault();
                    if (checkIsHotDeal != null)
                    {
                        IsHotDeal = true;
                    }
                    /* check is hot deal end */


                    Properties.Add(new PropertyDataTableRowDataModal
                    {
                        Id = property_data.Id,
                        City = GetCityName(property_data.CityId),
                        Region = GetRegionName(property_data.RegionId),
                        FullTotal = property_data.FullTotal,
                        Title = property_data.Title,
                        Type = property_data.Type,
                        NumberOfBathrooms = property_data.NumberOfBathrooms,
                        NumberOfRooms = property_data.NumberOfRooms,
                        PropertySize = property_data.PropertySize,
                        MainImage = MainImage,
                        IsHotDeal = IsHotDeal,
                    });
                }
            }
            /* get Rents end */
            return View(Properties);
        }

        public IActionResult Show(int Id)
        {
            Property? getPropertyData = _context.Properties
                                    .Where(property => property.Id == Id && property.OwnerLevel == 2 && property.Status == PropertyStatus.Confirmed && property.IsDeleted == 0)
                                    .OrderByDescending(property => property.CreatedAt)
                                    .FirstOrDefault();


            if (getPropertyData != null)
            {
                /* check if Contract completed exists start */
                Contract? checkCompletedContractExists = _context.Contracts
                   .Where(contract => contract.PropertyId == getPropertyData.Id && contract.Status == Contract.ContractStatus.Done && contract.Type == Contract.ContractTypes.Rent && contract.ToDate > DateTime.Today && contract.IsDeleted == 0)
                   .FirstOrDefault();
                if (checkCompletedContractExists != null)
                {
                    return NotFound();
                }
                /* check if Contract completed exists end */


                string MainImage = "";
                List<string> Images = new List<string>();
                bool IsHotDeal = false;
                bool haveContract = false;
                bool IsUserLoggedIn = false;
                int userId = GetUserId();

                if (userId > 0)
                {
                    IsUserLoggedIn = true;
                    /* check if Contract exists start */
                    Contract? checkContractExists = _context.Contracts
                       .Where(contract => contract.PropertyId == getPropertyData.Id && contract.CustomerId == userId && contract.Status != Contract.ContractStatus.Canceled && (contract.Type == Contract.ContractTypes.Sale || (contract.Type == Contract.ContractTypes.Rent && contract.ToDate > DateTime.Today)) && contract.IsDeleted == 0)
                       .FirstOrDefault();
                    if (checkContractExists != null)
                    {
                        haveContract = true;
                    }
                    /* check if Contract exists end */
                }
                ViewBag.IsUserLoggedIn = IsUserLoggedIn;


                /* get images start */
                List<RealEstateFile>? getImages = _context.RealEstateFiles
                         .Where(realEstateFile => realEstateFile.TableId == getPropertyData.Id && realEstateFile.TableName == "Properties" && realEstateFile.IsDeleted == 0)
                         .ToList();
                if (getImages != null)
                {
                    MainImage = getImages[0].Url;
                    foreach (RealEstateFile image_data in getImages)
                    {
                        Images.Add(image_data.Url);
                    }
                }
                /* get images end */

                /* check is hot deal start */
                Models.Region? checkIsHotDeal = _context.Regions
                         .Where(region => region.Id == getPropertyData.RegionId && getPropertyData.FullTotal < region.DefultPrice && region.IsDeleted == 0)
                         .FirstOrDefault();
                if (checkIsHotDeal != null)
                {
                    IsHotDeal = true;
                }
                /* check is hot deal end */

                PropertyViewDetailsModel ViewData = new PropertyViewDetailsModel
                {
                    Id = getPropertyData.Id,
                    City = GetCityName(getPropertyData.CityId),
                    Region = GetRegionName(getPropertyData.RegionId),
                    FullTotal = getPropertyData.FullTotal,
                    Title = getPropertyData.Title,
                    Type = getPropertyData.Type,
                    NumberOfBathrooms = getPropertyData.NumberOfBathrooms,
                    NumberOfRooms = getPropertyData.NumberOfRooms,
                    PropertySize = getPropertyData.PropertySize,
                    Note = getPropertyData.Note,
                    Images = Images,
                    MainImage = MainImage,
                    IsHotDeal = IsHotDeal,
                    haveContract = haveContract,
                    ServiceType = getPropertyData.ServiceType,
                    FloorNumber = getPropertyData.FloorNumber,
                    NumberOfFloors = getPropertyData.NumberOfFloors,
                    WithMaintenanceContract = getPropertyData.WithMaintenanceContract == true ? "Yes" : "No",
                };


                /* get More Properties start */
                List<PropertyDataTableRowDataModal> MoreProperties = new List<PropertyDataTableRowDataModal>();
                List<Property> getRentsProperties = _context.Properties
                                        .Where(property => property.OwnerLevel == 2 && property.Id != getPropertyData.Id && property.Status == PropertyStatus.Confirmed && property.ServiceType == getPropertyData.ServiceType && property.IsDeleted == 0)
                                        .OrderByDescending(property => property.CreatedAt)
                                        .ToList();

                foreach (Property more_property in getRentsProperties)
                {

                    string more_property_main_image = "";
                    bool more_property_is_hot_deal = false;

                    /* get main image start */
                    RealEstateFile? more_property_get_main_image = _context.RealEstateFiles
                             .Where(realEstateFile => realEstateFile.TableId == more_property.Id && realEstateFile.TableName == "Properties" && realEstateFile.IsDeleted == 0)
                             .FirstOrDefault();
                    if (more_property_get_main_image != null)
                    {
                        more_property_main_image = more_property_get_main_image.Url;
                    }
                    /* get main image end */

                    /* check is hot deal start */
                    Models.Region? more_property_check_is_hot_deal = _context.Regions
                             .Where(region => region.Id == more_property.RegionId && more_property.FullTotal < region.DefultPrice && region.IsDeleted == 0)
                             .FirstOrDefault();
                    if (more_property_check_is_hot_deal != null)
                    {
                        more_property_is_hot_deal = true;
                    }
                    /* check is hot deal end */


                    MoreProperties.Add(new PropertyDataTableRowDataModal
                    {
                        Id = more_property.Id,
                        City = GetCityName(more_property.CityId),
                        Region = GetRegionName(more_property.RegionId),
                        FullTotal = more_property.FullTotal,
                        Title = more_property.Title,
                        Type = more_property.Type,
                        NumberOfBathrooms = more_property.NumberOfBathrooms,
                        NumberOfRooms = more_property.NumberOfRooms,
                        PropertySize = more_property.PropertySize,
                        MainImage = more_property_main_image,
                        IsHotDeal = more_property_is_hot_deal,
                    });
                }
                if (MoreProperties.Count() > 0)
                {
                    ViewBag.MoreProperties = MoreProperties;
                }
                /* get more Properties end */


                return View(ViewData);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public IActionResult CreateContract(int PropertyId)
        {
            int userId = GetUserId();

            if (PropertyId > 0)
            {
                /* Check Property Exists start */
                Property? CheckPropertyExists = _context.Properties
                .Where(property => property.Id == PropertyId && property.OwnerLevel == 2 && property.Status == PropertyStatus.Confirmed && property.IsDeleted == 0)
                .FirstOrDefault();
                /* Check Property Exists end */
                if (CheckPropertyExists != null)
                {
                    /* check if Contract exists start */
                    Contract? checkContractExists = _context.Contracts
                       .Where(contract => contract.PropertyId == CheckPropertyExists.Id && contract.CustomerId == userId && contract.Status != Contract.ContractStatus.Canceled && (contract.Type == Contract.ContractTypes.Sale || (contract.Type == Contract.ContractTypes.Rent && contract.ToDate > DateTime.Today)) && contract.IsDeleted == 0)
                       .FirstOrDefault();
                    /* check if Contract exists end */

                    if (checkContractExists == null)
                    {
                        /* get Contract Type start */
                        Contract.ContractTypes ContractType = Contract.ContractTypes.Sale;

                        if (CheckPropertyExists.ServiceType == Property.ServiceTypes.Rent)
                        {
                            ContractType = Contract.ContractTypes.Rent;
                        }
                        /* get Contract Type end */

                        Contract ContractInsertData = new Contract
                        {
                            CustomerId = userId,
                            PropertyId = PropertyId,
                            ProviderId = CheckPropertyExists.ProviderId,
                            Status = Contract.ContractStatus.Pending,
                            Type = ContractType,
                        };

                        _context.Contracts.Add(ContractInsertData);
                        _context.SaveChanges();
                        return Json(new { success = true, message = "success" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Contract Exists" });

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
    }
}
