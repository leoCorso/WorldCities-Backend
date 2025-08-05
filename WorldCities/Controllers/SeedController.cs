using System.Runtime;
using System.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using WorldCities.Data;
using WorldCities.Data.Models;

namespace WorldCities.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "Administrator")]
    public class SeedController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public SeedController(
            ApplicationDbContext context,
            IWebHostEnvironment env,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration
            )
        {
            _context = context;
            _env = env;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult> Import()
        {
            if (!_env.IsDevelopment())
            {
                throw new SecurityException("Not allowed");
            }
            var path = Path.Combine(_env.ContentRootPath, "Data/Source/worldcities.xlsx");
            using var stream = System.IO.File.OpenRead(path);
            using var excelPackage = new ExcelPackage(stream);

            var worksheet = excelPackage.Workbook.Worksheets[0];

            var nEndRow = worksheet.Dimension.End.Row;

            var numberOfCountriesAdded = 0;
            var numberOfCitiesAdded = 0;

            var countriesByName = _context.Countries
                .AsNoTracking()
                .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

            for (int nRow = 2; nRow < nEndRow; nRow++) {
                var row = worksheet.Cells[nRow, 1, nRow, worksheet.Dimension.End.Column];
                var countryName = row[nRow, 5].GetValue<string>();
                var iso2 = row[nRow, 6].GetValue<string>();
                var iso3 = row[nRow, 7].GetValue<string>();

                if (countriesByName.ContainsKey(countryName))
                {
                    continue;
                }

                var country = new Country
                {
                    Name = countryName,
                    ISO2 = iso2,
                    ISO3 = iso3,
                };

                await _context.Countries.AddAsync(country);

                countriesByName.Add(countryName, country);

                numberOfCountriesAdded++;
            }

            if (numberOfCountriesAdded > 0)
            {
                await _context.SaveChangesAsync();
            }

            var cities = _context.Cities
                .AsNoTracking()
                .ToDictionary(x => (Name: x.Name, Lat: x.Lat, Lon: x.Lon, CountryId: x.CountryId));

            for (int nRow = 2; nRow <= nEndRow; nRow++) { 
                var row = worksheet.Cells[nRow, 1, nRow, worksheet.Dimension.End.Column];
                var name = row[nRow, 1].GetValue<string>();
                var lat = row[nRow, 3].GetValue<decimal>();
                var lon = row[nRow, 4].GetValue<decimal>();
                var countryName = row[nRow, 5].GetValue<string>();

                var countryId = countriesByName[countryName].Id;

                if(cities.ContainsKey((Name: name, Lat: lat, Lon: lon, CountryId: countryId)))
                {
                    continue;
                }

                var city = new City { 
                    Name = name, 
                    Lat = lat, 
                    Lon = lon, 
                    CountryId = countryId
                };

                _context.Cities.Add(city);

                numberOfCitiesAdded++;
            }

            if(numberOfCitiesAdded > 0)
            {
                await _context.SaveChangesAsync();
            }
            return new JsonResult(new
            {
                Cities = numberOfCitiesAdded,
                Countries = numberOfCountriesAdded
            });
        }

        [HttpGet]
        public async Task<ActionResult> CreateDefaultUsers()
        {
            var registeredUser = "RegisteredUser";
            var adminUser = "AdminUser";

            if(await _roleManager.FindByNameAsync(registeredUser) == null)
            {
                await _roleManager.CreateAsync(new IdentityRole(registeredUser));
            }
            if (await _roleManager.FindByNameAsync(adminUser) == null)
            {
                await _roleManager.CreateAsync(new IdentityRole(adminUser));
            }
            var addedUserList = new List<ApplicationUser>();
            var emailAdmin = "admin@email.com";
            if(await _userManager.FindByNameAsync(emailAdmin) == null)
            {
                var admin = new ApplicationUser
                {
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = emailAdmin,
                    Email = emailAdmin
                };
                await _userManager.CreateAsync(admin, _configuration["DefaultPassword:Administrator"]!);
                await _userManager.AddToRolesAsync(admin, [registeredUser, adminUser]);
                admin.EmailConfirmed = true;
                admin.LockoutEnabled = false;
                addedUserList.Add(admin);
            }
            var emailUser = "user@email.com";
            if(await _userManager.FindByNameAsync(emailUser) == null)
            {
                var user = new ApplicationUser
                {
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = emailUser,
                    Email = emailUser
                };
                await _userManager.CreateAsync(user, _configuration["DefaultPassword:RegisteredUser"]!);
                await _userManager.AddToRoleAsync(user, registeredUser);
                user.EmailConfirmed = true;
                user.LockoutEnabled = false;
                addedUserList.Add(user);
            }
            if(addedUserList.Count > 0)
            {
                await _context.SaveChangesAsync();
            }
            return new JsonResult(new
            {
                Count = addedUserList.Count,
                Users = addedUserList
            });
        }
    }
}
