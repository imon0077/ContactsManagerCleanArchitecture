using Entities;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using System;
using EntityFrameworkCoreMock;
using Moq;
using Microsoft.EntityFrameworkCore;

namespace ContactsManagerTests
{
    public class CountriesServiceTest
    {
        private readonly ICountriesService _countriesService;

        //constructor
        public CountriesServiceTest()
        {
            var countriesInitialData = new List<Country>() { };

            DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>(
                new DbContextOptionsBuilder<ApplicationDbContext>().Options    
            );

            ApplicationDbContext dbContext = dbContextMock.Object;

            dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData);

            _countriesService = new CountriesService(null);
        }

        #region AddCountry
        //When CountryAddRequest is null, it should throw ArgumentNullExeption
        [Fact]
        public async Task AddCountry_Nullcountry()
        {
            //Arrange
            CountryAddRequest? request = null;

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => {
                //Act
                await _countriesService.AddCountry(request);
            });
        }

        //When the Country is null, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_CountryNameIsNull()
        {
            //Arrange
            CountryAddRequest? request = new CountryAddRequest() { CountryName = null};

            //Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => {
                //Act
               await _countriesService.AddCountry(request);
            });
        }

        //When the CountryName is duplicate, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_DuplicateCountryName()
        {
            //Arrange
            CountryAddRequest? request1 = new CountryAddRequest() { CountryName = "USA" };
            CountryAddRequest? request2 = new CountryAddRequest() { CountryName = "USA" };

            //Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => {
                //Act
                await _countriesService.AddCountry(request1);
                await _countriesService.AddCountry(request2);
            });
        }

        //When you supply proper  country name, it should insert (add) the country to the existing list.
        [Fact]
        public async Task AddCountry_ProperCountryDetails()
        {
            //Arrange
            CountryAddRequest? request = new CountryAddRequest() { CountryName = "Japan" };

            //Act
            CountryResponse response = await _countriesService.AddCountry(request);
            List<CountryResponse> countries_from_GetAllCountries = await _countriesService.GetAllCountries();

            //Assert
            Assert.True(response.CountryID != Guid.Empty);
            Assert.Contains(response, countries_from_GetAllCountries);
        }
        #endregion AddCountry

        #region GetAllCountries

        [Fact]
        //List of countries should be empty by default (before adding any)
        public async Task GetAllCountries_EmptyList()
        {
            //Acts
            List<CountryResponse> actual_countries_response_list = await _countriesService.GetAllCountries();

            //Assert
            Assert.Empty(actual_countries_response_list);
        }

        [Fact]
        public async Task GetAllCountries_AddFewCountries()
        {
            //Arrange
            List<CountryAddRequest> country_request_list = new List<CountryAddRequest>()
            {
                new CountryAddRequest() {CountryName = "USA"},
                new CountryAddRequest(){CountryName = "UK"}
            };

            //Act
            List<CountryResponse> countries_list_from_add_country = new List<CountryResponse>();
            foreach (CountryAddRequest country_request in country_request_list)
            {
                countries_list_from_add_country.Add( await _countriesService.AddCountry(country_request));
            }

            List<CountryResponse> actualcountryResponseList = await _countriesService.GetAllCountries();

            //read each element from countries_list_from_add_country
            foreach(CountryResponse expected_country in countries_list_from_add_country)
            {
                Assert.Contains(expected_country, actualcountryResponseList);
            }
        }

        #endregion GetAllCountries

        #region GetCountryByCountryID

        //If we supply null as CountryID, it should return null as CountryResponse
        [Fact]
        public async Task GetCountryByCountryID_NullCountryID()
        {
            //Arrange
            Guid? countryID = null;

            //Act
            CountryResponse? country_response_from_get_method = await _countriesService.GetCountryByCountryID(countryID);

            //Assert
            Assert.Null(country_response_from_get_method);
        }


        //If we supply a valid country id, it should return the matching country details as CountryResponse object
        [Fact]
        public async Task GetCountryByCountryID_ValidCountryID()
        {
            //Arrange
            CountryAddRequest? country_add_request = new CountryAddRequest()
            {
                CountryName = "China"
            };

            CountryResponse? country_response_from_add = await _countriesService.AddCountry(country_add_request);

            //Act
            CountryResponse? country_response_from_get = await _countriesService.GetCountryByCountryID(country_response_from_add.CountryID);

            //Assert
            Assert.Equal(country_response_from_add, country_response_from_get);
        }

        #endregion
    }
}
