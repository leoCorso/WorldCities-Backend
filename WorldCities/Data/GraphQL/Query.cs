using WorldCities.Data.Models;

namespace WorldCities.Data.GraphQL
{
    public class Query
    {
        [Serial]
        [UsePaging]
        [UseFiltering]
        [UseSorting]
        public IQueryable<City> GetCities([Service] ApplicationDbContext context) => context.Cities;
        
        [Serial]
        [UsePaging]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Country> GetCountriesw([Service] ApplicationDbContext context) => context.Countries;
    }
}
