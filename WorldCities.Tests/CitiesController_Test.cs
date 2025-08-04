using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorldCities.Data;
using Microsoft.EntityFrameworkCore.InMemory;
using WorldCities.Data.Models;
using WorldCities.Controllers;

namespace WorldCities.Tests
{
    public class CitiesController_Test
    {
        // Test the GetCity() method
        [Fact]
        public async Task GetCity()
        {
            // Arrange
            // Configure options and context
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "WorldCities")
                .Options;
            using var context = new ApplicationDbContext(options);

            // Create a city
            context.Add(new City()
            {
                Id = 1,
                CountryId = 1,
                Name = "TestCity1",
                Lat = 1,
                Lon = 1
            });
            context.SaveChanges();

            // Arrange controller
            var controller = new CitiesController(context);
            City? city_existing = null;
            City? city_not_existing = null;

            // Act
            city_existing = (await controller.GetCity(1)).Value;
            city_not_existing = (await controller.GetCity(2)).Value;


            // Assert
            Assert.NotNull(city_existing);
            Assert.Null(city_not_existing);
        }
    }
}
