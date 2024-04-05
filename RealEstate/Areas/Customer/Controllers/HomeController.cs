using Microsoft.AspNetCore.Mvc;
using RealEstate.Core;
using RealEstate.DBContext;
using RealEstate.Models;
using static RealEstate.Models.Property;

namespace RealEstate.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : BaseController
    {
        private readonly AppDbContext _context;

        public HomeController(IConfiguration configuration, AppDbContext dbContext) : base(configuration, dbContext)
        {
            _context = dbContext;
        }

        public IActionResult Index()
        {
            /* get hot deals start */
            List<PropertyDataTableRowDataModal> HotDeals = new List<PropertyDataTableRowDataModal>();
            var getHotDealsProperties = _context.Properties
                                        .Where(property => property.OwnerLevel == 2
                                            && property.Status == PropertyStatus.Confirmed
                                            && property.IsDeleted == 0)
                                        .Join(_context.Regions,
                                            property => property.RegionId,
                                            region => region.Id,
                                            (property, region) => new { Property = property, Region = region })
                                        .Where(joined => joined.Region.IsDeleted == 0
                                            && joined.Property.FullTotal < joined.Region.DefultPrice)
                                        .Select(joined => joined.Property)
                                        .OrderByDescending(property => property.CreatedAt)
                                        .ToList();

            foreach (Property hot_deal_property in getHotDealsProperties)
            {
                /* check if Contract completed exists start */
                Contract? checkCompletedContractExists = _context.Contracts
                   .Where(contract => contract.PropertyId == hot_deal_property.Id && contract.Status == Contract.ContractStatus.Done && contract.Type == Contract.ContractTypes.Rent && contract.ToDate > DateTime.Today && contract.IsDeleted == 0)
                   .FirstOrDefault();
                /* check if Contract completed exists end */
                if (checkCompletedContractExists == null)
                {
                    string MainImage = "";
                    RealEstateFile? getMainImage = _context.RealEstateFiles
                             .Where(realEstateFile => realEstateFile.TableId == hot_deal_property.Id && realEstateFile.TableName == "Properties" && realEstateFile.IsDeleted == 0)
                             .FirstOrDefault();
                    if (getMainImage != null)
                    {
                        MainImage = getMainImage.Url;
                    }

                    HotDeals.Add(new PropertyDataTableRowDataModal
                    {
                        Id = hot_deal_property.Id,
                        City = GetCityName(hot_deal_property.CityId),
                        Region = GetRegionName(hot_deal_property.RegionId),
                        FullTotal = hot_deal_property.FullTotal,
                        Title = hot_deal_property.Title,
                        ServiceType = hot_deal_property.ServiceType,
                        Type = hot_deal_property.Type,
                        NumberOfBathrooms = hot_deal_property.NumberOfBathrooms,
                        NumberOfRooms = hot_deal_property.NumberOfRooms,
                        PropertySize = hot_deal_property.PropertySize,
                        MainImage = MainImage,
                    });
                }
            }
            if (HotDeals.Count() > 0)
            {
                ViewBag.HotDeals = HotDeals;
            }
            /* get hot deals end */

            /* get Rents start */
            List<PropertyDataTableRowDataModal> Rents = new List<PropertyDataTableRowDataModal>();
            List<Property> getRentsProperties = _context.Properties
                                    .Where(property => property.OwnerLevel == 2 && property.Status == PropertyStatus.Confirmed && property.ServiceType == Property.ServiceTypes.Rent && property.IsDeleted == 0)
                                    .OrderByDescending(property => property.CreatedAt)
                                    .ToList();

            foreach (Property rent_property in getRentsProperties)
            {
                /* check if Contract completed exists start */
                Contract? checkCompletedContractExists = _context.Contracts
                   .Where(contract => contract.PropertyId == rent_property.Id && contract.Status == Contract.ContractStatus.Done && contract.Type == Contract.ContractTypes.Rent && contract.ToDate > DateTime.Today && contract.IsDeleted == 0)
                   .FirstOrDefault();
                /* check if Contract completed exists end */
                if (checkCompletedContractExists == null)
                {
                    string MainImage = "";
                    bool IsHotDeal = false;

                    /* get main image start */
                    RealEstateFile? getMainImage = _context.RealEstateFiles
                             .Where(realEstateFile => realEstateFile.TableId == rent_property.Id && realEstateFile.TableName == "Properties" && realEstateFile.IsDeleted == 0)
                             .FirstOrDefault();
                    if (getMainImage != null)
                    {
                        MainImage = getMainImage.Url;
                    }
                    /* get main image end */

                    /* check is hot deal start */
                    Models.Region? checkIsHotDeal = _context.Regions
                             .Where(region => region.Id == rent_property.RegionId && rent_property.FullTotal < region.DefultPrice && region.IsDeleted == 0)
                             .FirstOrDefault();
                    if (checkIsHotDeal != null)
                    {
                        IsHotDeal = true;
                    }
                    /* check is hot deal end */


                    Rents.Add(new PropertyDataTableRowDataModal
                    {
                        Id = rent_property.Id,
                        City = GetCityName(rent_property.CityId),
                        Region = GetRegionName(rent_property.RegionId),
                        FullTotal = rent_property.FullTotal,
                        Title = rent_property.Title,
                        Type = rent_property.Type,
                        NumberOfBathrooms = rent_property.NumberOfBathrooms,
                        NumberOfRooms = rent_property.NumberOfRooms,
                        PropertySize = rent_property.PropertySize,
                        MainImage = MainImage,
                        IsHotDeal = IsHotDeal,
                    });
                }
            }
            if (Rents.Count() > 0)
            {
                ViewBag.Rents = Rents;
            }
            /* get Rents end */


            /* get Sales start */
            List<PropertyDataTableRowDataModal> Sales = new List<PropertyDataTableRowDataModal>();
            List<Property> getSalesProperties = _context.Properties
                                    .Where(property => property.OwnerLevel == 2 && property.Status == PropertyStatus.Confirmed && property.ServiceType == Property.ServiceTypes.Sale && property.IsDeleted == 0)
                                    .OrderByDescending(property => property.CreatedAt)
                                    .ToList();

            foreach (Property sale_property in getSalesProperties)
            {
                /* check if Contract completed exists start */
                Contract? checkCompletedContractExists = _context.Contracts
                   .Where(contract => contract.PropertyId == sale_property.Id && contract.Status == Contract.ContractStatus.Done && contract.Type == Contract.ContractTypes.Rent && contract.ToDate > DateTime.Today && contract.IsDeleted == 0)
                   .FirstOrDefault();
                /* check if Contract completed exists end */
                if (checkCompletedContractExists == null)
                {
                    string MainImage = "";
                    bool IsHotDeal = false;

                    /* get main image start */
                    RealEstateFile? getMainImage = _context.RealEstateFiles
                             .Where(realEstateFile => realEstateFile.TableId == sale_property.Id && realEstateFile.TableName == "Properties" && realEstateFile.IsDeleted == 0)
                             .FirstOrDefault();
                    if (getMainImage != null)
                    {
                        MainImage = getMainImage.Url;
                    }
                    /* get main image end */

                    /* check is hot deal start */
                    Models.Region? checkIsHotDeal = _context.Regions
                             .Where(region => region.Id == sale_property.RegionId && sale_property.FullTotal < region.DefultPrice && region.IsDeleted == 0)
                             .FirstOrDefault();
                    if (checkIsHotDeal != null)
                    {
                        IsHotDeal = true;
                    }
                    /* check is hot deal end */

                    Sales.Add(new PropertyDataTableRowDataModal
                    {
                        Id = sale_property.Id,
                        City = GetCityName(sale_property.CityId),
                        Region = GetRegionName(sale_property.RegionId),
                        FullTotal = sale_property.FullTotal,
                        Title = sale_property.Title,
                        Type = sale_property.Type,
                        NumberOfBathrooms = sale_property.NumberOfBathrooms,
                        NumberOfRooms = sale_property.NumberOfRooms,
                        PropertySize = sale_property.PropertySize,
                        MainImage = MainImage,
                        IsHotDeal = IsHotDeal,
                    });
                }
            }
            if (Sales.Count() > 0)
            {
                ViewBag.Sales = Sales;
            }
            /* get Sales end */
            return View();
        }
    }
}
