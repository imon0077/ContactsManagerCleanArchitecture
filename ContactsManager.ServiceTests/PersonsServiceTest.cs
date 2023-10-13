using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using ServiceContracts.Enums;
using Xunit.Abstractions;
using Entities;
using Microsoft.EntityFrameworkCore;
using EntityFrameworkCoreMock;
using AutoFixture;
using Moq;
using RepositoryContracts;
using System.Linq.Expressions;
using Serilog;
using Microsoft.Extensions.Logging;

namespace ContactsManagerTests
{
    public class PersonsServiceTest
    {
        private readonly IPersonsService _personsService;
        private readonly Mock<IPersonsRepository> _personsRepositoryMock;
        private readonly IPersonsRepository _personsRepository;

        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;

        public PersonsServiceTest(ITestOutputHelper testOutputHelper)
        {
            _fixture = new Fixture();
            _personsRepositoryMock = new Mock<IPersonsRepository>();
            _personsRepository = _personsRepositoryMock.Object;
            var diagnosticContextMock = new Mock<IDiagnosticContext>();
            var loggerMock = new Mock<ILogger<PersonsService>>();

            _personsService = new PersonsService(_personsRepository, loggerMock.Object, diagnosticContextMock.Object);
            
            _testOutputHelper = testOutputHelper;
        }

        #region AddPerson
        [Fact]
        public async Task AddPerson_NullPerson_ToBeArgumentNullException()
        {
            //Arrange
            PersonAddRequest? request = null;

            //Act
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _personsService.AddPerson(request);
            });
        }

        [Fact]
        public async Task AddPerson_PersonNameIsNull_ToBeArgumentException()
        {
            //Arrange
            PersonAddRequest request = new PersonAddRequest { PersonName = null };

            Person person = request.ToPerson();

            //When PersonsRepository.Add is called, it has to return the same "person" object
            _personsRepositoryMock
                .Setup(temp => temp.Add(It.IsAny<Person>()))
                .ReturnsAsync(person);

            //Act
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
               await _personsService.AddPerson(request);
            });
        }

        [Fact]
        public async Task AddPerson_FullPersonDetails_ToBeSuccessful()
        {
            //Arrange
            PersonAddRequest request = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "someone@email.com")
                .Create();

            //PersonAddRequest request = new PersonAddRequest
            //{
            //    PersonName = "Person name...",
            //    Email = "person@email.com",
            //    Address = "Sample Address ",
            //    CountryID = Guid.NewGuid(),
            //    Gender = GenderOptions.Male,
            //    DateOfBirth = DateTime.Parse("2000-01-01"),
            //    ReceiveNewsLetters = true
            //};

            Person person = request.ToPerson();
            PersonResponse person_resonse_expected = person.ToPersonResponse();

            _personsRepositoryMock.Setup(temp => temp.Add(It.IsAny<Person>()))
                .ReturnsAsync(person);

            //Act
            PersonResponse person_response_from_add = await _personsService.AddPerson(request);
            person_resonse_expected.PersonID = person_response_from_add.PersonID;

            //List<PersonResponse> person_list = await _personsService.GetAllPersons();

            //Assert
            Assert.True(person_response_from_add.PersonID != Guid.Empty);

            Assert.Equal(person_response_from_add, person_resonse_expected);
        }

        #endregion

        #region GetPersonByPersonID

        [Fact]
        public async Task GetPersonByPersonID_NullPersonID_ToBeNull()
        {
            //Arrange
            Guid? PersonID = null;

            //Act
            PersonResponse? personResponse_from_get = await _personsService.GetPersonByPersonID(PersonID);

            //Assert
            Assert.Null(personResponse_from_get);

        }

        [Fact]
        public async Task GetPersonByPersonID_WithPersonID_ToBeSuccessful()
        {
            //Arrange
            //CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();
            //CountryResponse countryResponse = await _countriesService.AddCountry(countryAddRequest);

            Person person = _fixture.Build<Person>()
                .With(temp => temp.Email, "someone@email.com")
                .With(temp => temp.Country, null as Country)
                .Create();

            PersonResponse person_response_expected = person.ToPersonResponse();

            _personsRepositoryMock.Setup(temp => temp.GetById(It.IsAny<Guid>())).ReturnsAsync(person);

            //Act
            //PersonResponse personResponse_from_add = await _personsService.AddPerson(person);

            PersonResponse? personResponse_from_get = await _personsService.GetPersonByPersonID(person.PersonID);

            //Assert
            Assert.Equal(person_response_expected, personResponse_from_get);
        }

        #endregion

        #region GetAllPersons

        //The GetAllPersons() should return an empty list by defualt
        [Fact]
        public async Task GetAllPersons_ToBeEmptyList()
        {
            //Arrange
            _personsRepositoryMock
                .Setup(temp => temp.GetAll())
                .ReturnsAsync(new List<Person>());

            //Act
            List<PersonResponse> personResponses = await _personsService.GetAllPersons();

            //Assert
            Assert.Empty(personResponses);
        }

        //First, when we add some persons, it should return all
        [Fact]
        public async Task GetAllPersons_WithFewPersons_ToBeSuccessful()
        {
            //Arrange
            List<Person> persons = new List<Person>() {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@example.com")
                .With(temp => temp.Country, null as Country)
                .Create()
            };

            List<PersonResponse> person_response_list_expected = persons.Select(temp => temp.ToPersonResponse()).ToList();

            //print personResponses using ItestOutputHelper
            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse item in person_response_list_expected)
            {
                _testOutputHelper.WriteLine(item.ToString());
            }

            _personsRepositoryMock.Setup(temp => temp.GetAll()).ReturnsAsync(persons);

            //Act
            List<PersonResponse> persons_list_from_get = await _personsService.GetAllPersons();

            //print persons_list_from_get using ItestOutputHelper
            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse item in persons_list_from_get)
            {
                _testOutputHelper.WriteLine(item.ToString());
            }

            //Assert
            foreach (PersonResponse person_response_from_add in person_response_list_expected)
            {
                Assert.Contains(person_response_from_add, persons_list_from_get);
            }
        }

        #endregion GetAllPersons

        #region GetFilteredPersons

        //If search text is empty, its should return all persons
        [Fact]
        public async Task GetFilteredPersons_EmptySearchText_ToBeSuccessful()
        {
            //Arrange
            List<Person> persons = new List<Person>() {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@example.com")
                .With(temp => temp.Country, null as Country)
                .Create()
            };

            List<PersonResponse> person_response_list_expected = persons.Select(temp => temp.ToPersonResponse()).ToList();


            //print personResponses using ItestOutputHelper
            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse item in person_response_list_expected)
            {
                _testOutputHelper.WriteLine(item.ToString());
            }

            _personsRepositoryMock
                .Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(persons);

            //Act
            List<PersonResponse> persons_list_from_search = await _personsService.GetFilteredPersons(nameof(Person.PersonName), "");

            //print persons_list_from_get using ItestOutputHelper
            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse item in persons_list_from_search)
            {
                _testOutputHelper.WriteLine(item.ToString());
            }

            //Assert
            foreach (PersonResponse person_response_from_add in person_response_list_expected)
            {
                Assert.Contains(person_response_from_add, persons_list_from_search);
            }
        }

        //Search based on personame & search string, its should return all matching persons
        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName_ToBeSuccessful()
        {
            //Arrange
            List<Person> persons = new List<Person>() {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@example.com")
                .With(temp => temp.Country, null as Country)
                .Create()
            };

            List<PersonResponse> person_response_list_expected = persons.Select(temp => temp.ToPersonResponse()).ToList();


            //print personResponses using ItestOutputHelper
            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse item in person_response_list_expected)
            {
                _testOutputHelper.WriteLine(item.ToString());
            }

            _personsRepositoryMock
                .Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(persons);

            //Act
            List<PersonResponse> persons_list_from_search = await _personsService.GetFilteredPersons(nameof(Person.PersonName), "sa");

            //print persons_list_from_get using ItestOutputHelper
            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse item in persons_list_from_search)
            {
                _testOutputHelper.WriteLine(item.ToString());
            }

            //Assert
            foreach (PersonResponse person_response_from_add in person_response_list_expected)
            {
                Assert.Contains(person_response_from_add, persons_list_from_search);
            }
        }

        #endregion GetFilteredPersons

        #region GetSortedPersons

        //When we sort based on person name in DESC, it should return person list in descending order
        [Fact]
        public async Task GetSortedPersons_ToBeSuccessful()
        {
            //Arrange
            List<Person> persons = new List<Person>() {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@example.com")
                .With(temp => temp.Country, null as Country)
                .Create()
            };

            List<PersonResponse> person_response_list_expected = persons.Select(temp => temp.ToPersonResponse()).ToList();

            //List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();

            _personsRepositoryMock.Setup(temp => temp.GetAll()).ReturnsAsync(persons);

            //foreach (PersonAddRequest request in personAddRequests)
            //{
            //    PersonResponse personResponse = await _personsService.AddPerson(request);
            //    person_response_list_from_add.Add(personResponse);
            //}

            //print personResponses using ItestOutputHelper
            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse item in person_response_list_expected)
            {
                _testOutputHelper.WriteLine(item.ToString());
            }

            List<PersonResponse> allPersons =  await _personsService.GetAllPersons();

            //Act
            List<PersonResponse> persons_list_from_sort = await _personsService.GetSortedPersons(allPersons, nameof(Person.PersonName), SortOrderOptions.DESC);

            //print persons_list_from_get using ItestOutputHelper
            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse item in persons_list_from_sort)
            {
                _testOutputHelper.WriteLine(item.ToString());
            }

            person_response_list_expected = person_response_list_expected.OrderByDescending(temp =>  temp.PersonName).ToList();

            //Assert
            for (int i = 0; i < person_response_list_expected.Count; i++)
            {
                Assert.Equal(person_response_list_expected[i], persons_list_from_sort[i]);
            }
        }

        #endregion GetSortedPersons

        #region UpdatePerson
        //when we supply PersonUpdateRequest as null, then it should throw ArgumentNullException
        [Fact]
        public async Task UpdatePerson_NullPerson_ToBeArgumentNullException()
        {
            //Arrange
            PersonUpdateRequest? personUpdateRequest = null;

            //Act
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _personsService.UpdatePerson(personUpdateRequest);
            });            
        }

        //when we supply InvalidPersonID, then it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_InvalidPersonID_ToBeArgumentException()
        {
            //Arrange
            PersonUpdateRequest personUpdateRequest = new PersonUpdateRequest() { PersonID = Guid.NewGuid() };
            
            //Act
            await Assert.ThrowsAsync<ArgumentException>(async () => {
               await _personsService.UpdatePerson(personUpdateRequest);
            });
        }

        //When we supply PersonName as null, then it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_PersonNameIsNull_ToBeArgumentException()
        {
            //Arrange
            Person person = _fixture.Build<Person>()
                .With(temp => temp.PersonName, null as string)
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.Email, "someone_1@email.com")
                .With(temp => temp.Gender, "Male")
                .Create();

            PersonResponse personResponse_from_add = person.ToPersonResponse();

            PersonUpdateRequest personUpdateRequest = personResponse_from_add.ToPersonUpdateRequest();

            //Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => {
                //Act
               await _personsService.UpdatePerson(personUpdateRequest);
            });
        }

        //add a new person and try to update the person name & email
        [Fact]
        public async Task UpdatePerson_PersonFullDetails_ToBeSuccessful()
        {
            //Arrange
            Person person = _fixture.Build<Person>()
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.Email, "someone_1@email.com")
                .With(temp => temp.Gender, "Male")
                .Create();

            PersonResponse personResponse_expected = person.ToPersonResponse();

            PersonUpdateRequest personUpdateRequest = personResponse_expected.ToPersonUpdateRequest();

            _personsRepositoryMock.Setup(temp => temp.Update(It.IsAny<Person>())).ReturnsAsync(person);
            _personsRepositoryMock.Setup(temp => temp.GetById(It.IsAny<Guid>())).ReturnsAsync(person);

            //Act
            PersonResponse personResponse_from_update = await _personsService.UpdatePerson(personUpdateRequest);

            //Assert 
            Assert.Equal(personResponse_expected, personResponse_from_update);
        }

        #endregion UpdatePerson

        #region DeletePerson

        //if you supply valid PersonID then it should return true
        [Fact]
        public async Task DeletePerson_ValidPersonID_ToBeSuccessful()
        {
            //Arrange
            Person person = _fixture.Build<Person>()
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.Email, "someone_1@email.com")
                .With(temp => temp.Gender, "Male")
                .Create();

            //PersonResponse personResponse = person.ToPersonResponse();

            _personsRepositoryMock.Setup(temp => temp.Delete(It.IsAny<Guid>())).ReturnsAsync(true);
            _personsRepositoryMock.Setup(temp => temp.GetById(It.IsAny<Guid>())).ReturnsAsync(person);

            //Act
            bool isDeleted = await _personsService.DeletePerson(person.PersonID);

            //Assert
            Assert.True(isDeleted);
        }

        //if you supply invalid PersonID then it should return false
        [Fact]
        public async Task DeletePerson_InvalidPersonID()
        {
            //Act
            bool isDeleted = await _personsService.DeletePerson(Guid.NewGuid());

            //Assert
            Assert.False(isDeleted);
        }

        #endregion
    }
}