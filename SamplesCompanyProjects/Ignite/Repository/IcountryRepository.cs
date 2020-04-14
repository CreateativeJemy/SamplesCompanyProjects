using Ignite.Areas.Office.Models;
using Ignite.Areas.Office.ViewModels;
using System.Collections.Generic;

namespace Ignite.Areas.Office.Repository
{
    public interface IcountryRepository
    {
        IEnumerable<CountryVM> Countrys(); 
        IEnumerable<CountryVM> GetCountrys();
        IEnumerable<CountryVM> GetCountrysLevels();
        IEnumerable<CountryVM> GetCountrysGrades();
        Country GetCountryByID(int CountryId);
        void CreateCountry(Country Country);
        Country UpdateCountry(CountryVM Country);
    }
}
