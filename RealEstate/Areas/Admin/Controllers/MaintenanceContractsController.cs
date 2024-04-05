using RealEstate.Core;
using RealEstate.DBContext;
using RealEstate.Models;
using Microsoft.AspNetCore.Mvc;
using static RealEstate.Models.MaintenanceContract;
using RealEstate.Helper;
namespace RealEstate.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MaintenanceContractsController : AdminCoreController
    {
        private readonly AppDbContext _context;
        public MaintenanceContractsController(IConfiguration configuration, AppDbContext dbContext) : base(configuration, dbContext)
        {
            _context = dbContext;
        }

        public IActionResult Index()
        {

            List<MaintenanceContractDataTableRowDataModal> MaintenanceContracts = new List<MaintenanceContractDataTableRowDataModal>();
            List<MaintenanceContract> getMaintenanceContracts = _context.MaintenanceContracts
                                    .Where(maintenance_contract => maintenance_contract.IsDeleted == 0)
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
                    Customer = GetUserName(maintenance_contract_data.CustomerId),
                    Provider = GetUserName(maintenance_contract_data.ProviderId),
                    Note = maintenance_contract_data.Note,
                });
            }


            return View(MaintenanceContracts);
        }

        [HttpGet]
        public IActionResult New()
        {
            return View();
        }


        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create(MaintenanceContractCreateActionModel payloadData)
        {
            if (ModelState.IsValid)
            {
                /* check Customer data start */
                User? check_customer_data = _context.Users
                                .Where(user => user.IsDeleted == 0 && user.Level == 3 && user.Id == payloadData.CustomerId)
                                .FirstOrDefault();
                /* check Customer data end */
                if (check_customer_data != null)
                {
                    /* check Property data start */
                    Property? check_property_data = _context.Properties
                                    .Where(property => property.IsDeleted == 0 && (property.OwnerUserId == check_customer_data.Id || property.CustomerId == check_customer_data.Id) && property.WithMaintenanceContract == true && property.OwnerLevel == 3)
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
                            CustomerId = payloadData.CustomerId,
                            ProviderId = check_property_data.ProviderId,
                            FullTotal = payloadData.FullTotal,
                            Note = payloadData.Note,
                            Status = payloadData.Status,
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
            MaintenanceContract? maintenanceContract = _context.MaintenanceContracts
                            .Where(maintenance_contract => maintenance_contract.Id == id && maintenance_contract.IsDeleted == 0)
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

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(MaintenanceContractUpdateActionModel payloadData)
        {
            if (ModelState.IsValid)
            {
                /* check if maintenance contract exists start */
                MaintenanceContract? check_maintenance_contract_exists = _context.MaintenanceContracts
                .Where(maintenance_contract => maintenance_contract.Id == payloadData.Id && maintenance_contract.IsDeleted == 0)
                .FirstOrDefault();
                if (check_maintenance_contract_exists == null)
                {
                    return Json(new { success = false, message = "Something Went Wrong" });
                }
                /* check if maintenance contract exists end */

                /* check Customer data start */
                User? check_customer_data = _context.Users
                                .Where(user => user.IsDeleted == 0 && user.Level == 3 && user.Id == check_maintenance_contract_exists.CustomerId)
                                .FirstOrDefault();
                /* check Customer data end */
                if (check_customer_data != null)
                {
                    /* check Property data start */
                    Property? check_property_data = _context.Properties
                                    .Where(property => property.IsDeleted == 0 && (property.OwnerUserId == check_customer_data.Id || property.CustomerId == check_customer_data.Id) && property.OwnerLevel == 3)
                                    .FirstOrDefault();
                    /* check Property data end */
                    if (check_property_data != null)
                    {
                        /* check title data start */
                        if (_context.MaintenanceContracts.Any(maintenance_contract => maintenance_contract.Id != check_maintenance_contract_exists.Id && maintenance_contract.Title == payloadData.Title && maintenance_contract.IsDeleted == 0))
                        {
                            return Json(new { success = false, message = "Title already exists. Please choose a different Title." });
                        }
                        /* check title data end */

                        /* set the new values start */
                        check_maintenance_contract_exists.Note = payloadData.Note;
                        check_maintenance_contract_exists.FullTotal = payloadData.FullTotal;
                        check_maintenance_contract_exists.Title = payloadData.Title;
                        check_maintenance_contract_exists.Status = payloadData.Status;
                        check_maintenance_contract_exists.LastUpdate = DateTime.Now;
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
                            List<string> documentsUrls = UploadHelper.UploadFiles((List<IFormFile>)formData.Files, "images/maintenanceContracts");

                            foreach (string document_url in documentsUrls)
                            {
                                RealEstateFile real_estate_file_data = new RealEstateFile
                                {
                                    TableId = check_maintenance_contract_exists.Id,
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

                        /* update data start */
                        _context.MaintenanceContracts.Update(check_maintenance_contract_exists);
                        _context.SaveChanges();

                        /* update data end */


                        return Json(new { success = true, message = "Updated successfully" });
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
            MaintenanceContract? check_maintenance_contract_exists = _context.MaintenanceContracts
                                .Where(p => p.IsDeleted == 0 && p.Id == id)
                                .FirstOrDefault();
            if (check_maintenance_contract_exists == null)
            {
                return Json(new { success = false, message = "Something Went Wrong" });
            }
            else
            {
                check_maintenance_contract_exists.IsDeleted = 1;
                _context.MaintenanceContracts.Update(check_maintenance_contract_exists);
                _context.SaveChanges();
                return Json(new { success = true, message = "Record updated successfully" });
            }

        }
    }

    public class MaintenanceContractCreateActionModel
    {
        public string Title { get; set; }
        public string Note { get; set; }
        public double FullTotal { get; set; }
        public MaintenanceContractStatus Status { get; set; }
        public int PropertyId { get; set; }
        public int CustomerId { get; set; }
    }


    public class MaintenanceContractUpdateActionModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Note { get; set; }
        public double FullTotal { get; set; }
        public List<int>? DeletedDocumentsIds { get; set; }
        public MaintenanceContractStatus Status { get; set; }
    }
}
