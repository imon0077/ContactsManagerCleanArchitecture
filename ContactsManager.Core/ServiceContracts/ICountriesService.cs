using Microsoft.AspNetCore.Http;
using ServiceContracts.DTO;

namespace ServiceContracts
{
    /// <summary>
    /// Represents business logic for manipulating country entity
    /// </summary>
    public interface ICountriesService
    {
        /// <summary>
        /// Adds a country object to a list of countries
        /// </summary>
        /// <param name="countryAddRequest">Country object to add</param>
        /// <returns>Return the country object after adding it(incliding newly generated country id)</returns>
        Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest);

        /// <summary>
        /// Return all countries from the list
        /// </summary>
        /// <returns>All countries from the list as list of CountryResponse </returns>
        Task<List<CountryResponse>> GetAllCountries();

        /// <summary>
        /// Returns a country object based on the given country id
        /// </summary>
        /// <param name="countryID">CountryID(Guid) to search</param>
        /// <returns>Matching Country as CountryResponse Object</returns>
        Task<CountryResponse?> GetCountryByCountryID(Guid? countryID);

        /// <summary>
        /// Upload Countries form excel file into db
        /// </summary>
        /// <param name="formFile">Excel file with list of countries</param>
        /// <returns>number rows inserted</returns>
        Task<int> UploadCountriesFromExcel(IFormFile formFile);
    }

}