using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using RealEstate.Core;
using RealEstate.DBContext;
using RealEstate.Helper;
using RealEstate.Models;
using static RealEstate.Models.Contract;

namespace RealEstate.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ContractsController : AdminCoreController
    {
        private readonly AppDbContext _context;
        public ContractsController(IConfiguration configuration, AppDbContext dbContext) : base(configuration, dbContext)
        {
            _context = dbContext;
        }

        public IActionResult Index()
        {
            List<ContractDataTableRowDataModal> Contracts = new List<ContractDataTableRowDataModal>();
            List<Models.Contract> getContracts = _context.Contracts
                                    .Where(contract => contract.IsDeleted == 0)
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
                    Customer = GetUserName(contract_data.CustomerId),
                    Provider = GetUserName(contract_data.ProviderId),
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
            Models.Contract? contract_data = _context.Contracts
                            .Where(contract => contract.Id == id && contract.IsDeleted == 0)
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

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Contract? contract_data = _context.Contracts
                            .Where(contract => contract.Id == id && contract.Status != ContractStatus.Canceled && contract.Status != ContractStatus.Done && contract.IsDeleted == 0)
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

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(ContractUpdateActionModel payloadData)
        {
            if (ModelState.IsValid)
            {
                /* check Contract data start */
                Models.Contract? check_contract_exists = _context.Contracts
                                  .Where(contract => contract.Id == payloadData.Id && contract.Status != ContractStatus.Canceled && contract.Status != ContractStatus.Done && contract.IsDeleted == 0)
                                  .FirstOrDefault();
                /* check Contract data end */
                if (check_contract_exists != null)
                {
                    /* check if Contract is done start */
                    if (payloadData.Status == ContractStatus.Done)
                    {
                        /* get */
                        Property? get_property_data = _context.Properties
                                  .Where(property => property.Id == check_contract_exists.PropertyId && property.IsDeleted == 0)
                                  .FirstOrDefault();

                        if (get_property_data != null)
                        {
                            get_property_data.CustomerId = check_contract_exists.CustomerId;
                            if (check_contract_exists.Type == ContractTypes.Sale)
                            {
                                get_property_data.OwnerLevel = 3;
                                get_property_data.OwnerUserId = check_contract_exists.CustomerId;
                            }
                            _context.Properties.Update(get_property_data);
                            _context.SaveChanges();
                        }
                        else
                        {
                            return Json(new { success = false, message = "Something Went Wrong" });
                        }

                    }
                    /* check if Contract is done end */

                    /* set the new values start */
                    check_contract_exists.FromDate = payloadData.FromDate;
                    check_contract_exists.ToDate = payloadData.ToDate;
                    check_contract_exists.Note = payloadData.Note;
                    check_contract_exists.FullTotal = payloadData.FullTotal;
                    check_contract_exists.Status = payloadData.Status;
                    check_contract_exists.LastUpdate = DateTime.Now;
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
                        List<string> documentsUrls = UploadHelper.UploadFiles((List<IFormFile>)formData.Files, "images/Contracts");

                        foreach (string document_url in documentsUrls)
                        {
                            RealEstateFile real_estate_file_data = new RealEstateFile
                            {
                                TableId = check_contract_exists.Id,
                                TableName = "Contracts",
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
                    _context.Contracts.Update(check_contract_exists);
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
    }
    public class ContractUpdateActionModel
    {
        public int Id { get; set; }
        public ContractStatus Status { get; set; }
        public double FullTotal { get; set; }
        public string Note { get; set; }
        public List<int>? DeletedDocumentsIds { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

}
