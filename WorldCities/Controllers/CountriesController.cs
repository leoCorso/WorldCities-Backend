using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldCities.Data;
using WorldCities.Data.Models;
using Microsoft.AspNetCore.Authorization;

namespace WorldCities.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CountriesController> _logger;
        public CountriesController(ApplicationDbContext context, ILogger<CountriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Countries
        [HttpGet]
        public async Task<ActionResult<ApiResult<CountryDTO>>> GetCountries(
            int pageIndex = 0, 
            int pageSize = 10, 
            string? sortColumn = null, 
            string? sortOrder = null,
            string? filterColumn = null,
            string? filterQuery = null)
        {
            _logger.LogInformation("Hello World");

            return await ApiResult<CountryDTO>.CreateAsync(
                _context.Countries.AsNoTracking().Select(country => new CountryDTO(){ 
                    Id = country.Id, 
                    Name = country.Name, 
                    ISO2 = country.ISO2, 
                    ISO3 = country.ISO3, 
                    TotCities = country.Cities!.Count}), 
                pageIndex, pageSize, sortColumn, sortOrder, filterColumn, filterQuery);
        }

        // GET: api/Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Country>> GetCountry(int id)
        {
            var country = await _context.Countries.FindAsync(id);

            if (country == null)
            {
                return NotFound();
            }

            return country;
        }

        // PUT: api/Countries/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "RegisteredUser")]
        public async Task<IActionResult> PutCountry(int id, Country country)
        {
            if (id != country.Id)
            {
                return BadRequest();
            }

            _context.Entry(country).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CountryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Countries
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "RegisteredUser")]
        [HttpPost]
        public async Task<ActionResult<Country>> PostCountry(Country country)
        {
            _context.Countries.Add(country);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCountry", new { id = country.Id }, country);
        }

        // DELETE: api/Countries/5
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("IsDupeCountry")]
        public bool IsDupeCountry(Country country)
        {
            return _context.Countries.Any(existingCountry =>
                existingCountry.Id != country.Id
                &&
                existingCountry.Name == country.Name
                &&
                existingCountry.ISO2 == country.ISO2
                &&
                existingCountry.ISO3 == country.ISO3);
        }

        [HttpGet("IsDupeField")]
        public bool IsDupeField(string fieldName, string fieldValue, int countryId)
        {
            switch (fieldName)
            {
                case "name":
                    return _context.Countries.Any(existingCountry => existingCountry.Name == fieldValue && countryId != existingCountry.Id);
                case "iso2":
                    return _context.Countries.Any(existingCountry => existingCountry.ISO2 == fieldValue && existingCountry.Id != countryId);
                case "iso3":
                    return _context.Countries.Any(existingCountry => existingCountry.ISO3 == fieldValue && existingCountry.Id != countryId);
                default:
                    return false;
            }
        }
        
        private bool CountryExists(int id)
        {
            return _context.Countries.Any(e => e.Id == id);
        }
    }
}
