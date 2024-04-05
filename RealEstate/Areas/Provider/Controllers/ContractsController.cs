using Microsoft.AspNetCore.Mvc;
using RealEstate.Core;
using RealEstate.DBContext;
using RealEstate.Models;
using static RealEstate.Models.Contract;

namespace RealEstate.Areas.Provider.Controllers
{
    [Area("Provider")]
    public class ContractsController : ProviderCoreController
    {
        private readonly AppDbContext _context;
        public ContractsController(IConfiguration configuration, AppDbContext dbContext) : base(configuration, dbContext)
        {
            _context = dbContext;
        }
        public IActionResult Index()
        {
            int userId = GetUserId();
            List<ContractDataTableRowDataModal> Contracts = new List<ContractDataTableRowDataModal>();
            List<Models.Contract> getContracts = _context.Contracts
                                    .Where(contract => contract.ProviderId == userId && contract.IsDeleted == 0)
                                    .OrderByDescending(contract => contract.CreatedAt)
                                    .ToList();

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
                    Provider = GetUserName(contract_data.ProviderId),
                    Customer = GetUserName(contract_data.CustomerId),
                    Note = contract_data.Note,
                    FromDate = contract_data.FromDate,
                    ToDate = contract_data.ToDate,
                });
            }

            return View(Contracts);
        }


        [HttpGet]
        public IActionResult Show(int id)
        {
            int userId = GetUserId();
            Models.Contract? contract_data = _context.Contracts
                            .Where(contract => contract.Id == id && contract.ProviderId == userId && contract.IsDeleted == 0)
                            .FirstOrDefault();
            if (contract_data != null)
            {
                List<RealEstateFile>? Documents = _context.RealEstateFiles
                                                       .Where(realEstateFile => realEstateFile.TableId == contract_data.Id && realEstateFile.TableName == "Contracts" && realEstateFile.IsDeleted == 0)
                                                       .ToList();
                ViewBag.Documents = Documents;

                ViewBag.Property = GetPropertyName(contract_data.PropertyId);
                ViewBag.Provider = GetUserName(contract_data.ProviderId);
                ViewBag.Customer = GetUserName(contract_data.CustomerId);
                return View(contract_data);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
