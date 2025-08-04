using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WorldCities.Controllers;
using WorldCities.Data;
using WorldCities.Data.Models;

namespace WorldCities.Tests
{
    public class CountriesController_Test
    {
        [Fact]
        public async Task GetCountries()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "WorldCities")
                .Options;
            var context = new ApplicationDbContext(options);

            var logger = NullLogger<CountriesController>.Instance;

            var controller = new CountriesController(context, logger);

            Country? not_empty_country = null;
            Country? empty_country = null;

            context.Add(new Country
            {
                Id = 1,
                ISO2 = "T1",
                ISO3 = "TE1",
                Name = "TestCountry1",
            });

            context.Add(new Country
            {
                Id = 2,
                ISO2 = "T2",
                ISO3 = "TE3",
                Name = "TestCountry2"
            });

            context.SaveChanges();

            // Act

            not_empty_country = (await controller.GetCountry(1)).Value;
            empty_country = (await controller.GetCountry(0)).Value;

            // Assert

            Assert.NotNull(not_empty_country);
            Assert.Null(empty_country);

        }

    }
}
