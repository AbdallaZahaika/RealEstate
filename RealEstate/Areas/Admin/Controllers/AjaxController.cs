using RealEstate.Core;
using RealEstate.DBContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstate.Models;

namespace RealEstate.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AjaxController : AdminCoreController
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
        public async Task<IActionResult> GetOwnerName([FromQuery] string term, [FromQuery] int OwnerLevel)
        {
            if (!string.IsNullOrEmpty(term) && (OwnerLevel == 2 || OwnerLevel == 3))
            {
                var users = await _context.Users
                    .Where(user => user.Name.Contains(term) && user.Level == OwnerLevel && user.IsDeleted == 0)
                    .Select(user => new { id = user.Id, text = user.Name })
                    .Take(15)
                    .ToListAsync();

                return Ok(new { results = users });
            }
            else
            {
                return Ok(new { results = Array.Empty<object>() });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetPropertyNameByOwner([FromQuery] string term, [FromQuery] int OwnerId)
        {
            if (!string.IsNullOrEmpty(term) && OwnerId > 0)
            {
                var properties = await _context.Properties
                    .Where(property => property.Title.Contains(term) && (property.OwnerUserId == OwnerId || property.CustomerId == OwnerId) && property.IsDeleted == 0)
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
    }
}
