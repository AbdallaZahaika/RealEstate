using RealEstate.Core;
using RealEstate.DBContext;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Models;
using static RealEstate.Models.Property;

namespace RealEstate.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : AdminCoreController
    {
        private readonly AppDbContext _context;
        public DashboardController(IConfiguration configuration, AppDbContext dbContext) : base(configuration, dbContext)
        {
            _context = dbContext;
        }

        public IActionResult Index()
        {
            /* get header cards data start */
            ViewBag.count_of_properties = _context.Properties
                            .Where(property => property.IsDeleted == 0)
                            .Count();
            ViewBag.count_of_properties_sale = _context.Properties
                     .Where(property => property.ServiceType == Property.ServiceTypes.Sale && property.IsDeleted == 0)
                     .Count();
            ViewBag.count_of_properties_rent = _context.Properties
                      .Where(property => property.ServiceType == Property.ServiceTypes.Rent && property.IsDeleted == 0)
                      .Count();
            ViewBag.count_of_providers = _context.Users
                       .Where(user => user.Level == 2 && user.IsDeleted == 0)
                       .Count();
            ViewBag.count_of_customers = _context.Users
                        .Where(user => user.Level == 3 && user.IsDeleted == 0)
                        .Count();
            /* get header cards data end */

            /* get Properties data start */
            List<PropertyDataTableRowDataModal> Properties = new List<PropertyDataTableRowDataModal>();
            List<Property> getProperties = _context.Properties
                                    .Where(property => property.Status == PropertyStatus.Pending && property.IsDeleted == 0)
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
            ViewBag.Properties = Properties;
            /* get Properties data end */

            /* get Contracts data start */
            List<Models.Contract.ContractDataTableRowDataModal> Contracts = new List<Models.Contract.ContractDataTableRowDataModal>();
            List<Models.Contract> getContracts = _context.Contracts
                                    .Where(contract => contract.Status != Contract.ContractStatus.Canceled && contract.Status != Contract.ContractStatus.Done && contract.IsDeleted == 0)
                                    .OrderByDescending(contract => contract.CreatedAt)
                                    .ToList();
            foreach (Models.Contract contract_data in getContracts)
            {
                string StatusColor = "";
                if (contract_data.Status == Models.Contract.ContractStatus.Pending)
                {
                    StatusColor = "orange";
                }
                else if (contract_data.Status == Models.Contract.ContractStatus.Canceled)
                {
                    StatusColor = "red";
                }
                else if (contract_data.Status == Models.Contract.ContractStatus.Done)
                {
                    StatusColor = "green";
                }
                else if (contract_data.Status == Models.Contract.ContractStatus.InProcessing)
                {
                    StatusColor = "blue";
                }

                Contracts.Add(new Models.Contract.ContractDataTableRowDataModal
                {
                    Id = contract_data.Id,
                    FullTotal = contract_data.FullTotal,
                    Status = contract_data.Status,
                    StatusColor = StatusColor,
                    Type = contract_data.Type,
                    Property = GetPropertyName(contract_data.PropertyId),
                    Customer = GetUserName(contract_data.CustomerId),
                    Provider = GetUserName(contract_data.ProviderId),
                    Note = contract_data.Note,
                    FromDate = contract_data.FromDate,
                    ToDate = contract_data.ToDate,
                });
            }
            ViewBag.Contracts = Contracts;
            /* get Contracts data end */
            return View();
        }
    }
}
