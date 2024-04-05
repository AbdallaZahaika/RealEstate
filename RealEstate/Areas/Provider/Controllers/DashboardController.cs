using RealEstate.Core;
using RealEstate.DBContext;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Models;
using static RealEstate.Models.Contract;

namespace RealEstate.Areas.Provider.Controllers
{
    [Area("Provider")]
    public class DashboardController : ProviderCoreController
    {
        private readonly AppDbContext _context;
        public DashboardController(IConfiguration configuration, AppDbContext dbContext) : base(configuration, dbContext)
        {
            _context = dbContext;
        }

        public IActionResult Index()
        {
            int userId = GetUserId();
            /* get header cards data start */
            ViewBag.count_of_properties = _context.Properties
                            .Where(property => property.OwnerLevel == 2 && property.OwnerUserId == userId && property.IsDeleted == 0)
                            .Count();
            ViewBag.count_of_properties_sale = _context.Properties
                     .Where(property => property.OwnerLevel == 2 && property.OwnerUserId == userId && property.ServiceType == Property.ServiceTypes.Sale && property.IsDeleted == 0)
                     .Count();
            ViewBag.count_of_properties_rent = _context.Properties
                      .Where(property => property.OwnerLevel == 2 && property.OwnerUserId == userId && property.ServiceType == Property.ServiceTypes.Rent && property.IsDeleted == 0)
                      .Count();
            /* get header cards data end */

            /* get Contracts start */
            List<ContractDataTableRowDataModal> Contracts = new List<ContractDataTableRowDataModal>();
            List<Models.Contract> getContracts = _context.Contracts
                                    .Where(contract => contract.ProviderId == userId && contract.IsDeleted == 0)
                                    .OrderByDescending(contract => contract.CreatedAt)
                                    .ToList();
            ViewBag.count_of_contracts = getContracts.Count();
            foreach (Models.Contract contract_data in getContracts)
            {
                string StatusColor = "";
                if (contract_data.Status == ContractStatus.Pending)
                {
                    StatusColor = "orange";
                }
                else if (contract_data.Status == ContractStatus.Canceled)
                {
                    StatusColor = "red";
                }
                else if (contract_data.Status == ContractStatus.Done)
                {
                    StatusColor = "green";
                }
                else if (contract_data.Status == ContractStatus.InProcessing)
                {
                    StatusColor = "blue";
                }

                Contracts.Add(new ContractDataTableRowDataModal
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
            /* get Contracts end */
            return View();
        }
    }
}
