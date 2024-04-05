using RealEstate.Core;
using RealEstate.DBContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RealEstate.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class AjaxController : BaseController
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



        [HttpGet]
        public async Task<IActionResult> GetPropertyName([FromQuery] string term)
        {
            if (!string.IsNullOrEmpty(term))
            {
                int userId = GetUserId();
                if (userId > 0)
                {
                    var properties = await _context.Properties
                             .Where(property => property.Title.Contains(term) && (property.OwnerUserId == userId || property.CustomerId == userId) && property.IsDeleted == 0)
                             .Select(property => new { id = property.Id, text = property.Title })
                             .Take(15)
                             .ToListAsync();

                    return Ok(new { results = properties });
                }
                else
                {
                    return Ok(new { results = Array.Empty<object>() });
                }
            }
            else
            {
                return Ok(new { results = Array.Empty<object>() });
            }
        }
    }
}
