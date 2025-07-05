using System.Globalization;
using CsvHelper;

namespace BlazorCities;

public class CityDatabase
{
    public CityDatabase()
    {
        using (var reader = new StreamReader("world-cities.csv"))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            Cities.AddRange(csv.GetRecords<City>());
        }
    }

    public List<City> Cities { get; } = [];
}
