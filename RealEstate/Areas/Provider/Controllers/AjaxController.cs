using RealEstate.Core;
using RealEstate.DBContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstate.Models;

namespace RealEstate.Areas.Provider.Controllers
{
    [Area("Provider")]
    public class AjaxController : ProviderCoreController
    {
        private readonly AppDbContext _context;
        public AjaxController(IConfiguration configuration, AppDbContext dbContext) : base(configuration, dbContext)
        {
            _context = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetCities([FromQuery] string term, [FromQuery] int regionId)
        {
            if (!string.IsNullOrEmpty(term) && regionId > 0)
            {
                var cities = await _context.Cities
                    .Where(city => city.Title.Contains(term) && city.RegionId == regionId && city.IsDeleted == 0)
                    .Select(city => new { id = city.Id, text = city.Title })
                    .Take(15)
                    .ToListAsync();

                return Ok(new { results = cities });
            }
            else
            {
                return Ok(new { results = Array.Empty<object>() });
            }
        }
    }
}
