
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ServiceContracts
{
    /// <summary>
    /// Represents Business Logic for manupulating Person entity
    /// </summary>
    public interface IPersonsService
    {
        /// <summary>
        /// Add a new person into the list of Persons
        /// </summary>
        /// <param name="personAddRequest">Person to add</param>
        /// <returns>Return same person details along with newly generated PersonID</returns>
        Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest);

        /// <summary>
        /// Return all persons
        /// </summary>
        /// <returns>Return a list of objects of PersonResponse type</returns>
        Task<List<PersonResponse>> GetAllPersons();

        /// <summary>
        /// Return the Person object based on given person id
        /// </summary>
        /// <param name="PersonID">Person Id to search</param>
        /// <returns>Return matching Person object</returns>
        Task<PersonResponse?> GetPersonByPersonID(Guid? PersonID);

        /// <summary>
        /// Return all persons object that matches with the searchBy & searchString
        /// </summary>
        /// <param name="searchBy">Field to search</param>
        /// <param name="searchString">string to search</param>
        /// <returns>Return all matching persons </returns>
        Task<List<PersonResponse>> GetFilteredPersons(string? searchBy, string? searchString);

        Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder);

        /// <summary>
        /// Update a person based on update person request along with PersonID
        /// </summary>
        /// <param name="personUpdateRequest">person details including PersonID to update</param>
        /// <returns>returns the same person response object after update</returns>
        Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest);

        /// <summary>
        /// Deletes a person based on PersonID
        /// </summary>
        /// <param name="PersonID">PersonID to delete</param>
        /// <returns>true, if the deletion is successful; otherwise false</returns>
        Task<bool> DeletePerson(Guid? PersonID);

        /// <summary>
        /// Return persons as CSV
        /// </summary>
        /// <returns>Return the memory stream with CSV data</returns>
        Task<MemoryStream> GetPersonsCSV();

        Task<MemoryStream> GetPersonsExcel();

    }
}
