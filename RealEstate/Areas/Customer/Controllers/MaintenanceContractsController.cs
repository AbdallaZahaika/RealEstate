using Microsoft.AspNetCore.Mvc;
using RealEstate.Areas.Admin.Controllers;
using RealEstate.Core;
using RealEstate.DBContext;
using RealEstate.Helper;
using RealEstate.Models;
using static RealEstate.Models.MaintenanceContract;

namespace RealEstate.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class MaintenanceContractsController : CustomerCoreController
    {
        private readonly AppDbContext _context;
        public MaintenanceContractsController(IConfiguration configuration, AppDbContext dbContext) : base(configuration, dbContext)
        {
            _context = dbContext;
        }
        public IActionResult Index()
        {
            int userId = GetUserId();
            List<MaintenanceContractDataTableRowDataModal> MaintenanceContracts = new List<MaintenanceContractDataTableRowDataModal>();
            List<MaintenanceContract> getMaintenanceContracts = _context.MaintenanceContracts
                                    .Where(maintenance_contract => maintenance_contract.CustomerId == userId && maintenance_contract.IsDeleted == 0)
                                    .OrderByDescending(maintenance_contract => maintenance_contract.CreatedAt)
                                    .ToList();

            foreach (MaintenanceContract maintenance_contract_data in getMaintenanceContracts)
            {
                string StatusColor = "";
                if (maintenance_contract_data.Status == MaintenanceContractStatus.Pending)
                {
                    StatusColor = "orange";
                }
                else if (maintenance_contract_data.Status == MaintenanceContractStatus.Canceled)
                {
                    StatusColor = "red";
                }
                else if (maintenance_contract_data.Status == MaintenanceContractStatus.Done)
                {
                    StatusColor = "green";
                }
                else if (maintenance_contract_data.Status == MaintenanceContractStatus.InProcessing)
                {
                    StatusColor = "blue";
                }

                MaintenanceContracts.Add(new MaintenanceContractDataTableRowDataModal
                {
                    Id = maintenance_contract_data.Id,
                    FullTotal = maintenance_contract_data.FullTotal,
                    Title = maintenance_contract_data.Title,
                    Status = maintenance_contract_data.Status,
                    StatusColor = StatusColor,
                    Property = GetPropertyName(maintenance_contract_data.PropertyId),
                    Note = maintenance_contract_data.Note,
                });
            }

            return View(MaintenanceContracts);
        }
        [HttpGet]
        public IActionResult New()
        {
            ViewBag.userId = GetUserId();
            return View();
        }


        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create(MaintenanceContractCreateActionModel payloadData)
        {
            if (ModelState.IsValid)
            {
                int userId = GetUserId();
                /* check Property data start */
                Property? check_property_data = _context.Properties
                                .Where(property => property.IsDeleted == 0 && property.OwnerLevel == 3 && property.OwnerUserId == userId && property.WithMaintenanceContract == true)
                                .FirstOrDefault();
                /* check Property data end */
                if (check_property_data != null)
                {
                    /* check title data start */
                    if (_context.MaintenanceContracts.Any(maintenance_contract => maintenance_contract.Title == payloadData.Title && maintenance_contract.IsDeleted == 0))
                    {
                        return Json(new { success = false, message = "Title already exists. Please choose a different Title." });
                    }
                    /* check title data end */

                    MaintenanceContract maintenance_contract_create_data = new MaintenanceContract
                    {
                        Title = payloadData.Title,
                        PropertyId = payloadData.PropertyId,
                        CustomerId = userId,
                        ProviderId = check_property_data.ProviderId,
                        Note = payloadData.Note,
                        Status = MaintenanceContractStatus.Pending,
                    };

                    /* store data start */
                    _context.MaintenanceContracts.Add(maintenance_contract_create_data);
                    await _context.SaveChangesAsync();
                    /* store data end */

                    if (maintenance_contract_create_data.Id > 0)
                    {
                        var formData = await HttpContext.Request.ReadFormAsync();
                        /* upload Documents start */
                        if (formData.Files.Count > 0)
                        {
                            List<string> documentsUrls = UploadHelper.UploadFiles((List<IFormFile>)formData.Files, "images/maintenanceContracts");

                            foreach (string document_url in documentsUrls)
                            {
                                RealEstateFile real_estate_file_data = new RealEstateFile
                                {
                                    TableId = maintenance_contract_create_data.Id,
                                    TableName = "MaintenanceContracts",
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
        public IActionResult View(int id)
        {
            int userId = GetUserId();
            MaintenanceContract? maintenanceContract = _context.MaintenanceContracts
                            .Where(maintenance_contract => maintenance_contract.CustomerId == userId && maintenance_contract.Id == id && maintenance_contract.IsDeleted == 0)
                            .FirstOrDefault();
            if (maintenanceContract == null)
            {
                return NotFound();
            }

            List<RealEstateFile>? Documents = _context.RealEstateFiles
                   .Where(realEstateFile => realEstateFile.TableId == maintenanceContract.Id && realEstateFile.TableName == "MaintenanceContracts" && realEstateFile.IsDeleted == 0)
                   .ToList();
            ViewBag.Documents = Documents;
            ViewBag.PropertyName = GetPropertyName(maintenanceContract.PropertyId);
            ViewBag.ProviderName = GetUserName(maintenanceContract.ProviderId);
            ViewBag.CustomerName = GetUserName(maintenanceContract.CustomerId);
            return View(maintenanceContract);
        }
    }
}
