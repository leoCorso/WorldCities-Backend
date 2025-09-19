using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using WorldCities.Data.Models;

namespace WorldCities.Data.GraphQL
{
    public class Mutation
    {
        [Serial]
        [Authorize(Roles = ["RegisteredUser"])]
        public async Task<City> AddCity([Service] ApplicationDbContext context, CityDTO cityDTO)
        {
            var city = new City
            {
                Name = cityDTO.Name,
                Lat = cityDTO.Lat,
                Lon = cityDTO.Lon,
                CountryId = cityDTO.CountryId,
            };
            context.Add(city);
            await context.SaveChangesAsync();
            return city;
        }
        [Serial]
        [Authorize(Roles = ["RegisteredUser"])]
        public async Task<Country> AddCountry([Service] ApplicationDbContext context, CountryDTO countryDTO)
        {
            var country = new Country
            {
                Name = countryDTO.Name,
                ISO2 = countryDTO.ISO2,
                ISO3 = countryDTO.ISO3
            };
            context.Add(country);
            await context.SaveChangesAsync();
            return country;
        }

        [Serial]
        [Authorize(Roles = ["RegisteredUser"])]
        public async Task<City> UpdateCountry([Service] ApplicationDbContext context, CityDTO cityDTO)
        {
            var city = await context.Cities.Where(city => city.Id == cityDTO.Id).FirstOrDefaultAsync();
            if (city == null) {
                throw new NotSupportedException();
            }
            city.Name = cityDTO.Name;
            city.Lat = cityDTO.Lat;
            city.Lon = cityDTO.Lon;
            city.CountryId = cityDTO.CountryId;
            context.Cities.Update(city);
            await context.SaveChangesAsync();
            return city;
        }
        [Serial]
        [Authorize(Roles = ["RegisteredUser"])]
        public async Task<City> UpdateCity([Service] ApplicationDbContext context, CityDTO cityDTO)
        {
            var city = await context.Cities.Where(city => city.Id == cityDTO.Id).FirstOrDefaultAsync();
            if (city == null)
            {
                throw new NotSupportedException();
            }
            city.Name = cityDTO.Name;
            city.Lat = cityDTO.Lat;
            city.Lon = cityDTO.Lon;
            city.CountryId = cityDTO.CountryId;
            context.Cities.Update(city);
            await context.SaveChangesAsync();
            return city;
        }
    }
}
