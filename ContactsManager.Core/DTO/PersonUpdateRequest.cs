using Entities;
using ServiceContracts.Enums;
using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTO
{
    /// <summary>
    /// Represents the DTO class that contains the person details to update
    /// </summary>
    public class PersonUpdateRequest
    {
        [Required(ErrorMessage = "Person ID can't be blank.")]
        public Guid PersonID { get; set; }

        [Required(ErrorMessage = "Person name can't be empty.")]
        public string? PersonName { get; set; }

        [Required(ErrorMessage = "Email can't be empty.")]
        [EmailAddress(ErrorMessage = "Email should be valid format.")]
        public string? Email { get; set; }

        public GenderOptions? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public Guid? CountryID { get; set; }
        public string? Address { get; set; }
        public bool ReceiveNewsLetters { get; set; }

        /// <summary>
        /// Converts the current object of PersonUpdateRequest into a new Person type
        /// </summary>
        /// <returns>Person object</returns>
        public Person ToPerson()
        {
            return new Person
            {
                PersonID = PersonID,
                PersonName = PersonName,
                Email = Email,
                Gender = Gender.ToString(),
                DateOfBirth = DateOfBirth,
                CountryID = CountryID,
                Address = Address,
                ReceiveNewsLetters = ReceiveNewsLetters
            };
        }
    }
}
